# -*- coding: utf-8 -*-
from calendar import timegm, monthrange
from json import loads
from re import compile, match
from datetime import date, datetime, time, timedelta
from math import ceil, fabs
from django import forms
from django.utils.functional import curry
from django.shortcuts import render, redirect
from django.utils.safestring import mark_safe
from django.http import HttpResponse, HttpResponseRedirect
from django.core.urlresolvers import reverse
from django.core.exceptions import ValidationError
# from .curencyService import TestDoctien
# TestDoctien()
##2017-12-14
from django.core.paginator import Paginator, EmptyPage, PageNotAnInteger
##
from django.contrib import admin, messages
from django.contrib.auth.admin import UserAdmin
from django.contrib.auth.decorators import login_required
import autocomplete_light
from services import *
from models import *
from support import decode_vehicle_type, encode_vehicle_type, to_local_time, get_day_of_week, int_format, get_status, get_status_new
import  locale
from site_settings.settings import TIME_ZONE
from common import VEHICLE_STATUS_CHOICE, VEHICLE_STATUS_COLOR_VALUE_DICT
from django.contrib.admin.helpers import ActionForm
from django.db import connections
from parking.views import callfeeforexception
from . import  report

# Register your models here.
REGEX_PARKINGFEE_ID = compile(ur'.+/(\d+)/')  # Nghia su dung trong admin ParkingFee

def convert_to_currency(i):
    return mark_safe('<span style="float:right; ">{0}</span>'.format("{:,}".format(i).replace(',', '.')))


def check_ip_syntax(ip):
    elements = ip.split('.')
    if len(elements) == 4:
        try:
            for e in elements:
                int(e)
            return True
        except ValueError:
            return False
    return False

def my_view(request, *args, **kwargs):
    return render(request, 'admin/coupon_generate_form.html', {
        'title': 'Coupon Generate',
    })

def clean_param(param):
        if hasattr(param, '_get_pk_val'):
            # has a pk value -- must be a model
            return str(param._get_pk_val())

        if callable(param):
            # it's callable, should call it.
            return str(param())

        return str(param)
class Utilities:
    def NonQuery(self, proc_name, *proc_params):
        new_params = [clean_param(param) for param in proc_params]
        cursor = connections['default'].cursor()
        ret = cursor.execute("%s %s(%s)" % ("CALL",
                                            proc_name,
                                            ', '.join('%s' for x in new_params)),
                             new_params)
        return ret

    def Query(self, proc_name, *proc_params):
        new_params = [clean_param(param) for param in proc_params]
        cursor = connections['default'].cursor()
        ret = cursor.execute("%s %s(%s)" % ("CALL",
                                            proc_name,
                                            ', '.join('%s' for x in new_params)),
                             new_params)

        rows=cursor.fetchall()
        retVal  = []
        for row in rows:
            retVal.append(row)
        return retVal
    def QueryDirect(self, qr):
        cursor = connections['default'].cursor()
        ret = cursor.execute(qr)
        rows = cursor.fetchall()
        retVal = []
        for row in rows:
            retVal.append(row)
        return retVal

class VehecleRegistryForm(forms.Form):
    customer=forms.CharField( required= True)
    card=forms.IntegerField( required= True)
    feelevel=forms.CharField( required=True)
    vehecletype=forms.CharField( required=True)
    vehecle_number=forms.CharField(required=True)
    vehecle_paint = forms.CharField(required=False)
    vehecle_brand = forms.CharField(required=False)
    note = forms.CharField(required=False)
    drive_name=forms.CharField(required=False)
    drive_phone = forms.CharField(required=False)
    from_date=forms.DateField(required=True, widget=forms.DateInput(format = '%d/%m/%Y'),input_formats='%d/%m/%Y' )
    to_date = forms.DateField( required=True, widget=forms.DateInput(format = '%d/%m/%Y'),input_formats='%d/%m/%Y')

class FeeAdjustmentAdmin(admin.ModelAdmin):
    def has_delete_permission(self, request, obj=None):
        return False
    model = FeeAdjustment
    list_display = ['vehicle_type', 'get_time', 'fee', 'remark' ]

    def get_time(self, obj):
        return obj.time.strftime("%d/%m/%Y")
    get_time.short_description = 'Thời gian'

class UserProfileForm(autocomplete_light.ModelForm):
    class Meta:
        model = UserProfile
        autocomplete_fields = ('card',)

class UserProfileInline(admin.StackedInline):
    form = UserProfileForm
    model = UserProfile

class ApmsUserAdmin(UserAdmin):
    def has_delete_permission(self, request, obj=None):
        return False
    fieldsets = (
        (None, {'fields': ('username', 'password')}),
        ('Permissions', {'fields': ('is_active', 'is_staff', 'is_superuser', 'groups', 'user_permissions')}),
    )
    list_display = ('username', 'userprofile', 'is_staff', 'is_superuser')
    inlines = [UserProfileInline]


class TerminalGroupForm(forms.ModelForm):
    class Meta:
        model = TerminalGroup

    class Media:
        css = {
            "": ("css/page.css",)
        }

    terminals = forms.ModelMultipleChoiceField(queryset=Terminal.objects.all(), label=u'Trạm',
                                               widget=forms.CheckboxSelectMultiple())

    def __init__(self, *args, **kwargs):
        super(TerminalGroupForm, self).__init__(*args, **kwargs)
        if self.instance:
            self.fields['terminals'].initial = self.instance.terminal_set.all()

    def save(self, *args, **kwargs):
        instance = super(TerminalGroupForm, self).save(commit=False)
        self.fields['terminals'].initial.update(terminal_group=None)
        self.cleaned_data['terminals'].update(terminal_group=instance)
        return instance


class TerminalGroupAdmin(admin.ModelAdmin):
    def has_delete_permission(self, request, obj=None):
        return False
    form = TerminalGroupForm

    class Meta:
        model = TerminalGroup

class CardForm(autocomplete_light.ModelForm):
    def __init__(self, *args, **kwargs):
        self.base_fields['card_type'].choices = load_card_type()
        self.base_fields['vehicle_type'].choices = load_vehicle_type()
        super(CardForm, self).__init__(*args, **kwargs)

    class Meta:
        model = Card
        autocomplete_fields = ('card_label',)

class CardTypeAdmin(admin.ModelAdmin):
    def has_delete_permission(self, request, obj=None):
        return False
    model = CardType
    list_display = ['id', 'name', ]
    list_display_links = ['name']
    ordering = ('id',)

    # def save_model(self, request, obj, form, change):
    # super(CardTypeAdmin, self).save_model(request, obj, form, change)
    #     load_card_type()


class VehicleTypeForm(forms.ModelForm):
    def save(self, *args, **kwargs):
        new_obj = self.instance
        vehicle_type_set = set()
        for vehicle_type in VehicleType.objects.all():
            vehicle_type_set.add(vehicle_type.id)
        if not new_obj.id:
            id_range = decode_vehicle_type(new_obj.category)
            for idx in range(id_range[0], id_range[1]):
                selected_id = encode_vehicle_type(idx, idx)
                if selected_id not in vehicle_type_set:
                    new_obj.id = selected_id
                    break
        return super(VehicleTypeForm, self).save(*args, **kwargs)

    class Meta:
        model = VehicleType
        fields = ('name', 'category','slot')


class VehicleTypeAdmin(admin.ModelAdmin):
    def has_delete_permission(self, request, obj=None):
        return False
    model = VehicleType
    list_display = ['id', 'name','slot']
    form = VehicleTypeForm

    def save_model(self, request, obj, form, change):
        super(VehicleTypeAdmin, self).save_model(request, obj, form, change)
        load_vehicle_type()
###ePass
import uuid
class ApiTokenAdmin(admin.ModelAdmin):
    model = ApiToken
    list_display = ('user', 'key', 'created')
    def save_model(self, request, obj, form, change):
        obj.key = uuid.uuid4().hex
        obj.save()

class EPassPrefixAdmin(admin.ModelAdmin):
    model = EPassPrefixCheck
    list_display = ('PrefixCode', 'Description')

class EPassCollectedAdmin(admin.ModelAdmin):
    model = EPassCollected
    list_per_page = 100
    list_display = ('epc', 'vehicelnumber', 'plateColor', 'hasSync', 'lastSync')
    search_fields = ['epc', 'vehiclenumber']
    list_filter = ('lastSync', 'hasSync')

class EPassPartnerAdmin(admin.ModelAdmin):
    def has_delete_permission(self, request, obj=None):
        return False
    model = EPassPartner

    list_display = ('partnerCode', 'parkingCode', 'activated', 'loginSuccess')
    def save_model(self, request, obj, form, change):
        if obj.activated is True:
            try:
                id = 0
                if obj.id:
                    id = obj.id
                dts = EPassPartner.objects.filter(activated = True).exclude(id = id)
                for d in dts:
                    d.activated = False
                    d.save()
            except:
                pass
        obj.save()

class EPAssApiAdmin(admin.ModelAdmin):
    # def has_delete_permission(self, request, obj=None):
    #     return False
    model = EPassAPI

    list_display = ('target', 'partner', 'url', 'method', 'headers', 'body')

###

####Invoices
class InvoiceTaxRuleAdmin(admin.ModelAdmin):
    model = InvoiceTaxRule

    list_display = ('mode', 'get_taxpercentage', 'feeincludesvat', 'activated')
    def save_model(self, request, obj, form, change):
        if obj.activated is True:
            try:
                id = 0
                mode = obj.mode
                if obj.id:
                    id = obj.id
                dts = InvoiceTaxRule.objects.filter(mode= mode,activated = True).exclude(id = id)
                for d in dts:
                    d.activated = False
                    d.save()
            except:
                pass
        obj.save()

    def get_taxpercentage(self, obj):
        return '%s' % obj.taxpercentage + ' %'

    get_taxpercentage.short_description = u'% VAT'

class PartnerInvoiceAdmin(admin.ModelAdmin):
    def has_delete_permission(self, request, obj=None):
        return False
    model = PartnerInvoice

    list_display = ('code', 'name', 'activated')
    def save_model(self, request, obj, form, change):
        if obj.activated is True:
            try:
                id = 0
                if obj.id:
                    id = obj.id
                dts = PartnerInvoice.objects.filter(activated = True).exclude(id = id)
                for d in dts:
                    d.activated = False
                    d.save()
            except:
                pass
        obj.save()

class InvoiceApiAdmin(admin.ModelAdmin):
    # def has_delete_permission(self, request, obj=None):
    #     return False
    model = InvoiceApiInitation

    list_display = ('target', 'partner', 'url', 'method', 'headers', 'body')

import requests
import json
from  invoicePlan import SetTimeConsolidatedInvoice
class InvoiceConnectorAdmin(admin.ModelAdmin):
    # def has_delete_permission(self, request, obj=None):
    #     return False
    model = InvoiceConnector

    list_display = ('appid', 'partner', 'username', 'taxcode', 'invoiceserie', 'invoicetemplate', 'maxamount', 'lastupdate','scheduletime', 'isvalid')
    fieldsets = [
        ('Thông đăng nhập', {'fields': ['appid', 'taxcode', 'username',
                                             'password'
                                             ]}),
        ('Thông tin điều hướng', {'fields': ['partner', 'invoiceserie', 'invoicetemplate', 'maxamount', 'scheduletime'],
                                                  'classes': ['collapse', 'extrapretty']}),
    ]
    actions = ['make_authenticate']
    def make_authenticate(modeladmin, request, queryset):
        urlmisaToken = ""
        for obj in queryset:
            if obj.partner.code == 'TESTMISAINVOICE' or obj.partner.code == 'MISAINVOICE':
                if obj.partner.code == 'TESTMISAINVOICE':
                    urlmisaToken = "https://testapi.meinvoice.vn/api/integration/auth/token"
                elif obj.partner.code == 'MISAINVOICE':
                    urlmisaToken = "https://api.meinvoice.vn/api/integration/auth/token"
                sendData = {
                    "appid": obj.appid,
                    "taxcode": obj.taxcode,
                    "username": obj.username,
                    "password": obj.password
                }
                data = str(json.dumps(sendData, ensure_ascii=False))
                headers = {'Content-Type': "application/json"}
                x = requests.post(url=urlmisaToken, data=data, headers=headers)
                ct = x.content
                st = x.status_code
                if st == 200:
                    try:
                        jsd = json.loads(ct)
                        if "success" in jsd and jsd["success"] is True and "data" in jsd and jsd["data"] is not None:
                            obj.token = jsd["data"]
                            obj.isvalid = True
                            obj.lastupdate = datetime.datetime.now()
                            obj.save()
                            if obj.partner.activated is True and obj.scheduletime is not None:
                                SetTimeConsolidatedInvoice(obj.scheduletime.hour, obj.scheduletime.minute,obj.scheduletime.second)
                    except Exception as ex:
                        pass
            elif obj.partner.code == 'FAST_CAMRANH_INVOICE':
                urlmisaToken = "https://14.248.85.18:9987/camranhs/oauth2/token"
                data = {
                  "client_secret": "f1f9424a-925f-4fa9-9007-ad3adf8c18b9",
                  "grant_type": "client_credentials",
                  "client_id": "camranhport"
                }
                headers = {
                  "Content-Type": "application/x-www-form-urlencoded"
                }
                x = requests.post(url=urlmisaToken, data=data, headers=headers)
                ct = x.content
                st = x.status_code
                if st == 200:
                    try:
                        jsd = json.loads(ct)
                        if "access_token" in jsd and jsd["access_token"] is not None:
                            obj.token = jsd["access_token"]
                            obj.isvalid = True
                            obj.lastupdate = datetime.datetime.now()
                            obj.save()
                            if obj.partner.activated is True and obj.scheduletime is not None:
                                SetTimeConsolidatedInvoice(obj.scheduletime.hour, obj.scheduletime.minute,
                                                           obj.scheduletime.second)
                    except Exception as ex:
                        pass
    make_authenticate.short_description = u"Chứng thực với đối tác"
class InvoiceBuyerAdmin(admin.ModelAdmin):
    # def has_delete_permission(self, request, obj=None):
    #     return False
    model = InvoiceBuyer

    list_display = ('mode', 'legalname',  'taxcode', 'buyername', 'email', 'receivername', 'receiveremails')
    fieldsets = [
        ('Người mua', {'fields': ['buyername', 'email', 'phone','code', 'mode']}),
        ('Đơn vị', {'fields': ['legalname', 'taxcode', 'address'],
                                                  'classes': ['collapse', 'extrapretty']}),
        ('Gửi mail', {'fields': ['receivername', 'receiveremails'],
                    'classes': ['collapse', 'extrapretty']}),
    ]
    def save_model(self, request, obj, form, change):
        if obj.mode ==1:
            try:
                id = 0

                if obj.id:
                    id = obj.id
                dts = InvoiceBuyer.objects.filter(mode= 1).exclude(id = id)
                if not dts:
                    obj.save()
                else:
                    messages.error(request, "Đã có thông tin người mua hóa đơn tổng hợp")
                    messages.success(request, "")
            except Exception as ex:
                messages.error(request, ex)
                messages.success(request, "")
        else:
            messages.error(request, "Thông tin người mua lẻ chỉ được phép thêm hoặc thay đổi bằng ứng dụng")
            messages.success(request, "")

class InvoiceFilterForm(forms.Form):
    from_date = forms.DateField(required=False, label="Từ ngày")
    to_date = forms.DateField(required=False, label="Đến ngày")
class RetailInvoiceAdmin(admin.ModelAdmin):
    model = RetailInvoice
    list_per_page = 10
    list_display = ('parkingrefid', 'refid', 'transactionid', 'contentrequest', 'contentresponse', 'invoicedate', 'parkingfee', 'iscompleted')
    search_fields = ['parkingrefid', 'transactionid', 'refid']
    list_filter = ('invoicedate','iscompleted')

    def get_queryset(self, request):
        qs = super(RetailInvoiceAdmin, self).get_queryset(request)

        # Chỉ lọc nếu chưa có query string filter nào được áp dụng
        if 'invoicedate__gte' not in request.GET and 'invoicedate__lte' not in request.GET:
            # Lấy ngày đầu tuần (thứ Hai)
            today = datetime.datetime.now().date()
            start_of_week = today - datetime.timedelta(days=7)
            end_of_week = today

            # Lọc các object trong tuần này
            qs = qs.filter(invoicedate__gte=start_of_week, invoicedate__lte=end_of_week)
        return qs
    def has_add_permission(self, request):
        return False  # không cho thêm

    def has_change_permission(self, request, obj=None):
        return True  # ✅ Cho phép truy cập để xem chi tiết

    def has_delete_permission(self, request, obj=None):
        return False  # không cho xóa

    def get_readonly_fields(self, request, obj=None):
        # Toàn bộ các trường đều readonly
        return [field.name for field in self.model._meta.fields]

    def change_view(self, request, object_id, form_url='', extra_context=None):
        extra_context = extra_context or {}
        extra_context['show_save'] = False
        extra_context['show_save_and_continue'] = False
        extra_context['show_save_and_add_another'] = False
        return super(RetailInvoiceAdmin, self).change_view(request, object_id, form_url, extra_context=extra_context)

    actions = None  # Ẩn các action xóa hàng loạt

class ConsolidatedInvoiceAdmin(admin.ModelAdmin):
    model = ConsolidatedInvoice
    list_per_page = 10
    list_display = ('refid', 'transactionid', 'contentrequest', 'contentresponse', 'invoicedate', 'parkingfees', 'iscompleted')
    search_fields = ['transactionid', 'refid']
    list_filter = ('invoicedate','iscompleted')

    def get_queryset(self, request):
        qs = super(ConsolidatedInvoiceAdmin, self).get_queryset(request)

        # Chỉ lọc nếu chưa có query string filter nào được áp dụng
        if 'invoicedate__gte' not in request.GET and 'invoicedate__lte' not in request.GET:
            # Lấy ngày đầu tuần (thứ Hai)
            today = datetime.datetime.now().date()
            start_of_week = today - datetime.timedelta(days=7)
            end_of_week = today

            # Lọc các object trong tuần này
            qs = qs.filter(invoicedate__gte=start_of_week, invoicedate__lte=end_of_week)
        return qs
    def has_add_permission(self, request):
        return False  # không cho thêm

    def has_change_permission(self, request, obj=None):
        return True  # ✅ Cho phép truy cập để xem chi tiết

    def has_delete_permission(self, request, obj=None):
        return False  # không cho xóa

    def get_readonly_fields(self, request, obj=None):
        # Toàn bộ các trường đều readonly
        return [field.name for field in self.model._meta.fields]

    def change_view(self, request, object_id, form_url='', extra_context=None):
        extra_context = extra_context or {}
        extra_context['show_save'] = False
        extra_context['show_save_and_continue'] = False
        extra_context['show_save_and_add_another'] = False
        return super(ConsolidatedInvoice, self).change_view(request, object_id, form_url, extra_context=extra_context)

    actions = None  # Ẩn các action xóa hàng loạt

####

class CardAdmin(admin.ModelAdmin):
    def has_delete_permission(self, request, obj=None):
        return False
    model = Card
    form = CardForm
    list_per_page = 100
    list_display = ('card_label', 'card_id', 'card_type', 'vehicle_type', 'get_vehicle_driver_name', "get_customer", 'status')
    search_fields = ['card_label',]
    list_filter = ('status', 'vehicle_type', 'card_type',)
    actions = ['make_locked', 'make_unlock']

    # def has_add_permission(self, request):
    #     return False

    def get_queryset(self, request):
        qs = super(CardAdmin, self).get_queryset(request)

        if len(request.GET) == 0:
            return qs.exclude(status='2')
        return qs

    def make_unlock(modeladmin, request, queryset):
        queryset.update(status='1')

    make_unlock.short_description = u"Mở khóa tất cả các thẻ được chọn"

    def make_locked(modeladmin, request, queryset):
        if 'apply' in request.POST:
            note = request.POST['note']

            for obj in queryset:
                obj.status = '2'
                obj.note = note
                obj.save()

            # queryset.update(status='2', note=note)

            return HttpResponseRedirect(request.get_full_path())

        return render(request,
                      'admin/card_lock_reason.html',
                      {'cards': queryset})
        # queryset.update(status='2')

    def get_customer(self, obj):
        registration = VehicleRegistration.objects.filter(card_id=obj.id, status=1)
        if len(registration) > 0:
            return registration[0].customer.customer_name
        return ''
    get_customer.short_description = u'Khách hàng'

    def get_vehicle_driver_name(self, obj):
        vehicle_registrations = VehicleRegistration.objects.filter(card_id=obj.id, status=1)
        if vehicle_registrations:
            temp3 = u'<p>%s</p>'
            final = []
            for vehicle_registration in vehicle_registrations:
                final.append(temp3 % (vehicle_registration.vehicle_driver_name))
            return "".join(final)
        return ''

    # get_vehicle_driver_name
    get_vehicle_driver_name.short_description = u'Tên chủ xe'
    get_vehicle_driver_name.allow_tags = True
    #
    # def get_company(self, obj):
    #     registration = VehicleRegistration.objects.filter(card_id=obj.id, status=1)
    #     if len(registration) > 0:
    #         company = registration[0].customer.company
    #         if company:
    #             return company.name
    #     return ''
    # get_company.short_description = u'Công ty'

    make_locked.short_description = u"Khóa tất cả các thẻ được chọn"

class ParkingSettingAdmin(admin.ModelAdmin):
    def has_delete_permission(self, request, obj=None):
        return False
    model = ParkingSetting
    list_display = ('name', 'value')
    fields = ['name', 'value', 'get_notes']
    readonly_fields = ['get_notes',]

    def has_add_permission(self, request):
        return False

    def get_notes(self, obj):
        if obj.key == 'invoice_pdf_form':
            return mark_safe(
                '<a onclick="return showAddAnotherPopup(this);" style="font-weight:bold; text-decoration:underline" href="{0}">Nhập mẫu hóa đơn PDF</a>'.format(
                    '/admin/setting/upload/invoice_pdf_form'))
        elif obj.key == 'logo_report':
            return mark_safe(
                '<a onclick="return showAddAnotherPopup(this);" style="font-weight:bold; text-decoration:underline" href="{0}">Nhập mẫu logo dùng trong báo cáo</a>'.format(
                    '/admin/setting/upload/logo_report'))
        return obj.notes or ''

    get_notes.allow_tags = True
    get_notes.short_description = u'Ghi chú'


class CardAutoCompleteForm(autocomplete_light.ModelForm):
    class Meta:
        model = VehicleRegistration
        autocomplete_fields = ('card',)


class CardInline(admin.TabularInline):
    # form = CardForm
    model = VehicleRegistration
    def has_delete_permission(self, request, obj=None):
        return False

class VehicleRegistrationInline(admin.TabularInline):

    model = VehicleRegistration
    extra = 0
    fieldsets = [
        ('...', {'fields': ['get_id', 'get_vehicle_number', 'vehicle_type', 'vehicle_driver_name', 'card', 'level_fee',
                            'get_first_renewal_effective_date', 'get_last_renewal_date',
                            'get_last_renewal_effective_date', 'get_expired_date', 'get_remain_duration',
                            'get_vehicle_status'], }), ]

    readonly_fields = ['get_id', 'card', 'vehicle_type', 'get_vehicle_number', 'level_fee', 'get_vehicle_status',
                       'get_first_renewal_effective_date', 'get_last_renewal_date', 'get_last_renewal_effective_date',
                       'get_expired_date', 'vehicle_driver_name', 'get_remain_duration']

    def has_delete_permission(self, request, obj=None):
        return False

    def get_id(self, obj):
        return obj.id
    get_id.short_description = u'Mã xe'

    def get_vehicle_status(self, obj):
        try:
            return get_status_new(obj, True)
        except Exception:
            return get_status(obj.status, True)

    get_vehicle_status.allow_tags = True
    get_vehicle_status.short_description = u'Trạng thái'

    def get_vehicle_number(self, obj):
        if obj:
            return mark_safe(u'<a style="font-weight:bold" href="/admin/parking/vehicleregistration/%s">%s</a>' % (
            obj.id, obj.vehicle_number))

    def get_registration_date(self, obj):
        if obj.registration_date:
            return obj.registration_date.astimezone(pytz.timezone(TIME_ZONE)).strftime('%d/%m/%Y')
        return ''

    def get_first_renewal_effective_date(self, obj):
        if obj.first_renewal_effective_date:
            day = datetime.datetime(obj.first_renewal_effective_date.year, obj.first_renewal_effective_date.month,
                                    obj.first_renewal_effective_date.day)
            return pytz.utc.localize(day).astimezone(pytz.timezone(TIME_ZONE)).strftime('%d/%m/%Y')
        return ''

    def get_last_renewal_date(self, obj):
        if obj.last_renewal_date:
            day = datetime.datetime(obj.last_renewal_date.year, obj.last_renewal_date.month, obj.last_renewal_date.day)
            return pytz.utc.localize(day).astimezone(pytz.timezone(TIME_ZONE)).strftime('%d/%m/%Y')
        return ''

    get_last_renewal_date.short_description = u'Ngày đóng phí'

    def get_old_expired_date(self, obj):
        if obj.old_expired_date:
            day = datetime.datetime(obj.old_expired_date.year, obj.old_expired_date.month, obj.old_expired_date.day)
            return pytz.utc.localize(day).astimezone(pytz.timezone(TIME_ZONE)).strftime('%d/%m/%Y')
        return ''

    def get_expired_date(self, obj):
        if obj.expired_date:
            day = datetime.datetime(obj.expired_date.year, obj.expired_date.month, obj.expired_date.day)
            return pytz.utc.localize(day).astimezone(pytz.timezone(TIME_ZONE)).strftime('%d/%m/%Y')
        return ''

    def get_pause_date(self, obj):
        if obj.pause_date:
            day = datetime.datetime(obj.pause_date.year, obj.pause_date.month, obj.pause_date.day)
            return pytz.utc.localize(day).astimezone(pytz.timezone(TIME_ZONE)).strftime('%d/%m/%Y')
        return ''

    def get_last_renewal_effective_date(self, obj):
        if obj.last_renewal_effective_date:
            day = datetime.datetime(obj.last_renewal_effective_date.year, obj.last_renewal_effective_date.month,
                                    obj.last_renewal_effective_date.day)
            return pytz.utc.localize(day).astimezone(pytz.timezone(TIME_ZONE)).strftime('%d/%m/%Y')
        return ''

    def get_remain_duration(self, obj):
        remain_duration = 0

        if obj.id:
            last_pause = PauseResumeHistory.objects.filter(vehicle_registration_id=obj.id, used=False).order_by(
                '-request_date')
            if last_pause:
                remain_duration = last_pause[0].remain_duration
            elif obj.expired_date:
                expired_date = obj.expired_date
                start_date = obj.start_date

                today = datetime.date.today()
                if expired_date > today:
                    if start_date >= today:
                        remain_duration = (expired_date - start_date).days
                    else:
                        remain_duration = (expired_date - today).days
        return remain_duration

    get_remain_duration.short_description = u'Thời gian còn lại'

    get_vehicle_number.short_description = u'Biển số'
    get_registration_date.short_description = u'Ngày tạo'
    get_first_renewal_effective_date.short_description = u'Ngày đăng ký'
    get_last_renewal_effective_date.short_description = u'Ngày hiệu lực'
    get_expired_date.short_description = u'Hạn hiện tại'
    get_pause_date.short_description = u'Ngày tạm ngừng'


class TicketPaymentInline(admin.TabularInline):
    model = TicketPayment
    extra = 0
    fields = ['get_ticket_payment_link', 'get_receipt_number', 'get_vehicle_registration', 'get_vehicle_type',
              'get_payment_date', 'get_this_old_expired_date', 'get_this_expired_date', 'get_payment_fee', 'staff', 'get_pdf_ticket_payment']
    readonly_fields = ['get_ticket_payment_link', 'get_receipt_number', 'get_vehicle_registration', 'get_vehicle_type',
                       'get_payment_date', 'get_this_old_expired_date', 'get_this_expired_date', 'get_payment_fee', 'staff', 'get_pdf_ticket_payment']
    ordering = ['-receipt_number', '-payment_date']

    def has_add_permission(self, request):
        return False

    def has_delete_permission(self, request, obj=None):
        return False

    def get_ticket_payment_link(self, obj):
        if obj:
            return mark_safe(
                u'<a style="font-weight:bold" href="/admin/parking/ticketpayment/%s">%s</a>' % (obj.id, obj.id))

    def get_payment_fee(self, obj):
        ticket_payment_details = TicketPaymentDetail.objects.filter(ticket_payment_id=obj.id)
        # temp = u'>%s</p>'
        s = ''
        if ticket_payment_details:
            for payment_detail_fee in ticket_payment_details.values_list('payment_detail_fee'):
                s += '%s<br />' % (convert_to_currency(payment_detail_fee[0]))
            s += u'<b>TC: %s</b>' % convert_to_currency(int(obj.payment_fee))
            return mark_safe(s)
        return ''
        # return convert_to_currency(int(obj.payment_fee))

    get_payment_fee.allow_tags = True
    get_payment_fee.short_description = u'Số tiền (đ)'

    def get_vehicle_registration(self, obj):
        ticket_payment_details = TicketPaymentDetail.objects.filter(ticket_payment_id=obj.id)
        temp = u'<p>%s<p>'
        s = ''

        if ticket_payment_details:
            # record = []
            for ticket_payment_detail in list(ticket_payment_details):
                current = ticket_payment_detail.vehicle_number or ''
                # if current not in record:
                #     record.append(current)
                s += temp % (current)
            return mark_safe(s)
        return ''

    get_vehicle_registration.short_description = u'Biển số xe'
    get_vehicle_registration.allow_tags = True

    def get_payment_date(self, obj):
        return obj.payment_date.astimezone(pytz.timezone(TIME_ZONE)).strftime('%d/%m/%Y %H:%M:%S')

    get_payment_date.short_description = u'Ngày đóng phí'

    def get_this_old_expired_date(self, obj):
        ticket_payment_details = TicketPaymentDetail.objects.filter(ticket_payment_id=obj.id)
        temp = u'<p>%s<p>'
        s = ''
        if ticket_payment_details:
            for old_expired_date in ticket_payment_details.values_list('old_expired_date'):
                if old_expired_date[0]:
                    s += temp % (old_expired_date[0].strftime("%d/%m/%Y"))
            return mark_safe(s)
        return ''

    get_this_old_expired_date.short_description = u'Hạn cũ'
    get_this_old_expired_date.allow_tags = True

    def get_this_expired_date(self, obj):
        ticket_payment_details = TicketPaymentDetail.objects.filter(ticket_payment_id=obj.id)
        temp = u'<p>%s<p>'
        s = ''
        if ticket_payment_details:
            for expired_date in ticket_payment_details.values_list('expired_date'):
                if expired_date[0]:
                    s += temp % (expired_date[0].strftime("%d/%m/%Y"))
            return mark_safe(s)
        return ''

    get_this_expired_date.short_description = u'Hạn mới'
    get_this_expired_date.allow_tags = True


    def get_vehicle_type(self, obj):
        ticket_payment_details = TicketPaymentDetail.objects.filter(ticket_payment_id=obj.id)

        temp = u'<p>%s<p>'
        s = ''

        if ticket_payment_details:
            # record = []
            for ticket_payment_detail in list(ticket_payment_details):
                current = u"%s" % ticket_payment_detail.vehicle_registration.vehicle_type if ticket_payment_detail.vehicle_registration else ''

                # if current not in record:
                #     record.append(current)
                s += temp % (current)
            return mark_safe(s)
        return ''

    def get_receipt_number(self, obj):
        if obj.receipt_number:
            return mark_safe(u'<a target="_blank" href="/admin/parking/receipt/{0}">{1}</a>'.format(obj.receipt_id,
                                                                                                    obj.receipt_number))
        return ''

    get_receipt_number.allow_tags = True
    get_receipt_number.short_description = u'Mã phiếu thu'

    def get_pdf_ticket_payment(self, obj):
        if obj.id:
            return mark_safe(
                '<div style="text-align:center"><a target="_blank" onclick="return showAddAnotherPopup(this);" style="font-size:120%" class="fa fa-print" href="/pdf/ticket-payment/{0}"></a> <a target="_blank" onclick="return showAddAnotherPopup(this);" style="font-size:120%; color: orange" class="fa fa-print" href="/pdf/ticket-payment/{0}/1"></a></div>'.format(
                    obj.id))
        else:
            return ''

    get_pdf_ticket_payment.allow_tags = True
    get_pdf_ticket_payment.short_description = u'Phiếu thu'

    # def formfield_for_foreignkey(self, db_field, request, **kwargs):
    #     s = request.path
    #     if db_field.name == "staff":
    #         print request.user.id
    #         kwargs["queryset"] = UserProfile.objects.filter(user_id=request.user.id)
    #     return super(TicketPaymentInline, self).formfield_for_foreignkey(db_field, request, **kwargs)

    get_ticket_payment_link.short_description = u'Mã'
    get_vehicle_type.short_description = u'Loại xe'


class DepositPaymentInline(admin.TabularInline):
    model = DepositPayment
    extra = 0
    fields = ['get_deposit_payment_link', 'get_receipt_number', 'get_vehicle_number', 'get_vehicle_type',
              'get_payment_date', 'get_payment_fee', 'staff', 'get_pdf_ticket_payment']
    readonly_fields = ['get_deposit_payment_link', 'get_receipt_number', 'get_vehicle_number', 'get_vehicle_type',
                       'get_payment_date', 'get_payment_fee', 'staff', 'get_pdf_ticket_payment']
    ordering = ['-receipt_number', '-payment_date']

    def has_add_permission(self, request):
        return False

    def has_delete_permission(self, request, obj=None):
        return False

    def get_receipt_number(self, obj):
        if obj.receipt_number:
            return mark_safe(u'<a target="_blank" href="/admin/parking/receipt/{0}">{1}</a>'.format(obj.receipt_id,
                                                                                                    obj.receipt_number))
        return ''

    get_receipt_number.allow_tags = True
    get_receipt_number.short_description = u'Mã phiếu thu'

    def get_payment_fee(self, obj):
        return convert_to_currency(int(obj.payment_fee))

    get_payment_fee.allow_tags = True
    get_payment_fee.short_description = u'Số tiền (đ)'

    def get_deposit_payment_link(self, obj):
        if obj:
            return mark_safe(
                u'<a style="font-weight:bold" href="/admin/parking/depositpayment/%s">%s</a>' % (obj.id, obj.id))

    def get_payment_date(self, obj):
        return obj.payment_date.astimezone(pytz.timezone(TIME_ZONE)).strftime('%d/%m/%Y %H:%M:%S')

    def get_vehicle_number(self, obj):
        deposit_payment_details = DepositPaymentDetail.objects.filter(deposit_payment_id=obj.id)

        temp = u'<p>%s<p>'
        s = ''

        if deposit_payment_details:
            # record = []
            for deposit_payment_detail in list(deposit_payment_details):
                current = deposit_payment_detail.vehicle_number or ''

                # if current not in record:
                #     record.append(current)
                s += temp % (current)
            return mark_safe(s)
        return ''

    def get_vehicle_type(self, obj):
        deposit_payment_details = DepositPaymentDetail.objects.filter(deposit_payment_id=obj.id)

        temp = u'<p>%s<p>'
        s = ''

        if deposit_payment_details:
            # record = []
            for deposit_payment_detail in list(deposit_payment_details):
                current = u"%s" % deposit_payment_detail.vehicle_registration.vehicle_type if deposit_payment_detail.vehicle_registration else ''

                # if current not in record:
                #     record.append(current)
                s += temp % (current)
            return mark_safe(s)
        return ''

    get_vehicle_number.short_description = u'Biển số xe'
    get_vehicle_type.short_description = u'Loại xe'
    get_deposit_payment_link.short_description = u'Mã'
    get_payment_date.short_description = u'Thanh toán lần cuối'

    def get_pdf_ticket_payment(self, obj):
        if obj.id:
            return mark_safe(
                '<div style="text-align:center"><a target="_blank" onclick="return showAddAnotherPopup(this);" style="font-size:120%" class="fa fa-print" href="/pdf/deposit-payment/{0}"></a> <a target="_blank" onclick="return showAddAnotherPopup(this);" style="font-size:120%; color:orange" class="fa fa-print" href="/pdf/deposit-payment/{0}/1"></a></div>'.format(
                    obj.id))
        else:
            return ''

    get_pdf_ticket_payment.allow_tags = True
    get_pdf_ticket_payment.short_description = u'Phiếu thu'
    # def formfield_for_foreignkey(self, db_field, request, **kwargs):
    #     s = request.path
    #     if db_field.name == "staff":
    #         print request.user.id
    #         kwargs["queryset"] = UserProfile.objects.filter(user_id=request.user.id)
    #     return super(TicketPaymentInline, self).formfield_for_foreignkey(db_field, request, **kwargs)


class CustomerAdmin(admin.ModelAdmin):
    def has_delete_permission(self, request, obj=None):
        return False
    list_display = ['customer_name', 'customer_id','get_Card_id' ,'get_vehicle_driver_name' , 'get_vehicle_number' , 'get_vehicle_type', 'get_expired_date', 'customer_type',
                    'building', 'company', 'get_renewal']

    list_per_page = 40

    fieldsets = [
        ('Thông tin khách hàng', {'fields': ['customer_name', 'customer_id', 'customer_birthday',
                                             'customer_type', 'customer_phone', 'customer_mobile', 'customer_email',
                                             'staff'
                                             ]}),
        ('Thông tin tòa nhà - căn hộ - công ty', {'fields': ['apartment', 'building', 'company', ],
                                                  'classes': ['collapse', 'extrapretty']}),
        ('Thông tin hóa đơn', {'fields': ['order_register_name', 'order_tax_code', 'order_register_address', ],
                               'classes': ['collapse', 'extrapretty']}),
        ('Thông tin nhắc phí', {'fields': [['messaging_via_sms', 'messaging_sms_phone'],
                                           ['messaging_via_phone', 'messaging_phone'],
                                           ['messaging_via_email', 'messaging_email'],
                                           ['messaging_via_apart_mail', 'messaging_address'],
                                           ['messaging_via_wiper_mail'], ],
                                'classes': ['collapse', 'extrapretty']
                                }),
    ]

    readonly_fields = ['staff']

    inlines = [VehicleRegistrationInline, TicketPaymentInline, DepositPaymentInline]

    search_fields = ['customer_name']

    list_filter = ['customer_type']

    ordering = ['customer_name']

    change_form_template = ['admin/admin-customer.html']

    def get_vehicle_number(self, obj):
        vehicle_registrations = VehicleRegistration.objects.filter(customer_id=obj.id)
        if vehicle_registrations:
            temp2 = u'<p style="max-width:120px;" ><a target="_blank" href="%s">%s</a></p>'
            # s = u''
            final = []
            for vehicle_registration in vehicle_registrations:
                final.append(temp2 % ("/admin/parking/vehicleregistration/" + str(vehicle_registration.id),
                                      vehicle_registration.vehicle_number))
            return "".join(final)
        return ''

    def get_Card_id(self, obj):
        vehicle_registrations = VehicleRegistration.objects.filter(customer_id=obj.id)
        if vehicle_registrations:
            temp4 = u'<p>%s</p>'
            # s = u''
            final = []
            for vehicle_registration in vehicle_registrations:
                final.append(temp4 % (vehicle_registration.card))
            return "".join(final)
        return ''

    # vehicle_driver_name


    def get_vehicle_driver_name(self, obj):
        vehicle_registrations = VehicleRegistration.objects.filter(customer_id=obj.id)
        if vehicle_registrations:
            temp3 = u'<p>%s</p>'
            # s = u''
            final = []
            for vehicle_registration in vehicle_registrations:
                final.append(temp3 % (vehicle_registration.vehicle_driver_name))
            return "".join(final)
        return ''

    def get_vehicle_type(self, obj):
        vehicle_registrations = VehicleRegistration.objects.filter(customer_id=obj.id)
        if vehicle_registrations:
            temp = u'<p>%s</p>'
            # s = u''
            final = []
            for vehicle_registration in vehicle_registrations:
                # s += temp % (vehicle_registration.vehicle_type)
                final.append(temp % (vehicle_registration.vehicle_type))
            return "".join(final)
        return ''

    def get_expired_date(self, obj):
        vehicle_registrations = VehicleRegistration.objects.filter(customer_id=obj.id)
        if vehicle_registrations:
            temp = u'<p>%s</p>'
            final = []
            for vehicle_registration in vehicle_registrations:
                final.append(temp % (
                vehicle_registration.expired_date.strftime('%d/%m/%Y') if vehicle_registration.expired_date else ''))
            return "".join(final)
        return ''

    def get_renewal(self, obj):
        s = u"<div style='text-align:center'><a class='fa fa-money' target='_blank' onclick='return showAddAnotherPopup(this);'  style='font-size:140%;' href='/admin/parking/ticketpayment/add/?customer={0}&customer_list_display=1'></a></div>".format(
            obj.id)
        return mark_safe(s)

    get_vehicle_number.short_description = u'Xe đăng ký'
    get_vehicle_number.allow_tags = True
    get_vehicle_type.short_description = u'Loại xe'
    get_vehicle_type.allow_tags = True
    get_expired_date.short_description = u'Hạn hiện tại'
    get_expired_date.allow_tags = True

    get_Card_id.short_description = u'Tên thẻ'
    get_Card_id.allow_tags = True

    # get_vehicle_driver_name
    get_vehicle_driver_name.short_description = u'Tên chủ xe'
    get_vehicle_driver_name.allow_tags = True

    get_renewal.short_description = u'Gia hạn'
    get_renewal.allow_tags = True

    # def action_export(self, request, queryset):
    #     selected = request.POST.getlist(admin.ACTION_CHECKBOX_NAME)
    #     return HttpResponse("http://.../admin/customer/Search/")
    # action_export.short_description = u'Xuất thông tin khách hàng '

    def action_delete_selected(self, request, queryset):
        try:
            selected = request.POST.getlist(admin.ACTION_CHECKBOX_NAME)

            for customer in queryset:
                # Xoa khach hang
                customer_id = customer.id
                customer.audit_log.disable_tracking()

                vehicle_registrations = VehicleRegistration.objects.filter(customer_id=customer_id)
                for vehicle_registration in vehicle_registrations:
                    vehicle_registration.audit_log.disable_tracking()
                    vehicle_registrations.delete()

                customer.delete()
            # queryset.delete()
            messages.success(request, "Xóa {0} khách hàng thành công".format(len(selected) if selected else 0))
        except Exception as e:
            print e
            messages.error(request, "Lỗi xóa khách hàng, liên hệ quản trị viên.")

    action_delete_selected.short_description = u'Xóa khách hàng được chọn'

    def get_actions(self, request):
        actions = super(CustomerAdmin, self).get_actions(request)
        del actions['delete_selected']
        return actions

    actions = ['action_delete_selected']

    def save_model(self, request, obj, form, change):
        if not obj.id and request.user:
            try:
                obj.staff = request.user
            except UserProfile.DoesNotExist:
                pass
            except:
                pass
        obj.save()

        # def save_formset(self, request, form, formset, change):
        #     instances = formset.save(commit=False)
        #     print instances
        #     for instance in instances:
        #         if isinstance(instance, VehicleRegistration):
        #             if not id: # Dang ky moi
        #                 print "dang ky moi"

        # if isinstance(instance, TicketPayment): #Check if it is the correct type of inline
        #     if not instance.id:
        #         print "im inside 2"
        #         #instance.staff = UserProfile.objects.get(user_id=request.user.id)
        # instance.save()
        # formset.save_m2m()


class ImageReplicationSettingsForm(forms.ModelForm):
    class Meta:
        model = ImageReplicationSetting
        fields = ('sour_ip', 'dest_ip_list')

    def clean(self):
        obj = self.cleaned_data
        sour_ip = obj['sour_ip'].strip()
        if not check_ip_syntax(sour_ip):
            raise ValidationError(u'Địa chỉ IP gốc không hợp lệ', code='invalid', )
        obj['sour_ip'] = sour_ip
        dest_ip_list = obj['dest_ip_list']
        dest_ip_list = dest_ip_list.replace(' ', '')
        dest_ip_list = dest_ip_list.replace(';', '|')
        dest_ip_list = dest_ip_list.replace(',', '|')
        dest_ips = dest_ip_list.split('|')
        dest_ip_rs = ''
        idx = 0
        for ip in dest_ips:
            if not check_ip_syntax(ip.strip()):
                raise ValidationError(u'Danh sách địa chỉ IP sao chép ảnh không hợp lệ', code='invalid', )
            if idx == 0:
                dest_ip_rs += ip.strip()
            else:
                dest_ip_rs += ' | ' + ip.strip()
            idx += 1
        obj['dest_ip_list'] = dest_ip_rs
        return super(ImageReplicationSettingsForm, self).clean()


class ImageReplicationSettingAdmin(admin.ModelAdmin):
    def has_delete_permission(self, request, obj=None):
        return False
    search_fields = ['sour_ip']
    list_display = ('sour_ip', 'dest_ip_list')
    form = ImageReplicationSettingsForm


# Trang thai Tram
class TerminalStatusAdmin(admin.ModelAdmin):
    def has_delete_permission(self, request, obj=None):
        return False
    def __init__(self, *args, **kwargs):
        super(TerminalStatusAdmin, self).__init__(*args, **kwargs)
        self.list_display_links = (None,)

    # Readonly model
    def has_add_permission(self, request):  # Khong the them
        # Nobody is allowed to add
        return False

    list_display = ['terminal_name', 'terminal_id', 'ip', 'terminal_last_check', 'terminal_status']

    # Dinh nghia cac field
    def terminal_name(self, obj):
        return obj.name

    def terminal_last_check(self, obj):
        return obj.last_check_health.strftime('%d/%m/%Y %H:%M:%S')

    def terminal_status(self, obj):
        status = get_now_utc() <= obj.last_check_health + datetime.timedelta(seconds=86400)
        text_status = '<p style="color:%s;%s">%s'

        if status:
            return text_status % ("green", "font-weight:bold;", "ONLINE")
        else:
            return text_status % ("red", "", "OFFLINE")

    # Ten cac cot
    terminal_name.short_description = u'Tên'
    terminal_last_check.short_description = u'Lần cập nhật cuối'
    terminal_status.short_description = u'Trạng thái'
    terminal_status.allow_tags = True

    list_per_page = 14


# # CheckOutException Admin hien thi inline ParkingSessionAdmin
class CheckOutExceptionInline(admin.StackedInline):
    model = CheckOutException

    fields = ('notes',)
    readonly_fields = ('notes',)

    def has_add_permission(self, request):  # Khong the them
        return False

    def has_delete_permission(self, request, obj=None):  # Khong the xoa
        return False


# # Custom filter: mac dinh loc nhung parkingsession chua checkout
class IsCheckOutFilter(admin.SimpleListFilter):
    title = ('đã cho ra')

    parameter_name = 'is_checkout'

    def lookups(self, request, model_admin):
        return (
            (None, ('chưa cho ra')),
            ('true', ('đã cho ra')),
            # ('all', ('Tất cả')),
        )

    def choices(self, cl):
        for lookup, title in self.lookup_choices:
            yield {
                'selected': self.value() == lookup,
                'query_string': cl.get_query_string({
                    self.parameter_name: lookup,
                }, []),
                'display': title,
            }

    def queryset(self, request, queryset):
        # sessions = ParkingSession.objects.all()
        # Danh sach parking session id da check out ngoai le
        # all_check_out_exception = CheckOutException.objects.values_list('parking_session__id', flat=True)
        if self.value() == None:
            return queryset.filter(check_out_time=None).order_by('id')

        if self.value() == 'true':
            # return queryset.filter(~Q(check_out_time=None))
            # return sessions.filter(id__in=all_check_out_exception)
            return queryset.filter(check_out_exception__isnull=False).order_by('check_out_time')


# # ParkingSessionAdmin
# Checkout bang tay
class ParkingSessionAdmin(admin.ModelAdmin):
    def has_delete_permission(self, request, obj=None):
        return False
    # # Override filter mac dinh: chi loc cac session chua checkout
    # def queryset(self, request):
    # qs = super(ParkingSessionAdmin, self).queryset(request)
    #     return qs.filter(check_out_time=None)

    # Readonly model
    def has_add_permission(self, request):  # Khong the them
        # Nobody is allowed to add
        return False

    def has_delete_permission(self, request, obj=None):
        return False

    # Hien thi ngoai cung dang bang
    list_display = (
        'get_card_label',
        'vehicle_number',
        # 'vehicle_type',
        'check_in_operator',
        'check_out_operator',
        'short_check_in_time',
        'short_check_out_time',
        'check_out_exception'

    )

    list_filter = [IsCheckOutFilter, 'check_in_time']  # Filter
    ordering = ['-check_in_time']
    # list_per_page = 60  # Phan trang

    search_fields = ['vehicle_number']

    def get_search_results(self, request, queryset, search_term):
        queryset, use_distinct = super(ParkingSessionAdmin, self).get_search_results(request, queryset, search_term)
        try:
            search_term_as_card_label = search_term
        except ValueError:
            pass
        else:
            if len(search_term_as_card_label) > 0:
                queryset |= self.model.objects.filter(card__card_label__icontains=search_term_as_card_label)
        return queryset, use_distinct

    # Long ghep thong tin model CheckOutException trong ParkingSessionAdmin
    inlines = [CheckOutExceptionInline]

    # Cac truong thong tin chi tiet
    fieldsets = [
        ('Thẻ', {'fields': ['get_card_label']}),
        ('Xe', {'fields': ['vehicle_type', 'vehicle_number']}),
        ('Thời gian', {'fields': [
            'get_check_in_terminal', 'get_check_in_lane',
            # 'short_check_in_time',
            'check_in_time',
            'get_check_out_terminal', 'get_check_out_lane',
            # 'short_check_out_time',
            'check_out_time',
            'get_duration']}),
        ('Hình ảnh', {'fields': ['get_check_in_images', 'check_in_alpr_vehicle_number', 'get_check_out_images',
                                 'check_out_alpr_vehicle_number']}),
        ('Nhân viên', {'fields': ['check_in_operator', 'check_out_operator']}),
        ('Thông tin', {'fields': ['check_out_exception']})
    ]

    # Readonly
    readonly_fields = (
        'get_card_label',
        # 'vehicle_type',
        'short_check_in_time', 'short_check_out_time', 'get_duration',
        'get_check_in_terminal', 'get_check_out_terminal',
        'get_check_in_lane', 'get_check_out_lane',
        'check_in_operator', 'check_out_operator',
        'get_check_in_images', 'get_check_out_images',
        'check_in_alpr_vehicle_number', 'check_out_alpr_vehicle_number',
        'check_out_exception'
    )

    # Custom action
    def checkout_selected_with_exception(self, request, queryset):
        # Cac session id duoc chon
        selected = request.POST.getlist(admin.ACTION_CHECKBOX_NAME)
        # return HttpResponseRedirect("/admin/CheckOutException/%s" % ",".join(selected))
        # return HttpResponseRedirect("/admin/ConfirmCOE/%s" % ",".join(selected))
        # print selected
        data = list()
        for item in selected:
            idx = int(item)
            psess = ParkingSession.objects.get(id=idx)

            data.append({'parking_session_id': idx,
                         'card_label': psess.card.card_label,
                         'check_in_time': psess.check_in_time.strftime('%d/%m/%Y %H:%M:%S'),
                         'check_in_operator': psess.check_in_operator})

        return render(request, 'admin/confirmCOE.html', {'selected': ",".join(selected), 'data': data})

    # Custom list_display
    # Hien thi ngay gio dang short
    def short_check_in_time(self, obj):
        return to_local_time(obj.check_in_time).strftime('%d/%m/%Y %H:%M:%S')

    def short_check_out_time(self, obj):
        if obj.check_out_time:
            return to_local_time(obj.check_out_time).strftime('%d/%m/%Y %H:%M:%S')
        return '(Không)'

    def get_card_label(self, obj):
        return obj.card.card_label

    def get_duration(self, obj):
        if obj.duration:
            duration = obj.duration
        else:
            duration = int((get_now_utc() - obj.check_in_time).total_seconds())
        rs = ''
        if duration / 86400 > 0:
            days = duration / 86400
            duration %= 86400
            rs += u'%d ngày ' % days
        if duration / 3600 > 0:
            hours = duration / 3600
            duration %= 3600
            rs += u'%d giờ ' % hours
        if duration / 60 > 0:
            mins = duration / 60
            rs += u'%d phút' % mins
        else:
            rs += u'<1 phút'
        return rs

    def get_check_in_terminal(self, obj):
        if obj.check_in_lane:
            return obj.check_in_lane.terminal.name
        return '(Không)'

    def get_check_out_terminal(self, obj):
        if obj.check_out_lane:
            return obj.check_out_lane.terminal.name
        return '(Không)'

    def get_check_in_lane(self, obj):
        if obj.check_in_lane:
            return obj.check_in_lane.name
        return '(Không)'

    def get_check_out_lane(self, obj):
        if obj.check_out_lane:
            return obj.check_out_lane.name
        return '(Không)'

    def get_actions(self, request):
        actions = super(ParkingSessionAdmin, self).get_actions(request)
        del actions['delete_selected']

        # all_check_out_exception = CheckOutException.objects.values_list('parking_session__id', flat=True)

        if request.META['QUERY_STRING'] == 'is_checkout=true':
            del actions['checkout_selected_with_exception']

        return actions

    # Mo ta cac truong
    actions = ['checkout_selected_with_exception']
    short_check_in_time.short_description = u'Thời điểm vào'
    short_check_out_time.short_description = u'Thời điểm ra'
    checkout_selected_with_exception.short_description = u'Cho ra ngoại lệ'
    get_card_label.short_description = u"Tên thẻ"
    get_duration.short_description = u"Thời gian trong bãi"
    get_check_in_terminal.short_description = u"Trạm vào"
    get_check_out_terminal.short_description = u"Trạm ra"
    get_check_in_lane.short_description = u"Làn vào"
    get_check_out_lane.short_description = u"Làn ra"

    def render_images_field(self, images):
        html = """
        <div>
        <div>
        <b>TRƯỚC</b>
        <img src='{0}/{1}'></img>
        </div>
        <div>
        <b>SAU</b>
        <img src='{2}/{3}'></img>
        </div>
        </div>
        """
        try:
            server = Server.objects.get(id=1)
            image_host = 'http://%s/images/' % server.ip
        except Server.DoesNotExist:
            return ''
        if images:
            html = html.format(image_host, images['front'], image_host, images['back'])
            return mark_safe(html)
        else:
            return '(Không)'

    def get_check_in_images(self, obj):
        return self.render_images_field(obj.check_in_images)

    get_check_in_images.short_description = u"Hình ảnh cho vào"

    def get_check_out_images(self, obj):
        return self.render_images_field(obj.check_out_images)

    get_check_out_images.short_description = u"Hình ảnh cho ra"


class UpdateNoteExceptionForm(forms.Form):
    note = forms.CharField()
    selected = forms.Textarea()


class TurnFeeInline(admin.TabularInline):
    model = TurnFee
    extra = 1
    max_num = 1

    def has_delete_permission(self, request, obj=None):
        return False
        # def get_formset(self, request, obj=None, **kwargs):
        #     """
        #     Pre-populating formset using GET params
        #     """
        #     initial = []
        #     if request.method == "GET":
        #         #
        #         # Populate initial based on request
        #         #
        #         #print request
        #         if request.path.find('add') != -1:
        #             print "Add new parking fee!!"
        #             #print request
        #
        #             self.extra = 2
        #             print request
        #             initial= [
        #                 {'name': 'NGAY'},
        #                 {'name': 'DEM', 'is_overnight': 1, 'duration': 12}]
        #             # if u'customer' in request.GET:
        #             #     customer_id = request.GET['customer']
        #             #     if len(customer_id) > 0:
        #             #         vehicle_registrations = VehicleRegistration.objects.filter(customer_id=customer_id)
        #             #         if vehicle_registrations:
        #             #             self.extra += vehicle_registrations.count()
        #             #             for vehicle_registration in vehicle_registrations:
        #             #                 initial.append({
        #             #                     'vehicle_registration': vehicle_registration.id,
        #             #
        #             #                     'level_fee': vehicle_registration.level_fee.fee if vehicle_registration.level_fee else 0,
        #             #                 })
        #     formset = super(TurnFeeInline, self).get_formset(request, obj, **kwargs)
        #     formset.__init__ = curry(formset.__init__, initial=initial)
        #     return formset


class BlockFeeInline(admin.TabularInline):
    model = BlockFee
    extra = 1
    max_num = 1

    def has_delete_permission(self, request, obj=None):
        return False

        # def has_add_permission(self, request):  # Khong the them
        #     # Nobody is allowed to add
        #     s = request.META['PATH_INFO']
        #     try:
        #         parking_fee_id = re.match(REGEX_PARKINGFEE_ID, s).group(1)
        #         block_fees = BlockFee.objects.filter(parking_fee_id=parking_fee_id)
        #         if block_fees.count() >= 1:
        #             return False
        #     except:
        #         return True
        #     return True


class ParkingFeeAdmin(admin.ModelAdmin):
    def has_delete_permission(self, request, obj=None):
        return False
    list_display = ['vehicle_type',
                    'calculation_method',
                    'min_calculation_time',
                    'get_info'
                    ]

    fieldsets = [
        ('Thông tin cơ bản', {'fields': ['vehicle_type', 'calculation_method', 'min_calculation_time']}), ]

    inlines = [TurnFeeInline, BlockFeeInline]

    change_form_template = ['admin/admin-parkingfee.html']

    def get_info(self, obj):
        info = ''
        if obj.calculation_method == 'luot':
            turn_fee = TurnFee.objects.filter(parking_fee_id=obj.id)
            if turn_fee:
                turn_fee = turn_fee[0]
                s = u'{0}: {1:,}{2} '
                infos = []
                infos.append(s.format(u"Ngày", turn_fee.day_fee, ''))
                infos.append(s.format(u"Đêm", turn_fee.night_fee, ''))
                infos.append(s.format(u"Qua Đêm", turn_fee.overnight_fee, ''))

                info = ' - '.join(infos)
        elif obj.calculation_method == 'block':
            block_fees = BlockFee.objects.filter(parking_fee_id=obj.id)
            if block_fees:
                s = u'{0:,}/{1} phút đầu - {2:,}/{3} phút sau'
                for block_fee in block_fees:
                    info += s.format(block_fee.first_block_fee, block_fee.first_block_duration,
                                     block_fee.next_block_fee, block_fee.next_block_duration)
                    # info = u'Tính theo BLOCK: ' + info

        return info

    get_info.short_description = u"Chú giải"
    radio_fields = {"calculation_method": admin.VERTICAL}


class TicketPaymentDetailInline(admin.TabularInline):

    model = TicketPaymentDetail
    extra = 0
    temp_i = []
    fieldsets = [
        (None, {'fields': ('get_enable_check_box', 'vehicle_registration','vehicle_number', 'cardnumber', 'get_vehicle_type', 'get_status',
                           'effective_date', 'duration', 'day_duration', 'old_expired_date', #'get_current_expired_date',
                           'expired_date', 'level_fee', 'payment_detail_fee'),
                'description': 'Chỉ gia hạn những xe không có trạng thái tạm ngừng hoặc hủy'}),
    ]

    readonly_fields = ['get_enable_check_box','get_vehicle_type', 'get_status', 'get_current_expired_date', #'get_current_expired_date',
                       'get_level_fee']

    def has_delete_permission(self, request, obj=None):
        return False

    def get_enable_check_box(self, obj, temp_i=temp_i):
        temp_i.append(len(temp_i))
        if not obj.id:
            return mark_safe(
                "<input onclick='togglePaymentDetail(this)' class='get_enable_check_box' type='checkbox'/>")
        return ''

    get_enable_check_box.allow_tags = True
    get_enable_check_box.short_description = ''
    def vehicle_registration__card_card_lable(self,obj):
        if obj.vehicle_registration:
            return obj.vehicle_registration.card
        return ''
    vehicle_registration__card_card_lable.short_description = u'Số thẻ'


    def get_vehicle_type(self, obj):
        if obj.vehicle_registration:
            return obj.vehicle_registration.vehicle_type
        return ''

    get_vehicle_type.short_description = u'Loại xe'

    def get_status(self, obj):
        if obj.vehicle_registration:
            if obj.vehicle_registration.status==0:
                return  'Hủy'
            elif obj.vehicle_registration.status==1:
                return  'Đang dùng'
            elif obj.vehicle_registration.status==2:
                return  'Tạm ngưng'
            elif obj.vehicle_registration.status == 3:
                return 'Chưa đăng ký'
            else:
                return ''
        return ''

    get_status.allow_tags = True
    get_status.short_description = u'Tình trạng'

    def get_current_expired_date(self, obj):
        if obj.vehicle_registration.expired_date:
            expired_date = obj.vehicle_registration.expired_date
            day = datetime.datetime(expired_date.year, expired_date.month, expired_date.day)
            return pytz.utc.localize(day).astimezone(pytz.timezone(TIME_ZONE)).strftime('%d/%m/%Y')
        return ''

    def get_level_fee(self, obj):
        if obj.vehicle_registration.level_fee:
            return obj.vehicle_registration.level_fee.__str__()
        return ''

        # get_id.short_description = 'Mã chi tiết thanh toán'

    get_current_expired_date.short_description = u'Hạn cũ'
    get_level_fee.short_description = u'Mức phí'

    def get_formset(self, request, obj=None, **kwargs):
        """
        Pre-populating formset using GET params
        """
        initial = []
        if request.method == "GET":
            #
            # Populate initial based on request
            #
            # print request
            if request.path.find('add') != -1:
                # print "Add new ticketpayment!!"

                if u'customer' in request.GET:
                    customer_id = request.GET['customer']
                    if len(customer_id) > 0:
                        vehicle_registrations = VehicleRegistration.objects.filter(~Q(status=2),
                                                                                   customer_id=customer_id)
                        today = datetime.date.today()

                        if vehicle_registrations:
                            self.extra += vehicle_registrations.count()
                            for vehicle_registration in vehicle_registrations:
                                new_expired_date = None
                                if vehicle_registration.expired_date:
                                    new_expired_date = vehicle_registration.expired_date + datetime.timedelta(days=1)
                                if new_expired_date is None or new_expired_date < today: new_expired_date = today
                                if vehicle_registration.status not in [2]:  # Khong cho gia han xe dang tam dug
                                    initial.append({
                                        'vehicle_registration': vehicle_registration.id,
                                        'vehicle_number': vehicle_registration.vehicle_number,
                                        'cardnumber':vehicle_registration.card,
                                        'get_vehicle_type':vehicle_registration.vehicle_type,
                                        'get_status':vehicle_registration.status,
                                        'effective_date': new_expired_date,
                                        'old_expired_date': vehicle_registration.expired_date if vehicle_registration.expired_date else datetime.date.today(),
                                        'level_fee': vehicle_registration.level_fee.fee if vehicle_registration.level_fee else 0,
                                    })


        formset = super(TicketPaymentDetailInline, self).get_formset(request, obj, **kwargs)
        formset.__init__ = curry(formset.__init__, initial=initial)
        return formset

    # Loc nhung dang ky xe thuoc khach hang do
    # def formfield_for_foreignkey(self, db_field, request, **kwargs):
    #     s = request.path
    #     if 'customer' in request.GET:
    #         kwargs["queryset"] = VehicleRegistration.objects.filter(~Q(status=2), customer=request.GET['customer'])
    #     elif db_field.name == "vehicle_registration":
    #         vehicle_ticket_id = match(REGEX_PARKINGFEE_ID, s)
    #         if vehicle_ticket_id:
    #             vehicle_ticket_id = vehicle_ticket_id.group(1)
    #             kwargs["queryset"] = VehicleRegistration.objects.filter(~Q(status=2),
    #                                                                     customer=TicketPayment.objects.get(
    #                                                                         id=vehicle_ticket_id).customer)  # Loc cac xe khong dang tam ngung
    #
    #             # kwargs['widget'] = VehicleTicketChooserWidget
    #     return super(TicketPaymentDetailInline, self).formfield_for_foreignkey(db_field, request, **kwargs)


# Cap nhat trang thai gia han ve xe (sau khi thanh toan)
def update_vehicle_registration_status(vehicle_registration_id, ticket_payment_detail_id):
    today = datetime.date.today()  # Hom nay
    vehicle_registration = VehicleRegistration.objects.get(id=vehicle_registration_id)
    ticket_payment_detail = TicketPaymentDetail.objects.get(id=ticket_payment_detail_id)

    # Xe het han hoac dang dung?
    if vehicle_registration.status in [1, 3] and ticket_payment_detail:
        month_duration = ticket_payment_detail.duration
        day_duration = ticket_payment_detail.day_duration
        # Xe gia han lan dau tien (chua co hancu, hanmoi, trangthai la het han)
        if not (vehicle_registration.expired_date):
            if month_duration > 0 or day_duration > 0:
                # print "Xe %s gia han lan dau tien" % vehicle_registration.vehicle_number
                vehicle_registration.first_renewal_effective_date = ticket_payment_detail.effective_date
                vehicle_registration.last_renewal_date = today

                vehicle_registration.last_renewal_effective_date = ticket_payment_detail.effective_date
                vehicle_registration.start_date = ticket_payment_detail.effective_date

                vehicle_registration.expired_date = ticket_payment_detail.expired_date

                vehicle_registration.status = 1  # Dang dung
                vehicle_registration.save()
            else:  # Neu sua lai duration = 0 => khong co expired date
                vehicle_registration.expired_date = None
                vehicle_registration.save()
        else:
            # print "Xe %s dang dung hoac het han, gia han them" % vehicle_registration.vehicle_number
            if month_duration > 0 or day_duration > 0:
                if vehicle_registration.expired_date:
                    vehicle_registration.last_renewal_date = today
                    vehicle_registration.last_renewal_effective_date = ticket_payment_detail.effective_date
                    range = ticket_payment_detail.effective_date - vehicle_registration.expired_date

                    # print range.days

                    if range.days > 1:
                        vehicle_registration.start_date = ticket_payment_detail.effective_date
                    vehicle_registration.expired_date = ticket_payment_detail.expired_date

                    vehicle_registration.status = 1  # Dang dung
                    vehicle_registration.save()
            else:
                vehicle_registration.expired_date = ticket_payment_detail.effective_date + datetime.timedelta(days=-1)
                vehicle_registration.save()


class TicketPaymentAdmin(admin.ModelAdmin):  # Mot lan thanh toan gom nhieu chi tiet thanh toan
    def has_delete_permission(self, request, obj=None):
        return False
    model = TicketPayment

    list_display = ['get_id', 'get_receipt_number', 'get_customer', 'get_payment_date', 'payment_method',
                    'get_vehicle_number', 'get_vehicle_type', 'get_payment_fee',
                    'notes', ]  # 'get_pdf_ticket_payment_list_display']
    list_filter = ['payment_date']

    fields = ['customer', 'payment_fee', 'payment_method', 'notes', 'get_payment_date', 'staff',
              'get_field_receipt_number', 'get_receipt_feature']
    readonly_fields = ['get_payment_date', 'get_field_receipt_number',
                       'get_receipt_feature', ]  # 'get_pdf_ticket_payment_list_display'
    ordering = ['-receipt_number', '-payment_date']

    inlines = [TicketPaymentDetailInline]

    change_form_template = ['admin/admin-ticketpayment.html']

    radio_fields = {'payment_method': admin.HORIZONTAL}

    search_fields = ['customer__id']

    verbose_name = u'Gia han ve thang'
    verbose_name_plural = u'Gia han ve thang'

    def get_receipt_number(self, obj):
        if obj.receipt_number:
            return mark_safe(u'<a target="_blank" href="/admin/parking/receipt/{0}">{1}</a>'.format(obj.receipt_id,
                                                                                                    obj.receipt_number))
        return ''

    get_receipt_number.allow_tags = True
    get_receipt_number.short_description = u'Mã phiếu thu'

    def get_field_receipt_number(self, obj):
        if obj.receipt_number:
            return mark_safe(u'<a target="_blank" href="/admin/parking/receipt/{0}">{1}</a>'.format(obj.receipt_id,
                                                                                                    obj.receipt_number))
        return ''

    get_field_receipt_number.allow_tags = True
    get_field_receipt_number.short_description = u'Mã phiếu thu'

    def get_receipt_feature(self, obj):
        if obj.id:
            return mark_safe(
                u'''<a target="_blank" onclick="return showAddAnotherPopup(this)" style="text-align: center;height: 20px; padding-top: 7px; width: 80px; color: white; font-weight:bold; font-family:Arial" class="fa fa-# print my-button-color" href="/pdf/ticket-payment/%s">IN</a><a target="_blank" onclick="return showAddAnotherPopup(this)" style="margin-left: 10px;text-align: center;height: 20px; padding-top: 7px; width: 120px; color: white; font-weight:bold;" class="fa fa-# print my-button-color" href="/admin/receipt/action/0/%s">HỦY/TẠO MỚI</a>''' % (
                obj.id, obj.id))
        return ''

    get_receipt_feature.allow_tags = True
    get_receipt_feature.short_description = u'Chức năng phiếu thu'

    def get_customer(self, obj):
        if obj.customer:
            s = '<a target="_blank" href="/admin/parking/customer/%s">%s</a>' % (
            obj.customer_id, obj.customer.customer_name)
            return mark_safe(s)
        return ''

    get_customer.short_description = u'Khách hàng'
    get_customer.allow_tags = True

    def get_pdf_ticket_payment_list_display(self, obj):
        if obj.id:
            javascript = """onclick="
            var w = (screen.width/2);
            var h = (screen.height/2);

            var top = h - h/2;
            var left = w - w/2;

            return !window.open(this.href, name, 'scrollbars=yes, resizable=yes, width='+w+', height='+h+ ', top='+top+', left='+left);"
            """

            return mark_safe(
                "<div style='text-align:center'><a {0} style='font-size:120%' class='fa fa-print' href='/pdf/ticket-payment/{1}?_popup=1'></a></div>".format(
                    javascript, obj.id))
        else:
            return ''

    get_pdf_ticket_payment_list_display.allow_tags = True
    get_pdf_ticket_payment_list_display.short_description = u'Phiếu thu'

    # def get_payment_fee(self, obj):
    #     return convert_to_currency(int(obj.payment_fee))
    #
    # get_payment_fee.allow_tags = True
    # get_payment_fee.short_description = u'Số tiền (đ)'

    def get_payment_fee(self, obj):
        ticket_payment_details = TicketPaymentDetail.objects.filter(ticket_payment_id=obj.id)
        s = ''
        if ticket_payment_details:
            for payment_detail_fee in ticket_payment_details.values_list('payment_detail_fee'):
                s += '%s<br />' % (convert_to_currency(payment_detail_fee[0]))
            s += u'<b>TC: %s</b>' % convert_to_currency(int(obj.payment_fee))
            return mark_safe(s)
        return ''

    get_payment_fee.allow_tags = True
    get_payment_fee.short_description = u'Số tiền (đ)'

    def get_vehicle_number(self, obj):
        ticket_payment_details = TicketPaymentDetail.objects.filter(ticket_payment_id=obj.id)

        temp = u'<p>%s<p>'
        s = ''
        if ticket_payment_details:
            for ticket_payment_detail in list(ticket_payment_details):
                current = ticket_payment_detail.vehicle_number or ''
                s += temp % (current)
            return mark_safe(s)
        return ''

    get_vehicle_number.short_description = u'Biển số xe'
    get_vehicle_number.allow_tags = True

    def get_vehicle_type(self, obj):
        ticket_payment_details = TicketPaymentDetail.objects.filter(ticket_payment_id=obj.id)

        temp = u'<p>%s<p>'
        s = ''

        if ticket_payment_details:
            # record = []
            for ticket_payment_detail in list(ticket_payment_details):
                current = u"%s" % ticket_payment_detail.vehicle_registration.vehicle_type if ticket_payment_detail.vehicle_registration else ''
                # if current not in record:
                #     record.append(current)
                s += temp % (current)
            return mark_safe(s)
        return ''

    get_vehicle_type.short_description = u'Loại xe'

    def get_payment_date(self, obj):
        return obj.payment_date.astimezone(pytz.timezone(TIME_ZONE)).strftime(
            '%d/%m/%Y %H:%M:%S') if obj.payment_date else ''

    def get_id(self, obj):
        return obj.id

    get_payment_date.short_description = u'Ngày thanh toán'
    get_id.short_description = u'Mã'

    def save_model(self, request, obj, form, change):
        if not obj.id and obj.payment_fee > 0:
            try:
                obj.staff = UserProfile.objects.get(user_id=request.user.id)
                obj.save()
            except UserProfile.DoesNotExist:
                pass

        if obj.id or (not obj.id and obj.payment_fee >= 0):
            if not obj.receipt_number:
                setting_item = ParkingSetting.objects.filter(key='next_receipt_number')
                current_new_receipt_number = 0
                if not setting_item:
                    current_new_receipt_number = int(get_setting('next_receipt_number', u'Số phiếu thu tiếp theo', 1))
                else:
                    setting_item = setting_item[0]
                    current_new_receipt_number = int(setting_item.value)
                    setting_item.value = unicode(current_new_receipt_number + 1)
                    setting_item.save()

                obj.receipt_number = current_new_receipt_number
                obj.save()
                receipt = Receipt(receipt_number=current_new_receipt_number, type=0, ref_id=obj.id)
                receipt.save()
                obj.receipt_id = receipt.id
            obj.save()



    # Hook: cap nhat gia han xe sau khi thanh toan
    def save_formset(self, request, form, formset, change):
        customer_id = int(request.POST['customer'])
        staff_id = int(request.POST['staff']) if request.POST['staff'] else 1

        instances = formset.save(commit=False)

        can_renew_different_vehicle_registration_expired_date = get_setting(
            'can_renew_different_vehicle_registration_expired_date', u'Cho phép gia hạn các xe đăng ký khác thời hạn',
            0)

        if int(can_renew_different_vehicle_registration_expired_date) == 0:
            last_effective_date = ''
            last_expired_date = ''

            has_data = False
            for instance in instances:
                if instance.payment_detail_fee >= 0:
                    current_effective_date = instance.effective_date.strftime('%d/%m/%Y')
                    current_expired_date = instance.effective_date.strftime('%d/%m/%Y')

                    if len(last_effective_date) == 0 and len(last_expired_date) == 0:
                        last_effective_date = current_effective_date
                        last_expired_date = current_expired_date

                    if (last_effective_date != current_effective_date and last_expired_date != current_expired_date):
                        # print "@@@DBG: Khong the gia han cac xe dang ky khac han"

                        last_ticket_payment = TicketPayment.objects.filter(customer_id=customer_id).order_by('-id')

                        if last_ticket_payment:
                            last_ticket_payment = last_ticket_payment[0]
                            last_ticket_payment.delete()

                        messages.error(request, u'Không thể gia hạn các xe đăng ký khác hạn', fail_silently=True)
                        return redirect(
                            'admin/parking/ticketpayment/add/?customer={0}&staff={1}'.format(customer_id, staff_id))

        for instance in instances:
            if instance.payment_detail_fee >= 0:
                instance.save()  # Van luu cac thay doi vao CSDL truoc

        formset.save_m2m()

        for instance in instances:
            if instance.payment_detail_fee >= 0:
                update_vehicle_registration_status(instance.vehicle_registration.id, instance.id)

    def response_add(self, request, obj, post_url_continue=None):
        # print "response_add TicketPayment"
        if u'customer' in request.POST:
            customer_id = request.POST[u'customer']
            staff_id = request.POST[u'staff']

            s = '/admin/parking/customer/%s' % customer_id
            if '_continue' not in request.POST:

                # return HttpResponse('<script type="text/javascript">window.close(); window.opener.parent.location.href ="/"; location.reload()</script>')
                if obj.id:
                    redirect_url = '/pdf/ticket-payment/%s' % obj.id

                    ticket_payment = TicketPayment.objects.filter(id=obj.id)
                    if not ticket_payment:
                        return redirect(
                            '/admin/parking/ticketpayment/add/?customer={0}&staff={1}'.format(customer_id, staff_id))

                    meta = request.META['QUERY_STRING']
                    if meta.find('view_customer_search') != - 1:
                        return HttpResponse(
                            '<script type="text/javascript">window.opener.parent.open("%s"); window.close();</script>' % (
                            redirect_url))
                    elif meta.find('customer_list_display') != - 1:
                        return HttpResponse(
                            '<script type="text/javascript">var origin = window.opener.parent.location.href; window.opener.parent.open("%s"); window.opener.parent.location.href = origin; window.close();</script>' % (
                            redirect_url))
                    elif meta.find('popup') != -1:
                        return HttpResponse(
                            '<script type="text/javascript">var origin = window.opener.parent.location.href; window.location.href = "%s"; window.opener.parent.location.href = "%s"; </script>' % (
                            redirect_url, s))
            else:
                return super(TicketPaymentAdmin, self).response_add(request, obj, post_url_continue)
        return super(TicketPaymentAdmin, self).response_add(request, obj, post_url_continue)


class DepositPaymentDetailInline(admin.TabularInline):  # Coc the
    model = DepositPaymentDetail
    extra = 0

    fields = [  # 'get_id',
                'vehicle_registration', 'vehicle_number', 'get_vehicle_type', 'get_status',
                'deposit_action_fee',
                'deposit_payment_detail_fee']

    readonly_fields = ['get_vehicle_type', 'get_status']

    def has_delete_permission(self, request, obj=None):
        return False

    def get_vehicle_type(self, obj):
        if obj.vehicle_registration:
            return obj.vehicle_registration.vehicle_type
        return ''

    def get_status(self, obj):
        if obj.vehicle_registration:
            return VEHICLE_STATUS_CHOICE[obj.vehicle_registration.status][1]
        return ''

    def get_current_expired_date(self, obj):
        if obj.vehicle_registration.expired_date:
            expired_date = obj.vehicle_registration.expired_date
            day = datetime.datetime(expired_date.year, expired_date.month, expired_date.day)
            return pytz.utc.localize(day).astimezone(pytz.timezone(TIME_ZONE)).strftime('%d/%m/%Y')
        return ''

    def get_level_fee(self, obj):
        if obj.vehicle_registration.level_fee:
            return obj.vehicle_registration.level_fee.__str__()
        return ''

    def get_formset(self, request, obj=None, **kwargs):
        """
        Pre-populating formset using GET params
        """
        initial = []
        if request.method == "GET":
            #
            # Populate initial based on request
            #
            # print request
            if request.path.find('add') != -1:
                # print "Add new depositpayment!!"
                if u'customer' in request.GET:
                    customer_id = request.GET['customer']
                    if len(customer_id) > 0:
                        vehicle_registrations = VehicleRegistration.objects.filter(customer_id=customer_id)
                        if vehicle_registrations:
                            self.extra += vehicle_registrations.count()
                            for vehicle_registration in vehicle_registrations:
                                if vehicle_registration.status != 2:  # Khong cho gia han xe dang tam dug
                                    initial.append({
                                        'vehicle_registration': vehicle_registration.id,
                                        'vehicle_number': vehicle_registration.vehicle_number,
                                    })

        formset = super(DepositPaymentDetailInline, self).get_formset(request, obj, **kwargs)
        formset.__init__ = curry(formset.__init__, initial=initial)
        return formset

    # Loc nhung dang ky xe thuoc khach hang do
    def formfield_for_foreignkey(self, db_field, request=None, **kwargs):
        s = request.path
        if 'customer' in request.GET:
            kwargs["queryset"] = VehicleRegistration.objects.filter(
                customer=request.GET['customer'])  # ~Q(status=2), Xe dang tam ngung
        if db_field.name == "deposit_action_fee":
            kwargs["queryset"] = DepositActionFee.objects.all()
        # if db_field.name == "vehicle_registration":
        #     vehicle_ticket_id = re.match(REGEX_PARKINGFEE_ID, s)
        #     if vehicle_ticket_id:
        #         print 'im inside'
        #         vehicle_ticket_id = vehicle_ticket_id.group(1)
        #         kwargs["queryset"] = VehicleRegistration.objects.filter(~Q(status=2), customer=DepositPayment.objects.get(id=vehicle_ticket_id).customer) # Loc cac xe khong dang tam ngung

        return super(DepositPaymentDetailInline, self).formfield_for_foreignkey(db_field, request, **kwargs)

    # get_id.short_description = 'Mã chi tiết thanh toán'
    get_vehicle_type.short_description = u'Loại xe'
    get_status.short_description = u'Tình trạng'
    get_current_expired_date.short_description = u'Hạn hiện tại'
    get_level_fee.short_description = u'Mức phí'


# Mot lan thanh toan gom nhieu chi tiet thanh toan
class DepositPaymentAdmin(admin.ModelAdmin):
    def has_delete_permission(self, request, obj=None):
        return False
    model = DepositPayment

    list_display = ['get_id', 'get_receipt_number2', 'get_customer', 'get_payment_date', 'payment_method',
                    'get_vehicle_number', 'get_payment_fee', 'notes', ]  # 'get_pdf_ticket_payment_list_display']

    list_filter = ['payment_date']

    fields = ['customer', 'payment_fee', 'payment_method', 'notes', 'get_payment_date', 'staff', 'get_receipt_number2',
              'get_receipt_feature']
    readonly_fields = ['get_payment_date', 'get_receipt_number2', 'get_receipt_feature']

    inlines = [DepositPaymentDetailInline]

    change_form_template = ['admin/admin-depositpayment.html']

    radio_fields = {'payment_method': admin.HORIZONTAL}
    # fieldsets = [
    #     ('None', {'fields': ['get_customer', 'staff', 'payment_fee'] }),
    # ]
    search_fields = ['customer__id']

    verbose_name = u'Cọc thẻ'
    verbose_name_plural = u'Cọc thẻ'

    def get_customer(self, obj):
        if obj.customer:
            s = '<a target="_blank" href="/admin/parking/customer/%s">%s</a>' % (
            obj.customer_id, obj.customer.customer_name)
            return mark_safe(s)
        return ''

    get_customer.short_description = u'Khách hàng'
    get_customer.allow_tags = True

    def get_pdf_ticket_payment_list_display(self, obj):
        if obj.id:
            javascript = """onclick="
            var w = (screen.width/2);
            var h = (screen.height/2);

            var top = h - h/2;
            var left = w - w/2;

            return !window.open(this.href, name, 'scrollbars=yes, resizable=yes, width='+w+', height='+h+ ', top='+top+', left='+left);"
            """

            return mark_safe(
                "<div style='text-align:center'><a {0} style='font-size:120%' class='fa fa-print' href='/pdf/deposit-payment/{1}?_popup=1'></a></div>".format(
                    javascript, obj.id))
        else:
            return ''

    get_pdf_ticket_payment_list_display.allow_tags = True
    get_pdf_ticket_payment_list_display.short_description = u'Phiếu thu'

    def get_receipt_number2(self, obj):
        if obj.receipt_number:
            return mark_safe(u'<a target="_blank" href="/admin/parking/receipt/{0}">{1}</a>'.format(obj.receipt_id,
                                                                                                    obj.receipt_number))
        return ''

    get_receipt_number2.allow_tags = True
    get_receipt_number2.short_description = u'Mã phiếu thu'

    def get_receipt_feature(self, obj):
        if obj.id:
            return mark_safe(
                u'''<a target="_blank" onclick="return showAddAnotherPopup(this)" style="color:white; text-align: center;height: 20px; padding-top: 7px; width: 120px; font-weight:bold;" class="fa fa-# print my-button-color" href="/pdf/deposit-payment/%s">IN PHIẾU THU</a><a target="_blank" onclick="return showAddAnotherPopup(this)" style="margin-left: 10px;text-align: center;height: 20px; padding-top: 7px; width: 120px; font-weight:bold; color:white; " class="fa fa-# print my-button-color" href="/admin/receipt/action/1/%s">HỦY/TẠO MỚI</a>''' % (
                obj.id, obj.id))
        return ''

    get_receipt_feature.allow_tags = True
    get_receipt_feature.short_description = u'Chức năng'

    def get_payment_fee(self, obj):
        deposit_payment_details = DepositPaymentDetail.objects.filter(deposit_payment_id=obj.id)
        s = ''
        if deposit_payment_details:
            for payment_detail_fee in deposit_payment_details.values_list('deposit_payment_detail_fee'):
                s += '%s<br />' % (convert_to_currency(payment_detail_fee[0]))
            s += u'<b>TC: %s</b>' % convert_to_currency(int(obj.payment_fee))
            return mark_safe(s)
        return ''

    get_payment_fee.allow_tags = True
    get_payment_fee.short_description = u'Số tiền (đ)'

    def get_vehicle_number(self, obj):
        deposit_payment_details = DepositPaymentDetail.objects.filter(deposit_payment_id=obj.id)

        temp = u'<p>%s<p>'
        s = ''
        if deposit_payment_details:
            for payment_detail in list(deposit_payment_details):
                current = payment_detail.vehicle_registration.vehicle_number if payment_detail.vehicle_registration else ''
                s += temp % (current)
            return mark_safe(s)
        return ''

    get_vehicle_number.short_description = u'Biển số xe'
    get_vehicle_number.allow_tags = True

    def get_payment_date(self, obj):
        return obj.payment_date.astimezone(pytz.timezone(TIME_ZONE)).strftime(
            '%d/%m/%Y %H:%M:%S') if obj.payment_date else ''

    def get_id(self, obj):
        return obj.id

    get_payment_date.short_description = u'Thanh toán lần cuối'
    get_id.short_description = u'Mã'
    final_payment_fee = 0

    def save_formset(self, request, form, formset, change):
        instances = formset.save(commit=False)

        for instance in instances:
            if instance.deposit_payment_detail_fee > 0:
                instance.save()  # Van luu cac thay doi vao CSDL truoc

        formset.save_m2m()

    def save_model(self, request, obj, form, change):
        if not obj.id and obj.payment_fee > 0:
            try:
                obj.staff = UserProfile.objects.get(user_id=request.user.id)
                obj.save()
            except UserProfile.DoesNotExist:
                pass

        if obj.id or (not obj.id and obj.payment_fee > 0):
            if not obj.receipt_number:
                new_receipt_number = 0
                setting_item = ParkingSetting.objects.filter(key='next_receipt_number')
                if not setting_item:
                    new_receipt_number = int(get_setting('next_receipt_number', u'Số phiếu thu tiếp theo', 1))
                else:
                    setting_item = setting_item[0]
                    new_receipt_number = int(setting_item.value)
                    setting_item.value = unicode(new_receipt_number + 1)
                    setting_item.save()

                obj.receipt_number = new_receipt_number
                obj.save()
                receipt = Receipt(receipt_number=new_receipt_number, type=1, ref_id=obj.id)
                receipt.save()
                obj.receipt_id = receipt.id

            obj.save()

    def response_add(self, request, obj, post_url_continue=None):
        # print request.POST
        if u'customer' in request.POST:
            # print "response_add DepositPayment"
            customer_id = request.POST[u'customer']
            s = '/admin/parking/customer/%s' % customer_id

            if '_continue' not in request.POST:
                # return redirect('/admin/parking/customer/%s' % customer_id)
                # return HttpResponse('<script type="text/javascript">window.close(); window.opener.parent.location.href ="/"; location.reload()</script>')

                if obj.id:
                    redirect_url = '/pdf/deposit-payment/%s' % obj.id
                    return HttpResponse(
                            '<script type="text/javascript">var origin = window.opener.parent.location.href; window.location.href = "%s"; window.opener.parent.location.href = "%s"; </script>' % (
                            redirect_url, s))
                return HttpResponse(
                    '<script type="text/javascript">window.close(); window.opener.parent.location.href ="%s"</script>' % s)
            else:
                # print "IM HERE"
                return super(DepositPaymentAdmin, self).response_add(request, obj, post_url_continue)
        return super(DepositPaymentAdmin, self).response_add(request, obj, post_url_continue)


class VehicleRegistrationAdmin(admin.ModelAdmin):
    def has_delete_permission(self, request, obj=None):
        return False
    list_display = ['vehicle_number', 'get_customer', 'vehicle_driver_name', 'card', 'vehicle_brand', 'vehicle_type','vehicle_driver_phone',
                    'get_start_date', 'get_expired_date', 'get_remain_duration', 'get_vehicle_status', 'get_renewal']
    form = CardAutoCompleteForm
    fieldsets = [
        (u'Thông tin cơ bản', {'fields': ['customer', 'card', 'level_fee', 'get_vehicle_status', 'staff']}),
        (u'Thông tin ngày giờ', {'fields': ['get_registration_date',
                                           'get_first_renewal_effective_date',
                                           ['get_last_renewal_date', 'get_last_renewal_effective_date', ],
                                           'start_date', 'expired_date',
                                           'get_pause_date', 'get_cancel_date']}),
        (u'Thông tin xe', {'fields': ['vehicle_number','vehicle_paint','vehicle_driver_id',  'vehicle_type', 'vehicle_brand', ]}),
        (u'Thông tin lái xe', {'fields': ['vehicle_driver_name', 'vehicle_driver_phone']}),
    ]

    readonly_fields = ['staff', 'get_vehicle_status','start_date', 'expired_date',
                       'get_registration_date', 'get_first_renewal_effective_date', 'get_last_renewal_date',
                       'get_last_renewal_effective_date', 'get_pause_date', 'get_cancel_date', 'get_customer']

    # list_filter = ['customer']
    search_fields = ['card__card_label', 'card__card_id', 'vehicle_driver_name', 'vehicle_number','vehicle_brand','vehicle_driver_phone']

    change_form_template = ['admin/admin-vehicleregistration.html']
    # list_filter = ['customer__id']

    # def response_post_save_add(self, request, obj):
    #     print request.POST
    #     if u'customer' in request.POST:
    #         customer_id = request.POST[u'customer']
    #         if '_continue' not in request.POST:
    #             #return redirect('/admin/parking/customer/%s' % customer_id)
    #             s = '/admin/parking/customer/%s' % customer_id
    #             return HttpResponse('<script type="text/javascript">window.close(); window.opener.parent.location.href =' + s + ';</script>')
    #         else:
    #             return super(VehicleRegistrationAdmin, self).response_add(request, obj)
    #
    #     return super(VehicleRegistrationAdmin, self).response_add(request, obj)

    def response_add(self, request, obj, post_url_continue=None):
        # print request.POST
        if u'customer' in request.POST:
            # print "IM customer"
            customer_id = request.POST[u'customer']
            s = '/admin/parking/customer/%s' % customer_id
            if '_continue' not in request.POST:
                # return redirect('/admin/parking/customer/%s' % customer_id)
                # return HttpResponse('<script type="text/javascript">window.close(); window.opener.parent.location.href ="/"; location.reload()</script>')
                return HttpResponse(
                    '<script type="text/javascript">window.close(); window.opener.parent.location.href ="' + s + '";</script>')
            else:
                # print "IM HERE"
                return super(VehicleRegistrationAdmin, self).response_add(request, obj, post_url_continue)
        return super(VehicleRegistrationAdmin, self).response_add(request, obj, post_url_continue)

    def get_fieldsets(self, request, obj=None):
        fs = super(VehicleRegistrationAdmin, self).get_fieldsets(request, obj)

        if request.path.find('add') != -1:
            # Khong hien thong tin ngay gio khi them moi dang ky xe.
            new_fs = [f for f in fs if 'get_registration_date' not in f[1]['fields']]
            if new_fs:
                return new_fs
        return fs

    def get_vehicle_status(self, obj):
        try:
            return get_status_new(obj, True)
        except Exception:
            return get_status(obj.status, True)

    get_vehicle_status.allow_tags = True
    get_vehicle_status.short_description = u'Trạng thái'

    def get_customer(self, obj):
        if obj.customer:
            s = '<a target="_blank" href="/admin/parking/customer/%s">%s</a>' % (
                obj.customer_id, obj.customer.customer_name)
            return mark_safe(s)
        return ""

    get_customer.short_description = u'Khách hàng'
    get_customer.allow_tags = True

    def get_registration_date(self, obj):
        if obj.registration_date:
            return obj.registration_date.astimezone(pytz.timezone(TIME_ZONE)).strftime("%d/%m/%Y %H:%M")
        return ''

    def get_last_renewal_effective_date(self, obj):
        # print "@@@@@@@@@@@@@@@@@@", obj.last_renewal_effective_date
        if obj.last_renewal_effective_date:
            last_renewal_effective_date = obj.last_renewal_effective_date
            day = datetime.datetime(last_renewal_effective_date.year, last_renewal_effective_date.month,
                                    last_renewal_effective_date.day)
            return pytz.utc.localize(day).astimezone(pytz.timezone(TIME_ZONE)).strftime('%d/%m/%Y')
        return ''

    def get_first_renewal_effective_date(self, obj):
        if obj.first_renewal_effective_date:
            first_renewal_effective_date = obj.first_renewal_effective_date
            day = datetime.datetime(first_renewal_effective_date.year, first_renewal_effective_date.month,
                                    first_renewal_effective_date.day)
            return pytz.utc.localize(day).astimezone(pytz.timezone(TIME_ZONE)).strftime('%d/%m/%Y')
        return ''

    def get_last_renewal_date(self, obj):
        if obj.last_renewal_date:
            last_renewal_date = obj.last_renewal_date
            day = datetime.datetime(last_renewal_date.year, last_renewal_date.month, last_renewal_date.day)
            return pytz.utc.localize(day).astimezone(pytz.timezone(TIME_ZONE)).strftime('%d/%m/%Y')
        return ''

    def get_start_date(self, obj):
        if obj.start_date:
            start_date = obj.start_date
            day = datetime.datetime(start_date.year, start_date.month, start_date.day)
            return pytz.utc.localize(day).astimezone(pytz.timezone(TIME_ZONE)).strftime('%d/%m/%Y')
        return ''

    get_start_date.short_description = u'Ngày bắt đầu hạn'

    def get_expired_date(self, obj):
        if obj.expired_date:
            expired_date = obj.expired_date
            day = datetime.datetime(expired_date.year, expired_date.month, expired_date.day)
            return pytz.utc.localize(day).astimezone(pytz.timezone(TIME_ZONE)).strftime('%d/%m/%Y')
        return ''

    get_expired_date.short_description = u'Hạn hiện tại'

    def get_pause_date(self, obj):
        if obj.pause_date:
            pause_date = obj.pause_date
            day = datetime.datetime(pause_date.year, pause_date.month, pause_date.day)
            return pytz.utc.localize(day).astimezone(pytz.timezone(TIME_ZONE)).strftime('%d/%m/%Y')
        return ''

    def get_cancel_date(self, obj):
        if obj.cancel_date:
            cancel_date = obj.cancel_date
            day = datetime.datetime(cancel_date.year, cancel_date.month, cancel_date.day)
            return pytz.utc.localize(day).astimezone(pytz.timezone(TIME_ZONE)).strftime('%d/%m/%Y')
        return ''

    def get_renewal(self, obj):
        s = u"<div style='text-align:center'><a class='fa fa-money' style='font-size:140%;' href='/admin/parking/ticketpayment/add/?customer={0}'></a></div>".format(
            obj.customer_id)
        return mark_safe(s)

    def get_remain_duration(self, obj):
        remain_duration = 0

        if obj.id:
            last_pause = PauseResumeHistory.objects.filter(vehicle_registration_id=obj.id, used=False).order_by(
                '-request_date')
            if last_pause:
                remain_duration = last_pause[0].remain_duration
            elif obj.expired_date:
                expired_date = obj.expired_date
                start_date = obj.start_date

                today = datetime.date.today()
                if expired_date > today:
                    if start_date >= today:
                        remain_duration = (expired_date - start_date).days
                    else:
                        remain_duration = (expired_date - today).days
        return remain_duration

    get_remain_duration.short_description = u'Thời gian còn lại'

    get_registration_date.short_description = u'Ngày tạo'
    get_first_renewal_effective_date.short_description = u'Ngày đăng ký'
    get_last_renewal_date.short_description = u'Ngày đóng phí'
    get_last_renewal_effective_date.short_description = u'Ngày hiệu lực'
    get_pause_date.short_description = u'Ngày tạm ngừng'
    get_cancel_date.short_description = u'Ngày hủy'
    get_renewal.short_description = u'Gia hạn'
    get_renewal.allow_tags = True

    # Custom action: Pause
    def action_pause(self, request, queryset):
        customer_id = request.GET[u'customer'] if 'customer' in request.GET else ''

        current_user = request.user
        if not current_user.has_perm('parking.pause_resume_cancel_vehicle_registration'):
            messages.error(request, u'Tài khoản hiện tại không có quyền này!', fail_silently=True)
            return redirect('/admin/parking/vehicleregistration/?customer=%s' % customer_id)

        selected = request.POST.getlist(admin.ACTION_CHECKBOX_NAME)

        data = list()
        real_data = list()
        today = datetime.date.today()

        for item in selected:
            idx = int(item)
            vehicle_registration = VehicleRegistration.objects.get(id=idx)
            remain_duration = 0

            if vehicle_registration.expired_date:
                expired_date = vehicle_registration.expired_date
                start_date = vehicle_registration.start_date

                if expired_date > today:
                    if start_date >= today:
                        remain_duration = (expired_date - start_date).days
                    else:
                        remain_duration = (expired_date - today).days

            _dict = {'vehicle_registration_id': idx,
                     'customer_name': vehicle_registration.customer.customer_name if vehicle_registration.customer else '',
                     'card_label': vehicle_registration.card.card_label if vehicle_registration.card else '',
                     'vehicle_number': vehicle_registration.vehicle_number,
                     'vehicle_driver_name': vehicle_registration.vehicle_driver_name,
                     'status': VEHICLE_STATUS_CHOICE[vehicle_registration.status][1],
                     'last_renewal_effective_date': get_time_zone_time(
                         vehicle_registration.start_date) if vehicle_registration.start_date else '',
                     'expired_date': get_time_zone_time(
                         vehicle_registration.expired_date) if vehicle_registration.expired_date else '',
                     'remain_duration': remain_duration}

            if vehicle_registration.status != 1 or remain_duration == 0:  # Xe dang o trang thai huy hoac khong con thoi han de tam ngung
                _dict['cannot_pause'] = "true"
            else:
                real_data.append(str(idx))
            data.append(_dict)

        return render(request, 'admin/confirmPause.html',
                      {'customer_id': customer_id, 'today': today.strftime("%d/%m/%Y"), 'selected': ",".join(selected),
                       'data': data, 'real_data': "@".join(real_data)})

    def action_resume(self, request, queryset):
        class VehicleRegistrationResumeForm(forms.Form):
            effective_date = forms.DateField(label=u'Ngày hiệu lực')

        selected = request.POST.getlist(admin.ACTION_CHECKBOX_NAME)
        customer_id = request.GET['customer'] if 'customer' in request.GET else ''

        current_user = request.user
        if not current_user.has_perm('parking.pause_resume_cancel_vehicle_registration'):
            messages.error(request, u'Tài khoản hiện tại không có quyền này!', fail_silently=True)
            return redirect('/admin/parking/vehicleregistration/?customer=%s' % customer_id)


        # print "selected  ", selected, len(selected)
        if len(selected) > 1:
            messages.error(request, u'Mỗi thao tác chỉ được phục hồi một xe!', fail_silently=True)
            return redirect('/admin/parking/vehicleregistration/?customer=%s' % customer_id)
        else:
            idx = int(selected[0])
            vehicle_registration = VehicleRegistration.objects.filter(id=idx)

            if vehicle_registration:
                vehicle_registration = vehicle_registration[0]

                if vehicle_registration.status == 0:
                    vehicle_registration.status = 1
                    vehicle_registration.cancel_date = None
                    vehicle_registration.save()
                    messages.success(request, 'Phục hồi trạng thái huỷ vé tháng khách hàng thành công!',
                                     fail_silently=True)

                    # Get lastest ticket payment
                    latest_ticket_payment_detail = TicketPaymentDetail.objects.filter(vehicle_registration_id=queryset[0].id).latest("expired_date")
                    # Check ticket payment available
                    today = datetime.date.today()
                    if latest_ticket_payment_detail.expired_date >= today:
                        # Remove cancel_date
                        latest_ticket_payment_detail.cancel_date = None
                        latest_ticket_payment_detail.save()

                    return redirect('/admin/parking/vehicleregistration/?customer=%s' % customer_id)

                elif vehicle_registration.status != 2:  # Chi duoc phuc hoi xe dang tam ngung
                    messages.error(request, u'Chi duoc phuc hoi xe dang tam ngung!', fail_silently=True)
                    return redirect('/admin/parking/vehicleregistration/?customer=%s' % customer_id)

                pause_resume = PauseResumeHistory.objects.filter(vehicle_registration_id=idx, request_type=0, used=False,
                                                                 request_date=vehicle_registration.pause_date)

                if pause_resume:
                    pause_resume = pause_resume[0]

                    effective_date = ''
                    last_check_in_but_not_check_out = ''
                    card_id = vehicle_registration.card.card_id if vehicle_registration.card else None

                    if card_id:
                        last_session = ParkingFeeSession.objects.filter(card_id=card_id).order_by('-calculation_time')

                        if last_session:
                            last_check_in = last_session.filter(session_type='IN')
                            last_check_out = last_session.filter(session_type='OUT')

                            if last_check_in and not last_check_out:
                                last_check_in = last_check_in[0]
                                effective_date = last_check_in.calculation_time.strftime('%d/%m/%Y')
                                last_check_in_but_not_check_out = effective_date

                    if len(effective_date) == 0:
                        effective_date = datetime.date.today().strftime('%d/%m/%Y')

                    form = VehicleRegistrationResumeForm(initial={'effective_date': effective_date})

                    return render(request, 'admin/confirmResume.html', {'selected': selected[0],
                                                                        'customer_id': customer_id,
                                                                        'vehicle_number': vehicle_registration.vehicle_number,
                                                                        'customer_name': vehicle_registration.customer.customer_name,
                                                                        'pause_date': get_time_zone_time(
                                                                            vehicle_registration.pause_date),
                                                                        'remain_duration': pause_resume.remain_duration,
                                                                        'today': datetime.date.today().strftime(
                                                                            "%d/%m/%Y"),
                                                                        'last_check_in_but_not_check_out': last_check_in_but_not_check_out,
                                                                        'form': form,
                                                                        })
                    # print "Phuc hoi"

    def action_cancel(self, request, queryset):
        customer_id = request.GET[u'customer'] if 'customer' in request.GET else ''

        current_user = request.user
        if not current_user.has_perm('parking.pause_resume_cancel_vehicle_registration'):
            messages.error(request, u'Tài khoản hiện tại không có quyền này!', fail_silently=True)
            return redirect('/admin/parking/vehicleregistration/?customer=%s' % customer_id)
        today = datetime.date.today()
        queryset.update(status=0)
        queryset.update(cancel_date=today)

        for query in queryset:
            # Get lastest ticket payment
            latest_ticket_payment_detail = TicketPaymentDetail.objects.filter(vehicle_registration_id=query.id).latest("expired_date")
            # Check ticket payment available
            if latest_ticket_payment_detail.expired_date >= today:
                # Update cancel_date to today
                latest_ticket_payment_detail.cancel_date = today
                latest_ticket_payment_detail.save()


    action_pause.short_description = u'Tạm ngừng vé tháng'
    action_resume.short_description = u'Phục hồi vé tháng'
    action_cancel.short_description = u'Hủy đăng ký xe'

    def get_actions(self, request):
        actions = super(VehicleRegistrationAdmin, self).get_actions(request)
        # del actions['delete_selected']
        # if request.META['QUERY_STRING'] == 'is_checkout=true':
        #     del actions['checkout_selected_with_exception']

        return actions

    actions = ['action_pause', 'action_resume', 'action_cancel']

    def save_model(self, request, obj, form, change):
        # print "save model"
        if not obj.id:
            try:
                obj.staff = UserProfile.objects.get(user_id=request.user.id)
                obj.save()
            except UserProfile.DoesNotExist:
                pass
        obj.save()


class LevelFeeAdmin(admin.ModelAdmin):
    def has_delete_permission(self, request, obj=None):
        return False
    list_display = ['name', 'get_customer_type', 'get_vehicle_type', 'get_fee']
    ordering = ['name', 'customer_type', 'vehicle_type', 'fee']

    def get_fee(self, obj):
        return convert_to_currency(obj.fee)

    get_fee.allow_tags = True
    get_fee.short_description = u'Số tiền (đ)'

    def get_customer_type(self, obj):
        if obj.customer_type:
            return obj.customer_type
        return ''

    def get_vehicle_type(self, obj):
        if obj.vehicle_type:
            return obj.vehicle_type
        return ''

    get_customer_type.short_description = u'Loại khách hàng'
    get_vehicle_type.short_description = u'Loại xe'


class DepositActionFeeAdmin(admin.ModelAdmin):
    def has_delete_permission(self, request, obj=None):
        return False
    list_display = ['name', 'customer_type', 'vehicle_type', 'get_fee']

    def get_fee(self, obj):
        return convert_to_currency(obj.fee)

    get_fee.allow_tags = True
    get_fee.short_description = u'Số tiền (đ)'


class ReceiptAdmin(admin.ModelAdmin):
    def has_delete_permission(self, request, obj=None):
        return False
    model = Receipt
    list_display = ['receipt_number', 'get_ref_id', 'get_fee', 'cancel', 'notes', 'get_pdf_ticket_payment_list_display',]
                    #'get_pdf_company_ticket_payment_list_display']
    list_filter = ['type']
    search_fields = ['receipt_number']

    fields = ['receipt_number', 'get_ref_id', 'cancel', 'notes', 'get_receipt_feature']
    readonly_fields = ['id', 'receipt_number', 'get_ref_id', 'cancel', 'notes', 'get_receipt_feature']

    def get_fee(self, obj):
        if obj.id and not obj.cancel:
            payment_fee = ''
            try:
                if obj.type == 0:
                    payment_fee = TicketPayment.objects.get(id=obj.ref_id).payment_fee
                elif obj.type == 1:
                    payment_fee = DepositPayment.objects.get(id=obj.ref_id).payment_fee
                return u"<p style='text-align: right'>{:,}</p>".format(payment_fee)
            except:
                return ''
        return ''

    get_fee.allow_tags = True
    get_fee.short_description = u'Số tiền'

    def get_pdf_ticket_payment_list_display(self, obj):
        if obj.id and not obj.cancel:
            javascript = """onclick="
            var w = (screen.width/2);
            var h = (screen.height/2);

            var top = h - h/2;
            var left = w - w/2;

            return !window.open(this.href, name, 'scrollbars=yes, resizable=yes, width='+w+', height='+h+ ', top='+top+', left='+left);"
            """
            if obj.type == 0:
                return mark_safe(
                    "<div style='text-align:center'><a {0} style='font-size:120%' class='fa fa-print' href='/pdf/ticket-payment/{1}?_popup=1'></a> <a {0} style='font-size:120%; color: orange' class='fa fa-print' href='/pdf/ticket-payment/{1}/1/?_popup=1'></a></div>".format(
                        javascript, obj.ref_id))
            elif obj.type == 1:
                return mark_safe(
                    "<div style='text-align:center'><a {0} style='font-size:120%' class='fa fa-print' href='/pdf/deposit-payment/{1}?_popup=1'></a> <a {0} style='font-size:120%; color: orange' class='fa fa-print' href='/pdf/deposit-payment/{1}/1/?_popup=1'></a></div>".format(
                        javascript, obj.ref_id))

        else:
            return ''
    get_pdf_ticket_payment_list_display.allow_tags = True
    get_pdf_ticket_payment_list_display.short_description = u''

    # def get_pdf_company_ticket_payment_list_display(self, obj):
    #     if obj.id and not obj.cancel:
    #         javascript = """onclick="
    #         var w = (screen.width/2);
    #         var h = (screen.height/2);
    #
    #         var top = h - h/2;
    #         var left = w - w/2;
    #
    #         return !window.open(this.href, name, 'scrollbars=yes, resizable=yes, width='+w+', height='+h+ ', top='+top+', left='+left);"
    #         """
    #         if obj.type == 0:
    #             return mark_safe(
    #                 "<div style='text-align:center'><a {0} style='font-size:120%; color: orange' class='fa fa-print' href='/pdf/ticket-payment/{1}/1/?_popup=1'></a></div>".format(
    #                     javascript, obj.ref_id))
    #         elif obj.type == 1:
    #             return mark_safe(
    #                 "<div style='text-align:center'><a {0} style='font-size:120%; color: orange' class='fa fa-print' href='/pdf/deposit-payment/{1}/1/?_popup=1'></a></div>".format(
    #                     javascript, obj.ref_id))
    #
    #     else:
    #         return ''


    # get_pdf_company_ticket_payment_list_display.allow_tags = True
    # get_pdf_company_ticket_payment_list_display.short_description = u''

    def get_receipt_feature(self, obj):
        if obj.id and not obj.cancel:
            if obj.type == 0:
                return mark_safe(
                    u'''<a target="_blank" onclick="return showAddAnotherPopup(this)" style="text-align: center;height: 20px; padding-top: 7px; width: 120px; background-color: #26C281; color: white; font-weight:bold;" class="fa fa-# print" href="/pdf/ticket-payment/%s">IN PHIẾU THU</a>''' % (
                    obj.ref_id))
            elif obj.type == 1:
                return mark_safe(
                    u'''<a target="_blank" onclick="return showAddAnotherPopup(this)" style="text-align: center;height: 20px; padding-top: 7px; width: 120px; background-color: #26C281; color: white; font-weight:bold;" class="fa fa-# print" href="/pdf/deposit-payment/%s">IN PHIẾU THU</a>''' % (
                    obj.ref_id))
        return ''

    get_receipt_feature.allow_tags = True
    get_receipt_feature.short_description = u'Chức năng'

    def has_add_permission(self, request):
        return False

    def get_ref_id(self, obj):
        if obj.type == 0 and obj.ref_id:
            return mark_safe(
                u'<a target="_blank" href="/admin/parking/ticketpayment/{0}">{1}</a>'.format(obj.ref_id, u'Gia hạn'))
        elif obj.type == 1 and obj.ref_id:
            return mark_safe(
                u'<a target="_blank" style="color: brown" href="/admin/parking/depositpayment/{0}">{1}</a>'.format(
                    obj.ref_id, u'Cọc thẻ'))
        return ''

    get_ref_id.allow_tags = True
    get_ref_id.short_description = u'Loại'

##
# CLAIM PROMOTION
##

class ClaimPromotionTenantAdmin(admin.ModelAdmin):
    model = ClaimPromotionTenant

    list_display = ['name']

    ordering = ['name']


    def has_delete_permission(self, request, obj=None):
        return False
#208Aug14
class ClaimPromotionGroupTenantAdmin(admin.ModelAdmin):
    model = ClaimPromotionGroupTenant

    list_display = ['groupname']

    ordering = ['groupname']
    def has_delete_permission(self, request, obj=None):
        return False
class VehicleBalcklistAdmin(admin.ModelAdmin):
    model = VehicleBalcklist
    list_display = ['vehicle_number','vehicle_type','notes']
    #
    ordering = ['vehicle_type']
    def has_delete_permission(self, request, obj=None):
        return False
##2018Dec13
class SlotAdmin(admin.ModelAdmin):
    model = Slot
    list_display = ['name','slottotal','prefix','suffixes','numlength','hascheckkey']
    #
    ordering = ['name']
    def has_delete_permission(self, request, obj=None):
        return False
##2018Dec13
class ClaimPromotionVoucherAdmin(admin.ModelAdmin):
    model = ClaimPromotionVoucher

    list_display = ['name', 'value']

    ordering = ['value']
    def has_delete_permission(self, request, obj=None):
        return False
class BuildingAdmin(admin.ModelAdmin):
    model = Building
    def has_delete_permission(self, request, obj=None):
        return False
class ApartmentAdmin(admin.ModelAdmin):
    model = Apartment
    def has_delete_permission(self, request, obj=None):
        return False
@login_required(redirect_field_name='', login_url='/admin/')
def render_confirm_checkout_exception(request):  # Admin view: cho ra ngoai le
    if 'btn_CANCEL' in request.POST:
        # print "cancel"
        return redirect(reverse('admin:parking_parkingsession_changelist'))

    if 'btn_OK' in request.POST:
        # print "IN OK:", request.POST
        selected1 = request.POST['selected'].split(',')
        note = request.POST['note'].strip()
        if len(note) > 0:
            # print "len check"

            for item in selected1:
                idx = int(item)
                psess = ParkingSession.objects.get(id=idx)
                # psess.check_out_alpr_vehicle_number = psess.check_in_alpr_vehicle_number
                psess.check_out_operator = request.user  # Nguoi cho ra la nguoi dang dang nhap hien tai
                psess.check_out_time = get_now_utc()
                psess.duration = (psess.check_out_time - psess.check_in_time).total_seconds()
                pfee=callfeeforexception(psess.card.card_id)
                info = CheckOutExceptionInfo.objects.create(notes=note,parking_fee=pfee)
                psess.check_out_exception = info
                psess.save()
                card_status = CardStatus.objects.filter(parking_session_id=psess.id)
                if card_status:
                    card_status.update(status=0)
            messages.success(request, 'Cho ra ngoại lệ thành công!', fail_silently=True)
            return redirect(reverse('admin:parking_parkingsession_changelist'))

    data = list()
    selected = request.POST['selected']
    selected1 = selected.split(',')

    for item in selected1:
        idx = int(item)
        psess = ParkingSession.objects.get(id=idx)
        data.append({'parking_session_id': idx,
                     'card_label': psess.card.card_label,
                     'check_in_time': psess.check_in_time.strftime('%d/%m/%Y %H:%M:%S'),
                     'check_in_operator': psess.check_in_operator})

    messages.error(request, u'Bắt buộc nhập lí do cho ra ngoại lệ!', fail_silently=True)
    if data: return render(request, 'admin/confirmCOE.html', {'selected': selected, 'data': data})


# Admin view: Tim kiem ParkingSession
TRANG_THAI = {
    '0': 'Xe trong bãi',
    '1': 'Xe đã ra',
    '2': 'Tất cả'
}


@login_required(redirect_field_name='', login_url='/admin/')
def render_search_parking_session(request):
    data = list()
    card_label = ''
    vehicle_number = ''
    vehicle_type = str(VEHICLE_TYPE_CATEGORY[0][0])  # Tat ca
    vehicle_status = '0'  # Xe trong bai
    from_time = datetime.datetime.now().replace(hour=0, minute=0, second=0, microsecond=0).strftime('%d-%m-%Y %H:%M:%S')
    to_time = datetime.datetime.now().replace(hour=23, minute=59, second=59).strftime('%d-%m-%Y %H:%M:%S')

    # Loai xe
    vehicle_type_data = list()
    vehicle_type_dict = dict()
    for type in VehicleType.objects.all():
        vehicle_type_data.append({'vehicle_id': str(type.id), 'vehicle_name': type.name})
        vehicle_type_dict[get_storaged_vehicle_type(type.id)] = type.name

    # Trang thai xe
    vehicle_status_data = list()
    for item in sorted(TRANG_THAI.items()):
        vehicle_status_data.append({'status': item[0], 'status_name': item[1]})

    if 'btn_SEARCH' in request.POST:
        # Input
        card_label = request.POST['card_label']
        vehicle_number = request.POST['vehicle_number']
        vehicle_type = request.POST['vehicle_type']
        vehicle_status = request.POST['vehicle_status']
        from_time = request.POST['from_time']
        to_time = request.POST['to_time']
    localtime = pytz.timezone(TIME_ZONE)
    from_time_query = None
    to_time_query = None
    if from_time:
        from_time_query = datetime.datetime.strptime(from_time, '%d-%m-%Y %H:%M:%S')  # Input naive datetime
        from_time_query = localtime.localize(from_time_query)  # Gia su gio input co mui gio TIME_ZONE (Asia/Saigon)
        from_time_query = from_time_query.astimezone(pytz.utc)  # Chuyen sang gio UTC de so sanh voi CSDL.
    if to_time:
        to_time_query = datetime.datetime.strptime(to_time, '%d-%m-%Y %H:%M:%S')
        to_time_query = localtime.localize(to_time_query)
        to_time_query = to_time_query.astimezone(pytz.utc)
    rs = search_parking_session(mode=int(vehicle_status), limit=50000, card_label=card_label,
                                vehicle_number=vehicle_number,
                                vehicle_type=int(vehicle_type), from_time=from_time_query, to_time=to_time_query)
    # Filter
    # rs = filter_parking_session(card_label, vehicle_number, int(vehicle_type), vehicle_status, from_time, to_time)
    # 100000000L
    if rs and rs.count() > 0:
        for r in rs:
            data.append({'card_label': get_card_label(r),
                         'vehicle_number': r.vehicle_number,
                         'vehicle_type': vehicle_type_dict[r.vehicle_type],
                         'check_in_time': to_local_time(r.check_in_time).strftime('%d-%m-%Y %H:%M:%S'),
                         'check_out_time': to_local_time(r.check_out_time).strftime(
                             '%d-%m-%Y %H:%M:%S') if r.check_out_time else '',
                         'is_exception': r.check_out_exception.notes if r.check_out_exception else ''})

    return render(request, 'admin/parkingsession.html', {'data': data,
                                                         'vehicle_type_data': vehicle_type_data,
                                                         'vehicle_status_data': vehicle_status_data,
                                                         'card_label': card_label,
                                                         'vehicle_number': vehicle_number,
                                                         'vehicle_type': vehicle_type,
                                                         'vehicle_status': vehicle_status,
                                                         'from_time': from_time,
                                                         'to_time': to_time,
                                                         })


def get_card_label(r):
    url = ''
    if r.card:
        url = mark_safe('<a href="/admin/parking/parkingsession/%s">%s</a>' % (r.id, r.card.card_label))
    return url


@login_required(redirect_field_name='', login_url='/admin/')
def render_export_parking_session(request):
    from_time = datetime.datetime.now().replace(hour=0, minute=0, second=0, microsecond=0).strftime('%d-%m-%Y %H:%M:%S')
    to_time = datetime.datetime.now().strftime('%d-%m-%Y %H:%M:%S')
    if request.method == 'POST':
        # Input
        from_time = request.POST['from_time']
        to_time = request.POST['to_time']
        localtime = pytz.timezone(TIME_ZONE)
        from_time_query = None
        to_time_query = None
        if from_time:
            from_time_query = datetime.datetime.strptime(from_time, '%d-%m-%Y %H:%M:%S')  # Input naive datetime
            from_time_query = localtime.localize(from_time_query)  # Gia su gio input co mui gio TIME_ZONE (Asia/Saigon)
            from_time_query = from_time_query.astimezone(pytz.utc)  # Chuyen sang gio UTC de so sanh voi CSDL.
        if to_time:
            to_time_query = datetime.datetime.strptime(to_time, '%d-%m-%Y %H:%M:%S')
            to_time_query = localtime.localize(to_time_query)
            to_time_query = to_time_query.astimezone(pytz.utc)
        filename = 'Data_from_%s_to_%s.xlsx' % (from_time.replace(':', '').replace(' ', '_').replace('-', ''),
                                                to_time.replace(':', '').replace(' ', '_').replace('-', ''))
        export_parking_sessions_to_file(filename, from_time_query, to_time_query)
        return render(request, 'admin/export_parking_session.html', {'from_time': from_time, 'to_time': to_time,
                                                                     'link': '/export/' + filename})
    else:
        return render(request, 'admin/export_parking_session.html', {'from_time': from_time, 'to_time': to_time,
                                                                     'link': None})

def get_statistics_new(time_from, time_to, user_id, terminal):
    #from django.utils.timezone import utc
    ufrom =time_from + timedelta(hours=-7)
    uto=time_to + timedelta(hours=-7)
    uftime = ufrom.strftime('%Y-%m-%d %H:%M:%S')
    uttime = uto.strftime('%Y-%m-%d %H:%M:%S')
    ut = Utilities()
    qr = "select p.id,p.vehicle_type,c.card_type,p.check_in_time,p.check_in_operator_id,p.check_in_lane_id, p.check_out_time,p.check_out_operator_id,p.check_out_lane_id from parking_parkingsession p inner join parking_card c on c.id= p.card_id where p.check_in_time between '%s' and '%s' order by p.vehicle_type,c.card_type,p.check_in_time;" % (
    uftime, uttime)
    cins= ut.QueryDirect(qr)
    qr = "select p.id,p.vehicle_type,c.card_type,p.check_in_time,p.check_in_operator_id,p.check_in_lane_id, p.check_out_time,p.check_out_operator_id,p.check_out_lane_id from parking_parkingsession p inner join parking_card c on c.id= p.card_id where p.check_out_time between '%s' and '%s' order by p.vehicle_type,c.card_type,p.check_in_time;" % (
        uftime, uttime)
    couts=ut.QueryDirect(qr)
    checkins = [c for c in cins if ((not user_id) or (user_id) and c[4] == user_id) and (
    (not terminal) or (terminal and c[5] == terminal))]
    checkouts = [c for c in couts if ((not user_id) or (user_id) and c[7] == user_id) and (
    (not terminal) or (terminal and c[8] == terminal))]
    rs = dict()
    rs['card_types'] = list()
    rs['vehicle_types'] = list()
    rs['data'] = dict()
    rs['card_types'].append({'id': -1, 'name': u'Tất cả'})
    for item in CardType.objects.all():
        rs['card_types'].append({'id': item.id, 'name': item.name})
    for vehicle_type in VehicleType.objects.all():
        if vehicle_type.id == 100000000:
            continue
        rs['vehicle_types'].append(
            {'id': vehicle_type.id, 'name': u'Tổng cộng' if vehicle_type.id == 100000000 else vehicle_type.name})
        rs['data'][vehicle_type.id] = dict()
        for card_type in rs['card_types']:
            chins = [c for c in checkins if ((card_type['id'] == -1) or (c[2] == card_type['id'])) and (
            (vehicle_type.id == 100000000) or (int(vehicle_type.id / 10000) == c[1]))]
            chouts = [c for c in checkouts if ((card_type['id'] == -1) or (c[2] == card_type['id'])) and (
            (vehicle_type.id == 100000000) or (int(vehicle_type.id / 10000) == c[1]))]
            remains = [c for c in chins if ((not c[6]) or (c[6] and ((c[6].replace(tzinfo=None) - uto.replace(tzinfo=None)).total_seconds()>0)))]
            rs['data'][vehicle_type.id][card_type['id']] = {
                'check_in': len(chins) if chins else 0,
                'check_out': len(chouts) if chouts else 0,
                'remain': len(remains) if remains else 0
            }
    rs['vehicle_types'].append(
        {'id': 100000000, 'name': u'Tổng cộng'})
    rs['data'][100000000] = dict()
    for card_type in rs['card_types']:
        chins = [c for c in checkins if ((card_type['id'] == -1) or (c[2] == card_type['id']))]
        chouts = [c for c in checkouts if ((card_type['id'] == -1) or (c[2] == card_type['id']))]
        remains = [c for c in chins if (
            (not c[6]) or (c[6] and ((c[6].replace(tzinfo=None) - uto.replace(tzinfo=None)).total_seconds() > 0)))]
        rs['data'][vehicle_type.id][card_type['id']] = {
            'check_in': len(chins) if chins else 0,
            'check_out': len(chouts) if chouts else 0,
            'remain': len(remains) if remains else 0
        }
    return  rs
@login_required(redirect_field_name='', login_url='/admin/')
def render_statistics(request):
    data = list()
    head = list()
    fullname = ''
    now = datetime.datetime.now()
    time_from = datetime.datetime(now.year, now.month, now.day)
    time_to = time_from + datetime.timedelta(days=1)
    terminal_id = '-1'
    terminal_data = list()
    terminal_data.append({'id': '-1', 'name': u'Tất cả'})
    user_id = None
    for item in Terminal.objects.all():
        terminal_data.append({'id': str(item.id), 'name': item.name})

    if 'btn_submit' in request.POST:
        time_from = datetime.datetime.strptime(request.POST['txt_in'], '%d-%m-%Y %H:%M:%S')
        time_to = datetime.datetime.strptime(request.POST['txt_out'], '%d-%m-%Y %H:%M:%S')
        terminal_id = request.POST['slt_terminal']
        fullname = request.POST['txt_user']
        if fullname:
            staff_id = request.POST['txt_user'].split('|')[0].strip()
            users = UserProfile.objects.filter(staff_id=staff_id)
            if users.count() > 0:
                user_id = users[0].user_id
    terminal = None
    if terminal_id != '-1':
        terminal = terminal_id
    # rs = get_statistics(time_from, time_to, user_id, terminal)
    rs = get_statistics_new(time_from, time_to, user_id, terminal)
    for item in rs['card_types']:
        head.append(item['name'])
    for vehicle_type in rs['vehicle_types']:
        temp_data = list()
        for card_type in rs['card_types']:
            count_rs = rs['data'][vehicle_type['id']][card_type['id']]
            temp_data.append(int_format(count_rs['check_in']))
            temp_data.append(int_format(count_rs['check_out']))
            temp_data.append(int_format(count_rs['remain']))
        data.append({'name': vehicle_type['name'], 'data': temp_data, 'IsBold': True if vehicle_type['id']== 100000000 else False})

    return render(request, "admin/statistics.html", {'fullname': fullname,
                                                     'terminal_data': terminal_data,
                                                     'terminal': terminal_id,
                                                     'data': data,
                                                     'head': head,
                                                     'time_in': time_from.strftime("%d-%m-%Y %H:%M:%S"),
                                                     'time_out': time_to.strftime("%d-%m-%Y %H:%M:%S")})


@login_required(redirect_field_name='', login_url='/admin/')
def render_statistics_by_location(request):
    head = list()
    now = datetime.datetime.now()
    time_from = datetime.datetime(now.year, now.month, now.day)
    time_to = time_from + datetime.timedelta(days=1)
    card_type_id = '-1'
    card_type_data = list()
    card_type_data.append({'id': '-1', 'name': u'Tất cả'})
    for item in CardType.objects.all():
        card_type_data.append({'id': str(item.id), 'name': item.name})
    if 'btn_submit' in request.POST:
        time_from = datetime.datetime.strptime(request.POST['txt_in'], '%d-%m-%Y %H:%M:%S')
        time_to = datetime.datetime.strptime(request.POST['txt_out'], '%d-%m-%Y %H:%M:%S')
        card_type_id = request.POST['slt_card_type']

    card_type = None
    if card_type_id != '-1':
        card_type = card_type_id
    localtime = pytz.timezone(TIME_ZONE)
    from_time_query = None
    to_time_query = None
    if time_from:
        from_time_query = localtime.localize(time_from)  # Gia su gio input co mui gio TIME_ZONE (Asia/Saigon)
        from_time_query = from_time_query.astimezone(pytz.utc)  # Chuyen sang gio UTC de so sanh voi CSDL.
    if time_to:
        to_time_query = localtime.localize(time_to)
        to_time_query = to_time_query.astimezone(pytz.utc)
    rs = get_statistics_by_location_only_checkin(from_time_query, to_time_query, card_type)
    for item in rs['vehicle_types']:
        head.append(item['name'])
    return render(request, "admin/statistics_by_location.html", {'card_type_data': card_type_data,
                                                                 'card_type': card_type_id,
                                                                 'data': rs['data'],
                                                                 'head': head,
                                                                 'time_in': time_from.strftime("%d-%m-%Y %H:%M:%S"),
                                                                 'time_out': time_to.strftime("%d-%m-%Y %H:%M:%S")})


@login_required(redirect_field_name='', login_url='/admin/')
def render_chart(request):
    timestamp = list()
    check_in = list()
    check_out = list()
    if 'btn_submit' in request.POST:
        vehicle_type_id = int(request.POST['slt_vehicle'])
        card_type_id = int(request.POST['slt_card_type'])
        time_begin = datetime.datetime.strptime(request.POST['txt_in'], '%d-%m-%Y %H:%M:%S')
        time_end = datetime.datetime.strptime(request.POST['txt_out'], '%d-%m-%Y %H:%M:%S')
    else:
        vehicle_type_id = VEHICLE_TYPE_CATEGORY[0][0]
        card_type_id = -1
        time_end = datetime.datetime.now()
        time_begin = datetime.datetime(year=time_end.year, month=time_end.month, day=time_end.day)
    check_in_data = list()
    check_out_data = list()
    count = 0
    for db_obj in ReportData.objects.filter(time__range=[time_begin - datetime.timedelta(hours=1), time_end]):
        timestamp.append(timegm(db_obj.time.utctimetuple()))
        check_in_data.append(loads(db_obj.check_in))
        check_out_data.append(loads(db_obj.check_out))
        count += 1
    for idx in range(1, count, 1):
        if vehicle_type_id == VEHICLE_TYPE_CATEGORY[0][0] and card_type_id == -1:
            check_in.append(check_in_data[idx]['total'] - check_in_data[idx - 1]['total'])
            check_out.append(check_out_data[idx]['total'] - check_out_data[idx - 1]['total'])
        elif vehicle_type_id != VEHICLE_TYPE_CATEGORY[0][0] and card_type_id == -1:
            check_in.append(
                check_in_data[idx]['vehicle_type'][str(vehicle_type_id)] - check_in_data[idx - 1]['vehicle_type'][
                    str(vehicle_type_id)])
            check_out.append(
                check_out_data[idx]['vehicle_type'][str(vehicle_type_id)] - check_out_data[idx - 1]['vehicle_type'][
                    str(vehicle_type_id)])
        elif vehicle_type_id == VEHICLE_TYPE_CATEGORY[0][0] and card_type_id != -1:
            check_in.append(check_in_data[idx]['card_type'][str(card_type_id)] - check_in_data[idx - 1]['card_type'][
                str(card_type_id)])
            check_out.append(check_out_data[idx]['card_type'][str(card_type_id)] - check_out_data[idx - 1]['card_type'][
                str(card_type_id)])
        else:
            check_in.append(check_in_data[idx]['vehicle_type__card_type'][str(vehicle_type_id)][str(card_type_id)] -
                            check_in_data[idx - 1]['vehicle_type__card_type'][str(vehicle_type_id)][str(card_type_id)])
            check_out.append(check_out_data[idx]['vehicle_type__card_type'][str(vehicle_type_id)][str(card_type_id)] -
                             check_out_data[idx - 1]['vehicle_type__card_type'][str(vehicle_type_id)][
                                 str(card_type_id)])

    vehicle_type_data = list()
    for item in VehicleType.objects.all():
        vehicle_type_data.append({'id': str(item.id), 'name': item.name})
    card_type_data = list()
    card_type_data.append({'id': '-1', 'name': u'Tất cả'})
    for item in CardType.objects.all():
        card_type_data.append({'id': str(item.id), 'name': item.name})
    json_data = {
        'timestamp': timestamp[1:],
        'check_in': check_in,
        'check_out': check_out
    }
    return render(request, 'admin/chart.html', {'data_content': json_data,
                                                'vehicle_type_data': vehicle_type_data,
                                                'vehicle_type': str(vehicle_type_id),
                                                'card_type_data': card_type_data,
                                                'card_type': str(card_type_id),
                                                'chart_title': None,
                                                'user_name': None,
                                                'time_in': time_begin.strftime('%d-%m-%Y %H:%M:%S'),
                                                'time_out': time_end.strftime('%d-%m-%Y %H:%M:%S')})


@login_required(redirect_field_name='', login_url='/admin/')
def render_attendance(request):
    data = list()
    time_to = datetime.datetime.now()
    time_from = datetime.datetime(time_to.year, time_to.month, 1)
    fullname = ''
    card_label = ''

    if 'btn_submit' in request.POST:
        if request.POST['txt_user']:
            staff_id = request.POST['txt_user'].split('|')[0].strip()
            users = UserProfile.objects.filter(staff_id=staff_id)
            total_time = 0.0
            if users.count() > 0:
                user = users[0]
                objects = Attendance.objects.filter(user_id=user.user_id, time_in__range=[
                    datetime.datetime.strptime(request.POST['txt_in'], '%d-%m-%Y'),
                    datetime.datetime.strptime(request.POST['txt_out'], '%d-%m-%Y')])
                if user.card:
                    card_label = user.card.card_label
                else:
                    card_label = ''
                fullname = request.POST['txt_user']
                for o in objects:
                    if o.time_out:
                        data.append({
                            'day_begin': o.time_in.strftime('%d-%m-%Y'),
                            'time_begin': o.time_in.strftime('%H : %M'),
                            'day_end': o.time_out.strftime('%d-%m-%Y'),
                            'time_end': o.time_out.strftime('%H : %M'),
                            'total': str(round(o.total_time_of_date, 2))})
                        total_time += o.total_time_of_date
                    else:
                        data.append({
                            'day_begin': o.time_in.strftime('%d-%m-%Y'),
                            'time_begin': o.time_in.strftime('%H : %M'), })

                data.append(str(round(total_time, 2)))

    return render(request, "admin/attendance.html", {'data': data,
                                                     'user_name': fullname,
                                                     'card_label': card_label,
                                                     'time_in': time_from.strftime('%d-%m-%Y'),
                                                     'time_out': time_to.strftime('%d-%m-%Y')})


@login_required(redirect_field_name='', login_url='/admin/')
def render_attendance_all_staff(request):
    data = list()
    title = list()
    content = list()
    count_user = []
    data.append(title)
    data.append(content)
    data.append(count_user)
    _time = datetime.datetime.now()
    if 'btn_submit' in request.POST:
        time_from = datetime.datetime.strptime("{0}-1 00:00:00".format(request.POST['txt_date']), '%m-%Y-%d %H:%M:%S')
        time_to = datetime.datetime.strptime(
            "{0}-{1} 23:59:59".format(request.POST['txt_date'], monthrange(time_from.year, time_from.month)[1]),
            '%m-%Y-%d %H:%M:%S')
    else:
        time_from = datetime.datetime(_time.year, _time.month, 1, 0, 0, 0)
        time_to = datetime.datetime(_time.year, _time.month, monthrange(time_from.year, time_from.month)[1], 23, 59, 59)

    for i in range(1, time_to.day + 1, 1):
        count_user.append(0)
        title.append({'day': str(i),
                      'day_of_week': get_day_of_week(
                          datetime.datetime.strptime("{0}-{1}-{2}".format(time_from.year, time_from.month, i),
                                                     "%Y-%m-%d").strftime("%A"))})

    users_profile = UserProfile.objects.all()

    for user in users_profile:
        attendance_objs = Attendance.objects.filter(user_id=user.user_id,
                                                    time_in__range=[time_from,
                                                                    time_to])
        days_list = list()
        for i in range(1, time_to.day + 1, 1):
            temp_objs = attendance_objs.filter(time_in__range=[
                datetime.datetime.strptime("{0}-{1}-{2} 00:00:00".format(time_to.month, time_to.year, i),
                                           '%m-%Y-%d %H:%M:%S') + datetime.timedelta(hours=7),
                datetime.datetime.strptime("{0}-{1}-{2} 23:59:59".format(time_to.month, time_to.year, i),
                                           '%m-%Y-%d %H:%M:%S') + datetime.timedelta(hours=7)])
            content_day = list()
            days_list.append(content_day)
            if temp_objs.count() > 0:
                count_user[i - 1] += 1
                for temp_obj in temp_objs:
                    if temp_obj.time_out:
                        data_obj = {'time_in': temp_obj.time_in.strftime('%H:%M'),
                                    'time_out': temp_obj.time_out.strftime('%H:%M'),
                                    'total': round((temp_obj.time_out - temp_obj.time_in).total_seconds() / 3600, 1)}
                    else:
                        data_obj = {'time_in': temp_obj.time_in.strftime('%H:%M')}

                    content_day.append(data_obj)
        if user.card_id:
            card_label = Card.objects.get(id=user.card_id).card_label
        else:
            card_label = ''
        content.append({'card_label': card_label,
                        'fullname': user.fullname,
                        'data': days_list})

    return render(request, 'admin/attendance_all_staff.html', {'month': _time.strftime("%m-%Y"),
                                                               'data': data})


def get_day_of_week(x):
    return {
        'Monday': 'T2',
        'Tuesday': 'T3',
        'Wednesday': 'T4',
        'Thursday': 'T5',
        'Friday': 'T6',
        'Saturday': 'T7',
        'Sunday': 'CN'
    }[x]


@login_required(redirect_field_name='', login_url='/admin/')
def render_confirm_pause(request):
    customer_id = request.POST['customer_id']
    if 'btn_CANCEL' in request.POST:
        # print "cancel"
        return redirect('/admin/parking/vehicleregistration/?customer=%s' % customer_id)

    if 'btn_OK' in request.POST:
        notes = request.POST['notes'].strip() or ''
        real_data = request.POST['real_data']
        today = datetime.date.today()
        # print ">> POST: ", request.POST

        if not real_data:
            messages.error(request, u'Không có xe nào phù hợp!', fail_silently=True)
            return redirect('/admin/parking/vehicleregistration/?customer=%s' % customer_id)
        else:
            real_data_list = real_data.split('@')
            for item in real_data_list:
                # print "Perform"
                idx = int(item)
                vehicle_registration = VehicleRegistration.objects.get(id=idx)

                if vehicle_registration.status != 2:  # Neu trang thai hien tai khong phai tam ngung (phong truong hop Back form)
                    remain_duration = 0
                    expired_date = None
                    if vehicle_registration.expired_date:
                        expired_date = vehicle_registration.expired_date
                        last_renewal_effective_date = vehicle_registration.last_renewal_effective_date

                        if expired_date > today:
                            if last_renewal_effective_date >= today:
                                remain_duration = (
                                vehicle_registration.expired_date - vehicle_registration.last_renewal_effective_date).days
                                # print "REMAIN DURATION 1", remain_duration
                            else:
                                remain_duration = (vehicle_registration.expired_date - today).days
                                # print "REMAIN DURATION 2", remain_duration

                    pause_resume = PauseResumeHistory(vehicle_registration=vehicle_registration, expired_date=expired_date, request_type=0,
                                                      request_notes=notes, remain_duration=remain_duration)
                    pause_resume.save()

                    # vehicle_registration.last_renewal_effective_date = None
                    vehicle_registration.start_date = None
                    vehicle_registration.expired_date = None
                    vehicle_registration.pause_date = today
                    vehicle_registration.status = 2  # Tam ngung

                    print "TAM NGUNG XE THANH CONG", today
                    vehicle_registration.save()

            messages.success(request, 'Tạm ngừng vé tháng khách hàng thành công!', fail_silently=True)
            return redirect('/admin/parking/vehicleregistration/?customer=%s' % customer_id)


@login_required(redirect_field_name='', login_url='/admin/')
def render_confirm_resume(request):
    customer_id = request.POST['customer_id']
    if 'btn_CANCEL' in request.POST:
        return redirect('/admin/parking/vehicleregistration/?customer=%s' % customer_id)
    # Kiem tra rang buoc (khong duoc truoc ngay check in gan nhat, khong duoc truoc ngay hom nay...)

    # Tien hanh phuc hoi
    if 'btn_OK' in request.POST:
        effective_date = request.POST['effective_date']
        if len(effective_date) == 0:
            messages.error(request, u'Ngày hiệu lực không phù hợp!', fail_silently=True)
            return redirect('/admin/parking/vehicleregistration/?customer=%s' % customer_id)

        vehicle_registration_id = int(request.POST['selected'])

        vehicle_registration = VehicleRegistration.objects.filter(id=vehicle_registration_id)
        if vehicle_registration:
            vehicle_registration = vehicle_registration[0]

            last_pause = PauseResumeHistory.objects.filter(vehicle_registration_id=vehicle_registration_id,
                                                           request_type=0, used=False).order_by('-request_date')
            if last_pause:
                # print "last pause history ", last_pause
                last_pause = last_pause[0]

                effective_date = request.POST['effective_date']
                try:
                    effective_datetime = datetime.datetime.strptime(effective_date, "%d/%m/%Y")
                    effective_date = datetime.date(effective_datetime.year, effective_datetime.month,
                                                   effective_datetime.day)
                    remain_duration = last_pause.remain_duration
                    vehicle_registration.start_date = effective_date
                    vehicle_registration.expired_date = effective_date + datetime.timedelta(days=remain_duration)
                    vehicle_registration.status = 1  # Dang dung
                    vehicle_registration.pause_date = None
                    vehicle_registration.save()

                    today = datetime.date.today().strftime("%d/%m/%Y")
                    last_pause.used = True
                    last_pause.save()

                    current_resume = PauseResumeHistory(vehicle_registration=vehicle_registration, request_type=1,
                                                        remain_duration=remain_duration, start_date=effective_date, request_notes=today, used=True)
                    current_resume.save()
                except:
                    return HttpResponseRedirect('')

                messages.success(request, 'Phục hồi vé tháng thành công!', fail_silently=True)
                return redirect('/admin/parking/vehicleregistration/?customer=%s' % customer_id)
            else:
                return HttpResponseRedirect("")
        else:
            return HttpResponseRedirect("")
    return HttpResponse("")


def is_vehicle_to_be_expired(customer_id):
    vr = VehicleRegistration.objects.filter(customer_id=customer_id)
    rs = [v for v in vr if get_duration(v.id, v.expired_date)[0] < 10]
    if rs:
        return True
    return False


@login_required(redirect_field_name='', login_url='/admin/')
def render_vehicle_ticket_sales(request):  # Doanh thu ve thang
    data = list()
    # TG bat dau: Dau thang hien tai
    now = datetime.datetime.now()
    start_time = datetime.datetime(now.year, now.month, now.day).replace(day=1)
    # TG ket thuc: Cuoi thang hien tai
    month = now.month
    year = now.year + month / 12
    month = month % 12 + 1
    end_time = start_time.replace(year=year, month=month, day=1) + datetime.timedelta(days=-1)

    vehicle_ticket_sales = 0
    filter_string = '/admin/parking/ticketpayment/'
    data = list()
    if 'btn_submit' in request.POST:
        start_time = datetime.datetime.strptime(request.POST['start_time'], '%d-%m-%Y %H:%M')
        end_time = datetime.datetime.strptime(request.POST['end_time'], '%d-%m-%Y %H:%M')

        ticket_payments = TicketPayment.objects.filter(payment_date__gte=start_time, payment_date__lte=end_time)
        filter_string += "?payment_date__gte=%s&payment_date__lte=%s" % (start_time, end_time)
        if ticket_payments:
            vehicle_ticket_sales = ticket_payments.aggregate(Sum('payment_fee'))['payment_fee__sum']

            for ticket_payment in ticket_payments:
                data.append({'id': ticket_payment.id, 'customer_name': ticket_payment.customer,
                             'payment_date': ticket_payment.payment_date.astimezone(pytz.timezone(TIME_ZONE)).strftime(
                                 '%d-%m-%Y %H:%M'),
                             'payment_fee': ticket_payment.payment_fee})



    # print start_time
    # print end_time

    return render(request, 'admin/ticketsales.html',
                  {'start_time': start_time.strftime('%d-%m-%Y %H:%M'),
                   'end_time': end_time.strftime('%d-%m-%Y %H:%M'),
                   'filename': 'GP_DoanhThuVeThang_%s_%s.xls' % (
                   start_time.strftime('%d%m%Y%H%M'), end_time.strftime('%d%m%Y%H%M')),
                   'ticket_payments': data,
                   'vehicle_ticket_sales': vehicle_ticket_sales,
                   'filter_string': filter_string,
                   })
##2017-12-28
@login_required(redirect_field_name='', login_url='/admin/')
def vehicle_renewal(request):
    util = Utilities()
    customer = Customer.objects.all()
    cusname = customer[0].customer_name
    customer_id = customer[0].id
    current_user = request.user.id
    if request.method == "GET":
        customer_id = request.GET['customer']
        cus1 = Customer.objects.filter(id=customer_id)
        cusname = cus1[0].customer_name
    registations = []
    regis = util.Query("renewalget", customer_id)
    for r in regis:
        registations.append({
            'id':r[0],
            'vehicelnumber': r[1],
            'cardnumber': r[2],
            'vehicletype': r[3],
            'status':r[4],
            'activedate':r[5],
            'expireddate':r[6],
            'feepermonth':r[7],
        })
    return render(request, 'admin/renewalregistry.html',{'registations': registations, 'cusname': cusname,'cusid':customer_id, 'current_user':current_user})
@login_required(redirect_field_name='', login_url='/admin/')
def vehecle_registration(request):
    util = Utilities()
    current_user = request.user.id
    customer = Customer.objects.all()
    cusname=customer[0].customer_name
    customer_id=customer[0].id
    pagenumber=50
    pageindex=1
    if request.method == "GET":
        if u'customer' in request.GET:
            customer_id = request.GET['customer']
            cus1=Customer.objects.filter(id=customer_id)
            customer=cus1
            cusname=cus1[0].customer_name
        if u'pageindex' in request.GET:
            pageindex=request.GET['pageindex']
    vehecletype=VehicleType.objects.all()
    card = []
    cards =util.Query(proc_name="cardlist")
    for c in cards:
        card.append({
            'cardId': c[0],
            'cardLable': c[2],
        })
    registations=[]
    regis=util.Query("getvehecleregitration",pageindex,pagenumber,customer_id)

    for r in regis:
        registations.append({
            'card': r[0],
            'customer': r[1],
            'drivename': r[5],
            'vehecletype':r[2],
            'vehecelnumber':r[3],
            'veheclebrand':r[4],
            'registiondate':r[7],
            'startdate': r[8],
            'expiratedate': r[9],
            'status': r[10],
            'receipt_number': r[11],
            'duration': r[12],
            'totalfee': r[13],
            'paymentid': r[14],
            'totalrows': r[16],
        })
    totalpage = 0
    totalrow = 0
    if len(registations)>0:
        totalrow=registations[0]['totalrows']
    rmp = divmod(totalrow, pagenumber)
    if rmp[1] > 0:
        totalpage = rmp[0]  + 1
    else:
        totalpage = rmp[0]
    pp=[]

    for i in range(1,int(totalpage)+1):
        if i==1 or i== int(totalpage) or i== int(pageindex) or  ( i<=int(pageindex)+5 and i>=int(pageindex)-5):
            pp.append(i)
    if int(totalpage) not in pp:
        pp.append(int(totalpage))
    feelevel=LevelFee.objects.all()

    # if request.method == "POST":
    #     util = Utilities()
    #     cus=request.POST.getlist('customer')[0]
    #     customer_id = Customer.objects.filter(customer_name=cus)[0].id
    #     cr=request.POST.getlist('card')[0]
    #     fl=request.POST.getlist('feelevel')[0]
    #     vt = request.POST.getlist('vehecletype')[0]
    #     v_number = request.POST.getlist('vehecle_number')[0]
    #     v_paint = request.POST.getlist('vehecle_paint')[0]
    #     note = request.POST.getlist('note')[0]
    #     d_name = request.POST.getlist('drive_name')[0]
    #     d_phone = request.POST.getlist('drive_phone')[0]
    #     fds=request.POST.getlist('from_date')[0]#.split('/')
    #     f_date =datetime.datetime.strptime(fds, "%d/%m/%Y").date()# '%s-%s-%s'%(fds[2],fds[1],fds[0])
    #     tds=request.POST.getlist('to_date')[0]#.split('/')
    #     t_date =datetime.datetime.strptime(tds, "%d/%m/%Y").date()#'%s-%s-%s'%(tds[2],tds[1],tds[0])
    #     current_user = request.user
    #     feemonth=LevelFee.objects.filter(id=fl)[0].fee
    #     tfee = getfeeByMonthSGCT(fdate=f_date, tdate=t_date, feepermonth=feemonth)#getfeeByMonthViettel(fdate=f_date, tdate=t_date, feepermonth=feemonth)
    #     rm=divmod(tfee,1000)
    #     if rm[1] > 0:
    #         tfee = rm[0] * 1000 + 1000
    #     if util.NonQuery("vehicleregistrationsave",customer_id,cr,fl,vt,v_number,v_paint,note,d_name,d_phone,f_date,t_date,tfee,current_user)>0:
    #         util.NonQuery(proc_name='autoupdateticket')
    #         card = []
    #         cards = util.Query(proc_name="cardlist")
    #         for c in cards:
    #             card.append({
    #                 'cardId': c[0],
    #                 'cardLable': c[2],
    #             })
    #         registations = []
    #         pageindex = 1
    #         pagenumber = 50
    #         regis = util.Query("getvehecleregitration",pageindex,pagenumber)
    #         for r in regis:
    #             registations.append({
    #                 'card': r[0],
    #                 'customer': r[1],
    #                 'drivename': r[5],
    #                 'vehecletype': r[2],
    #                 'vehecelnumber': r[3],
    #                 'veheclebrand': r[4],
    #                 'registiondate': r[7],
    #                 'startdate': r[8],
    #                 'expiratedate': r[9],
    #                 'status': r[10],
    #                 'receipt_number':r[11],
    #                 'duration': r[12],
    #                 'totalfee': r[13],
    #                 'paymentid': r[14],
    #                 'totalrows': r[16],
    #             })
    #         totalpage = 0
    #         totalrow = 0
    #         if len(registations) > 0:
    #             totalrow = registations[0]['totalrows']
    #         rmp = divmod(totalrow, pagenumber)
    #         if rmp[1] > 0:
    #             totalpage = rmp[0] + 1
    #         else:
    #             totalpage = rmp[0]
    #
    #         pp = []
    #         for i in range(1, int(totalpage) + 1):
    #             if i == 1 or i == int(totalpage) or i == int(pageindex) or (
    #                                     i <= int(pageindex) + 5 and i >= int(pageindex) - 5):
    #                 pp.append(i)
    #         if int(totalpage) not in pp:
    #             pp.append(int(totalpage))
    #         messages.success(request,"Đăng ký thành công cho lái xe: %s"%d_name)
    #
    #         return render(request, 'admin/registryvehecle.html',
    #                       {'customer': customer, 'vehecletype': vehecletype, 'card': card, 'feelevel': feelevel,
    #                        'registations': registations, 'cusname': cusname, 'pageindex': int(pageindex),
    #                        'totalpages': pp})
    #     else:
    #         messages.error(request,"Đăng ký thất bại cho lái xe: %s. Vui lòng thử lại"%d_name)
    #         return render(request, 'admin/registryvehecle.html',
    #                       {'customer': customer, 'vehecletype': vehecletype, 'card': card, 'feelevel': feelevel,
    #                        'registations': registations, 'cusname': cusname, 'pageindex': int(pageindex),
    #                        'totalpages': pp})
    # else:
    return render(request,'admin/registryvehecle.html',{'customer':customer,'vehecletype':vehecletype,'card':card,'feelevel':feelevel,'registations':registations,'cusname':cusname, 'pageindex':int(pageindex), 'totalpages':pp,'cusid':customer_id,'staff':current_user})
##
###Config fee page
# @login_required(redirect_field_name='', login_url='/admin/')
# def configfee(request):
#     current_user = request.user.id
#     return render(request, 'admin/configfee.html',{'current_user':current_user})
##2018-01-05
##
###

def get_time_zone_time(dt, tz=TIME_ZONE, format="%d/%m/%Y"):
    day = datetime.datetime(dt.year, dt.month, dt.day)
    return pytz.utc.localize(day).astimezone(pytz.timezone(tz)).strftime(format)
def last_day_of_month(any_day):
    next_month = any_day.replace(day=28) + datetime.timedelta(days=4)  # this will never fail
    return next_month - datetime.timedelta(days=next_month.day)
def first_day_of_month(any_day):
    return any_day.replace(day=1)
def daysofmonth(any_day):
    endate = last_day_of_month(any_day)
    firstdate=any_day.replace(day=1)
    return (endate-firstdate).days+1

def monthsoftwodate(any_fromday,any_enddate):
    num_months=1;
    month1=any_fromday.month
    year1=any_fromday.year
    month2=any_enddate.month
    year2=any_enddate.year
    while year1 <= year2 and month1 != month2:
        month1 += 1
        num_months += 1
        if month1 > 12:
            month1 = 1
            year1 += 1
    return num_months
def getfeeByMonthSGCT(fdate, tdate, feepermonth):
    ttmonth=monthsoftwodate(fdate,tdate)
    if ttmonth==1:
        return ceil(((tdate - fdate).days + 1) * float(feepermonth) / daysofmonth(fdate))
    elif ttmonth==2:
        return ceil(((last_day_of_month(fdate) - fdate).days + 1) * float(feepermonth) / daysofmonth(fdate))+ceil(((tdate - first_day_of_month(tdate)).days + 1) * float(feepermonth) / daysofmonth(tdate))
    elif ttmonth>2:
        mo = ttmonth-2
        return ceil(((last_day_of_month(fdate) - fdate).days + 1) * float(feepermonth) / daysofmonth(fdate)) + ceil(
            ((tdate - first_day_of_month(tdate)).days + 1) * float(feepermonth) / daysofmonth(tdate)) + float(feepermonth)*float(mo)
    else:
        return 0
def getfeeByMonthViettel(fdate, tdate, feepermonth):
    ttmonth=monthsoftwodate(fdate,tdate)
    if ttmonth==1:
        return ceil(((tdate - fdate).days + 1) * float(feepermonth) / 30)
    elif ttmonth==2:
        return ceil(((last_day_of_month(fdate) - fdate).days + 1) * float(feepermonth) / 30)+ceil(((tdate - first_day_of_month(tdate)).days + 1) * float(feepermonth) / 30)
    elif ttmonth>2:
        mo = ttmonth-2
        return ceil(((last_day_of_month(fdate) - fdate).days + 1) * float(feepermonth) / 30) + ceil(
            ((tdate - first_day_of_month(tdate)).days + 1) * float(feepermonth) / 30) + float(feepermonth)*float(mo)
    else:
        return 0

admin.site.unregister(User)

#
admin.site.register(EPassPrefixCheck,EPassPrefixAdmin)
admin.site.register(EPassCollected,EPassCollectedAdmin)
admin.site.register(EPassPartner,EPassPartnerAdmin)
admin.site.register(EPassAPI,EPAssApiAdmin)
##Invoices
admin.site.register(InvoiceTaxRule, InvoiceTaxRuleAdmin)
admin.site.register(PartnerInvoice, PartnerInvoiceAdmin)
admin.site.register(InvoiceApiInitation, InvoiceApiAdmin)
admin.site.register(InvoiceConnector, InvoiceConnectorAdmin)
admin.site.register(InvoiceBuyer, InvoiceBuyerAdmin)
admin.site.register(RetailInvoice, RetailInvoiceAdmin)
admin.site.register(ConsolidatedInvoice, ConsolidatedInvoiceAdmin)
##Invoices
admin.site.register(Card, CardAdmin)
admin.site.register(User, ApmsUserAdmin)
admin.site.register(CardType, CardTypeAdmin)
admin.site.register(TerminalGroup, TerminalGroupAdmin)
admin.site.register(CustomerType)
admin.site.register(Company)
admin.site.register(FeeAdjustment, FeeAdjustmentAdmin)
admin.site.register(Apartment, ApartmentAdmin)
admin.site.register(Building, BuildingAdmin)

admin.site.register(Receipt, ReceiptAdmin)

admin.site.register(DepositActionFee, DepositActionFeeAdmin)
admin.site.register(DepositPayment, DepositPaymentAdmin)
admin.site.register(DepositPaymentDetail)
#VehicleBalcklistAdmin

admin.site.register(Customer, CustomerAdmin)
admin.site.register(Terminal, TerminalStatusAdmin)
admin.site.register(VehicleType, VehicleTypeAdmin)
admin.site.register(ParkingSession, ParkingSessionAdmin)
admin.site.register(ParkingSetting, ParkingSettingAdmin)
admin.site.register(ImageReplicationSetting, ImageReplicationSettingAdmin)
admin.site.register(ParkingFee, ParkingFeeAdmin)
admin.site.register(TicketPayment, TicketPaymentAdmin)
admin.site.register(ParkingFeeSession)
admin.site.register(PauseResumeHistory)

admin.site.register(VehicleRegistration, VehicleRegistrationAdmin)
admin.site.register(LevelFee, LevelFeeAdmin)

admin.site.register(ClaimPromotionTenant, ClaimPromotionTenantAdmin)
admin.site.register(ClaimPromotionGroupTenant, ClaimPromotionGroupTenantAdmin)
admin.site.register(ClaimPromotionVoucher, ClaimPromotionVoucherAdmin)
admin.site.register(VehicleBalcklist, VehicleBalcklistAdmin)
admin.site.register(Slot, SlotAdmin)
admin.site.register_view('Chart/$', view=render_chart, visible=False)
admin.site.register_view('Statistics/$', view=render_statistics, visible=False)
# admin.site.register_view('report/BarierForced/$', view= report.render_report_barier_forced, visible=False)
# admin.site.register_view('report/user-list/$', view= report.render_report_user_list, visible=False)
admin.site.register_view('StatisticsByLocation/$', view=render_statistics_by_location, visible=False)
admin.site.register_view('Attendance/$', view=render_attendance, visible=False)
admin.site.register_view('AttendanceAllStaff/$', view=render_attendance_all_staff, visible=False)
admin.site.register_view('ConfirmCOE/$', view=render_confirm_checkout_exception, visible=False)
admin.site.register_view('SearchParkingSession/$', view=render_search_parking_session, visible=False)
admin.site.register_view('ExportParkingSession/$', view=render_export_parking_session, visible=False)
admin.site.register_view('VehicleTicketSales/$', view=render_vehicle_ticket_sales, visible=False)
##2017-12-28
admin.site.register_view('vehecleregistry/$',view=vehecle_registration,visible=False)
admin.site.register_view('renewalregistry/$',view=vehicle_renewal,visible=False)
# admin.site.register_view('configfee/$',view=configfee,visible=False)
##

admin.site.register_view('ConfirmPause/$', view=render_confirm_pause, visible=False)
admin.site.register_view('ConfirmResume/$', view=render_confirm_resume, visible=False)
admin.site.register(ApiToken, ApiTokenAdmin)
