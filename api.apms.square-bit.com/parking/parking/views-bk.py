# -*- coding: utf-8 -*-

# Standard
import requests
from datetime import date, datetime, time, timedelta
from math import ceil, fabs

# Django
from django.contrib import messages
from django.contrib.auth.decorators import login_required
from django.contrib.auth.models import User
from django.core.cache import get_cache
from django.core.urlresolvers import reverse
from django.db.models import Q

from django.forms import ModelForm, forms, Textarea
from django.utils.safestring import mark_safe
##2017-12-29
from django.views.decorators.csrf import csrf_exempt
##
from django.http import HttpResponse, HttpResponseRedirect, Http404
from django.shortcuts import render, redirect
from django.utils.timezone import utc, now as django_utc_now
from threading import Thread
import Queue
# 3rd party
from pytz import timezone
from xlsxwriter import Workbook

# Local
from site_settings.settings import TIME_ZONE
from support import form_fill, int_format, number_to_text
from models import ClaimPromotionGroupTenant, ClaimPromotionTenant,ClaimPromotionV2, Server, Terminal, TurnFee, BlockFee, ParkingFee, VehicleRegistration, UserProfile, \
    TicketPaymentDetail, \
    CustomerType, Customer, ParkingSession, Apartment, Building, Company, Card, LevelFee, VehicleType, DepositActionFee, \
    TicketPayment, DepositPaymentDetail, ParkingSetting, DepositPayment, Receipt, PauseResumeHistory
from models import VEHICLE_TYPE, decode_vehicle_type, get_setting,CardType
from common import VEHICLE_STATUS_CHOICE
from xlrd.xldate import xldate_as_tuple
##2017-12-29
from django.db import connections
##
##2017-04-11
import json
from django.core import serializers
from django.contrib.auth.models import Group
##
import  base64
B = False

VEHICLE_TYPE_ENCODE_DICT = {}


def load_vehicle_type():
    for data in VEHICLE_TYPE:
        rs = decode_vehicle_type(data[0])
        VEHICLE_TYPE_ENCODE_DICT[rs[1]] = data[0]


load_vehicle_type()

def get_now_utc():
    return datetime.utcnow().replace(microsecond=0, tzinfo=utc)


def get_last_update_text(duration_in_seconds):
    if duration_in_seconds < 60:
        return u'%d giây' % duration_in_seconds
    elif duration_in_seconds < 3600:
        return u'%d phút' % (duration_in_seconds / 60)
    elif duration_in_seconds < 86400:
        return u'%d giờ' % (duration_in_seconds / 3600)
    else:
        return u'%d ngày' % (duration_in_seconds / 86400)


def get_terminals(request):
    data = list()
    text_status = '<p style="color:%s;%s">%s'
    idx = 0
    for terminal in Terminal.objects.all():
        idx += 1
        duration_in_seconds = int((django_utc_now() - terminal.last_check_health).total_seconds())
        data.append({
            'idx': idx,
            'name': terminal.name,
            'ip': terminal.ip,
            'version': terminal.version,
            'last_update': get_last_update_text(duration_in_seconds),
            'status': (text_status % ("green", "font-weight:bold;", "ONLINE")) if duration_in_seconds <= 86400 else (
                text_status % ("red", "", "OFFLINE"))
        })
    return render(request, 'admin/terminal_status.html', {'data': data})


def get_servers(request):
    data = list()
    text_status = '<p style="color:%s;%s">%s'
    idx = 0
    for server in Server.objects.all():
        idx += 1
        status = (text_status % ("green", "font-weight:bold;", "ONLINE"))
        try:
            response = requests.get('http://%s:9191/api/health/' % server.ip)
            if response.status_code == 500:
                status = (text_status % ("red", "", "DATABASE OFFLINE"))
        except:
            status = (text_status % ("red", "", "OFFLINE"))
        data.append({
            'idx': idx,
            'name': server.name,
            'ip': server.ip,
            'status': status
        })
    return render(request, 'admin/server_status.html', {'data': data})


def get_google_chart_script(request):
    rs = "if (window['google'] != undefined && window['google']['loader'] != undefined) {" + '\n'
    rs += "if (!window['google']['visualization']) {" + '\n'
    rs += "window['google']['visualization'] = {};" + '\n'
    rs += "google.visualization.Version = '1.0';" + '\n'
    rs += "google.visualization.JSHash = '8c95b72e5c145d5b3d7bb8b4ea74fd63';" + '\n'
    rs += "google.visualization.LoadArgs = 'file\75visualization\46v\0751\46packages\75corechart';" + '\n'
    rs += "}" + '\n'
    rs += 'google.loader.writeLoadTag("css", "/static/css/core-chart.css", false);' + '\n'
    rs += 'google.loader.writeLoadTag("script", "/static/js/core-chart.js", false);' + '\n'
    rs += "}"
    return HttpResponse(rs)


def get_ticket_payment_info(request,
                            ticket_payment_id):  # Kiem tra xem co phai luot dong tien moi nhat cua khach hang nay, neu phai thi cho phep chinh sua ModelAdmin
    ticket_payment = TicketPayment.objects.filter(id=ticket_payment_id)

    if ticket_payment:
        customer_id = ticket_payment[0].customer_id
        customer_ticket_payments = TicketPayment.objects.filter(customer_id=customer_id).order_by('-payment_date')
        if customer_ticket_payments and int(customer_ticket_payments[0].id) == int(ticket_payment_id):
            return HttpResponse("True")
    return HttpResponse("False")


def get_vehicle_registration_info(request,
                                  vehicle_registration_id):  # API ho tro tinh nang "Quan ly ve thang khach hang"
    VALUE = u"%s"

    vehicle_registration = VehicleRegistration.objects.filter(id=vehicle_registration_id)
    if vehicle_registration:
        pairs = []
        today = date.today()

        new_expired_date = None
        if vehicle_registration.expired_date:
            new_expired_date = vehicle_registration.expired_date + timedelta(days=1)
        if new_expired_date < today: new_expired_date = today

        vehicle_registration = vehicle_registration[0]
        vehicle_type = vehicle_registration.vehicle_type
        status = vehicle_registration.status
        level_fee_name = vehicle_registration.level_fee.__unicode__() if vehicle_registration.level_fee else ''
        level_fee = vehicle_registration.level_fee.fee if vehicle_registration.level_fee else 0
        old_expired_date = vehicle_registration.expired_date if vehicle_registration.expired_date else ''
        expired_date = new_expired_date

        pairs.append(VALUE % vehicle_type)
        pairs.append(VALUE % VEHICLE_STATUS_CHOICE[status][1])
        pairs.append(VALUE % level_fee_name)

        pairs.append(VALUE % old_expired_date.strftime("%d/%m/%Y") if old_expired_date else '')
        pairs.append(VALUE % expired_date.strftime("%d/%m/%Y") if expired_date else '')

        pairs.append(VALUE % level_fee)

        final_pair = u'@'.join(pairs).strip()
        return HttpResponse(final_pair)

    return HttpResponse("")


def get_deposit_action_fee_list(request, vehicle_registration_id):  # API ho tro tinh nang "Dong coc the"
    PATTERN = '%s'
    vehicle_registration = VehicleRegistration.objects.filter(id=vehicle_registration_id)

    if vehicle_registration:
        vehicle_registration = vehicle_registration[0]
        deposit_action_fee_list = DepositActionFee.objects.filter(vehicle_type_id=vehicle_registration.vehicle_type,
                                                                  customer_type_id=vehicle_registration.customer.customer_type_id).values_list(
            'id')

        # print "Deposit action fee list ", deposit_action_fee_list

        if deposit_action_fee_list:
            pairs = []
            for item in deposit_action_fee_list:
                pairs.append(PATTERN % item[0])

            final_pair = u'@'.join(pairs).strip()

            return HttpResponse(final_pair)

    return HttpResponse("")


def get_deposit_action_fee(request, deposit_action_fee_id):
    deposit_action_fee = DepositActionFee.objects.filter(id=deposit_action_fee_id)

    final = ''
    if deposit_action_fee:
        final = "%s" % deposit_action_fee[0].fee

    return HttpResponse(final)


def get_new_expired_date(request, day, month, year, month_duration=0, day_duration=0, level_fee=0):
    PATTERN = "%s@%s"

    def count_months_between_dates(month1, year1, month2, year2):
        num_months = 0

        while year1 <= year2 and month1 != month2:
            month1 += 1
            num_months += 1
            if month1 > 12:
                month1 = 1
                year1 += 1

        return num_months

    def make_last_date_of_month(date1, months):
        if months == 0:
            return date1
        start_time = date(date1.year, date1.month, date1.day).replace(day=1)
        # TG ket thuc: Cuoi thang hien tai
        _month = date1.month - 1 + months
        _year = date1.year + _month / 12
        _month = _month % 12 + 1
        end_time = start_time.replace(year=_year, month=_month, day=1) + timedelta(days=-1)
        return end_time

    def get_num_days_of_month(_year, _month):
        from calendar import monthrange
        return int(monthrange(_year, _month)[1])

    def thread_get_new_expired_date(day, month, year, month_duration, day_duration, level_fee, queue):

        old_expired_date = date(year=int(year), month=int(month), day=int(day))
        final_date = make_last_date_of_month(old_expired_date, int(month_duration) / 30)

        final = PATTERN % ("", 0)

        if int(month_duration) == 0 and int(day_duration) == 0:
            final = PATTERN % ("", 0)
        elif final_date:
            if int(day_duration) == 0:  # Tinh theo trong ngay cuoi thang
                original_total_day_of_start_month = get_num_days_of_month(old_expired_date.year, old_expired_date.month)
                num_later_months = count_months_between_dates(old_expired_date.month, old_expired_date.year,
                                                              final_date.month, final_date.year)

                num_current_month_payable_days = original_total_day_of_start_month - old_expired_date.day + 1

                if num_later_months == 0 and num_current_month_payable_days == original_total_day_of_start_month:
                    final = PATTERN % (final_date.strftime("%d/%m/%Y"), level_fee)
                else:
                    price_per_day = float(level_fee) / get_num_days_of_month(old_expired_date.year,
                                                                             old_expired_date.month)
                    payable_fee = ceil(num_current_month_payable_days * price_per_day / 1000) * 1000 + num_later_months * float(level_fee)
                    final = PATTERN % (final_date.strftime("%d/%m/%Y"), int(payable_fee) if payable_fee else 0)
            else:  # Ket hop tinh theo so thang tron + so ngay
                fee = 0
                day_duration = int(day_duration)
                if int(month_duration) == 0:
                    day_duration -= 1

                final_date = final_date + timedelta(days=(int(day_duration)))

                last_date_current_month = make_last_date_of_month(old_expired_date, 1)

                is_next_month = False

                while (old_expired_date <= final_date):
                    last_date_current_month = make_last_date_of_month(old_expired_date, 1)
                    if last_date_current_month >= final_date:
                        last_date_current_month = final_date

                    num_current_month_payable_days = last_date_current_month.day - old_expired_date.day + 1
                    # if is_next_month:
                    #     num_current_month_payable_days += 1

                    if num_current_month_payable_days == get_num_days_of_month(last_date_current_month.year,
                                                                               last_date_current_month.month):  # Tron 1 thang
                        fee += int(level_fee)
                    else:
                        price_per_day = float(level_fee) / get_num_days_of_month(old_expired_date.year,
                                                                                 old_expired_date.month)

                        fee += ceil(num_current_month_payable_days * price_per_day / 1000) * 1000

                    old_expired_date = last_date_current_month + timedelta(days=1)
                    is_next_month = True

                final = PATTERN % (final_date.strftime("%d/%m/%Y"), int(fee) if fee else 0)

        queue.put(final)  # Put result

    q = Queue.Queue()
    Thread(target=thread_get_new_expired_date,
           args=(day, month, year, month_duration, day_duration, level_fee, q)).start()
    try:
        final = q.get(timeout=10)
        return HttpResponse(final)
    except Queue.Empty:
        final = PATTERN % ("", 0)
        return HttpResponse(final)


def get_current_user(request):
    try:
        user_id = UserProfile.objects.get(user_id=request.user.id).id
        return HttpResponse(user_id)
    except:
        return HttpResponse("")


def check_validity(request, vehicle_registration_id):
    # print "vehicle_registration_id: ", vehicle_registration_id
    return HttpResponse(vehicle_registration_id)



def add_worksheet(sheet_name, workbook, TEMPLATE, data, custom_param, sheet_protect=True):  # Them sheet vao workbook
    LOGO_EXCEL_REPORT_PATH = 'parking/static/image/logo_report.png'

    # Header
    TITLE = TEMPLATE['TITLE']
    HEADER = TEMPLATE['HEADER']
    STAT = TEMPLATE['STAT']

    sheet = workbook.add_worksheet(sheet_name)
    sheet.insert_image(0, 0, LOGO_EXCEL_REPORT_PATH)

    bold = workbook.add_format({'bold': True})
    wrap = workbook.add_format()
    wrap.set_text_wrap()

    border = workbook.add_format()
    border.set_border()

    bold_boder = workbook.add_format({'bold': True, 'border': 1})

    for r in TITLE:  # Viet tieu de: (dong r[0], cot r[1])
        sheet.write(r[0], r[1], r[2], bold)
        if len(r) == 4:
            sheet.write(r[0], r[1] + 1, r[3])

    for r in STAT:  # Viet dong thong ke theo loai xe: (dong r[0], cot r[1])
        sheet.write(r[0], r[1], r[2], bold)

        sum_r = 0
        if r[3] == 100000000L:
            sum_r = data.count()
        else:
            sum_r = data.filter(vehicle_type_id=r[3]).count()
        sheet.write(r[0], r[1] + 1, sum_r)

    for i in range(1, len(HEADER)):  # Viet table header (dong 5)
        sheet.write(5, i - 1, HEADER[i][0], bold_boder)

    list_val = [w[1] for w in HEADER if len(w) == 2 or (
        len(w) == 3 and isinstance(w[2], dict) and 'custom_function' in w[2])]  # Co field query hoac custom function

    rs_value_list = (data.values(*list_val))  # Query data
    final_data = list()
    for k, r in enumerate(rs_value_list):
        temp_data = list()
        count_newline = 0
        for j in range(1, len(HEADER)):
            column = HEADER[j]
            if column[0] == 'STT':  # Cell so thu tu tang dan
                temp_data.append(k + 1)
            elif len(column) == 2 and column[1] in r:
                # Nhung cot co dinh nghia query field
                temp_value = r[column[1]]
                if isinstance(temp_value, date):  # Du lieu cell kieu datetime.date
                    temp_value = temp_value.strftime("%d/%m/%Y")
                temp_data.append(temp_value)

                count_newline = max(count_newline, temp_value.count('\n')) if (
                    isinstance(temp_value, str) or isinstance(temp_value, unicode)) else count_newline
            elif len(column) == 3 and isinstance(column[2], dict) and 'custom_function' in column[2]:
                if column[2]['custom_function'] == 'get_vehicle_number':
                    custom_function = custom_param['get_vehicle_number']
                    temp_value = custom_function(int(r[column[1]]), False)  # column[1]: single parameter
                    temp_data.append(temp_value)

                count_newline = max(count_newline, temp_value.count('\n')) if (
                    isinstance(temp_value, str) or isinstance(temp_value, unicode)) else count_newline
            else:  # Cot rong
                temp_data.append('')  # Cell trong
        final_data.append(temp_data)

        # print temp_data

        sheet.write_row(k + 6, 0, temp_data, border)  # Ghi toan dong du lieu (xuat phat tu dong 6)
        sheet.set_row(k + 6, (count_newline + 1) * 15)

    if sheet_protect:
        sheet.protect('ndhoang', options={
            'format_cells': True,
            'format_columns': True,
            'format_rows': True,
            'select_locked_cells': True,
            'sort': True,
            'autofilter': True,
            'pivot_tables': True,
            'select_unlocked_cells': True,
        })


# @login_required(redirect_field_name='', login_url='/admin/')
# def render_report_deposit_payment(request):  # Bao cao tien coc xe
#     def add_worksheet_stat(sheet_name, workbook, TEMPLATE, data, custom_param, sheet_protect=True):
#         LOGO_EXCEL_REPORT_PATH = 'parking/static/image/logo_report.png'
#
#         # Header
#         TITLE = TEMPLATE['TITLE']
#         HEADER = TEMPLATE['HEADER']
#         HEADER2 = TEMPLATE['HEADER2']
#         HEADER3 = TEMPLATE['HEADER3']
#         STAT = TEMPLATE['STAT']
#
#         sheet = workbook.add_worksheet(sheet_name)
#         sheet.insert_image(0,0, LOGO_EXCEL_REPORT_PATH)
#
#         bold = workbook.add_format({'bold': True})
#         wrap = workbook.add_format()
#         wrap.set_text_wrap()
#
#         border = workbook.add_format()
#         border.set_border()
#
#         bold_border = workbook.add_format({'bold': True, 'border': 1})
#
#         for r in TITLE:  # Viet tieu de: (dong r[0], cot r[1])
#             sheet.write(r[0], r[1], r[2], bold)
#             if len(r) == 4:
#                 sheet.write(r[0], r[1] + 1, r[3])
#
#         for r in STAT:  # Viet dong thong ke theo loai xe: (dong r[0], cot r[1])
#             sheet.write(r[0], r[1], r[2], bold)
#
#             sum_r = 0
#             if r[3] == 100000000L:
#                 sum_r = data.count()
#             else:
#                 sum_r = data.filter(vehicle_type_id=r[3]).count()
#             sheet.write(r[0], r[1]+1, sum_r)
#
#         for i in range(1, len(TEMPLATE['HEADER'])):  # Viet table TEMPLATE[HEADER] (dong 5)
#             sheet.write(5, i-1, TEMPLATE['HEADER'][i][0], bold_border)
#
#         for i in range(1, len(TEMPLATE['HEADER2'])):  # Viet table TEMPLATE[HEADER] (dong 6)
#             sheet.write(6, i-1, TEMPLATE['HEADER2'][i][0], bold_border)
#
#         for r in xrange(3, 4):
#             for i in range(1, len(TEMPLATE['HEADER' + str(r)])):  # Viet table TEMPLATE[HEADER] (dong 5)
#                 sheet.write(6+r-2, i-1, TEMPLATE['HEADER' + str(r)][i][0], border)
#
#         if sheet_protect:
#             sheet.protect('ndhoang', options={
#                 'format_cells':          True,
#                 'format_columns':        True,
#                 'format_rows':           True,
#                 'select_locked_cells':   True,
#                 'sort':                  True,
#                 'autofilter':            True,
#                 'pivot_tables':          True,
#                 'select_unlocked_cells': True,
#             })
#
#     # Template Bao cao thu phi dau xe
#     TEMPLATE_SHEET_DEPOSIT_PAYMENT = {
#         'HEADER': [
#             (0,),
#             (u'Loại xe',),
#         ],
#
#         'HEADER2': [
#             (0,),
#             (u'Loại phí',),
#         ],
#
#         'HEADER3': [
#             (0,),
#             (u'Tiền cọc',),
#         ],
#
#         # 'HEADER4': [
#         #     (0,),
#         #     (u'Tổng cộng',),
#         # ],
#
#         'TITLE': [
#         (2, 5, u'BÁO CÁO TIỀN CỌC THẺ'),
#         [3, 5, u'Từ'],
#         [3, 8, u'Đến'],
#     ],
#         'STAT': [], # [(4, 6 + i*2, type[1], type[0]) for i, type in enumerate(VEHICLE_TYPE)]
#     }
#
#     vehicle_type_data = list()
#
#     vehicle_types = VehicleType.objects.all()
#     for type in vehicle_types:
#         vehicle_type_data.append({"value": type.id, "name": type.name})
#         TEMPLATE_SHEET_DEPOSIT_PAYMENT['HEADER'].append( (type.name,) )
#         TEMPLATE_SHEET_DEPOSIT_PAYMENT['HEADER'].append( ('',) )
#
#         TEMPLATE_SHEET_DEPOSIT_PAYMENT['HEADER2'].append( (u'Số lượng',) )
#         TEMPLATE_SHEET_DEPOSIT_PAYMENT['HEADER2'].append( (u'Thành tiền',) )
#
#     now = datetime.now()
#     from_time = datetime(now.year, now.month, now.day).replace(day=1)  # TG bat dau: Dau thang hien tai
#
#     month = now.month
#     year = now.year + month / 12
#     month = (month + 1)% 12
#     to_time = from_time.replace(year=year, month=month, day=1, hour=23) + timedelta(days=-1)  # TG ket thuc: Cuoi thang hien tai
#
#     if 'btn_REPORT' in request.POST:
#         is_protected_report = False if 'unprotected_report' in request.POST else True
#
#         current_user = request.user
#         if not is_protected_report and not current_user.has_perm('parking.export_unprotected_excel'):
#             messages.error(request, u'Tài khoản hiện tại không có quyền này!', fail_silently=True)
#             return redirect(reverse('render_report_deposit_payment'))
#
#         from_time = request.POST['from_time']
#         to_time = request.POST['to_time']
#
#         TEMPLATE_SHEET_DEPOSIT_PAYMENT['TITLE'][1].append(from_time)
#         TEMPLATE_SHEET_DEPOSIT_PAYMENT['TITLE'][2].append(to_time)
#
#         rs = DepositPaymentDetail.objects.all()  # Cac chi tiet thanh toan
#
#         try:
#             _from_time = datetime.strptime(from_time, "%d/%m/%Y")  # Datetime
#             _to_time = datetime.strptime(to_time, "%d/%m/%Y").replace(hour=23,minute=59,second=59)
#
#             rs = rs.filter(deposit_payment__payment_date__lte=_to_time, deposit_payment__payment_date__gte=_from_time)
#
#         except:
#             messages.error(request, u'Khoảng thời gian không phù hợp!', fail_silently=True)
#             return HttpResponseRedirect('')
#
#         # Now final data
#         #rs_ticket_payment_detail = rs1.values_list('vehicle_registration__vehicle_type_id', 'ticket_payment__payment_method','payment_detail_fee')
#         #rs_parking_fee_session = rs2.values_list('parking_session__vehicle_type_id', 'parking_fee')
#         for type in vehicle_types:
#             vehicle_type_id = type.id  # Loai xe
#
#             coc_the = rs[:]
#             if vehicle_type_id != 100000000:
#                 coc_the = coc_the.filter(vehicle_registration__vehicle_type_id=vehicle_type_id)
#
#             #coc_the = coc_the.filter()
#
#             coc_the_so_luong = coc_the.count()
#             coc_the_thanh_tien = coc_the.aggregate(Sum('deposit_payment_detail_fee'))['deposit_payment_detail_fee__sum'] or 0
#
#
#             TEMPLATE_SHEET_DEPOSIT_PAYMENT['HEADER3'].append( (coc_the_so_luong,) )
#             TEMPLATE_SHEET_DEPOSIT_PAYMENT['HEADER3'].append( (coc_the_thanh_tien,) )
#
#             # TEMPLATE_SHEET_DEPOSIT_PAYMENT['HEADER4'].append( (coc_the_so_luong, ) )
#             # TEMPLATE_SHEET_DEPOSIT_PAYMENT['HEADER4'].append( (coc_the_thanh_tien,) )
#
#         folder_name = 'templates/report'
#         file_name = 'GPMS_BC_CocThe.xlsx'
#         file_path = '%s/%s' % (folder_name, file_name)
#
#         import os
#         if not os.path.exists(folder_name):
#             os.mkdir(folder_name)
#
#         if os.path.isfile(file_path):
#             os.remove(file_path)
#             # print "Xoa file bao cao"
#
#         workbook = Workbook(file_path, {'constant_memory': True})
#
#         add_worksheet_stat('Coc the', workbook, TEMPLATE_SHEET_DEPOSIT_PAYMENT, None, {}, is_protected_report) # D
#
#         workbook.close()
#
#         with open(file_path, 'r') as f:
#             response = HttpResponse(f.read(), content_type="application/vnd.ms-excel")
#             response['Content-Disposition'] = "attachment; filename=%s" % file_name
#
#             return response
#
#     return render(request, 'admin/rp-depositpayment.html', {'vehicle_type_data': vehicle_type_data,
#                                                             'from_time': from_time.strftime("%d/%m/%Y"),
#                                                             'to_time': to_time.strftime("%d/%m/%Y"),})


def get_vehicle_number(id, html=True):
    vehicle_registrations = VehicleRegistration.objects.filter(customer_id=id)

    if vehicle_registrations:
        temp = u'<p>%s<p>'
        temp2 = u"%s"
        # s = ''
        final = []

        for vehicle_registration in list(vehicle_registrations):
            current = vehicle_registration.vehicle_number if vehicle_registration else ''
            # print current
            # if current not in record:
            #     record.append(current)
            if html:
                final.append(temp % current)
            else:
                final.append(temp2 % current)
        if html:
            final = mark_safe("".join(final))
        else:
            final = "\n".join(final)
        # print "@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@DBG final ", final
        return final
    return ''


from support import get_status


def get_vehicle_status(id, html=True):
    vehicle_registrations = VehicleRegistration.objects.filter(customer_id=id)

    if vehicle_registrations:
        temp = u'%s'
        temp2 = u"%s"
        # s = u''
        final = []

        for vehicle_registration in list(vehicle_registrations):
            # current = STATUS_CHOICE[vehicle_registration.status][1] if vehicle_registration else ''
            current = get_status(vehicle_registration.status, False)
            # print current
            # if current not in record:
            #     record.append(current)
            if html:
                # s += temp % (current)
                final.append(temp % (current))
            else:
                final.append(temp2 % current)
        if html:
            # final = mark_safe(s)
            final = mark_safe("".join(final))
        else:
            final = "\n".join(final)
        # print "@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@DBG final ", final
        return final
    return ''


def get_vehicle_type(id):
    vehicle_registrations = VehicleRegistration.objects.filter(customer_id=id)

    temp = u'<p>%s<p>'
    final = []

    if vehicle_registrations:
        # record = []
        for vehicle_registration in list(vehicle_registrations):
            current = vehicle_registration.vehicle_type if vehicle_registration else ''
            # print current
            # if current not in record:
            #     record.append(current)
            final.append(temp % (current))
        return mark_safe("".join(final))
    return ''


@login_required(redirect_field_name='', login_url='/admin/')
def render_search_customer(request):  # Khach hang > Tim kiem khach hang
    current_user = request.user

    if not current_user.has_perm('parking.view_search_customer'):
        messages.error(request, u'Tài khoản hiện tại không có quyền này!', fail_silently=True)
        return redirect('/admin/parking/')

    TEMPLATE_SHEET_SEARCH_CUSTOMER = {
        'HEADER': (
            (0,),
            (u'STT',),
            (u'Tên khách hàng', 'customer_name'),
            (u'Xe đăng ký', 'id', {'custom_function': 'get_vehicle_number'}),
            (u'Loại khách hàng', 'customer_type__name'),
            (u'Tòa nhà', 'building__name'),
            (u'Căn hộ', 'apartment__address'),
            (u'Công ty', 'company__name'),
            (u'Ghi chú',),
        ),
        'TITLE': (
            (2, 5, u'DANH SÁCH KHÁCH HÀNG'),
            (3, 5, u'Ngày'),
            (3, 8, u'Giờ'),
        ),
        'STAT': []
    }

    customer_type_data = list()

    customer_types = CustomerType.objects.all()
    customer_type_data.append({"value": "", "name": "Tất cả"})
    for customer_type in customer_types:
        customer_type_data.append({"value": customer_type.name, "name": customer_type.name})

    if 'btn_SEARCH' in request.POST:
        # print request.POST

        card_query = request.POST['card_query'].strip()  # Ma the, ten the
        building_query = request.POST['building_query'].strip()  # Toa nha: ten, dia chi
        apartment_query = request.POST['apartment_query'].strip()  # Can ho
        vehicle_number = request.POST['vehicle_number'].strip()  # Bien so xe
        customer_name = request.POST['customer_name'].strip()  # Ten khach hang
        company_name = request.POST['company_name'].strip()  # Ten cong ty
        customer_type = request.POST['customer_type']

        rs = Customer.objects.all().select_related('apartment__address',
                                                   'apartment__owner_name',
                                                   'apartment__owner_phone',
                                                   'customer_type__name',
                                                   'building',
                                                   'apartment',
                                                   'company',
                                                   )
        vr = VehicleRegistration.objects.all().select_related('card__card_label',
                                                              'card__card_id',
                                                              'customer',
                                                              )

        if len(card_query) > 0:
            vr = vr.filter(Q(card__card_label__icontains=card_query) | Q(card__card_id__icontains=card_query))
            # print "VR", vr
            rs = rs.filter(id__in=[v.customer.id for v in vr])
        if len(building_query) > 0:
            rs = rs.filter(Q(building__name__icontains=building_query) | Q(building__address__icontains=building_query))
            # print "rs1 ", rs
        if len(apartment_query) > 0:
            rs = rs.filter(Q(apartment__address__icontains=apartment_query) | Q(
                apartment__owner_name__icontains=apartment_query) | Q(
                apartment__owner_phone__icontains=apartment_query))
            # print "rs2 ", rs
        if len(vehicle_number) > 0:
            vr = vr.filter(vehicle_number__icontains=vehicle_number)
            # print "VR", vr
            rs = rs.filter(id__in=[v.customer.id for v in vr])
            # print "rs3 ", rs
        if len(customer_name) > 0:
            rs = rs.filter(customer_name__icontains=customer_name)
            # print "rs4 ", rs
        if len(company_name) > 0:
            rs = rs.filter(company__name__icontains=company_name)
            # print "rs5 ", rs
        if len(customer_type) > 0:
            rs = rs.filter(customer_type__name=customer_type)
            # print "rs6 ", rs

        # print "rs FINAL1 ", rs
        customer_data = list()
        for r in rs:
            customer = {"id": r.id, "name": r.customer_name}

            customer_data.append({"customer": customer,
                                  "vehicle_number": get_vehicle_number(r.id),
                                  "vehicle_type": get_vehicle_type(r.id),
                                  "vehicle_status": get_vehicle_status(r.id),
                                  "customer_type": r.customer_type.name if r.customer_type else '',
                                  "building": r.building if r.building else '',
                                  "apartment": r.apartment if r.apartment else '',
                                  "company": r.company if r.company else '',
                                  }, )

        # Giu tham so khi post
        return render(request, 'admin/customer-search.html',
                      {'customer_type_data': customer_type_data, 'customer_type': customer_type,
                       'building_query': building_query, 'apartment_query': apartment_query,
                       'vehicle_number': vehicle_number,
                       'customer_name': customer_name, 'company_name': company_name, 'customer_data': customer_data})

    if 'btn_EXPORT' in request.POST:
        # print "Bao cao khach hang"

        building_query = request.POST['building_query'].strip()  # Toa nha: ten, dia chi
        apartment_query = request.POST['apartment_query'].strip()  # Can ho
        vehicle_number = request.POST['vehicle_number'].strip()  # Bien so xe
        customer_name = request.POST['customer_name'].strip()  # Ten khach hang
        company_name = request.POST['company_name'].strip()  # Ten cong ty
        customer_type = request.POST['customer_type']

        rs = Customer.objects.all()

        if len(building_query) > 0:
            rs = rs.filter(Q(building__name__icontains=building_query) | Q(building__address__icontains=building_query))
            # print "rs1 ", rs
        if len(apartment_query) > 0:
            rs = rs.filter(Q(apartment__address__icontains=apartment_query) | Q(
                apartment__owner_name__icontains=apartment_query) | Q(
                apartment__owner_phone__icontains=apartment_query))
            # print "rs2 ", rs
        if len(vehicle_number) > 0:
            vr = VehicleRegistration.objects.filter(vehicle_number__icontains=vehicle_number)
            # print "VR", vr
            rs = rs.filter(id__in=[v.customer.id for v in vr])
            # print "rs3 ", rs
        if len(customer_name) > 0:
            rs = rs.filter(customer_name__icontains=customer_name)
            # print "rs4 ", rs
        if len(company_name) > 0:
            rs = rs.filter(company__name__icontains=company_name)
            # print "rs5 ", rs
        if len(customer_type) > 0:
            rs = rs.filter(customer_type__name=customer_type)
            # print "rs6 ", rs

        # print "rs FINAL1 ", rs

        # Excel
        folder_name = 'templates/report'
        file_name = 'GPMS_BCKhachHang.xlsx'
        file_path = '%s/%s' % (folder_name, file_name)

        import os
        if not os.path.exists(folder_name):
            os.mkdir(folder_name)

        if os.path.isfile(file_path):
            os.remove(file_path)
            # print "Xoa file bao cao"

        workbook = Workbook(file_path, {'constant_memory': True})

        # print "Sheet BCKhachHang", rs

        # Ghi sheet Excel
        add_worksheet('BCKhachHang', workbook, TEMPLATE_SHEET_SEARCH_CUSTOMER, rs,
                      {'get_vehicle_number': get_vehicle_number})

        workbook.close()

        with open(file_path, 'r') as f:
            response = HttpResponse(f.read(), content_type="application/vnd.ms-excel")
            response['Content-Disposition'] = "attachment; filename=%s" % file_name

            return response

    return render(request, 'admin/customer-search.html', {'customer_type_data': customer_type_data})

def test_new(request, parking_session_id):
    parking_session = ParkingSession.objects.get(id=parking_session_id)

    check_in_time = parking_session.check_in_time.astimezone(timezone(TIME_ZONE))
    check_out_time = parking_session.check_out_time.astimezone(timezone(TIME_ZONE))

    calculate_parking_fee_by_turn(parking_session.vehicle_type, check_in_time, check_out_time)

    return HttpResponse("Test tinh tien moi!")


def get_time(datetime1):
    return time(datetime1.hour, datetime1.minute, datetime1.second, datetime1.microsecond)


def is_overnight(in_time, out_time):
    _out_time_temp = in_time.replace(hour=0, minute=0, second=0)
    _out_time_temp = _out_time_temp + timedelta(days=1)

    if out_time >= _out_time_temp:
        # print "Co qua dem"
        return True
    return False


def copy_datetime(dt):
    return datetime(year=dt.year, month=dt.month, day=dt.day,
                    hour=dt.hour, minute=dt.minute, second=dt.second, microsecond=dt.microsecond,
                    tzinfo=dt.tzinfo)

def calculate_parking_fee_by_turn(vehicle_type, check_in_time, check_out_time):
    PARKING_TURN_FEE_LOG = u" + Từ %s đến %s: %s %s"
    PARKING_OVERNIGHT_TURN_FEE_LOG = u" + Ra lúc %s : %s %s"
    PARKING_FEE_LOG = u"""- Vào lúc: %s
- Ra lúc : %s
- Phương thức tính: %s
- Phương tiện: %s
- Tổng cộng: %s
%s
"""

    def count_overnight_days(in_time, out_time):
        num_days = 0
        _in_time = copy_datetime(in_time)

        while (_in_time < out_time):
            _in_time = _in_time + timedelta(days=1)
            _in_time_overnight = _in_time.replace(hour=0, minute=0, second=0, microsecond=0)

            if _in_time >= out_time:
                _in_time = out_time

            if _in_time >= _in_time_overnight:
                num_days += 1

        return num_days

    _vehicle_type_name = 'xe'
    for item in VEHICLE_TYPE:
        if item[0] == vehicle_type:
            _vehicle_type_name = item[1]

    duration_in_seconds = (check_out_time - check_in_time).total_seconds()

    # new_num_days = (check_out_time - check_in_time).days
    new_num_days = count_overnight_days(check_in_time, check_out_time)

    # print "********************************************DBG: so ngay moi: ", new_num_days

    turn_fee = TurnFee.objects.filter(parking_fee__vehicle_type_id=vehicle_type)  # Bang phi giu xe theo luot

    if turn_fee:
        min_calculation_time = turn_fee[0].parking_fee.min_calculation_time

        turn_fee = turn_fee.values()[0]

        # print "-" * 40

        if min_calculation_time > 0 and int(duration_in_seconds / 60) <= min_calculation_time:
            _log = PARKING_FEE_LOG % (check_in_time.strftime("%H:%M %d/%m/%Y"),
                                      check_out_time.strftime("%H:%M %d/%m/%Y"),
                                      u'theo lượt', _vehicle_type_name,
                                      0,
                                      u"(Thời gian lưu bãi < %s phút , không tính tiền!)" % min_calculation_time)

            # print _log
            return 0, _log
        if check_in_time > check_out_time:
            _log = PARKING_FEE_LOG % (check_in_time.strftime("%H:%M %d/%m/%Y"),
                                      check_out_time.strftime("%H:%M %d/%m/%Y"),
                                      u'theo lượt', _vehicle_type_name,
                                      0,
                                      u"(Thời gian vào vượt thời gian ra thực tế!)")
            return 0, _log
    else:
        _log = PARKING_FEE_LOG % (check_in_time.strftime("%H:%M %d/%m/%Y"),
                                  check_out_time.strftime("%H:%M %d/%m/%Y"),
                                  u'theo lượt', _vehicle_type_name,
                                  0,
                                  u"(Không có thông tin bảng giá phí gửi %s theo Lượt)" % _vehicle_type_name)

        # print _log
        return 0, _log

    # print "SO NGAY ", new_num_days

    time_in = get_time(check_in_time)
    time_out = get_time(check_out_time)

    if new_num_days <= 1:  # Gui khong qua mot ngay
        fee = 0.0

        logs = []

        if is_overnight(check_in_time, check_out_time):  # Tinh qua dem
            fee += turn_fee['overnight_fee']

            logs.append(PARKING_TURN_FEE_LOG % (check_in_time.strftime("%H:%M %d/%m/%Y"),
                                                check_out_time.strftime("%H:%M %d/%m/%Y"),
                                                turn_fee['overnight_fee'],
                                                u"(Qua đêm)"))

            if time_out <= turn_fee['day_end_time']:
                fee += turn_fee['day_fee']
                # print "\t Ra luc luot ngay +", turn_fee['day_fee']

                logs.append(
                    PARKING_OVERNIGHT_TURN_FEE_LOG % (time_out.strftime("%H:%M"), turn_fee['day_fee'], u'(ngày)'))

            elif time_out <= turn_fee['night_end_time']:
                fee += turn_fee['night_fee']
                # print "\t Ra luc luot dem +", turn_fee['night_fee']

                logs.append(
                    PARKING_OVERNIGHT_TURN_FEE_LOG % (time_out.strftime("%H:%M"), turn_fee['night_fee'], u'(đêm)'))

            # print ">> Tu %s den %s: %s (Qua dem va ra)" % (check_in_time, check_out_time, fee)

            final_log = PARKING_FEE_LOG % (check_in_time.strftime("%H:%M %d/%m/%Y"),
                                           check_out_time.strftime("%H:%M %d/%m/%Y"),
                                           u'theo lượt', _vehicle_type_name,
                                           fee,
                                           '\n'.join(logs))
            # print final_log
            return fee, final_log

        else:  # Hoac tinh binh thuong

            if turn_fee['day_start_time'] <= time_in and time_out <= turn_fee['day_end_time']:  # Luot ngay
                fee += turn_fee['day_fee']
                # print "\t Cong them luot ngay +", turn_fee['day_fee']

                logs.append(PARKING_TURN_FEE_LOG % (check_in_time.strftime("%H:%M"),
                                                    check_out_time.strftime("%H:%M %d/%m/%Y"),
                                                    turn_fee['day_fee'], u'(ngày)'))

            elif turn_fee['night_start_time'] <= time_in and time_out <= turn_fee['night_end_time']:  # Luot dem
                fee += turn_fee['night_fee']
                # print "\t Cong them luot dem +", turn_fee['night_fee']

                logs.append(PARKING_TURN_FEE_LOG % (check_in_time.strftime("%H:%M"),
                                                    check_out_time.strftime("%H:%M %d/%m/%Y"),
                                                    turn_fee['night_fee'], u'(đêm)'))

            elif turn_fee['day_start_time'] <= time_in and time_out <= turn_fee['night_end_time']:  # Luot ngay, dem
                fee += turn_fee['day_fee'] + turn_fee['night_fee']
                # print "\t Cong them 2 luot ngay, dem +", turn_fee['night_fee']

                logs.append(PARKING_TURN_FEE_LOG % (check_in_time.strftime("%H:%M"),
                                                    "%s %s" % (turn_fee['day_end_time'].strftime("%H:%M"),
                                                               check_out_time.strftime("%d/%m/%Y")),
                                                    turn_fee['day_fee'], u'(ngày)'))
                logs.append(PARKING_TURN_FEE_LOG % (turn_fee['night_start_time'].strftime("%H:%M"),
                                                    check_out_time.strftime("%H:%M %d/%m/%Y"),
                                                    turn_fee['night_fee'], u'(đêm)'))

            final_log = PARKING_FEE_LOG % (check_in_time.strftime("%H:%M %d/%m/%Y"),
                                           check_out_time.strftime("%H:%M %d/%m/%Y"),
                                           u'theo lượt', _vehicle_type_name,
                                           fee,
                                           '\n'.join(logs))
            # print final_log

            return fee, final_log

    else:
        # print "Nhieu hon 1 ngay: ", new_num_days
        fee = 0.0
        fee = new_num_days * turn_fee['overnight_fee']

        logs = []

        logs.append(PARKING_TURN_FEE_LOG % (check_in_time.strftime("%H:%M %d/%m/%Y"),
                                            check_out_time.strftime("%H:%M %d/%m/%Y"),
                                            u"%s (ngày qua đêm) x %s = %s" % (
                                                new_num_days, turn_fee['overnight_fee'], fee),
                                            u"(Qua đêm)"))

        if time_out <= turn_fee['day_end_time']:
            fee += turn_fee['day_fee']
            # print "\t Ra luc luot ngay +", turn_fee['day_fee']

            logs.append(PARKING_OVERNIGHT_TURN_FEE_LOG % (time_out.strftime("%H:%M"), turn_fee['day_fee'], u'(ngày)'))

        elif time_out <= turn_fee['night_end_time']:
            fee += turn_fee['night_fee']
            # print "\t Ra luc luot dem +", turn_fee['night_fee']

            logs.append(PARKING_OVERNIGHT_TURN_FEE_LOG % (time_out.strftime("%H:%M"), turn_fee['night_fee'], u'(đêm)'))

        final_log = PARKING_FEE_LOG % (check_in_time.strftime("%H:%M %d/%m/%Y"),
                                       check_out_time.strftime("%H:%M %d/%m/%Y"),
                                       u'theo lượt', _vehicle_type_name,
                                       fee,
                                       '\n'.join(logs))
        # print final_log

        return fee, final_log

def fee_night(vehicle_type):
    _vehicle_type_name = 'xe'
    for item in VEHICLE_TYPE:
        if item[0] == vehicle_type:
            _vehicle_type_name = item[1]
    if (vehicle_type == 1000001):
        return  20000
    elif (vehicle_type == 2000101):
        return  200000
    elif (vehicle_type == 4000301):
        return  20000
    elif (vehicle_type == 5000401):
        return 200000
    else:
        return 0

### Tinh tien gui xe theo BlockNew
def calculate_parking_fee_by_block_new(vehicle_type,card_type, check_in_time, check_out_time):
    PARKING_FIRST_BLOCK_LOG = u""" + 1 Block đầu: %s (%s/%s phút)"""
    # PARKING_MAX_BLOCK_LOG = u""" + %s Block sau = %s phút >= %s phút (Tổng TG
    # block tối đa) => %s (Phí block tối đa)"""
    PARKING_MAX_BLOCK_LOG = u""" + Luu bai %s phút >= %s phút (Tổng TG block tối đa) => %s (Phí block tối đa)"""
    PARKING_NEXT_BLOCK_LOG = u""" + %s Block sau: %s x %s = %s (%s/%s phút)"""
    PARKING_FEE_LOG = u"""- Vào lúc: %s
    - Ra lúc : %s
    - Phương thức tính: %s
    - Phương tiện: %s
    - Tổng cộng: %s
    %s
    """
    _vehicle_type_name = 'xe'
    for item in VEHICLE_TYPE:
        if item[0] == vehicle_type:
            _vehicle_type_name = item[1]

    block_fee = BlockFee.objects.filter(parking_fee__vehicle_type_id=vehicle_type)
    total_fee = 0
    if block_fee:
        logs = []
        min_calculation_time = block_fee[0].parking_fee.min_calculation_time
        block_fee = block_fee.values()[0]
        duration = (check_out_time - check_in_time).total_seconds()
        if card_type == 3:
            min_calculation_time = 10800
            if duration <= min_calculation_time:
                return 0, ''
            check_in_time = check_in_time + timedelta(seconds=10800)
            min_calculation_time = 0
        xtotal_fee = 0
        in_day_block_fee = block_fee['in_day_block_fee'] 
        # night_block_fee = block_fee['night_block_fee']
        parking_block_time = []
        # Xe máy
        parking_block_time.append({"vehicle_type": 1000001, "duration": 7200, "fee": 5000, "time_unit": 7200, "is_only": True})
        parking_block_time.append({"vehicle_type": 1000001, "duration": 0, "fee": 1000, "time_unit": 3600, "is_only": False})
        # Xe máy - Giao hàng
        parking_block_time.append({"vehicle_type": 4000301, "duration": 7200, "fee": 5000, "time_unit": 7200, "is_only": True})
        parking_block_time.append({"vehicle_type": 4000301, "duration": 0, "fee": 1000, "time_unit": 3600, "is_only": False})
        # Xe OTO
        parking_block_time.append({"vehicle_type": 2000101, "duration": 7200, "fee": 20000, "time_unit": 7200, "is_only": True})
        #before 2018may07
        parking_block_time.append({"vehicle_type": 2000101, "duration": 14400, "fee": 20000, "time_unit": 3600, "is_only": True})
        # after 2018may07
        #parking_block_time.append({"vehicle_type": 2000101, "duration": 21600, "fee": 20000, "time_unit": 3600, "is_only": True})
        #
        parking_block_time.append({"vehicle_type": 2000101, "duration": 0, "fee": 40000, "time_unit": 3600, "is_only": False})

        # Xe tải giao hàng
        parking_block_time.append({"vehicle_type": 5000401, "duration": 7200, "fee": 20000, "time_unit": 7200, "is_only": True})
        parking_block_time.append({"vehicle_type": 5000401, "duration": 7200, "fee": 20000, "time_unit": 3600, "is_only": True})
        parking_block_time.append({"vehicle_type": 5000401, "duration": 0, "fee": 40000, "time_unit": 3600, "is_only": False})
        # Xe VAN
        parking_block_time.append(
            {"vehicle_type": 1010101, "duration": 7200, "fee": 20000, "time_unit": 7200, "is_only": True}),
        parking_block_time.append(
            {"vehicle_type": 1010101, "duration": 7200, "fee": 20000, "time_unit": 3600, "is_only": True})
        parking_block_time.append(
            {"vehicle_type": 1010101, "duration": 0, "fee": 40000, "time_unit": 3600, "is_only": False})
        if duration <= 0:
            final_log = PARKING_FEE_LOG % (check_in_time.strftime("%H:%M %d/%m/%Y"),
                                           check_out_time.strftime("%H:%M %d/%m/%Y"),
                                           u'theo Block', _vehicle_type_name,
                                           0,
                                           u"(Không có thông tin bảng giá phí gửi %s theo Block)" % _vehicle_type_name)
            return 0, final_log

        if duration <= min_calculation_time:
            total_fee = 0
            # write log
            final_log = PARKING_FEE_LOG % (check_in_time.strftime("%H:%M %d/%m/%Y"),
                                           check_out_time.strftime("%H:%M %d/%m/%Y"),
                                           u'theo Block', _vehicle_type_name,
                                           total_fee,
                                           '\n'.join(logs))

            return total_fee, final_log

        time_in = get_time(check_in_time)
        time_out = get_time(check_out_time)

        if (not is_overnight(check_in_time, check_out_time)) and time_in >= time(17, 0,0,0):
            # inday block fee
            total_fee = in_day_block_fee
        else:
            # normal block fee
            vehicle_block_time = [i for i in parking_block_time if i["vehicle_type"] == vehicle_type]
            if not vehicle_block_time:
                return total_fee, ''

            _in_time = copy_datetime(check_in_time)
            while _in_time < check_out_time:

                # Check night block
                if get_time(_in_time) < time(5,0,0,0):
                    total_fee += fee_night(vehicle_type)
                    _in_time = _in_time.replace(hour=5,minute=0,second=0)

                # Check normal block
                if _in_time > check_out_time:
                    _in_time = check_out_time
                else:
                    # Set _out_time to end of day, if _out_time greater than check_out_time,
                    # set _out_time to check_out_time
                    _out_time = _in_time.replace(hour=0, minute=0, second=0) + timedelta(days=1)
                    if _out_time > check_out_time:
                        _out_time = check_out_time

                    for block in vehicle_block_time:
                        if "used" not in block:
                            block["used"] = False

                        if block["is_only"] and block["used"]:
                            continue

                        if _in_time >= _out_time:
                            break

                        end_time_block = _in_time + timedelta(seconds=block["duration"])
                        if end_time_block > _out_time or block["duration"] == 0:
                            end_time_block = _out_time

                        block_time = (end_time_block - _in_time).total_seconds()
                        total_fee += ceil(block_time / block["time_unit"]) * block["fee"]
                        _in_time = end_time_block
                        block["used"] = True
    return total_fee, ''
### Tinh tien gui xe theo Block
def calculate_parking_fee_by_block(vehicle_type, check_in_time, check_out_time):
    PARKING_FIRST_BLOCK_LOG = u""" + 1 Block đầu: %s (%s/%s phút)"""
    # PARKING_MAX_BLOCK_LOG = u""" + %s Block sau = %s phút >= %s phút (Tổng TG block tối đa) => %s (Phí block tối đa)"""
    PARKING_MAX_BLOCK_LOG = u""" + Luu bai %s phút >= %s phút (Tổng TG block tối đa) => %s (Phí block tối đa)"""
    PARKING_NEXT_BLOCK_LOG = u""" + %s Block sau: %s x %s = %s (%s/%s phút)"""
    PARKING_FEE_LOG = u"""- Vào lúc: %s
- Ra lúc : %s
- Phương thức tính: %s
- Phương tiện: %s
- Tổng cộng: %s
%s
"""
    _vehicle_type_name = 'xe'
    for item in VEHICLE_TYPE:
        if item[0] == vehicle_type:
            _vehicle_type_name = item[1]

    block_fee = BlockFee.objects.filter(parking_fee__vehicle_type_id=vehicle_type)
    if block_fee:
        logs = []
        block_fee = block_fee.values()[0]
        duration = (check_out_time - check_in_time).total_seconds() / 60
        total_fee = 0

        if duration > 0:
            if duration >= block_fee['max_block_duration']:
                total_fee = block_fee['max_block_fee']

                logs.append(
                    PARKING_MAX_BLOCK_LOG % (duration, block_fee['max_block_duration'], block_fee['max_block_fee']))

                final_log = PARKING_FEE_LOG % (check_in_time.strftime("%H:%M %d/%m/%Y"),
                                               check_out_time.strftime("%H:%M %d/%m/%Y"),
                                               u'theo Block', _vehicle_type_name,
                                               total_fee,
                                               '\n'.join(logs))

                return total_fee, final_log

            remain_duration = duration - block_fee['first_block_duration']

            total_fee = block_fee['first_block_fee']
            logs.append(PARKING_FIRST_BLOCK_LOG % (
                block_fee['first_block_fee'], block_fee['first_block_fee'], block_fee['first_block_duration']))

            if remain_duration > 0.0:
                block_num = ceil(fabs(remain_duration) / block_fee['next_block_duration'])
                total_next_block_fee = block_num * block_fee['next_block_fee']
                total_fee += total_next_block_fee
                logs.append(PARKING_NEXT_BLOCK_LOG % (
                    int(block_num), block_num, block_fee['next_block_fee'], total_next_block_fee,
                    block_fee['next_block_fee'], block_fee['next_block_duration']))

        final_log = PARKING_FEE_LOG % (check_in_time.strftime("%H:%M %d/%m/%Y"),
                                       check_out_time.strftime("%H:%M %d/%m/%Y"),
                                       u'theo Block', _vehicle_type_name,
                                       total_fee,
                                       '\n'.join(logs))
        return total_fee, final_log
    else:
        final_log = PARKING_FEE_LOG % (check_in_time.strftime("%H:%M %d/%m/%Y"),
                                       check_out_time.strftime("%H:%M %d/%m/%Y"),
                                       u'theo Block', _vehicle_type_name,
                                       0,
                                       u"(Không có thông tin bảng giá phí gửi %s theo Block)" % _vehicle_type_name)
        return 0, final_log


def calculate_parking_fee(parking_session_id,cardId, vehicle_type, in_time, out_time):  # Tinh tien gui xe vang lai
    vehicle_type_id = VEHICLE_TYPE_ENCODE_DICT[vehicle_type]
    parking_fee = ParkingFee.objects.filter(vehicle_type_id=vehicle_type_id)  # Lay phuong thuc tinh (block/luot)
    card = None
    if cardId:
        card = Card.objects.get(card_id=cardId)
    card_type = 0
    if card:
        card_type = card.card_type
    if parking_fee:
        in_time = in_time.astimezone(timezone(TIME_ZONE))
        out_time = out_time.astimezone(timezone(TIME_ZONE))
        calculation_method = parking_fee[0].calculation_method

        if calculation_method == 'luot':
            return calculate_parking_fee_by_turn(vehicle_type_id, in_time, out_time)
        elif calculation_method == 'block':
            return calculate_parking_fee_by_block_new(vehicle_type_id,card_type, in_time, out_time)
    return 0, ''


def update_renewal_status_info(_vehicle_registration):  ### Ho tro API CHECKIN CHECKOUT TINH TIEN GUI XE VANG LAI
    if _vehicle_registration.status not in [0, 2]:  # Huy, tam ngung
        today = date.today()
        if (not _vehicle_registration.expired_date) or (
                    _vehicle_registration.expired_date and _vehicle_registration.expired_date < today):
            _vehicle_registration.status = 3
            _vehicle_registration.save()


def get_total_remain_duration(vehicle_registration):
    # today = date.today()
    # current_remain_duration = 0
    # if vehicle_registration.expired_date:
    #     current_remain_duration = vehicle_registration.expired_date - today
    #     current_remain_duration = current_remain_duration.days
    #
    # sum_duration = TicketPaymentDetail.objects.filter(vehicle_registration_id=vehicle_registration.id, used=False).aggregate(Sum('duration'))
    # if sum_duration['duration__sum']:
    #     sum_duration = sum_duration['duration__sum']
    # else:
    #     sum_duration = 0
    # return current_remain_duration + sum_duration
    remain_duration = 0

    if vehicle_registration:
        last_pause = PauseResumeHistory.objects.filter(vehicle_registration_id=vehicle_registration.id,
                                                       used=False).order_by('-request_date')
        if last_pause:
            remain_duration = last_pause[0].remain_duration
        elif vehicle_registration.expired_date:
            expired_date = vehicle_registration.expired_date
            start_date = vehicle_registration.start_date
            today = datetime.now().date()
            # if expired_date > today:
            #     if start_date >= today:
            remain_duration = (expired_date - today).days
            # else:
            #     remain_duration = (expired_date - today).days

    return remain_duration

def get_customer_info(card_id):
    pre_checkout_data = {}
    vehicle_registration = VehicleRegistration.objects.filter(card__card_id=card_id)
    if vehicle_registration:
        vehicle_registration = vehicle_registration[0]

        update_renewal_status_info(vehicle_registration)

        customer = vehicle_registration.customer
        pre_checkout_data = {
            "customer_name": customer.customer_name,
            "customer_id": customer.customer_id,
            "customer_type": customer.customer_type.name if customer.customer_type else '',

            "birthday": customer.customer_birthday,
            "phone": customer.customer_phone,
            "mobile": customer.customer_mobile,
            "email": customer.customer_email,
            "apartment": customer.apartment.address if customer.apartment else '',
            "building": customer.building.name if customer.building else '',
            "company": customer.company.name if customer.company else '',

            "order_register_name": customer.order_register_name,
            "order_register_address": customer.order_register_address,
            "order_tax_code": customer.order_tax_code,

            "messaging_sms_phone": customer.messaging_sms_phone,
            "messaging_email": customer.messaging_email,
            "messaging_address": customer.messaging_address,
	    
	    "vehicle_type_from_card": vehicle_type_id,

            "vehicle_registration_info": {
                "status": vehicle_registration.status,
                "enum_status": dict(VEHICLE_STATUS_CHOICE),
                "total_remain_duration": get_total_remain_duration(vehicle_registration),
                "level_fee": vehicle_registration.level_fee.__unicode__() if vehicle_registration else '',

                "registration_date": vehicle_registration.registration_date.astimezone(timezone(TIME_ZONE)).strftime(
                    "%d/%m/%Y %H:%M"),
                "first_renewal_effective_date": vehicle_registration.first_renewal_effective_date if vehicle_registration.first_renewal_effective_date else None,
                "last_renewal_date": vehicle_registration.last_renewal_date if vehicle_registration.last_renewal_date else None,
                "last_renewal_effective_date": vehicle_registration.last_renewal_effective_date if vehicle_registration.last_renewal_effective_date else None,

                "start_date": vehicle_registration.start_date if vehicle_registration.start_date else None,
                "expired_date": vehicle_registration.expired_date if vehicle_registration.expired_date else None,

                "pause_date": vehicle_registration.pause_date if vehicle_registration.pause_date else None,
                "cancel_date": vehicle_registration.cancel_date if vehicle_registration.cancel_date else None,

                "vehicle_driver_name": vehicle_registration.vehicle_driver_name,
                "vehicle_driver_id": vehicle_registration.vehicle_driver_id,
                "vehicle_driver_phone": vehicle_registration.vehicle_driver_phone,

                "vehicle_type": vehicle_registration.vehicle_type.name if vehicle_registration.vehicle_type else '',
                "vehicle_number": vehicle_registration.vehicle_number,
                "vehicle_brand": vehicle_registration.vehicle_brand,
                "vehicle_paint": vehicle_registration.vehicle_paint,
            }
        }

    return pre_checkout_data

# Chi cho phep vao khi dang ky xe con hieu luc
# & So ngay cho phep no ve thang toi da
def is_vehicle_registration_available(card_id):
    pre_checkout_data = {}
    can_check_in_out_when_in_effective_range = int(
        get_setting('can_check_in_out_when_in_effective_range', u'Chỉ được vào ra khi đăng ký xe còn hiệu lực', 1))
    vehicle_registration = VehicleRegistration.objects.filter(card__card_id=card_id)
    if vehicle_registration:
        vehicle_registration = vehicle_registration[0]

        update_renewal_status_info(vehicle_registration)

        customer = vehicle_registration.customer
        pre_checkout_data = {
            "customer_name": customer.customer_name,
            "customer_id": customer.customer_id,
            "customer_type": customer.customer_type.name if customer.customer_type else '',

            "birthday": customer.customer_birthday,
            "phone": customer.customer_phone,
            "mobile": customer.customer_mobile,
            "email": customer.customer_email,
            "apartment": customer.apartment.address if customer.apartment else '',
            "building": customer.building.name if customer.building else '',
            "company": customer.company.name if customer.company else '',

            "order_register_name": customer.order_register_name,
            "order_register_address": customer.order_register_address,
            "order_tax_code": customer.order_tax_code,

            "messaging_sms_phone": customer.messaging_sms_phone,
            "messaging_email": customer.messaging_email,
            "messaging_address": customer.messaging_address,

            "vehicle_registration_info": {
                "status": vehicle_registration.status,
                "enum_status": dict(VEHICLE_STATUS_CHOICE),
                "total_remain_duration": get_total_remain_duration(vehicle_registration),
                "level_fee": vehicle_registration.level_fee.__unicode__() if vehicle_registration else '',

                "registration_date": vehicle_registration.registration_date.astimezone(timezone(TIME_ZONE)).strftime(
                    "%d/%m/%Y %H:%M"),
                "first_renewal_effective_date": vehicle_registration.first_renewal_effective_date if vehicle_registration.first_renewal_effective_date else None,
                "last_renewal_date": vehicle_registration.last_renewal_date if vehicle_registration.last_renewal_date else None,
                "last_renewal_effective_date": vehicle_registration.last_renewal_effective_date if vehicle_registration.last_renewal_effective_date else None,

                "start_date": vehicle_registration.start_date if vehicle_registration.start_date else None,
                "expired_date": vehicle_registration.expired_date if vehicle_registration.expired_date else None,

                "pause_date": vehicle_registration.pause_date if vehicle_registration.pause_date else None,
                "cancel_date": vehicle_registration.cancel_date if vehicle_registration.cancel_date else None,

                "vehicle_driver_name": vehicle_registration.vehicle_driver_name,
                "vehicle_driver_id": vehicle_registration.vehicle_driver_id,
                "vehicle_driver_phone": vehicle_registration.vehicle_driver_phone,

                "vehicle_type": vehicle_registration.vehicle_type.name if vehicle_registration.vehicle_type else '',
                "vehicle_number": vehicle_registration.vehicle_number,
                "vehicle_brand": vehicle_registration.vehicle_brand,
                "vehicle_paint": vehicle_registration.vehicle_paint,
            }
        }

        start_date = vehicle_registration.start_date
        expired_date = vehicle_registration.expired_date

        if start_date and expired_date:
            today = date.today()
            max_vehicle_registration_debt_days = get_setting('max_vehicle_registration_debt_days',
                                                             u'Số ngày nợ vé tháng tối đa', 10)
            new_expired_date = expired_date + timedelta(days=int(max_vehicle_registration_debt_days))
            if start_date <= today <= new_expired_date:
                return True, pre_checkout_data, True
            # elif vehicle_registration.status in [0, 2, 3] or (not vehicle_registration.expired_date):  # Huy tam ngung het han
            else:  # Het no
                if can_check_in_out_when_in_effective_range == 1:
                    return False, pre_checkout_data, True
                else:
                    return True, pre_checkout_data, False
        else:  # The moi dang ky, chua co thong tin gia han
            if can_check_in_out_when_in_effective_range == 1:
                return False, pre_checkout_data, True
            else:
                return True, pre_checkout_data, False
    else:  # Khach vang lai
        return True, pre_checkout_data, False
def callfeeforexception(card_id):
    try:
        card = Card.objects.get(card_id=card_id)
        if not card:
            return 0
        pkss = ParkingSession.objects.filter(check_out_time=None, card=card)
        if not pkss or len(pkss) <= 0:
            return 0
        parking_session = pkss[0]
        to_time = get_now_utc()
        claim_promotion_sessions = ClaimPromotionV2.objects.filter(parking_session_id=parking_session.id, used=False)
        if claim_promotion_sessions and claim_promotion_sessions.count > 0:
            claim_promotion_session = claim_promotion_sessions[0]
            duration = (to_time - claim_promotion_session.server_time).total_seconds()
            if duration <= 1800:
                return 0;
            else:
                parking_fee_result = get_parking_fee_info(card_id,
                                                           parking_session.vehicle_type,
                                                           claim_promotion_session.server_time, to_time)
                return parking_fee_result["parking_fee"]
        else:
            parking_fee_result = get_parking_fee_info(card_id,
                                                       parking_session.vehicle_type,card.card_type,
                                                       parking_session.check_in_time, to_time)
            return parking_fee_result["parking_fee"]
    except Exception as e:
        return 0
def get_parking_fee_or_customer_info(card_id, pid, vehicle_type, check_in_time, check_out_time):
    pre_checkout_data = {}

    parking_fee, parking_fee_detail = calculate_parking_fee(pid,card_id, vehicle_type,card.card_type,
                                                            check_in_time.astimezone(timezone(TIME_ZONE)),
                                                            check_out_time.astimezone(timezone(TIME_ZONE)))

    vehicle_registration = VehicleRegistration.objects.filter(card__card_id=card_id)

    if vehicle_registration:
        vehicle_registration = vehicle_registration[0]
        customer = vehicle_registration.customer

        update_renewal_status_info(vehicle_registration)

        pre_check = is_vehicle_registration_available(card_id)
        if pre_check[0]:
            if pre_check[2]:
                parking_fee = 0
                parking_fee_detail = u""

        pre_checkout_data = {
            "parking_fee": parking_fee,
            "parking_fee_detail": parking_fee_detail,

            "customer_name": customer.customer_name,
            "customer_id": customer.customer_id,
            "customer_type": customer.customer_type.name if customer.customer_type else '',

            "birthday": customer.customer_birthday,
            "phone": customer.customer_phone,
            "mobile": customer.customer_mobile,
            "email": customer.customer_email,
            "apartment": customer.apartment.address if customer.apartment else '',
            "building": customer.building.name if customer.building else '',
            "company": customer.company.name if customer.company else '',

            "order_register_name": customer.order_register_name,
            "order_register_address": customer.order_register_address,
            "order_tax_code": customer.order_tax_code,

            "messaging_sms_phone": customer.messaging_sms_phone,
            "messaging_email": customer.messaging_email,
            "messaging_address": customer.messaging_address,

            "vehicle_registration_info": {
                "status": vehicle_registration.status,
                "enum_status": dict(VEHICLE_STATUS_CHOICE),
                "total_remain_duration": get_total_remain_duration(vehicle_registration),
                "level_fee": vehicle_registration.level_fee.__unicode__() if vehicle_registration else '',

                "registration_date": vehicle_registration.registration_date.astimezone(timezone(TIME_ZONE)).strftime(
                    "%d/%m/%Y %H:%M"),
                "first_renewal_effective_date": vehicle_registration.first_renewal_effective_date if vehicle_registration.first_renewal_effective_date else None,
                "last_renewal_date": vehicle_registration.last_renewal_date if vehicle_registration.last_renewal_date else None,
                "last_renewal_effective_date": vehicle_registration.last_renewal_effective_date if vehicle_registration.last_renewal_effective_date else None,

                "start_date": vehicle_registration.start_date if vehicle_registration.start_date else None,
                "expired_date": vehicle_registration.expired_date if vehicle_registration.expired_date else None,

                "pause_date": vehicle_registration.pause_date if vehicle_registration.pause_date else None,
                "cancel_date": vehicle_registration.cancel_date if vehicle_registration.cancel_date else None,

                "vehicle_driver_name": vehicle_registration.vehicle_driver_name,
                "vehicle_driver_id": vehicle_registration.vehicle_driver_id,
                "vehicle_driver_phone": vehicle_registration.vehicle_driver_phone,

                "vehicle_type": vehicle_registration.vehicle_type.name if vehicle_registration.vehicle_type else '',
                "vehicle_number": vehicle_registration.vehicle_number,
                "vehicle_brand": vehicle_registration.vehicle_brand,
                "vehicle_paint": vehicle_registration.vehicle_paint,
            }
        }
    else:
        pre_checkout_data = {
            "parking_fee": parking_fee,
            "parking_fee_detail": parking_fee_detail
        }
    return pre_checkout_data

# VEHICLE_REGISTRATION_STATUS_CHOICE = (
#     ('status_active', u'Đang dùng'),
#     ('status_expired', u'Chưa đăng ký'),
#     ('status_cancel', u'Hủy đậu xe'),
#     ('status_pause', u'Tạm ngừng'),
# )


### Unit test: Tinh tien vang lai
def new_test_fee(request):
    in_time = get_now_utc().astimezone(timezone(TIME_ZONE))  # Lay gio thuc te test

    # 1000001: xe mays
    # Test luot xe trong ngay
    # print "-=" * 60, "TINH TIEN < 1 NGAY"

    # # print calculate_parking_fee_by_turn(1000001,
    #                               in_time.replace(hour=6, minute=0),
    #                               in_time.replace(hour=6, minute=2))[0] == 0.0
    #
    # # print calculate_parking_fee_by_turn(10000,
    #                               in_time.replace(hour=6, minute=0),
    #                               in_time.replace(hour=6, minute=2))[0] == 0.0
    #
    # # print calculate_parking_fee_by_turn(1000001,
    #                               in_time.replace(hour=6, minute=0),
    #                               in_time.replace(hour=8, minute=0))[0] == 5000.0
    # #
    # # print calculate_parking_fee_by_turn(1000001,
    #                               in_time.replace(hour=18, minute=0),
    #                               in_time.replace(hour=20, minute=0))[0] == 6000.0
    #
    # # xen giua 2 luot
    # # print calculate_parking_fee_by_turn(1000001,
    #                               in_time.replace(hour=16, minute=0),
    #                               in_time.replace(hour=19, minute=0))[0] == 11000.0
    #
    # # print calculate_parking_fee_by_turn(1000001,
    #                               in_time.replace(hour=7, minute=0),
    #                               in_time.replace(hour=20, minute=0))[0] == 11000.0
    #
    # # Test luot xe > 1 ngay
    # # print "=-" * 60, "TINH TIEN > 1 NGAY"
    # # print calculate_parking_fee_by_turn(1000001,
    #                               in_time.replace(hour=7, minute=0),
    #                               in_time.replace(hour=9, minute=0) + datetime.timedelta(1))[0] == 20000.0
    #
    # # print (calculate_parking_fee_by_turn(1000001,
    #                                in_time.replace(hour=7, minute=0),
    #                                in_time.replace(hour=19, minute=30) + datetime.timedelta(1)))[0] == 21000.0
    #
    # # print (calculate_parking_fee_by_turn(1000001,
    #                            in_time.replace(hour=7, minute=0),
    #                            in_time.replace(hour=8, minute=30) + datetime.timedelta(2)))[0] == 35000.0
    # # 1 tuan
    # # print (calculate_parking_fee_by_turn(1000001,
    #                            in_time.replace(hour=20, minute=0),
    #                            in_time.replace(hour=2, minute=0) + datetime.timedelta(weeks=1)))[0] == 110000.0
    # # 1 thang
    # # print (calculate_parking_fee_by_turn(1000001,
    #                            in_time.replace(hour=7, minute=0),
    #                            in_time.replace(hour=9, minute=0) + datetime.timedelta(weeks=4)))[0] == 425000.0
    # # 1 nam
    # # print (calculate_days_turn(1000001,
    # #                            in_time.replace(hour=7, minute=0),
    # #                            in_time.replace(hour=9, minute=0) + datetime.timedelta(weeks=52))) == 3645000.0
    # #
    # ## Test tinh tien theo block
    # # Khong co bang gia block
    # # print calculate_parking_fee_by_block(1000001,
    #                                      in_time.replace(hour=6, minute=0),
    #                                      in_time.replace(hour=8, minute=0))[0] == 0.0
    # # Block toi da
    # # print calculate_parking_fee_by_block(2000101,
    #                                      in_time.replace(hour=6, minute=0),
    #                                      in_time.replace(hour=8, minute=0) + datetime.timedelta(days=1))[0] == 3500000.0
    #
    # #1 block
    # # print calculate_parking_fee_by_block(2000101,
    #                                      in_time.replace(hour=6, minute=0),
    #                                      in_time.replace(hour=8, minute=0))[0] == 30000.0
    #
    # # 2 block
    # # print calculate_parking_fee_by_block(2000101,
    #                       in_time.replace(hour=6, minute=0),
    #                       in_time.replace(hour=8, minute=1))[0] == 70000.0
    #
    # # 12 block
    # # print calculate_parking_fee_by_block(2000101,
    #                                      in_time.replace(hour=6, minute=0),
    #                                      in_time.replace(hour=19, minute=30))[0] == 510000

    # pid
    # parking_session = ParkingSession.objects.get(id=32908)
    # parking_session.vehicle_type = 2000101#oto
    # parking_session.check_out_time = parking_session.check_out_time.replace(hour=9)
    # parking_session.save()
    # print calculate_block_by_pid(parking_session.id) == 50000
    #
    # parking_session.check_out_time = parking_session.check_out_time.replace(hour=11)
    # parking_session.save()
    # print calculate_block_by_pid(parking_session.id) == 90000
    return HttpResponse("Test TurnFee")


class UploadFileForm(forms.Form):
    file = forms.FileField(label='File')


@login_required(redirect_field_name='', login_url='/admin/')
def render_bulk_import_customer(request):
    def log_info(line, object, msg, status, file=None, mode='file'):
        s = u'%s, Dong %s, %s, %s'
        if mode == '# print':
            print "mode print"
            # print s % (line, object, msg, status)
        elif mode == 'file' and file != None:
            file.write(s % (status, line, object, msg) + "\n")

    def str_to_date(s, format="%d/%m/%Y"):
        try:
            return datetime.strptime(s, format)
        except:
            return None

    # Get id khoa ngoai
    def get_customer_type_id(s):
        if len(s) > 0:
            customer_type = CustomerType.objects.filter(Q(name__icontains=s))
            if customer_type:
                return customer_type[0].id
        return None

    # Get id can ho hoac tao moi
    def get_or_create_apartment(address, owner_name, owner_phone, owner_email):
        if len(owner_name) > 0 or len(address) > 0:
            try:
                # Get
                apartment = Apartment.objects.filter(Q(address__icontains=address), Q(owner_name=owner_name),
                                                     Q(owner_phone=owner_phone) | Q(owner_email=owner_email))
                if apartment:
                    return apartment[0].id

                # Or create
                apartment = Apartment(address=address, owner_name=owner_name, owner_phone=owner_phone,
                                      owner_email=owner_email)
                apartment.save()
                return apartment.id
            except:
                return None
        return None

    # Get id toa nha hoac tao moi
    def get_or_create_building(name, address):
        if len(name) > 0 or len(address) > 0:
            try:
                # Get
                building = Building.objects.filter(name=name, address__icontains=address)
                if building:
                    return building[0].id

                # Or create
                building = Building(name=name, address=address)
                building.save()
                return building.id
            except:
                return None

        return None

    # Get id can ho hoac tao moi
    def get_or_create_company(name, address, phone, email, representative_name, representative_phone):
        if len(name) > 0 or len(address) > 0:
            try:
                # Get
                company = Company.objects.filter(Q(name__icontains=name), Q(address__icontains=address)
                                                 | Q(representative_name=representative_name) | Q(
                    representative_phone=representative_phone)
                                                 | Q(phone=phone) | Q(email=email))
                if company:
                    return company[0].id

                # Or create
                company = Company(name=name, address=address, phone=phone, email=email,
                                  representative_name=representative_name, representative_phone=representative_phone)
                company.save()
                return company.id
            except:
                return None

        return None

    def get_card_id(s):  # card_label (or card_id)
        if len(s) > 0:
            try:
                card = Card.objects.filter(Q(card_label=s) | Q(card_id=s))

                if card:
                    return card[0].id
            except:
                return None
        return None

    def get_level_fee_id(s):
        if len(s) > 0:  # Dang: Cu dan - XM1
            try:
                level_fee = LevelFee.objects.filter(name__icontains=s)

                if level_fee:
                    return level_fee[0].id
            except:
                return None
        return None

    def get_vehicle_type_id(s):
        if len(s) > 0:  # Dang: Xe máy
            try:
                vehicle_type = VehicleType.objects.filter(name__icontains=s)

                if vehicle_type:
                    return vehicle_type[0].id
            except:
                return None
        return None

    def handle_uploaded_file(f):
        # Constant
        fields = {
            'STT': 0,
            'Ten_kh': 1,
            'CMND': 2,
            'Ngay_sinh': 3,
            'Loai_cu_dan': 4,
            'DT_nha': 5,
            'DTDD': 6,
            'Email': 7,

            'Canho_ten_chu_ho': 8,
            'Canho_dia_chi': 9,
            'Chuho_dt': 10,
            'Chuho_email': 11,

            'Toanha_ten': 12,
            'Toanha_dia_chi': 13,

            'Cty_ten': 14,
            'Cty_dia_chi': 15,
            'Cty_dt': 16,
            'Cty_email': 17,
            'Cty_ten_nguoi_dai_dien': 18,
            'Cty_dt_nguoi_dai_dien': 19,

            'Hoadon_ten_cty_ca_nhan': 20,
            'Hoadon_mst': 21,
            'Hoadon_dia_chi': 22,

            'Nhacphi_sms': 23,
            'Nhacphi_goi_dien': 24,
            'Nhacphi_email': 25,
            'Nhacphi_thu_can_ho': 26,
            'Nhacphi_thu_gat_nuoc_xe': 27,

            'Xe1_ten_the': 28,
            'Xe1_muc_phi': 29,
            'Xe1_bien_so': 30,
            'Xe1_loai_xe': 31,
            'Xe1_nhan_hieu': 32,
            'Xe1_mau_son': 33,
            'Xe1_ten_lx': 34,
            'Xe1_cmnd_lx': 35,
            'Xe1_sdt_lx': 36,
            'Xe1_ngay_dang_ky_hieu_luc': 37,
            'Xe1_han_hien_tai': 38,

            'Xe2_ten_the': 39,
            'Xe2_muc_phi': 40,
            'Xe2_bien_so': 41,
            'Xe2_loai_xe': 42,
            'Xe2_nhan_hieu': 43,
            'Xe2_mau_son': 44,
            'Xe2_ten_lx': 45,
            'Xe2_cmnd_lx': 46,
            'Xe2_sdt_lx': 47,
            'Xe2_ngay_dang_ky_hieu_luc': 48,
            'Xe2_han_hien_tai': 49,

            'Xe3_ten_the': 50,
            'Xe3_muc_phi': 51,
            'Xe3_bien_so': 52,
            'Xe3_loai_xe': 53,
            'Xe3_nhan_hieu': 54,
            'Xe3_mau_son': 55,
            'Xe3_ten_lx': 56,
            'Xe3_cmnd_lx': 57,
            'Xe3_sdt_lx': 58,
            'Xe3_ngay_dang_ky_hieu_luc': 59,
            'Xe3_han_hien_tai': 60,
        }

        folder_name = 'templates/bulkimport'
        file_name = 'temp_KH.xls'
        file_path = '%s/%s' % (folder_name, file_name)
        log_name = 'log_import_customer.txt'
        log_path = '%s/%s' % (folder_name, log_name)

        start_row = 8  # Vi tri dong bat dau du lieu (zero index)

        import os
        if not os.path.exists(folder_name):
            os.mkdir(folder_name)

        if os.path.isfile(file_path):
            os.remove(file_path)
            # print "Xoa file ton tai truoc do"

        with open(file_path, 'wb+') as destination:  # Luu file Excel local de xu ly
            for chunk in f.chunks():
                destination.write(chunk)

        book = None
        try:
            from xlrd import open_workbook  # Doc file Excel

            book = open_workbook(file_path, on_demand=True)
        except:
            log_info(0, file_name, "Loi dinh dang Excel khong phu hop", "FAIL", None, '')

        import codecs
        f = codecs.open(log_path, encoding='utf-8', mode='w+')
        f.write("-" * 50 + "\n")

        if book:
            sheet0 = book.sheet_by_index(0)
            num_rows = sheet0.nrows

            total_pass = 0

            for i in xrange(start_row, num_rows):  # Duyet tung dong du lieu
                # print "i ", i
                _slice = sheet0.row_slice(i)

                # print
                # print "*"*50
                # print _slice
                if not _slice[fields['STT']].value:
                    continue

                stt = int(_slice[fields['STT']].value)

                customer_name = unicode(_slice[fields['Ten_kh']].value).strip()  # Ten khach hang
                customer_id = unicode(_slice[fields['CMND']].value).strip()  # CMND
                customer_birthday = _slice[fields['Ngay_sinh']]  # Ngay sinh

                if len(customer_name) <= 0 or len(customer_id) < 0:
                    log_info(i + 1, customer_name, "Tao khach hang that bai", "FAIL", f)
                    log_info(i + 1, customer_name, "Ten khach hang/CMND khong phu hop", "FAIL", f)
                    continue
                has_duplicate_customer = Customer.objects.filter(customer_name=customer_name, customer_id=customer_id)

                # Neu khong co 3 thong tin bat buoc, bo qua!
                if has_duplicate_customer:
                    log_info(i + 1, customer_name, "Tao khach hang that bai", "FAIL", f)
                    log_info(i + 1, customer_id, "Ten khach hang + CMND trung", "FAIL", f)
                    continue

                birthday_format = False
                try:  # Dinh dang ngay sinh
                    if isinstance(customer_birthday.value, float):
                        # print "-xldate float"
                        customer_birthdate = xldate_as_tuple(customer_birthday.value, 0)
                        customer_birthday = "%s-%s-%s" % (
                            customer_birthdate[0], customer_birthdate[1], customer_birthdate[2])
                        # print "1. birthday ", customer_birthday
                        birthday_format = True
                    elif isinstance(customer_birthday.value, unicode):
                        # print "-Unicode date str"
                        customer_birthdate = datetime.strptime(str(customer_birthday.value), "%d/%m/%Y")
                        customer_birthday = customer_birthdate.strftime("%Y-%m-%d")
                        birthday_format = True

                    # print "> Final birthday: ", customer_birthday
                except:
                    log_info(i + 1, customer_name, "Tao khach hang that bai", "FAIL", f)
                    log_info(i + 1, customer_birthday, u"Loi dinh dang ngay sinh (dd/mm/YYYY)", "FAIL", f)
                    continue

                # if customer_birthday len(customer_birthday) <= 0:
                #     log_info(i + 1, customer_name, "Tao khach hang that bai", "FAIL", f)
                #     log_info(i + 1, customer_birthday, "Ngay sinh khong phu hop", "FAIL", f)
                #     continue

                customer_phone = unicode(_slice[fields['DT_nha']].value).strip()  # DT nha
                customer_mobile = unicode(_slice[fields['DTDD']].value).strip()  # DTDD
                customer_email = unicode(_slice[fields['Email']].value).strip()  # Email

                customer_type = unicode(_slice[fields['Loai_cu_dan']].value).strip()  # Loai cu dan
                customer_type = get_customer_type_id(customer_type)

                # Can ho
                apartment_owner_name = unicode(_slice[fields['Canho_ten_chu_ho']].value).strip()
                apartment_owner_phone = unicode(_slice[fields['Chuho_dt']].value).strip()
                apartment_owner_email = unicode(_slice[fields['Chuho_email']].value).strip()
                apartment_address = unicode(_slice[fields['Canho_dia_chi']].value).strip()

                apartment = get_or_create_apartment(apartment_address, apartment_owner_name, apartment_owner_phone,
                                                    apartment_owner_email)

                # Toa nha
                building_name = unicode(_slice[fields['Toanha_ten']].value).strip()
                building_address = unicode(_slice[fields['Toanha_dia_chi']].value).strip()

                building = get_or_create_building(building_name, building_address)

                # Cong ty
                company_name = unicode(_slice[fields['Cty_ten']].value).strip()
                company_address = unicode(_slice[fields['Cty_dia_chi']].value).strip()
                company_phone = unicode(_slice[fields['Cty_dt']].value).strip()
                company_email = unicode(_slice[fields['Cty_email']].value).strip()
                company_representative_name = unicode(_slice[fields['Cty_ten_nguoi_dai_dien']].value).strip()
                company_representative_phone = unicode(_slice[fields['Cty_dt_nguoi_dai_dien']].value).strip()

                company = get_or_create_company(company_name, company_address, company_phone, company_email,
                                                company_representative_name, company_representative_phone)

                # Hoa don
                order_register_name = unicode(_slice[fields['Hoadon_ten_cty_ca_nhan']].value).strip()
                order_register_address = unicode(_slice[fields['Hoadon_dia_chi']].value).strip()
                order_tax_code = unicode(_slice[fields['Hoadon_mst']].value).strip()

                # Nhac phi
                messaging_sms_phone = unicode(_slice[fields['Nhacphi_sms']].value).strip()
                messaging_phone = unicode(_slice[fields['Nhacphi_goi_dien']].value).strip()
                messaging_email = unicode(_slice[fields['Nhacphi_email']].value).strip()
                messaging_address = unicode(_slice[fields['Nhacphi_thu_can_ho']].value).strip()
                messaging_via_wiper_mail = unicode(_slice[fields['Nhacphi_thu_gat_nuoc_xe']].value).strip()

                # Phai tao khach hang truoc
                customer = Customer(customer_name=customer_name, customer_id=customer_id,
                                    customer_phone=customer_phone, customer_mobile=customer_mobile,
                                    customer_email=customer_email)

                if birthday_format and customer_birthday:
                    customer.customer_birthday=customer_birthday

                staff = None
                try:
                    staff = UserProfile.objects.get(user_id=request.user.id)
                    if staff:  # Nhan vien dang ky
                        customer.staff = staff
                except:
                    pass

                if customer_type:  # Loai cu dan
                    customer.customer_type_id = customer_type

                if building:  # Toa nha
                    customer.building_id = building

                if company:  # Cong ty
                    customer.company_id = company

                if apartment:  # Can ho
                    customer.apartment_id = apartment

                if len(order_register_name) > 0:  # Hoa don
                    customer.order_register_name = order_register_name

                if len(order_register_address) > 0:
                    customer.order_register_address = order_register_address

                if len(order_tax_code) > 0:
                    customer.order_tax_code = order_tax_code

                if len(messaging_sms_phone) > 0:  # Nhac phi
                    customer.messaging_via_sms = True
                    customer.messaging_sms_phone = messaging_sms_phone

                if len(messaging_phone) > 0:
                    customer.messaging_via_phone = True
                    customer.messaging_phone = messaging_phone

                if len(messaging_email) > 0:
                    customer.messaging_via_email = True
                    customer.messaging_email = messaging_email

                if len(messaging_address) > 0:
                    customer.messaging_via_apart_mail = True
                    customer.messaging_address = messaging_address

                if len(messaging_via_wiper_mail) > 0:
                    customer.messaging_via_wiper_mail = True

                customer.save()

                log_info(i + 1, customer_name, "Tao khach hang thanh cong", "OK", f)
                total_pass += 1

                customer_id = customer.id

                # Dang ky xe 1
                vr1_card_label = unicode(
                    _slice[fields['Xe1_ten_the']].value).strip()  # Card label hoac card id (khoa ngoai)
                vr1_level_fee = unicode(_slice[fields['Xe1_muc_phi']].value).strip()  # Muc phi (khoa ngoai)
                vr1_vehicle_number = unicode(_slice[fields['Xe1_bien_so']].value).strip()
                vr1_vehicle_type = unicode(_slice[fields['Xe1_loai_xe']].value).strip()
                vr1_vehicle_brand = unicode(_slice[fields['Xe1_nhan_hieu']].value).strip()
                vr1_vehicle_paint = unicode(_slice[fields['Xe1_mau_son']].value).strip()
                vr1_vehicle_driver_name = unicode(_slice[fields['Xe1_ten_lx']].value).strip()
                vr1_vehicle_driver_id = unicode(_slice[fields['Xe1_cmnd_lx']].value).strip()
                vr1_vehicle_driver_phone = unicode(_slice[fields['Xe1_sdt_lx']].value).strip()
                vr1_first_renewal_effective_date = unicode(_slice[fields['Xe1_ngay_dang_ky_hieu_luc']].value).strip()
                vr1_expired_date = unicode(_slice[fields['Xe1_han_hien_tai']].value).strip()

                if customer_id and len(vr1_vehicle_number) > 0 and len(vr1_vehicle_type) and len(
                        vr1_vehicle_driver_name) > 0:
                    some_vehicle_reg = VehicleRegistration.objects.filter(vehicle_number=vr1_vehicle_number)
                    if some_vehicle_reg:
                        log_info(i + 1, vr1_vehicle_number, "Bien so trung lap trong he thong", "WARNING", f)

                    vr1_card_id = get_card_id(vr1_card_label)
                    vr1_level_fee_id = get_level_fee_id(vr1_level_fee)
                    vr1_vehicle_type_id = get_vehicle_type_id(vr1_vehicle_type)

                    if vr1_vehicle_type_id:
                        vr1 = VehicleRegistration(customer_id=customer_id, vehicle_number=vr1_vehicle_number,
                                                  vehicle_type_id=vr1_vehicle_type_id,
                                                  vehicle_driver_name=vr1_vehicle_driver_name,
                                                  vehicle_brand=vr1_vehicle_brand, vehicle_paint=vr1_vehicle_paint,
                                                  vehicle_driver_id=vr1_vehicle_driver_id,
                                                  vehicle_driver_phone=vr1_vehicle_driver_phone)

                        if staff:  # Nhan vien dang ky
                            vr1.staff = staff
                        if vr1_card_id:  # The
                            vr1.card_id = vr1_card_id
                        if vr1_level_fee_id:  # Muc phi
                            vr1.level_fee_id = vr1_level_fee_id

                        if vr1_first_renewal_effective_date:
                            converted_vr1_first_renewal_effective_date = str_to_date(vr1_first_renewal_effective_date)
                            if converted_vr1_first_renewal_effective_date:
                                vr1.first_renewal_effective_date = converted_vr1_first_renewal_effective_date
                                vr1.last_renewal_effective_date = converted_vr1_first_renewal_effective_date
                                vr1.start_date = converted_vr1_first_renewal_effective_date
                                vr1.status = 1
                            else:
                                log_info(i + 1, customer_name, "Ngay dang ky hieu luc %s xe 1 khong hop le" % vr1_first_renewal_effective_date, "FAIL", f)

                        if vr1_expired_date:
                            converted_vr1_expired_date = str_to_date(vr1_expired_date)
                            if converted_vr1_expired_date:
                                vr1.expired_date = converted_vr1_expired_date
                            else:
                                log_info(i + 1, customer_name, "Ngay het han %s xe 2 khong hop le" % vr1_expired_date, "FAIL", f)

                        vr1.save()
                        log_info(i + 1, customer_name, "Xe %s dang ky thanh cong" % vr1_vehicle_number, "OK", f)


                # Dang ky xe 2
                vr2_card_label = unicode(
                    _slice[fields['Xe2_ten_the']].value).strip()  # Card label hoac card id (khoa ngoai)
                vr2_level_fee = unicode(_slice[fields['Xe2_muc_phi']].value).strip()  # Muc phi (khoa ngoai)
                vr2_vehicle_number = unicode(_slice[fields['Xe2_bien_so']].value).strip()
                vr2_vehicle_type = unicode(_slice[fields['Xe2_loai_xe']].value).strip()
                vr2_vehicle_brand = unicode(_slice[fields['Xe2_nhan_hieu']].value).strip()
                vr2_vehicle_paint = unicode(_slice[fields['Xe2_mau_son']].value).strip()
                vr2_vehicle_driver_name = unicode(_slice[fields['Xe2_ten_lx']].value).strip()
                vr2_vehicle_driver_id = unicode(_slice[fields['Xe2_cmnd_lx']].value).strip()
                vr2_vehicle_driver_phone = unicode(_slice[fields['Xe2_sdt_lx']].value).strip()
                vr2_first_renewal_effective_date = unicode(_slice[fields['Xe2_ngay_dang_ky_hieu_luc']].value).strip()
                vr2_expired_date = unicode(_slice[fields['Xe2_han_hien_tai']].value).strip()

                if customer_id and len(vr2_vehicle_number) > 0 and len(vr2_vehicle_type) and len(
                        vr2_vehicle_driver_name) > 0:
                    some_vehicle_reg = VehicleRegistration.objects.filter(vehicle_number=vr2_vehicle_number)
                    if some_vehicle_reg:
                        log_info(i + 1, vr2_vehicle_number, "Bien so trung lap trong he thong", "WARNING", f)

                    vr2_card_id = get_card_id(vr2_card_label)
                    vr2_level_fee_id = get_level_fee_id(vr2_level_fee)
                    vr2_vehicle_type_id = get_vehicle_type_id(vr2_vehicle_type)

                    if vr2_vehicle_type_id:
                        vr2 = VehicleRegistration(customer_id=customer_id, vehicle_number=vr2_vehicle_number,
                                                  vehicle_type_id=vr2_vehicle_type_id,
                                                  vehicle_driver_name=vr2_vehicle_driver_name,
                                                  vehicle_brand=vr2_vehicle_brand, vehicle_paint=vr2_vehicle_paint,
                                                  vehicle_driver_id=vr2_vehicle_driver_id,
                                                  vehicle_driver_phone=vr2_vehicle_driver_phone)

                        if staff:  # Nhan vien dang ky
                            vr2.staff = staff
                        if vr2_card_id:  # The
                            vr2.card_id = vr2_card_id
                        if vr2_level_fee_id:  # Muc phi
                            vr2.level_fee_id = vr2_level_fee_id
                        if vr2_first_renewal_effective_date:
                            converted_vr2_first_renewal_effective_date = str_to_date(vr2_first_renewal_effective_date)
                            if converted_vr2_first_renewal_effective_date:
                                vr2.first_renewal_effective_date = converted_vr2_first_renewal_effective_date
                                vr2.last_renewal_effective_date = converted_vr2_first_renewal_effective_date
                                vr2.start_date = converted_vr2_first_renewal_effective_date
                                vr2.status = 1
                            else:
                                log_info(i + 1, customer_name, "Ngay dang ky hieu luc %s xe 2 khong hop le" % vr2_first_renewal_effective_date, "FAIL", f)
                        if vr2_expired_date:
                            converted_vr2_expired_date = str_to_date(vr2_expired_date)
                            if converted_vr2_expired_date:
                                vr2.expired_date = converted_vr2_expired_date
                            else:
                                log_info(i + 1, customer_name, "Ngay het han %s xe 2 khong hop le" % vr2_expired_date, "FAIL", f)

                        vr2.save()
                        log_info(i + 1, customer_name, "Xe %s dang ky thanh cong" % vr2_vehicle_number, "OK", f)


                # Dang ky xe 3
                vr3_card_label = unicode(
                    _slice[fields['Xe3_ten_the']].value).strip()  # Card label hoac card id (khoa ngoai)
                vr3_level_fee = unicode(_slice[fields['Xe3_muc_phi']].value).strip()  # Muc phi (khoa ngoai)
                vr3_vehicle_number = unicode(_slice[fields['Xe3_bien_so']].value).strip()
                vr3_vehicle_type = unicode(_slice[fields['Xe3_loai_xe']].value).strip()
                vr3_vehicle_brand = unicode(_slice[fields['Xe3_nhan_hieu']].value).strip()
                vr3_vehicle_paint = unicode(_slice[fields['Xe3_mau_son']].value).strip()
                vr3_vehicle_driver_name = unicode(_slice[fields['Xe3_ten_lx']].value).strip()
                vr3_vehicle_driver_id = unicode(_slice[fields['Xe3_cmnd_lx']].value).strip()
                vr3_vehicle_driver_phone = unicode(_slice[fields['Xe3_sdt_lx']].value).strip()
                vr3_first_renewal_effective_date = unicode(_slice[fields['Xe3_ngay_dang_ky_hieu_luc']].value).strip()
                vr3_expired_date = unicode(_slice[fields['Xe3_han_hien_tai']].value).strip()

                if customer_id and len(vr3_vehicle_number) > 0 and len(vr3_vehicle_type) and len(
                        vr3_vehicle_driver_name) > 0:
                    some_vehicle_reg = VehicleRegistration.objects.filter(vehicle_number=vr3_vehicle_number)
                    if some_vehicle_reg:
                        log_info(i + 1, vr3_vehicle_number, "Bien so trung lap trong he thong", "WARNING", f)

                    vr3_card_id = get_card_id(vr3_card_label)
                    vr3_level_fee_id = get_level_fee_id(vr3_level_fee)
                    vr3_vehicle_type_id = get_vehicle_type_id(vr3_vehicle_type)

                    if vr3_vehicle_type_id:
                        vr3 = VehicleRegistration(customer_id=customer_id, vehicle_number=vr3_vehicle_number,
                                                  vehicle_type_id=vr3_vehicle_type_id,
                                                  vehicle_driver_name=vr3_vehicle_driver_name,
                                                  vehicle_brand=vr3_vehicle_brand, vehicle_paint=vr3_vehicle_paint,
                                                  vehicle_driver_id=vr3_vehicle_driver_id,
                                                  vehicle_driver_phone=vr3_vehicle_driver_phone)

                        if staff:  # Nhan vien dang ky
                            vr3.staff = staff
                        if vr3_card_id:  # The
                            vr3.card_id = vr3_card_id
                        if vr3_level_fee_id:  # Muc phi
                            vr3.level_fee_id = vr3_level_fee_id
                        if vr3_first_renewal_effective_date:
                            converted_vr3_first_renewal_effective_date = str_to_date(vr3_first_renewal_effective_date)
                            if converted_vr3_first_renewal_effective_date:
                                vr3.first_renewal_effective_date = converted_vr3_first_renewal_effective_date
                                vr3.last_renewal_effective_date = converted_vr3_first_renewal_effective_date
                                vr3.start_date = converted_vr3_first_renewal_effective_date
                                vr3.status = 1
                            else:
                                log_info(i + 1, customer_name, "Ngay dang ky hieu luc %s xe 3 khong hop le" % vr3_first_renewal_effective_date, "FAIL", f)
                        if vr3_expired_date:
                            converted_vr3_expired_date = str_to_date(vr3_expired_date)
                            if converted_vr3_expired_date:
                                vr3.expired_date = converted_vr3_expired_date
                            else:
                                log_info(i + 1, customer_name, "Ngay het han %s xe 3 khong hop le" % vr3_expired_date, "FAIL", f)
                        vr3.save()
                        log_info(i + 1, customer_name, "Xe %s dang ky thanh cong" % vr3_vehicle_number, "OK", f)

            f.seek(0)
            f.write("%s/%s OK\n" % (total_pass, len(range(start_row, num_rows))))  # Ti le thanh cong
            f.close()

    if 'btn_TEMPLATE' in request.POST:
        # print "Tai mau Excel"
        folder_name1 = 'templates/bulkimport'
        file_name1 = 'GPMS_Mau_Nhap_KH.xls'
        file_path1 = '%s/%s' % (folder_name1, file_name1)

        try:
            from xlrd import open_workbook
            rb = open_workbook(file_path1, formatting_info=True, on_demand=True)

            from xlutils.copy import copy
            wb = copy(rb)

            sheet1 = wb.get_sheet(0)
            sheet1.insert_bitmap('parking/static/image/logo_report.bmp', 0, 0, 0)

            sheet2 = None
            if rb.nsheets == 1:
                sheet2 = wb.add_sheet('THAM_KHAO')
            elif rb.nsheets >= 2:
                sheet2 = wb.get_sheet(1)

            customer_type_names = CustomerType.objects.all().values_list('name')  # Loai cu dan
            level_fee_names = LevelFee.objects.all().values_list('name')  # Ten muc phi
            vehicle_type_names = VehicleType.objects.all().values_list('name')  # Loai xe

            for i in xrange(len(customer_type_names)):
                sheet2.write(i, 0, customer_type_names[i][0])

            for j in xrange(len(level_fee_names)):
                sheet2.write(j, 1, level_fee_names[j][0])

            for k in xrange(len(vehicle_type_names)):
                sheet2.write(k, 2, vehicle_type_names[k][0])

            wb.save(file_path1)

            with open(file_path1, 'rb') as f:
                response = HttpResponse(f.read(), content_type="application/vnd.ms-excel")
                response['Content-Disposition'] = "attachment; filename=%s" % file_name1
                return response
        except:
            form = UploadFileForm()
            return render(request, 'admin/customer-bulk-import.html', {'form': form})

    elif 'btn_IMPORT' in request.POST:  # Receive request
        form = UploadFileForm(request.POST, request.FILES)
        if form.is_valid():
            # print "> Receive file"
            handle_uploaded_file(request.FILES['file'])
            # print ">> Done file handle"

            folder_name = 'templates/bulkimport'
            log_name = 'log_import_customer.txt'
            log_path = '%s/%s' % (folder_name, log_name)

            from os.path import isfile
            if isfile(log_path):
                with open(log_path, 'rb') as f1:
                    response1 = HttpResponse(f1.read(), content_type="content_type='text/plain")
                    response1['Content-Disposition'] = "attachment; filename=%s" % log_name
                    return response1
                    # return HttpResponseRedirect('')
        else:
            form = UploadFileForm()
            return render(request, 'admin/customer-bulk-import.html', {'form': form})
    else:
        form = UploadFileForm()
        return render(request, 'admin/customer-bulk-import.html', {'form': form})


@login_required(redirect_field_name='', login_url='/admin/')
def render_setting_upload(request, key=''):
    def handle_uploaded_file(f, key=''):
        import os
        if key == 'invoice_pdf_form':
            folder_name = 'templates/pdf-form'
            file_name = 'invoice-form1.pdf'
            file_path = '%s/%s' % (folder_name, file_name)

            if not os.path.exists(folder_name):
                os.mkdir(folder_name)

            if os.path.isfile(file_path):
                os.remove(file_path)

            with open(file_path, 'wb+') as destination:  # Luu file Excel local de xu ly
                for chunk in f.chunks():
                    destination.write(chunk)

            try:
                setting_item = ParkingSetting.objects.get(key='invoice_pdf_form')
                if setting_item:
                    # setting_item.value = file_path
                    setting_item.value = file_name
                    setting_item.save()
            except:
                get_setting('invoice_pdf_form', u'Mẫu phiếu thu PDF', 'invoice-form.pdf')
                messages.error(request, u"Cập nhật thất bại, phục hồi về mặc định!", fail_silently=True)
                return HttpResponseRedirect('')
        elif key == 'logo_report':
            folder_name = 'parking/static/image'
            file_name = 'logo_report_new.png'
            file_path = '%s/%s' % (folder_name, file_name)

            if not os.path.exists(folder_name):
                os.mkdir(folder_name)

            if os.path.isfile(file_path):
                os.remove(file_path)

            with open(file_path, 'wb+') as destination:
                for chunk in f.chunks():
                    destination.write(chunk)
            try:
                setting_item = ParkingSetting.objects.get(key='logo_report')
                if setting_item:
                    setting_item.value = file_name
                    setting_item.save()
            except:
                get_setting('logo_report', u'Mẫu logo dùng trong báo cáo', 'logo_report.png')
                messages.error(request, u"Cập nhật thất bại, phục hồi về mặc định!", fail_silently=True)
                return HttpResponseRedirect('')

    if 'btn_IMPORT' in request.POST:  # Receive request
        form = UploadFileForm(request.POST, request.FILES)
        if form.is_valid():
            handle_uploaded_file(request.FILES['file'], key)

            folder_name = 'templates/pdf-form'
            log_name = 'log_pdf_form.txt'
            log_path = '%s/%s' % (folder_name, log_name)

            from os.path import isfile
            if isfile(log_path):
                with open(log_path, 'rb') as f1:
                    response1 = HttpResponse(f1.read(), content_type="content_type='text/plain")
                    response1['Content-Disposition'] = "attachment; filename=%s" % log_name
                    return response1

            messages.success(request, u"Cập nhật thành công. Hãy kiểm tra thay đổi!",
                             fail_silently=True)
            return HttpResponse('<script>window.opener.location.href=window.opener.location.href; window.close()</script>')
        else:
            form = UploadFileForm()
            return render(request, 'admin/setting-upload.html', {'form': form, 'key': key})
    else:
        form = UploadFileForm()
        return render(request, 'admin/setting-upload.html', {'form': form, 'key': key})


@login_required(redirect_field_name='', login_url='/admin/')
def pdf_ticket_payment(request, ticket_payment_id, print_for_company=0):
    try:
        request_path = request.path

        payment = None
        valid_from = None
        valid_to = None
        detail = ''

        if print_for_company:
            print_for_company = True if int(print_for_company) == 1 else False
        else:
            print_for_company = False

        if request_path.find('ticket-payment') != -1:
            payment = TicketPayment.objects.get(id=ticket_payment_id)

            detail = u'Thu phí gửi xe:'
            valid_from = None
            valid_to = None

            new_detail = {'OT': [], 'XM': []}

            payment_details = TicketPaymentDetail.objects.filter(ticket_payment=payment)
            if print_for_company:
                count_oto = payment_details.filter(payment_detail_fee__gt=0,
                                                   vehicle_registration__vehicle_type__name=u'Ô tô').count()
                count_xemay = payment_details.filter(payment_detail_fee__gt=0).count() - count_oto
                detail = u"OT: {0} xe\nXM: {1} xe".format(count_oto, count_xemay)
            else:
                for payment_detail in payment_details:
                    if valid_from is None and payment_detail.effective_date and payment_detail.expired_date:
                        valid_from = payment_detail.effective_date
                        valid_to = payment_detail.expired_date
                    if payment_detail.vehicle_registration:
                        vehicle_registration = payment_detail.vehicle_registration
                        if vehicle_registration.vehicle_type and payment_detail.payment_detail_fee > 0:
                            new_vehicle_number = payment_detail.vehicle_number or ''
                            month_duration = payment_detail.duration / 30
                            day_duration = payment_detail.day_duration

                            if vehicle_registration.vehicle_type.name == u'Ô tô':
                                new_detail['OT'].append(u"{0} - {1} tháng {2} ngày".format(new_vehicle_number, month_duration, day_duration))
                            else:
                                new_detail['XM'].append(
                                    u"{0} - {1} tháng {2} ngày".format(new_vehicle_number, month_duration, day_duration))

                            detail = u"OT: {0}\nXM: {1}".format(u"; ".join(new_detail['OT']),
                                                                u"; ".join(new_detail['XM']))
                            # detail += u' %s/%d,' % (payment_detail.vehicle_registration.vehicle_number, payment_detail.duration / 30)

        elif request_path.find('deposit-payment') != -1:
            payment = DepositPayment.objects.get(id=ticket_payment_id)

            detail = u'Thu phí cọc thẻ:'

            for payment_detail in DepositPaymentDetail.objects.filter(deposit_payment=payment):
                # if valid_from is None and payment_detail.effective_date and payment_detail.expired_date:
                #     valid_from = payment_detail.effective_date
                #     valid_to = payment_detail.expired_date
                if payment_detail.vehicle_registration and payment_detail.vehicle_registration.vehicle_type and payment_detail.deposit_payment_detail_fee > 0:
                    detail += u' %s/%s,' % (payment_detail.vehicle_number or '',
                                            payment_detail.deposit_action_fee.name if payment_detail.deposit_action_fee else '')

        payment_date = payment.payment_date
        # serial = str(payment.pk)
        print_receipt_number = get_setting('print_receipt_number', u'In số phiếu thu', 1)
        serial = ''
        if int(print_receipt_number) == 1:
            serial = str(payment.receipt_number)

        customer = payment.customer
        customer_name = customer.customer_name if customer else ""
        address = []

        if customer:
            if customer.building:
                address.append(u"Toà nhà %s" % customer.building.name)
            if customer.apartment:
                address.append(u"Căn hộ %s" % customer.apartment.address)
            if customer.company:
                address.append(u"Công ty %s" % customer.company.name)

        address = u" - ".join(address)

        amount = u'%s đồng' % int_format(payment.payment_fee)
        amount_text = u'%s đồng' % number_to_text(payment.payment_fee)

        valid_from = valid_from.strftime('%d/%m/%Y') if valid_from else ''
        valid_to = valid_to.strftime('%d/%m/%Y') if valid_to else ''

        # invoice_pdf_form_path = get_setting('invoice_pdf_form', u'Mẫu hóa đơn PDF', 'templates/pdf-form/invoice-form.pdf')
        folder_name = 'templates/pdf-form'
        invoice_pdf_form_path = "%s/%s" % (
            folder_name, get_setting('invoice_pdf_form', u'Mẫu phiếu thu PDF', 'invoice-form.pdf'))

        # 'invoice-form.pdf'
        rs = form_fill(invoice_pdf_form_path, [
            ('Date', payment_date.strftime('%d/%m/%Y')),
            ('Serial', serial),
            ('CustomerName', customer_name),
            ('Address', address),
            # ('Detail', detail[:-1]),
            ('Detail', detail),
            ('Amount', amount),
            ('AmountText', amount_text),
            ('ValidFrom', valid_from),
            ('ValidTo', valid_to),
        ], strip_sign=False)
        response = HttpResponse(rs, content_type='application/pdf')
        return response
    except TicketPayment.DoesNotExist:
        raise Http404()


@login_required(redirect_field_name='', login_url='/admin/')
def render_receipt_action_form(request, type, payment_id):  # Huy/tao moi ma phieu thu
    class ReceiptForm(ModelForm):
        class Meta:
            model = Receipt
            fields = ['notes']
            widgets = {
                'notes': Textarea(attrs={'cols': 70, 'rows': 5}),
            }

    if 'btn_OK' in request.POST:
        # Huy phieu thu nay
        payment = None
        payment_id = int(payment_id)
        type = int(type)

        if type == 0:
            payment = TicketPayment.objects.filter(id=payment_id)
        elif type == 1:
            payment = DepositPayment.objects.filter(id=payment_id)

        if payment:
            payment = payment[0]
            receipt_number = payment.receipt_number
            receipt = Receipt.objects.filter(type=type, ref_id=payment_id, receipt_number=receipt_number, cancel=False)

            if receipt:
                receipt = receipt[0]
                receipt.cancel = True
                receipt.notes = request.POST['notes']
                receipt.save()

                payment.receipt_number = None

            if u'add_new_receipt' in request.POST:
                new_receipt_number = 0
                setting_item = ParkingSetting.objects.filter(key='next_receipt_number')
                if not setting_item:
                    new_receipt_number = int(get_setting('next_receipt_number', u'Số phiếu thu tiếp theo', 1))
                else:
                    setting_item = setting_item[0]
                    new_receipt_number = int(setting_item.value)

                setting_item.value = unicode(new_receipt_number + 1)
                setting_item.save()

                new_receipt = Receipt(receipt_number=new_receipt_number, type=type, ref_id=payment_id)
                new_receipt.save()

                payment.receipt_number = new_receipt_number
            payment.save()

        return HttpResponse(
            '<script type="text/javascript">var origin = window.opener.parent.location.href; window.opener.parent.location.href = origin; window.close();</script>')
    else:
        receipt_form = ReceiptForm()
        return render(request, 'admin/confirmReceiptAction.html',
                      {'form': receipt_form, 'type': type, 'payment_id': payment_id})

@login_required(redirect_field_name='', login_url='/admin/')
def render_customer_log(request, customer_id):
    def get_or_query_apartment_info(apartment_id, apartment_tracker_dict):
        if apartment_id in apartment_tracker_dict:
            return apartment_tracker_dict[apartment_id]
        else:
            try:
                apartment = Apartment.objects.get(id=apartment_id)
                apartment_tracker_dict[apartment_id] = {'apartment_address': apartment.address,
                                                        'apartment_owner_name': apartment.owner_name,
                                                        'apartment_owner_phone': apartment.owner_phone,
                                                        'apartment_owner_email': apartment.owner_email}
                return apartment_tracker_dict[apartment_id]
            except:
                return None

    if customer_id:
        vrs = VehicleRegistration.objects.filter(customer_id=customer_id)
        vehicle_registrations = VehicleRegistration.audit_log.filter(customer_id=customer_id).order_by('id',
                                                                                                       'action_date')
        customer_name = ''
        customer_building = ''
        customer_apartment = ''
        customer_company = ''
        customer_vehicle_registration_log_data = list()

        if vrs and vehicle_registrations:
            vr = vrs[0]
            customer_name = vr.customer.customer_name if vr.customer.customer_name else ''
            customer_building = vr.customer.building if vr.customer.building else ''
            customer_apartment = vr.customer.apartment if vr.customer.apartment else ''
            customer_company = vr.customer.company if vr.customer.company else ''

            track_vehicle_registration_changes = {}

            for log in vehicle_registrations:
                action_type = log.action_type
                current_card_label = log.card.card_label if log.card else ''
                current_vehicle_number = log.vehicle_number
                current_vehicle_registration_id = log.id

                action_content = []
                status_vehicle_number = "black"
                status_card_label = "black"
                status_action = "gray"

                if action_type == 'I':
                    action_type = 'Tạo mới'
                    status_action = status_vehicle_number = status_card_label = "green"
                    if current_vehicle_registration_id not in track_vehicle_registration_changes:
                        track_vehicle_registration_changes[current_vehicle_registration_id] = {}
                    track_vehicle_registration_changes[current_vehicle_registration_id][
                        'vehicle_number'] = current_vehicle_number
                    track_vehicle_registration_changes[current_vehicle_registration_id][
                        'card_label'] = current_card_label
                elif action_type == 'U':
                    action_type = 'Cập nhật'
                    status_action = "blue"
                    if current_vehicle_registration_id in track_vehicle_registration_changes:
                        last_track = track_vehicle_registration_changes[current_vehicle_registration_id]
                        if current_vehicle_number != last_track['vehicle_number']:
                            action_content.append(u"Đổi biển số")
                            status_vehicle_number = "blue"
                            last_track['vehicle_number'] = current_vehicle_number
                        if current_card_label != last_track['card_label']:
                            action_content.append(u"Đổi mã thẻ")
                            status_card_label = "blue"
                            last_track['card_label'] = current_card_label

                elif action_type == 'D':
                    action_type = 'Xoá'
                    status_action = status_card_label = status_vehicle_number = "gray"
                    if current_vehicle_registration_id in track_vehicle_registration_changes:
                        del track_vehicle_registration_changes[current_vehicle_registration_id]

                temp_data = {'card_label': current_card_label,
                             'vehicle_registration_id': log.id, 'vehicle_number': current_vehicle_number,
                             'vehicle_type': log.vehicle_type,
                             'action_date': log.action_date.strftime("%d/%m/%Y %H:%M"), 'action_type': action_type,
                             'action_content': ". ".join(action_content), "status_action": status_action,
                             'status_vehicle_number': status_vehicle_number, 'status_card_label': status_card_label}
                customer_vehicle_registration_log_data.append(temp_data)

        apartments = Customer.audit_log.filter(id=customer_id).order_by('id', 'action_date')
        customer_apartment_log_data = list()
        last_apartment_id = -1
        track_apartment = {}

        if apartments:
            for log in apartments:
                action_type = log.action_type
                current_apartment_id = log.apartment_id

                action_content = []
                status_apartment = "black"
                status_action = "gray"

                if action_type == 'U':
                    action_type = 'Cập nhật'
                    status_action = status_apartment = "blue"
                    if current_apartment_id != last_apartment_id:
                        if not current_apartment_id:
                            action_content.append(u"Rời căn hộ")
                        else:
                            action_content.append(u" Đổi căn hộ")
                        last_apartment_id = current_apartment_id
                    get_or_query_apartment_info(current_apartment_id, track_apartment)

                    temp_data = {'apartment_id': current_apartment_id if current_apartment_id else '',
                                 'apartment_address': track_apartment[current_apartment_id]['apartment_address'] if current_apartment_id and track_apartment[current_apartment_id] else '',
                                 'apartment_owner_name': track_apartment[current_apartment_id]['apartment_owner_name'] if current_apartment_id and track_apartment[current_apartment_id] else '',
                                 'apartment_owner_email': track_apartment[current_apartment_id]['apartment_owner_email'] if current_apartment_id and track_apartment[current_apartment_id] else '',
                                 'action_date': log.action_date.strftime("%d/%m/%Y %H:%M"), 'action_type': action_type,
                                 'action_content': ". ".join(action_content), "status_action": status_action,
                                 'status_apartment': status_apartment}
                    customer_apartment_log_data.append(temp_data)

        return render(request, 'admin/log-customer.html',
                      {'customer_name': customer_name, 'customer_apartment': customer_apartment,
                       'customer_company': customer_company, 'customer_building': customer_building,
                       'customer_vehicle_registration_log_data': customer_vehicle_registration_log_data,
                       'customer_apartment_log_data': customer_apartment_log_data})

    return render(request, 'admin/log-customer.html')


@login_required(redirect_field_name='', login_url='/admin/')
def render_vehicle_registration_log(request, vehicle_registration_id):
    def get_or_query_customer_info(customer_id, customer_tracker_dict):
        if customer_id in customer_tracker_dict:
            return customer_tracker_dict[customer_id]
        else:
            try:
                customer = Customer.objects.get(id=customer_id)
                customer_tracker_dict[customer_id] = {'customer_name': customer.customer_name}
                return customer_tracker_dict[customer_id]
            except:
                return None

    if vehicle_registration_id:
        vrs = VehicleRegistration.objects.filter(id=vehicle_registration_id)
        vehicle_registrations = VehicleRegistration.audit_log.filter(id=vehicle_registration_id).order_by('id',
                                                                                                          'action_date')

        if vrs and vehicle_registrations:
            vr = vrs[0]

            # Doi khach hang, doi bien so xe

            # customer_name = vr.customer.customer_name if vr.customer.customer_name else ''
            # customer_building = vr.customer.building if vr.customer.building else ''
            # customer_apartment = vr.customer.apartment if vr.customer.apartment else ''
            # customer_company = vr.customer.company if vr.customer.company else ''

            vehicle_registration_log_data = list()

            track_vehicle_registration_changes = {}

            track_customer = {}

            for log in vehicle_registrations:
                action_type = log.action_type
                current_card_label = log.card.card_label if log.card else ''
                current_vehicle_number = log.vehicle_number
                current_vehicle_registration_id = log.id
                current_customer_id = log.customer_id

                action_content = []
                status_vehicle_number = "black"
                status_card_label = "black"
                status_customer = "black"
                status_action = "gray"

                if action_type == 'I':
                    action_type = 'Tạo mới'
                    status_action = status_vehicle_number = status_card_label = "green"
                    if current_vehicle_registration_id not in track_vehicle_registration_changes:
                        track_vehicle_registration_changes[current_vehicle_registration_id] = {}
                    track_vehicle_registration_changes[current_vehicle_registration_id][
                        'vehicle_number'] = current_vehicle_number
                    track_vehicle_registration_changes[current_vehicle_registration_id][
                        'card_label'] = current_card_label
                    track_vehicle_registration_changes[current_vehicle_registration_id][
                        'customer_id'] = current_customer_id

                    get_or_query_customer_info(current_customer_id, track_customer)

                elif action_type == 'U':
                    action_type = 'Cập nhật'
                    status_action = "blue"
                    if current_vehicle_registration_id in track_vehicle_registration_changes:
                        last_track = track_vehicle_registration_changes[current_vehicle_registration_id]
                        if current_vehicle_number != last_track['vehicle_number']:
                            action_content.append(u"Đổi biển số")
                            status_vehicle_number = "blue"
                            last_track['vehicle_number'] = current_vehicle_number
                        if current_card_label != last_track['card_label']:
                            action_content.append(u"Đổi mã thẻ")
                            status_card_label = "blue"
                            last_track['card_label'] = current_card_label
                        if current_customer_id != last_track['customer_id']:
                            action_content.append(u"Đổi khách hàng")
                            status_customer = "blue"
                            last_track['customer_id'] = current_customer_id

                            get_or_query_customer_info(current_customer_id, track_customer)

                elif action_type == 'D':
                    action_type = 'Xoá'
                    status_action = status_card_label = status_vehicle_number = "gray"
                    if current_vehicle_registration_id in track_vehicle_registration_changes:
                        del track_vehicle_registration_changes[current_vehicle_registration_id]

                temp_data = {'card_label': current_card_label,
                             'vehicle_registration_id': log.id, 'vehicle_number': current_vehicle_number,
                             'vehicle_type': log.vehicle_type,
                             'customer_id': log.customer_id,
                             # 'customer_name': track_customer[log.customer_id]['customer_name'] or '',
                             'action_date': log.action_date.strftime("%d/%m/%Y %H:%M"), 'action_type': action_type,
                             'action_content': ". ".join(action_content), "status_action": status_action,
                             'status_vehicle_number': status_vehicle_number, 'status_card_label': status_card_label,
                             'status_customer': status_customer}
                vehicle_registration_log_data.append(temp_data)

            return render(request, 'admin/log-vehicle-registration.html', {
                'target_vehicle_number': vr.vehicle_number,
                'target_vehicle_id': vr.id,
                'target_card_label': vr.card.card_label if vr.card else None,
                'target_customer_name': vr.customer.customer_name,
                'target_customer_id': vr.customer_id,
                'target_vehicle_type': vr.vehicle_type,
                'vehicle_registration_log_data': vehicle_registration_log_data})

    return render(request, 'admin/log-vehicle-registration.html')


def get_change_notes(request):
    with open('changelog.txt', 'r') as f:
        response = HttpResponse(f.read(), content_type="text/plain")
        # response['Content-Disposition'] = "attachment; filename=%s" % 'changelog.txt'

        return response

def clear_cache(request):
    """
    Clear cache
    """
    get_cache('default').clear()
    return HttpResponse('OK')
##2017-12-29
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
    def NonQuerySeconds(self, proc_name, *proc_params):
        new_params = [clean_param(param) for param in proc_params]
        cursor = connections['secondary'].cursor()
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

        rows = cursor.fetchall()
        retVal = []
        for row in rows:
            retVal.append(row)
        return retVal
    def QuerySecond(self, proc_name, *proc_params):
        new_params = [clean_param(param) for param in proc_params]
        cursor = connections['secondary'].cursor()
        ret = cursor.execute("%s %s(%s)" % ("CALL",
                                            proc_name,
                                            ', '.join('%s' for x in new_params)),
                             new_params)

        rows = cursor.fetchall()
        retVal = []
        for row in rows:
            retVal.append(row)
        return retVal
@csrf_exempt
def post_renewalregistry(request):
    if request.method == "POST" and request.is_ajax():
        cusid = request.POST['customer']
        staff = int(request.POST['staff'])
        stff = UserProfile.objects.get(user_id=staff)
        if stff:
            staff=stff.id
            # if request.POST['staff'] else 1
        totalfee=request.POST['totalfee']
        note = request.POST['note']
        ids=request.POST.getlist('ids[]')
        olddays = request.POST.getlist('olddays[]')
        newdays = request.POST.getlist('newdays[]')
        fees = request.POST.getlist('fees[]')
        lvfees=request.POST.getlist('lvfees[]')
        util = Utilities()
        receiptnum = None
        receiptnums = util.Query("getreceiptnumber")
        for r in receiptnums:
            receiptnum=r[0]
        if receiptnum:
            cursor = connections['default'].cursor()
            cursor.execute("begin")
            currrtime=datetime.utcnow().replace(microsecond=0, tzinfo=utc).strftime("%Y-%m-%d %H:%M:%S")
            currrdate = datetime.utcnow().replace(microsecond=0, tzinfo=utc).strftime("%Y-%m-%d")
            try:
                rt = cursor.execute(
                    "insert into `parking_receipt`(`receipt_number`,`type`,`ref_id`,`cancel`,`notes`,`action_date`)values('%s',0,0,0,'%s','%s');" % (
                    receiptnum, note, currrtime))
                rcid=cursor.lastrowid
                rt = cursor.execute(
                    "insert into `parking_ticketpayment`(`customer_id`, `receipt_id`,`receipt_number`,`payment_date`,`payment_fee`,`payment_method`,`notes`, `staff_id`) values('%s','%s','%s','%s','%s','TM','Gia hạn','%s');" %(
                        cusid,rcid,receiptnum,currrtime,totalfee,staff
                    ))
                tkid=cursor.lastrowid
                rt = cursor.execute("update `parking_receipt` set `ref_id`=%s where `id`=%s;" % (
                    tkid,rcid
                ))
                l=len(ids)
                i=0
                while i<l:
                    id=ids[i]
                    oldday=datetime.strptime(olddays[i], "%d/%m/%Y").date()
                    newday=datetime.strptime(newdays[i], "%d/%m/%Y").date()
                    drt=(newday-oldday).days+1
                    fff=fees[i]
                    lvfee=lvfees[i]
                    qr="insert into parking_ticketpaymentdetail(ticket_payment_id,vehicle_registration_id,vehicle_number,level_fee,effective_date,duration,day_duration,old_expired_date,expired_date,payment_detail_fee, used) %s" %(
                        "select %s,pv.id,pv.vehicle_number,%s,'%s',%s,0,pv.expired_date,'%s',%s,0 from `parking_vehicleregistration` pv where pv.id=%s;" % (
                            tkid,lvfee,oldday,drt,newday,fff,id)
                        )
                    rt = cursor.execute(qr)
                    qr="update parking_vehicleregistration set expired_date='%s', last_renewal_date='%s', last_renewal_effective_date='%s', status=1 where id=%s;" %(
                        newday,currrdate,oldday,id)
                    rt = cursor.execute(qr)
                    i=i+1
                cursor.execute("commit")
                cursor.close()
                status = "OK"
                tkpms = util.Query("getpaymentid")
                tkpm = -1
                for tk in tkpms:
                    tkpm = tk[0]
                status = tkpm
            except:
                cursor.execute("rollback")
                status = "not excute"
        else:
            status = "Bad"
        return HttpResponse(status)
    else:
        status = "Bad"
        return HttpResponse(status)

@csrf_exempt
def post_registry(request):
    if request.method == "POST" and request.is_ajax():
        cusid = request.POST['customer']
        staff = int(request.POST['staff'])
        stff = UserProfile.objects.get(user_id=staff)
        if stff:
            staff = stff.id
            # if request.POST['staff'] else 1
        fromdate = request.POST['fromdate']
        todate = request.POST['todate']
        cardnums = request.POST.getlist('cardnums[]')
        vehicletypes = request.POST.getlist('vehicletypes[]')
        feelevels = request.POST.getlist('feelevels[]')
        vehiclenums = request.POST.getlist('vehiclenums[]')
        vehiclepaints = request.POST.getlist('vehiclepaints[]')
        vehiclebrands = request.POST.getlist('vehiclebrands[]')
        drivenames = request.POST.getlist('drivenames[]')
        drivephones = request.POST.getlist('drivephones[]')
        currrtime = datetime.utcnow().replace(microsecond=0, tzinfo=utc).strftime("%Y-%m-%d %H:%M:%S")
        currrdate = datetime.utcnow().replace(microsecond=0, tzinfo=utc).strftime("%Y-%m-%d")
        util = Utilities()
        if fromdate=='' or todate=='':
            cursor = connections['default'].cursor()
            cursor.execute("begin")
            try:
                l = len(cardnums)
                i = 0
                while i < l:
                    lvfee = feelevels[i]
                    cardnum = cardnums[i]
                    cardid = cardnum
                    crd = Card.objects.get(card_label=cardnum)
                    if crd:
                        cardid = crd.id
                    vehicletype = vehicletypes[i]
                    vehiclenum = vehiclenums[i]
                    vehiclepaint = vehiclepaints[i]
                    vehiclebrand = vehiclebrands[i]
                    drivename = drivenames[i]
                    drivephone = drivephones[i]
                    qr = "insert into `parking_vehicleregistration`(`card_id`,`customer_id`,`level_fee_id`,`registration_date`,`first_renewal_effective_date`,`start_date`, `expired_date`, `last_renewal_date`, `last_renewal_effective_date`,`vehicle_driver_name`,`vehicle_driver_id`,`vehicle_driver_phone`,`vehicle_type_id`, `vehicle_number`,`vehicle_brand`,`vehicle_paint`,`status`,`staff_id`) %s" % (
                        "values ('%s','%s','%s','%s','%s', '%s','%s','%s','%s','%s',0,'%s','%s','%s','%s','%s',3,'%s');" % (
                            cardid, cusid, lvfee, currrdate, currrdate, currrdate, currrdate,currrdate, currrdate, drivename, drivephone,
                            vehicletype, vehiclenum, vehiclebrand, vehiclepaint, staff
                        ))
                    rt = cursor.execute(qr)
                    i=i+1
                cursor.execute("commit")
                cursor.close()
                status = "onlyregis"
            except:
                cursor.execute("rollback")
                status = "not excute"
            return HttpResponse(status)
        else:
            receiptnum = None
            receiptnums = util.Query("getreceiptnumber")
            for r in receiptnums:
                receiptnum = r[0]
            if receiptnum:
                cursor = connections['default'].cursor()
                cursor.execute("begin")
                try:
                    oldday = datetime.strptime(fromdate, "%d/%m/%Y").date()
                    newday = datetime.strptime(todate, "%d/%m/%Y").date()
                    totalfee=0
                    rt = cursor.execute(
                        "insert into `parking_receipt`(`receipt_number`,`type`,`ref_id`,`cancel`,`notes`,`action_date`)values('%s',0,0,0,'%s','%s');" % (
                            receiptnum, 'Thêm mới', currrtime))
                    rcid = cursor.lastrowid
                    rt = cursor.execute(
                        "insert into `parking_ticketpayment`(`customer_id`, `receipt_id`,`receipt_number`,`payment_date`,`payment_fee`,`payment_method`,`notes`, `staff_id`) values('%s','%s','%s','%s','%s','TM','Thêm mới','%s');" % (
                            cusid, rcid, receiptnum, currrtime, totalfee, staff
                        ))
                    tkid = cursor.lastrowid
                    rt = cursor.execute("update `parking_receipt` set `ref_id`=%s where `id`=%s;" % (
                        tkid, rcid
                    ))
                    l = len(cardnums)
                    i = 0
                    while i < l:
                        drt = (newday - oldday).days + 1
                        lvfee = feelevels[i]
                        lvfee1=lvfee
                        lvf = LevelFee.objects.get(id=feelevels[i])
                        if lvf:
                            lvfee1=lvf.fee
                        cardnum=cardnums[i]
                        cardid=cardnum
                        crd = Card.objects.get(card_label=cardnum)
                        if crd:
                            cardid=crd.id
                        vehicletype = vehicletypes[i]
                        vehiclenum = vehiclenums[i]
                        vehiclepaint = vehiclepaints[i]
                        vehiclebrand = vehiclebrands[i]
                        drivename = drivenames[i]
                        drivephone = drivephones[i]
                        qr="insert into `parking_vehicleregistration`(`card_id`,`customer_id`,`level_fee_id`,`registration_date`,`first_renewal_effective_date`,`start_date`, `expired_date`, `last_renewal_date`, `last_renewal_effective_date`,`vehicle_driver_name`,`vehicle_driver_id`,`vehicle_driver_phone`,`vehicle_type_id`, `vehicle_number`,`vehicle_brand`,`vehicle_paint`,`status`,`staff_id`) %s" % (
                            "values ('%s','%s','%s','%s','%s', '%s','%s','%s','%s','%s',0,'%s','%s','%s','%s','%s',1,'%s');" % (
                                cardid,cusid,lvfee,currrdate,currrdate,oldday,newday,currrdate,oldday,drivename,drivephone,vehicletype,vehiclenum,vehiclebrand,vehiclepaint,staff
                            ))
                        rt = cursor.execute(qr)
                        id = cursor.lastrowid
                        tfee = getfeeByMonthSGCT(fdate=oldday, tdate=newday,
                                                 feepermonth=lvfee1)  # getfeeByMonthViettel(fdate=f_date, tdate=t_date, feepermonth=feemonth)
                        # rm=divmod(tfee,1000)
                        # if rm[1] > 0:
                        #     tfee = rm[0] * 1000 + 1000
                        qr = "insert into parking_ticketpaymentdetail(ticket_payment_id,vehicle_registration_id,vehicle_number,level_fee,effective_date,duration,day_duration,old_expired_date,expired_date,payment_detail_fee, used) %s" % (
                            "select %s,pv.id,pv.vehicle_number,%s,'%s',%s,0,pv.expired_date,'%s',%s,0 from `parking_vehicleregistration` pv where pv.id=%s;" % (
                                tkid, lvfee1, oldday, drt, newday, tfee, id)
                        )
                        rt = cursor.execute(qr)
                        totalfee=totalfee+tfee;
                        i = i + 1
                    qr = "update parking_ticketpayment set payment_fee='%s' where id=%s;" % (
                        totalfee,tkid)
                    rt = cursor.execute(qr)
                    cursor.execute("commit")
                    cursor.close()
                    status = tkid
                except:
                    cursor.execute("rollback")
                    status = "not excute"
            else:
                status = "Bad"
            return HttpResponse(status)
    else:
        status = "Bad"
        return HttpResponse(status)

def last_day_of_month(any_day):
    next_month = any_day.replace(day=28) + timedelta(days=4)
    # this will never fail
    return next_month - timedelta(days=next_month.day)
def first_day_of_month(any_day):
    return any_day.replace(day=1)
def daysofmonth(any_day):
    endate = last_day_of_month(any_day)
    firstdate=any_day.replace(day=1)
    return (endate-firstdate).days+1

def monthsoftwodate(any_fromday, any_enddate):
    num_months = 1;
    month1 = any_fromday.month
    year1 = any_fromday.year
    month2 = any_enddate.month
    year2 = any_enddate.year
    while year1 <= year2 and month1 != month2:
        month1 += 1
        num_months += 1
        if month1 > 12:
            month1 = 1
            year1 += 1
    return num_months

def getfeeByMonthSGCT(fdate, tdate, feepermonth):
    ttmonth = monthsoftwodate(fdate, tdate)
    tfee =0
    if ttmonth == 1:
        tfee = ceil(((tdate - fdate).days + 1) * float(feepermonth) / daysofmonth(fdate))
    elif ttmonth == 2:
        tfee = ceil(((last_day_of_month(fdate) - fdate).days + 1) * float(feepermonth) / daysofmonth(fdate)) + ceil(
            ((tdate - first_day_of_month(tdate)).days + 1) * float(feepermonth) / daysofmonth(tdate))
    elif ttmonth > 2:
        mo = ttmonth - 2
        tfee = ceil(((last_day_of_month(fdate) - fdate).days + 1) * float(feepermonth) / daysofmonth(fdate)) + ceil(
            ((tdate - first_day_of_month(tdate)).days + 1) * float(feepermonth) / daysofmonth(tdate)) + float(feepermonth)*float(mo)
    else:
        tfee = 0
    rm = divmod(tfee, 1000)
    if rm[1] > 0:
        tfee = rm[0] * 1000 + 1000
    return tfee

def getfeeByMonthViettel(fdate, tdate, feepermonth):
    ttmonth = monthsoftwodate(fdate, tdate)
    tfee = 0
    if ttmonth == 1:
        tfee = ceil(((tdate - fdate).days + 1) * float(feepermonth) / 30)
    elif ttmonth == 2:
        tfee = ceil(((last_day_of_month(fdate) - fdate).days + 1) * float(feepermonth) / 30) + ceil(
            ((tdate - first_day_of_month(tdate)).days + 1) * float(feepermonth) / 30)
    elif ttmonth > 2:
        mo = ttmonth - 2
        tfee = ceil(((last_day_of_month(fdate) - fdate).days + 1) * float(feepermonth) / 30) + ceil(
            ((tdate - first_day_of_month(tdate)).days + 1) * float(feepermonth) / 30) + float(feepermonth)*float(mo)
    else:
        tfee =0
    rm = divmod(tfee, 1000)
    if rm[1] > 0:
        tfee = rm[0] * 1000 + 1000
    return tfee

def get_fee(request, fdate, tdate, feepermonth):
    try:
        oldday = datetime.strptime(fdate, "%d%m%Y").date()
        newday = datetime.strptime(tdate, "%d%m%Y").date()
        fff=getfeeByMonthSGCT(oldday,newday,feepermonth)
        final = "0"
        if fff:
            final = "%s" % fff
    except:
        final="0"
    return HttpResponse(final)
##
###API calculate fee
##2018-01-05
@login_required(redirect_field_name='', login_url='/admin/')
def configfee(request):
    current_user = request.user.id
    username=getUserName(current_user)
    util = Utilities()
    qr = util.QuerySecond('getrootbyuser', current_user)
    if qr and len(qr) > 0:
        isroot = True
    else:
        isroot = False
    return render(request, 'admin/configfee.html',{'current_user':current_user,'username':username,'isroot':isroot})
##2018-04-11 Module Fee
##Get list Card type by jsondata
def get_card_types(request):
    ctl=[]
    try:
        cts = CardType.objects.all()
        # datajson=serializers.serialize('json',cts)
        #now= datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        for ct in cts:
            ctl.append({"id": ct.id, "name": ct.name})
        datajson = json.dumps(ctl, ensure_ascii=False).encode('utf8')
        return HttpResponse(datajson)
    except:
        ctl.append({"id": None, "name": None})
        datajson = json.dumps(ctl, ensure_ascii=False).encode('utf8')
        return HttpResponse(datajson)

##Get list Vehicle type by jsondata
def get_vehicle_types(request):
    ctl = []
    try:
        cts = VehicleType.objects.all()
        # datajson=serializers.serialize('json',cts)
        # now= datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        for ct in cts:
            ctl.append({"id": ct.id,"category":ct.category, "name": ct.name})
        datajson = json.dumps(ctl, ensure_ascii=False).encode('utf8')
        return HttpResponse(datajson)
    except:
        ctl.append({"id": None,"category":None, "name": None})
        datajson = json.dumps(ctl, ensure_ascii=False).encode('utf8')
        return HttpResponse(datajson)
def get_regisfeedetail(request,cardtype,vehicletype):
    ctl = []
    util = Utilities()
    qrs = util.QuerySecond('getsampleactivedetail', vehicletype,cardtype)
    try:
        for ct in qrs:
            ctl.append(
                {"id": ct[0], "activedate": str(ct[1]) if ct[1] else '', "expireddate": str(ct[2]) if ct[2] else '',
                 "vehicletype": ct[3], "cardtype": ct[4], "sampleid": ct[5] if ct[5] else -1,
                 "samplename": ct[6] if ct[6] else '', "sampleid1": ct[7] if ct[7] else -1,
                 "samplename1": ct[8] if ct[8] else '', "usercreated": getUserName(ct[9]) if ct[9] else '',"userid":ct[9],"ischange":ct[10],
                 "sampleid2": ct[11] if ct[11] else -1,
                 "samplename2": ct[12] if ct[12] else ''
                 })
        datajson = json.dumps(ctl, ensure_ascii=False).encode('utf8')
        return HttpResponse(datajson)
    except Exception as e:
        ctl=[]
        datajson = json.dumps(ctl, ensure_ascii=False).encode('utf8')
        return HttpResponse(datajson)
def get_regisfeesimilar(request):
    ctl = []
    samplesset = getsampleset()
    samplesset1 = getsampleset()
    samplesset2 = getsampleset()
    try:
        cts = CardType.objects.all()
        vhs = VehicleType.objects.all()
        vt=[]
        for v in vhs:
            vt.append({"id":v.id,"category":v.category,"name":v.name,"chosen":False})
        for ct in cts:
            ctl.append({"id": ct.id, "name": ct.name, "vehicles":vt,"chosen":False})
        res={"cardtypes":ctl,"sample":samplesset,"sample1":samplesset1,"sample2":samplesset2}
        datajson = json.dumps(res, ensure_ascii=False).encode('utf8')
        return HttpResponse(datajson)
    except Exception as e:
        ctl.append({"cardtypes": None, "sample": None,"sample1":None,"sample2":None})
        datajson = json.dumps(ctl, ensure_ascii=False).encode('utf8')
        return HttpResponse(datajson)
def get_regisredemptionsimilar(request):
    ctl = []
    samplesset = getredemptionset()
    try:
        # cts = CardType.objects.all()
        # util = Utilities()
        # cts = util.QuerySecond('gettenantgroup')
        tg=ClaimPromotionGroupTenant.objects.all()
        vhs = VehicleType.objects.all()
        vt=[]
        for v in vhs:
            vt.append({"id":v.id,"category":v.category,"name":v.name,"chosen":False})
        for ct in tg:
            ctl.append({"id": ct.id, "name": ct.groupname, "vehicles":vt,"chosen":False})
        res={"groups":ctl,"sample":samplesset}
        datajson = json.dumps(res, ensure_ascii=False).encode('utf8')
        return HttpResponse(datajson)
    except Excption as e:
        ctl.append({"groups": None, "sample": None})
        datajson = json.dumps(ctl, ensure_ascii=False).encode('utf8')
        return HttpResponse(datajson)

def get_regisfeesample(request):
    ctl = []
    samplesset = getsampleset()
    samplesset1 = getsampleset()
    samplesset2 = getsampleset()
    try:
        cts = CardType.objects.all()
        vhs = VehicleType.objects.all()
        for ct in cts:
            for vt in vhs:
                ctl.append({"cardtype": {"id": ct.id, "name": ct.name},
                            "vehicletype": {"id": vt.id, "category": vt.category, "name": vt.name},
                            "sampleset": samplesset, "samplesset1": samplesset1, "samplesset2": samplesset2})
        datajson = json.dumps(ctl, ensure_ascii=False).encode('utf8')
        return HttpResponse(datajson)
    except Exception as e:
        ctl.append({"cardtype": None, "vehicletype": None,"sampleset": None, "samplesset1": None, "samplesset2":None})
        datajson = json.dumps(ctl, ensure_ascii=False).encode('utf8')
        return HttpResponse(datajson)
##Get list formula fee
def getselectedformulas(id):
    ctl = []
    try:
        util = Utilities()
        qrs = util.QuerySecond('getfeeformularbyid',id)
        # datajson=serializers.serialize('json',cts)
        # now= datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        if len(qrs)>0:
            ctl={"id": qrs[0][0], "feetype": qrs[0][1], "callname": qrs[0][2], "detail": json.loads(str(qrs[0][3])) if qrs[0][3] else '',
                 "fullfee": qrs[0][4] if qrs[0][4] else ''}
        else:
            ctl={"id": -1, "feetype": -1, "callname": "-Chọn-", "detail": "",
             "fullfee": ""}
    except Exception as e:
        ctl = {"id": -1, "feetype": -1, "callname": "-Chọn-", "detail": "",
               "fullfee": ""}
    return ctl
def getindex(id,fml):
    index=0
    for f in fml:
        if f["id"]==id:
            return  index;
        index=index+1
    return 0
#getbillformula
def getbillformulas():
    ctl = []
    try:
        util = Utilities()
        qrs = util.QuerySecond('getbillformula')
        ctl.append(
            {"id": -1,"callname": "-Chọn-", "detail": "",
            "showdetail":False})
        for ct in qrs:
            ctl.append(
                {"id": ct[0],"callname": ct[1], "detail": json.loads(str(ct[2])) if ct[2] else "",
                 "showdetail":False})
        return ctl

    except Exception as e:
        ctl.append(
            {"id": -1, "callname": "-Chọn-", "detail": "",
              "showdetail":False})
        return ctl
def getformulas():
    ctl = []
    try:
        util = Utilities()
        qrs = util.QuerySecond('getfeeformular')
        ctl.append(
            {"id": -1, "feetype": -1, "callname": "-Chọn-", "detail": "",
             "fullfee": "","showdetail":False})
        for ct in qrs:
            ctl.append(
                {"id": ct[0], "feetype": ct[1], "callname": ct[2], "detail": json.loads(str(ct[3])) if ct[3] else "",
                 "fullfee": ct[4] if ct[4] else "","showdetail":False})
        return ctl

    except Exception as e:
        ctl.append(
            {"id": -1, "feetype": -1, "callname": "-Chọn-", "detail": "",
             "fullfee": "","showdetail":False})
        return ctl
def getsampleset():
    try:
        util = Utilities()
        qrs = util.QuerySecond('getsamplefee',0,0)
        rs=[x for x in qrs if x[4]!=4 and x[5]>=1 and x[5]!=3]
        samples=[]
        samples.append({"id": -1, "callname": "-Chọn-", "showdetail": False})
        for ct in rs:
            samples.append({"id": ct[0], "callname": ct[1], "showdetail": False})
        sampleselected=samples[0]
        return {"samples":samples,"sampleselected":sampleselected}

    except Exception as e:
        samples = []
        samples.append({"id": -1, "callname": "-Chọn-", "showdetail": False})
        sampleselected = samples[0]
        return {"samples": samples, "sampleselected": sampleselected}
def get_activepermission(request, menuid):
    try:
        userid = request.user.id
        util = Utilities()
        qr = util.QuerySecond('getrootbyuser', userid)
        if qr and len(qr) > 0:
            data = {"id": 0, "menuid": menuid, "userid": userid,
                    "isadd": True , "isedit": True ,
                    "iseditall": True , "isdel": True ,
                    "isdelall": True }
        else:
            qrs = util.QuerySecond('getactivepermission',userid,menuid)
            if len(qrs)<1:
                data = {"id": -1, "menuid": -1, "userid": -1, "isadd": False, "isedit": False, "iseditall": False,
                        "isdel": False, "isdelall": False}
                datajson = json.dumps(data, ensure_ascii=False).encode('utf8')
                return HttpResponse(datajson)
            data= {"id": int(qrs[0][0]), "menuid": menuid, "userid": userid,
                "isadd": True if int(qrs[0][3]) > 0 else False, "isedit": True if int(qrs[0][4]) > 0 else False,
                "iseditall": True if int(qrs[0][4]) > 1 else False, "isdel": True if int(qrs[0][5]) > 0 else False,
                "isdelall": True if int(qrs[0][5]) > 1 else False}
        datajson = json.dumps(data, ensure_ascii=False).encode('utf8')
        return HttpResponse(datajson)
    except Exception as e:
        data= {"id": -1, "menuid": -1, "userid": -1, "isadd": False, "isedit": False, "iseditall": False,
                "isdel": False, "isdelall": False}
        datajson = json.dumps(data, ensure_ascii=False).encode('utf8')
        return HttpResponse(datajson)
def getredemptionset():
    try:
        util = Utilities()
        qrs = util.QuerySecond('getsamplefee',0,0)
        rs=[x for x in qrs if x[4]==4 and x[5]>=1 and x[5]!=3]
        samples=[]
        samples.append({"id": -1, "callname": "-Chọn-", "showdetail": False})
        for ct in rs:
            samples.append({"id": ct[0], "callname": ct[1], "showdetail": False})
        sampleselected=samples[0]
        return {"samples":samples,"sampleselected":sampleselected}
    except Exception as e:
        samples = []
        samples.append({"id": -1, "callname": "-Chọn-", "showdetail": False})
        sampleselected = samples[0]
        return {"samples": samples, "sampleselected": sampleselected}
def get_activeredemption(request, groupid, vehicletypeid):
    samples = getredemptionset()
    try:
        util = Utilities()
        qrs = util.QuerySecond('gtredemptionactive',groupid,vehicletypeid)
        redemption=[]
        for ct in qrs:
            sp=[x for x in samples["samples"] if x["id"]==ct[4]]
            samplename=''
            if sp and len(sp)>0:
                samplename=sp[0]["callname"]
            sp1 = [x for x in samples["samples"] if x["id"] == ct[6]]
            samplename1 = ''
            if sp1 and len(sp1) > 0:
                samplename1 = sp1[0]["callname"]
            redemption.append({"id": ct[0], "activedate": ct[1].strftime("%d-%m-%Y") if ct[1] else '',
                            "expireddate": ct[2].strftime("%d-%m-%Y") if ct[2] else '', "tenantgroupid": ct[3],
                               "samplename": samplename, "samplename1": samplename1,
                               "usercreated": getUserName(ct[5]) if ct[5] else '',
                               "vehicletypeid": ct[7] if ct[7] else -1,"userid":ct[5],"ischange":True if int(ct[8])>0 else False})

        result={"redemption":redemption,"samples":samples}
    except Exception as e:
        redemption = []
        result = {"redemption": redemption, "samples": samples}
    datajson = json.dumps(result, ensure_ascii=False).encode('utf8')
    return HttpResponse(datajson)
def get_samplelist(request):
    sample = getsampleset()
    datajson = json.dumps(sample, ensure_ascii=False).encode('utf8')
    return HttpResponse(datajson)
def get_specialdate(request):
    ctl = []
    try:
        util = Utilities()
        qrs = util.QuerySecond('getspecialdate')
        # datajson=serializers.serialize('json',cts)
        # now= datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        for ct in qrs:
            ctl.append(
                {"id": ct[0],"dateactive":ct[1].strftime("%Y-%m-%d"), "callname": ct[2], "percentupordown": ct[3],
                    "createdby": getUserName(ct[4]),"userid":ct[4],"isvisible":True,"ischange":True if int(ct[5])>0 else False})
        datajson = json.dumps(ctl, ensure_ascii=False).encode('utf8')
        return HttpResponse(datajson)
    except Exception as e:
        ctl.append(
            {"id": None,"dateactive":None, "callname": None,"percentupordown":None,
                "createdby":None})
        datajson = json.dumps(ctl, ensure_ascii=False).encode('utf8')
        return HttpResponse(datajson)
def get_formulabill(request):
    ctl = []
    try:
        util = Utilities()
        qrs = util.QuerySecond('getbillformula')
        # datajson=serializers.serialize('json',cts)
        # now= datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        for ct in qrs:
            ctl.append(
                {"id": ct[0], "callname": ct[1], "detail": json.loads(str(ct[2])) if ct[2] else '',
                 "createdby": getUserName(ct[3]), "ischange": True if int(ct[4]) > 0 else False,
                 "userid": int(ct[3]) if ct[3] else -1,"isvisible":True})
        datajson = json.dumps(ctl, ensure_ascii=False).encode('utf8')
        return HttpResponse(datajson)
    except Exception as e:
        ctl=[]
        datajson = json.dumps(ctl, ensure_ascii=False).encode('utf8')
        return HttpResponse(datajson)
def get_tenantgroup(request):
    ctl = []
    try:
        # util = Utilities()
        # qrs = util.QuerySecond('gettenantgroup')
        # datajson=serializers.serialize('json',cts)
        # now= datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        tg = ClaimPromotionGroupTenant.objects.all();
        for ct in tg:
            ctl.append(
                {"id": ct.id, "groupname": ct.groupname, "ischange": True, "isvisible": True,
                 "detail": []})
            # ctl.append(
            #     {"id": ct[0], "groupname": ct[1], "ischange": True if int(ct[2]) > 0 else False, "isvisible": True,
            #      "detail": []})
        datajson = json.dumps(ctl, ensure_ascii=False).encode('utf8')
        return HttpResponse(datajson)
    except Exception as e:
        ctl = []
        datajson = json.dumps(ctl, ensure_ascii=False).encode('utf8')
        return HttpResponse(datajson)
def get_tenantgroupactive(request):
    ctl = []
    try:
        # util = Utilities()
        # qrs = util.QuerySecond('gettenantgroup')
        # datajson=serializers.serialize('json',cts)
        # now= datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        tg=ClaimPromotionGroupTenant.objects.all();
        vtype=VehicleType.objects.all()
        for ct in tg:
            for v in vtype:
                ctl.append(
                    {"id": ct.id, "groupname": ct.groupname,"vehicleid":v.id,"vehiclename":v.name, "detail": []})
        datajson = json.dumps(ctl, ensure_ascii=False).encode('utf8')
        return HttpResponse(datajson)
    except Exception as e:
        ctl = []
        ctl.append(
            {"id": -1, "groupname": None,"vehicleid":-1,"vehiclename":None, "detail": []})
        datajson = json.dumps(ctl, ensure_ascii=False).encode('utf8')
        return HttpResponse(datajson)
def get_tenantsbygroup(request,id):
    ctl = []
    try:
        util = Utilities()
        qrs = util.QuerySecond('gettennants',id)
        # datajson=serializers.serialize('json',cts)
        # now= datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        for ct in qrs:
            ctl.append(
                {"id": ct[0], "refid": ct[1], "tenantgroupid": ct[2],"tenant":ct[3]})
        datajson = json.dumps(ctl, ensure_ascii=False).encode('utf8')
        return HttpResponse(datajson)
    except Exception as e:
        ctl = []
        datajson = json.dumps(ctl, ensure_ascii=False).encode('utf8')
        return HttpResponse(datajson)
def getgrouptenants():
    ctl = []
    ctl.append(
        {"id": -1, "groupname": "-Chọn-"})
    try:
        util = Utilities()
        qrs = util.QuerySecond('gettenantgroup')
        # datajson=serializers.serialize('json',cts)
        # now= datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        for ct in qrs:
            ctl.append(
                {"id": ct[0], "groupname": ct[1]})
        #datajson = json.dumps(ctl, ensure_ascii=False).encode('utf8')
        return ctl
    except Exception as e:
        return ctl
def get_settenant(request):
    ctl = []
    group=getgrouptenants()
    try:
        cursor = connections['secondary'].cursor()
        cus =ClaimPromotionTenant.objects.all()
            #Customer.objects.all()
        util = Utilities()
        qrs = util.QuerySecond('gettennants',-1)
        cursor.execute("begin")
        for c in cus:
            it=[x for x in qrs if(x[1]==c.id)]
            if len(it)<1 and "'" not in c.name:
                qr="insert into tenants(refid,tenantgroupid,tennantsname) values('%s','%s','%s')"%(c.id,-1,c.name)
                cursor.execute(qr)
                id = cursor.lastrowid
        cursor.execute("commit")
        qrs = util.QuerySecond('gettennants', -1)
        for item in qrs:
            groupid=item[2] if item[2] else -1
            selectedindex=0
            index=0
            for it in group:
                if it["id"]==groupid:
                    selectedindex=index
                index=index+1
            groupselected=group[selectedindex]
            ctl.append(
                {"id": item[0], "refid": item[1], "selectedindex": selectedindex, "tenant": item[3],
                 "group": group, "groupselected":groupselected})
        datajson = json.dumps(ctl, ensure_ascii=False).encode('utf8')
        return HttpResponse(datajson)
    except Exception as e:
        cursor.execute("rollback")
        ctl = []
        datajson = json.dumps(ctl, ensure_ascii=False).encode('utf8')
        return HttpResponse(datajson)
def get_formulafee(request):
    ctl = []
    try:
        util = Utilities()
        qrs = util.QuerySecond('getfeeformular')
        # datajson=serializers.serialize('json',cts)
        # now= datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        for ct in qrs:
            ctl.append(
                {"id": ct[0], "feetype": ct[1], "callname": ct[2], "detail": json.loads(str(ct[3])) if ct[3] else '',
                 "fullfee": ct[4] if ct[4] else '', "createdby": getUserName(ct[5]), "userid": ct[5], "isvisible": True,
                 "ischange": True if int(ct[6]) > 0 else False})
        datajson = json.dumps(ctl, ensure_ascii=False).encode('utf8')
        return HttpResponse(datajson)
    except Exception as e:
        ctl=[]
        datajson = json.dumps(ctl, ensure_ascii=False).encode('utf8')
        return HttpResponse(datajson)
##Get list SampleFee by jsondata
def get_samplefees(request,id,typeid):
    ctl = []
    try:
        util = Utilities()
        qrs = util.QuerySecond('getsamplefee',id,typeid)
        # datajson=serializers.serialize('json',cts)
        # now= datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        for ct in qrs:
            ctl.append({"id": ct[0], "feename": ct[1], "freetime": int(ct[2]) if ct[2] else 0,
                        "tolerancetime": int(ct[3]) if ct[3] else 0, "feetype": ct[4],
                        "inused": ct[5], "createdby": getUserName(ct[6]), "totalfees": ct[7],
                        "startdate": str(ct[8]) if ct[8]  else ('00:00:00' if ct[8] == time(0, 0, 0, 0) else '00:00:00'),
                        "userid": ct[6],"issubtractfree": True if ct[9] and int(ct[9]) > 0 else False,
                        "issubtracttolerance": True if ct[10] and int(ct[10]) > 0 else False,
                        "feeday": ct[11], "startnight": str(ct[12]) if ct[12]  else ('00:00:00' if ct[12] == time(0, 0, 0, 0) else '00:00:00'),
                        "ischange": True if ct[13] and int(ct[13]) > 0 else False,"isvisible": True,"canlock": True if ct[14] and int(ct[14]) == 1 else False,"canunlock": True if ct[14] and int(ct[14]) == 2 else False})
        datajson = json.dumps(ctl, ensure_ascii=False).encode('utf8')
        return HttpResponse(datajson)
    except Exception as e:
        ctl=[]
        datajson = json.dumps(ctl, ensure_ascii=False).encode('utf8')
        return HttpResponse(datajson)
##get detail fee24h
def get_detailfee24h(request,sampleid):
    ctl = []
    try:
        util = Utilities()
        qrs = util.QuerySecond('getdetailfee24h',sampleid)
        # datajson=serializers.serialize('json',cts)
        # now= datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        if(len(qrs)<1):
            ctl={"id": None, "samplefeeid": None, "blockhours": None, "blockfees": None, "affterfee": None,
                        "canrepeat": None, "exceptfee": None}
            datajson = json.dumps(ctl, ensure_ascii=False).encode('utf8')
        else:
            ctl = {"id": qrs[0][0], "samplefeeid": qrs[0][1], "blockhours": json.loads(qrs[0][2]) if qrs[0][2] else '' , "blockfees": json.loads(qrs[0][3]) if qrs[0][3] else '', "affterfee": qrs[0][4],
                   "canrepeat": qrs[0][5], "exceptfee": qrs[0][6]}
            datajson = json.dumps(ctl, ensure_ascii=False).encode('utf8')
        return HttpResponse(datajson)
    except Exception as e:
        ctl={"id": None, "samplefeeid": None, "blockhours": None, "blockfees": None, "affterfee": None,
            "canrepeat": None, "exceptfee": None}
        datajson = json.dumps(ctl, ensure_ascii=False).encode('utf8')
        return HttpResponse(datajson)
##get detail định mức Nhà nước mở rộng 2018Dec
def get_detailfeeNN(request, sampleid):
    ctl = []
    try:
        util = Utilities()
        qrs = util.QuerySecond('getsamplefeedetailnn', sampleid)
        if (len(qrs) < 1):
            ctl = {"id": None, "callname": None, "startday": None, "startnight": None,
                   "freetime": None, "fee24h": None, "maxfee": None, "optioncase": None,
                   "formuladay": None, "formuladaydetail": None, "formulanight": None,
                   "formulanightdetail": None}
            datajson = json.dumps(ctl, ensure_ascii=False).encode('utf8')
        else:
            ctl = {"id": qrs[0][0], "callname": qrs[0][1], "startday": str(qrs[0][2]) if qrs[0][2]  else ('00:00:00' if qrs[0][2] == time(0, 0, 0, 0) else '00:00:00'),
                   "startnight": str(qrs[0][3]) if qrs[0][3]  else ('00:00:00' if qrs[0][3] == time(0, 0, 0, 0) else '00:00:00'),
                   "freetime": qrs[0][4], "fee24h": qrs[0][5], "maxfee": qrs[0][6], "optioncase": qrs[0][7],
                   "formuladay": qrs[0][9], "formuladaydetail": json.loads(str(qrs[0][10])), "formulanight": qrs[0][12],
                   "formulanightdetail": json.loads(str(qrs[0][13]))}
            datajson = json.dumps(ctl, ensure_ascii=False).encode('utf8')
        return HttpResponse(datajson)
    except Exception as e:
        ctl = {"id": None, "callname": None, "startday": None, "startnight": None,
               "freetime": None, "fee24h": None, "maxfee": None, "optioncase": None,
               "formuladay": None, "formuladaydetail": None, "formulanight": None,
               "formulanightdetail": None}
        datajson = json.dumps(ctl, ensure_ascii=False).encode('utf8')
        return HttpResponse(datajson)
##get detail complex
def get_detailredemtion(request,sampleid):
    ctl = []
    fml=getbillformulas()
    try:
        util = Utilities()
        qrs = util.QuerySecond('getdatetyperedemption',sampleid)
        # datajson=serializers.serialize('json',cts)
        # now= datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        if(len(qrs)<1):
            ctl.append({"id": None, "samplefeeid": None, "callname": None, "weekmap": None,
                        "cycletimes": None})
            datajson = json.dumps(ctl, ensure_ascii=False).encode('utf8')
        else:
            for dt in qrs:
                datetypeid=dt[0]
                cycletimenomal=[]
                dtnm = util.QuerySecond('getcycleredemption', datetypeid)
                for nm in dtnm:
                    cycletimenomal.append(
                        {"id": nm[0], "fromtime": str(nm[1]) if nm[1] else ('00:00:00' if nm[1]==time(0,0,0,0) else ''), "totime": str(nm[2]) if nm[2] else ('00:00:00' if nm[2]==time(0,0,0,0) else ''),"showdetai":False,
                         "formula": fml,"formulaselected":fml[getindex(nm[3],fml)],"cyclename": getcyclename(nm[5])
                         })
                ctl.append( {"id": dt[0], "samplefeeid": dt[3], "callname": dt[1], "weekmap": replaceweekname(dt[2]),
                       "cycletimes": cycletimenomal})
            datajson = json.dumps(ctl, ensure_ascii=False).encode('utf8')
        return HttpResponse(datajson)
    except Exception as e:
        ctl.append({"id": None, "samplefeeid": None, "callname": None, "weekmap": None,
                    "cycletimes": None})
        datajson = json.dumps(ctl, ensure_ascii=False).encode('utf8')
        return HttpResponse(datajson)
##get detail redemption
def get_detailcomplex(request,sampleid):
    ctl = []
    fml=getformulas()
    try:
        util = Utilities()
        qrs = util.QuerySecond('getdatetypecomplex',sampleid)
        # datajson=serializers.serialize('json',cts)
        # now= datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        if(len(qrs)<1):
            ctl.append({"id": None, "samplefeeid": None, "callname": None, "weekmap": None, "feefullday": None,
                        "cycletimes": None})
            datajson = json.dumps(ctl, ensure_ascii=False).encode('utf8')
        else:
            for dt in qrs:
                datetypeid=dt[0]
                cycletimenomal=[]
                dtnm = util.QuerySecond('getcyclecomplex', datetypeid)
                for nm in dtnm:
                    cycletimenomal.append(
                        {"id": nm[0], "fromtime": str(nm[1]) if nm[1] else ('00:00:00' if nm[1]==time(0,0,0,0) else ''), "totime": str(nm[2]) if nm[2] else ('00:00:00' if nm[2]==time(0,0,0,0) else ''),"formulafirst":fml,"showdetai":False,
                         "formulafirstselectted": fml[getindex(nm[3],fml)], "formula": fml,"formulaselected":fml[getindex(nm[4],fml)], "cycle": nm[5],
                         "cyclename": getcyclename(nm[5])})
                ctl.append( {"id": dt[0], "samplefeeid": dt[3], "callname": dt[1], "weekmap": replaceweekname(dt[2]), "feefullday": dt[4] if dt[4] else '',
                       "cycletimes": cycletimenomal})
            datajson = json.dumps(ctl, ensure_ascii=False).encode('utf8')
        return HttpResponse(datajson)
    except Exception as e:
        ctl.append({"id": None, "samplefeeid": None, "callname": None, "weekmap": None, "feefullday": None,
                    "cycletimes": None})
        datajson = json.dumps(ctl, ensure_ascii=False).encode('utf8')
        return HttpResponse(datajson)
def get_toolfeemenus(request):
    ctl = []
    userid = request.user.id
    try:
        util = Utilities()
        qr=util.QuerySecond('getrootbyuser',userid)
        if qr and len(qr)>0:
            isroot=True
        else:
            isroot=False
        qrs = util.QuerySecond('getmenus')
        for ct in qrs:
            if int(ct[0])==9:
                if isroot:
                    ctl.append(
                        {"id": ct[0], "menuname": ct[1]})
            else:
                ctl.append(
                    {"id": ct[0],"menuname":ct[1]})
        datajson = json.dumps(ctl, ensure_ascii=False).encode('utf8')
        return HttpResponse(datajson)
    except Exception as e:
        ctl = []
        datajson = json.dumps(ctl, ensure_ascii=False).encode('utf8')
        return HttpResponse(datajson)
def get_grouppermission(request,menuid):
    ctl = []
    try:
        util = Utilities()
        qrs = util.Query('getgroupuser')
        qr = util.QuerySecond('getgrouppermission',menuid)
        pms=[x for x in qr if x[2]==-1]
        isadd=0
        isedit=0
        isdel=0
        isactive=0
        id=-1
        if len(pms)>0:
            id=pms[0][0]
            isadd=int(pms[0][3]) if pms[0][3] else 0
            isedit = int(pms[0][4]) if pms[0][4] else 0
            isdel = int(pms[0][5]) if pms[0][5] else 0
            isactive=1
        ctl.append(
            {"id": id, "groupid": -1, "groupname": u"Others", "menuid": menuid,
             "isactive": True if isactive > 0 else False,
             "isadd": True if isadd > 0 else False, "isedit": True if isedit > 0 else False,
             "iseditall": True if isedit > 1 else False, "isdel": True if isdel > 0 else False,
             "isdelall": True if isdel > 1 else False,"userpermission":[]})
        for ct in qrs:
            pms = [x for x in qr if x[2] == ct[0]]
            isadd = 0
            isedit = 0
            isdel = 0
            isactive = 0
            id=-1
            if len(pms) > 0:
                id = pms[0][0]
                isadd = int(pms[0][3]) if pms[0][3] else 0
                isedit = int(pms[0][4]) if pms[0][4] else 0
                isdel = int(pms[0][5]) if pms[0][5] else 0
                isactive = 1
            ctl.append(
                {"id":id, "groupid": ct[0], "groupname": ct[1], "menuid": menuid,
                 "isactive": True if isactive > 0 else False,
                 "isadd": True if isadd > 0 else False, "isedit": True if isedit > 0 else False,
                 "iseditall": True if isedit > 1 else False, "isdel": True if isdel > 0 else False,
                 "isdelall": True if isdel > 1 else False,"userpermission":[]})
        qr = util.QuerySecond('getuserpermission', menuid)
        for it in ctl:
            userpermission = []
            gid=it["groupid"]
            qrs = util.Query('getuserbygroup',gid)#getuserpermission
            for u in qrs:
                qrrr = util.QuerySecond('getrootbyuser', u[0])
                if qrrr and len(qrrr) > 0:
                    userpermission.append(
                        {"id": id, "userid": u[0], "username": u[1], "menuid": menuid,
                         "isactive": True ,
                         "isadd": True , "isedit": True ,
                         "iseditall": True , "isdel": True,
                         "isdelall": True })
                    it["isactive"]=True
                    it["isadd"] = True
                    it["isedit"] = True
                    it["iseditall"] = True
                    it["isdel"] = True
                    it["isdelall"] = True
                else:
                    pms = [x for x in qr if x[2] == u[0]]
                    isadd = 0
                    isedit = 0
                    isdel = 0
                    isactive = 0
                    id = -1
                    if len(pms) > 0:
                        id = pms[0][0]
                        isadd = int(pms[0][3]) if pms[0][3] else 0
                        isedit = int(pms[0][4]) if pms[0][4] else 0
                        isdel = int(pms[0][5]) if pms[0][5] else 0
                        isactive = 1
                    userpermission.append(
                        {"id": id, "userid": u[0], "username": u[1], "menuid": menuid,
                         "isactive": True if isactive > 0 else False,
                         "isadd": True if isadd > 0 else False, "isedit": True if isedit > 0 else False,
                         "iseditall": True if isedit > 1 else False, "isdel": True if isdel > 0 else False,
                         "isdelall": True if isdel > 1 else False})
            it["userpermission"]=userpermission
        datajson = json.dumps(ctl, ensure_ascii=False).encode('utf8')
        return HttpResponse(datajson)
    except Exception as e:
        ctl = []
        datajson = json.dumps(ctl, ensure_ascii=False).encode('utf8')
        return HttpResponse(datajson)
def get_rootpermission(request):
    ctl = []
    userid = request.user.id
    try:
        util = Utilities()
        qr = util.QuerySecond('getrootlevel')
        pms = [x for x in qr if x[1] == userid and int(x[2])>0]
        if pms and len(pms) > 0:
            permission=True
        else:
            permission=False
        qrs = util.Query('getsysusers')
        if qrs and len(qrs)>0:
            l=len(qrs)
            users = []
            group={"groupid":qrs[0][2],"groupname":qrs[0][3],"users":[]}
            for q in qrs:
                if q[2]==group["groupid"] and q[2]!=qrs[l-1][2]:
                    pms=[x for x in qr if x[1]==q[0]]
                    if pms and len(pms)>0:
                        rootlevel = True if pms[0][2] and int(pms[0][2]) > 0 else False
                        usercreate = pms[0][3]
                    else:
                        rootlevel=False
                        usercreate = userid
                    if rootlevel ==True and ( q[0]==userid or usercreate!=userid):
                        canchange=True
                    else:
                        canchange=False
                    users.append({"userid":q[0],"username":q[1],"rootlevel":rootlevel,"usercreate":usercreate,"canchange":canchange})
                else:
                    if not users or len(qrs)==0:
                        pms = [x for x in qr if x[1] == q[0]]
                        if pms and len(pms) > 0:
                            rootlevel = True if pms[0][2] and int(pms[0][2])>0 else False
                            usercreate=pms[0][3]
                        else:
                            rootlevel = False
                            usercreate=userid
                        if rootlevel ==True and ( q[0] == userid or usercreate != userid):
                            canchange = True
                        else:
                            canchange = False
                        users.append({"userid": q[0], "username": q[1], "rootlevel": rootlevel,"usercreate":usercreate,"canchange":canchange})
                    group["users"]=users
                    ctl.append({"group":group,"permission":permission})
                    users = []
                    group = {"groupid": q[2], "groupname": q[3],"users":[]}
        datajson = json.dumps(ctl, ensure_ascii=False).encode('utf8')
        return HttpResponse(datajson)
    except Exception as e:
        ctl = []
        datajson = json.dumps(ctl, ensure_ascii=False).encode('utf8')
        return HttpResponse(datajson)
def getUserName(id):
    user = UserProfile.objects.filter(user__id=id)
    if user and len(user)>0:
        return user[0].fullname
    return  ''
def getGroupName(id):
    if id==-1:
        return "Others"
    gr = Group.objects.filter(user__id=id)
    if gr and len(gr)>0:
        return gr[0].name
    return "Others"
def getMenuName(id):
    util = Utilities()
    qr=util.QuerySecond('getMenuName',id)
    if qr and len(qr)>0:
        return qr[0][0]
    return  ''
def getVehicleName(id):
    vh = VehicleType.objects.filter(id=id)
    if vh and len(vh)>0:
        return vh[0].name
    return ''
def getGroupTanant(id):
    vh = ClaimPromotionGroupTenant.objects.filter(id=id)
    if vh and len(vh)>0:
        return vh[0].groupname
    return ''
def getCardName(id):
    vh = CardType.objects.filter(id=id)
    if vh and len(vh)>0:
        return vh[0].name
    return ''
def getcyclename(id):
    if id==1:
        return "Liền kề";
    if id==2:
        return "Giờ vàng";
    if id==3:
        return "Đặc biệt";
    return "Không xác định";
def replaceweekname(weekname):
    return  weekname.replace('2',' T2').replace('3',' T3').replace('4',' T4').replace('5',' T5').replace('6',' T6').replace('7',' T7').replace('1',' CN')
@csrf_exempt
def post_samplefees(request):
    if request.method == "POST":
        userid=request.user.id
        id = -1
        cursor = connections['secondary'].cursor()
        feetype = request.POST['feetype']
        callname=request.POST['callname']
        util = Utilities()
        qrss = util.QuerySecond('checkexistsample', callname)
        if not qrss or len(qrss) < 1 or qrss[0][0] == 1:
            return HttpResponse("failname");
        now = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        username=getUserName(userid)
        if feetype=="simple":
            cursor.execute("begin")
            totalfee=request.POST['totalfee']
            feebyday = request.POST['feebyday']
            startdate = request.POST['startdate']
            startnight = request.POST['startnight']
            freetime = 0 if request.POST['freetime'] == ''else int(request.POST['freetime'])
            qr = "insert into samplefee(`callname`,`feetype`,`totalfees`,`inused`,`createbyid`,`startdate`,`startnight`,`feeday`,`freetime`) values('%s','1','%s','1','%s','%s','%s','%s','%s')" % (
                callname, totalfee, userid, startdate, startnight, feebyday,freetime)
            try:
                cursor.execute(qr)
                id = cursor.lastrowid
                qr = "insert into historyaccess(target,useraction,content,actiondate,userid,username) values('Công thức phí - dạng ngày đêm','Thêm','%s','%s','%s','%s')" % (
                callname, now, userid, username)
                cursor.execute(qr)

                cursor.execute("commit")

            except Exception as e:
                cursor.execute("rollback")
        elif feetype=="simple24":
            cursor.execute("begin")
            after24hfee = 0 if request.POST['after24hfee']=='' else int(request.POST['after24hfee'])
            after24htype = 1 if request.POST['after24htype']=='true' else 0
            exceptfee = 1 if request.POST['except'] == 'true' else 0
            freetime = 0 if request.POST['freetime'] == ''else int(request.POST['freetime'])
            #hours=request.POST['hours']
            #moneys = request.POST['moneys']
            blocks=request.POST['blocks']
            jsdata = json.loads(str(blocks))
            desdata=[]
            for it in jsdata:
                desdata.append({"blockhour":it["hour"],"blockfee":it["money"]})
            jsdata = json.dumps(desdata)
            qr = "insert into samplefee(`callname`,`feetype`,`inused`,`createbyid`,`freetime`) values('%s','2','1','%s','%s')" % (callname,userid,freetime)
            try:
                cursor.execute(qr)
                id = cursor.lastrowid
                qr = "insert into fee24detail(`samplefeeid`,`blockhours`,`blockfee`,`affterfee`,`canrepeat`,`exceptfee`) values('%s','%s','%s','%s','%s','%s')" % (
                    id, jsdata,jsdata,after24hfee,after24htype,exceptfee)
                cursor.execute(qr)
                qr = "insert into historyaccess(target,useraction,content,actiondate,userid,username) values('Công thức phí - dạng 24h','Thêm','%s','%s','%s','%s')" % (
                    callname, now, userid, username)
                cursor.execute(qr)
                cursor.execute("commit")

            except Exception as e:
                cursor.execute("rollback")
        elif feetype=="complex":
            cursor.execute("begin")
            startdate = request.POST['startdate']
            datelist=request.POST['datelist']
            try:
                #js=json.dumps(datelist)
                jsdata=json.loads(str(datelist))
            except Exception as e:
                HttpResponse("fail")

            freetime=0 if request.POST['freetime']==''else int(request.POST['freetime'])
            tolerancetime=0 if request.POST['tolerancetime']=='' else int(request.POST['tolerancetime'])
            subtractfree=1 if request.POST['subtractfree']=='true' else 0
            subtracttolerance = 1 if request.POST['subtracttolerance'] == 'true' else 0
            qr = "insert into samplefee(`callname`,`feetype`,`startdate`,`freetime`,`tolerancetime`,`inused`,`createbyid`,`subtractfree`,`subtracttolerance`) values('%s','3','%s','%s','%s','0','%s','%s','%s')" % (
                callname, startdate, freetime, tolerancetime,userid,subtractfree,subtracttolerance)
            try:
                cursor.execute(qr)
                id = cursor.lastrowid
                for it in jsdata:
                    name = it["name"]
                    dayresult = ','.join(str(d) for d in it["dayresult"])
                    try:
                        amounttime1 = int(it["amounttime1"])
                    except:
                        amounttime1=1
                    try:
                        amounttime2 = int(it["amounttime2"])
                    except:
                        amounttime2=0
                    try:
                        amounttime3 = int(it["amounttime3"])
                    except:
                        amounttime3=0
                    qr="insert into `typeofdate`(`samplefeeid`,`weekmap`,`callname`) values('%s','%s','%s')"%(
                        id,dayresult,name)
                    cursor.execute(qr)
                    datetypeid = cursor.lastrowid
                    index = 0
                    while index<amounttime1:
                        qr="insert into cycletime(dateetypeid,cycletype) values(%s,1)"%(datetypeid)
                        cursor.execute(qr)
                        index=index+1
                    index = 0
                    while index < amounttime2:
                        qr = "insert into cycletime(dateetypeid,cycletype) values(%s,2)"%(datetypeid)
                        cursor.execute(qr)
                        index = index + 1
                    index = 0
                    while index < amounttime3:
                        qr = "insert into cycletime(dateetypeid,cycletype) values(%s,3)"%(datetypeid)
                        cursor.execute(qr)
                        index = index + 1
                    qr = "insert into historyaccess(target,useraction,content,actiondate,userid,username) values('Công thức phí - dạng phức hợp','Thêm','%s','%s','%s','%s')" % (
                        callname, now, userid, username)
                    cursor.execute(qr)
                cursor.execute("commit")
            except Exception as e:
                cursor.execute("rollback")
        elif feetype=="country":
            freetime = int(request.POST['freetime']) if request.POST['freetime'] and request.POST['freetime']!='undefined'  and int(request.POST['freetime']) else 0
            fee24 = int(request.POST['fee24']) if request.POST['fee24'] and request.POST['fee24']!='undefined' and int(request.POST['fee24']) else 0
            maxfee =  int(request.POST['maxfee']) if request.POST['maxfee'] and request.POST['maxfee']!='undefined' and int(request.POST['maxfee']) else 0
            startdate = request.POST['startdate']
            startnight = request.POST['startnight']
            formuladay = int(request.POST['formuladay']) if request.POST['formuladay'] else -1
            formulanight = int(request.POST['formulanight'])  if request.POST['formulanight'] else -1
            optioncase = int(request.POST['optioncase']) if request.POST['optioncase'] else  1
            qr = "insert into samplefee(`callname`,`feetype`,`maxfee`,`full24hfee`,`formuladay`,`formulanight`,`optioncase`,`inused`,`createbyid`,`startdate`,`startnight`,`freetime`) " \
                 "values('%s','5','%s','%s','%s','%s','%s','1','%s','%s','%s','%s')" % (
                callname, maxfee,fee24,formuladay,formulanight,optioncase, userid, startdate, startnight, freetime)
            try:
                cursor.execute(qr)
                id = cursor.lastrowid
                qr = "insert into historyaccess(target,useraction,content,actiondate,userid,username) values('Công thức phí - dạng nhà nước mở rộng','Thêm','%s','%s','%s','%s')" % (
                    callname, now, userid, username)
                cursor.execute(qr)

                cursor.execute("commit")

            except Exception as e:
                cursor.execute("rollback")
        else:
            cursor.execute("begin")
            startdate = request.POST['startdate']
            datelist = request.POST['datelist']
            try:
                # js=json.dumps(datelist)
                jsdata = json.loads(str(datelist))
            except Exception as e:
                HttpResponse("fail")
            qr = "insert into samplefee(`callname`,`feetype`,`startdate`,`inused`,`createbyid`) values('%s','4','%s','0','%s')" % (
                callname, startdate, userid)
            try:
                cursor.execute(qr)
                id = cursor.lastrowid
                for it in jsdata:
                    name = it["name"]
                    dayresult = ','.join(str(d) for d in it["dayresult"])
                    try:
                        amounttime1 = int(it["amounttime11"])
                    except:
                        amounttime1=1
                    try:
                        amounttime2 = int(it["amounttime12"])
                    except:
                        amounttime2=0
                    try:
                        amounttime3 = int(it["amounttime13"])
                    except:
                        amounttime3=0
                    qr="insert into `datetyperedemption`(`redemptfeeid`,`weekmap`,`callname`) values('%s','%s','%s')"%(
                        id,dayresult,name)
                    cursor.execute(qr)
                    datetypeid = cursor.lastrowid
                    index = 0
                    while index<amounttime1:
                        qr="insert into cycleredemption(datetypeid,cycletype) values(%s,1)"%(datetypeid)
                        cursor.execute(qr)
                        index=index+1
                    index = 0
                    while index < amounttime2:
                        qr = "insert into cycleredemption(datetypeid,cycletype) values(%s,2)" % (datetypeid)
                        cursor.execute(qr)
                        index = index + 1
                    index = 0
                    while index < amounttime3:
                        qr = "insert into cycleredemption(datetypeid,cycletype) values(%s,3)" % (datetypeid)
                        cursor.execute(qr)
                        index = index + 1
                    qr = "insert into historyaccess(target,useraction,content,actiondate,userid,username) values('Công thức khấu trừ','Thêm','%s','%s','%s','%s')" % (
                        callname, now, userid, username)
                    cursor.execute(qr)
                cursor.execute("commit")
            except Exception as e:
                cursor.execute("rollback")
        try:
            util = Utilities()
            qrs = util.QuerySecond('getsamplefee',id,0)
            # datajson=serializers.serialize('json',cts)
            # now= datetime.now().strftime("%Y-%m-%d %H:%M:%S")
            if len(qrs)>0:
                data = {"id": qrs[0][0], "feename": qrs[0][1], "freetime": qrs[0][2], "tolerancetime": qrs[0][3],
                        "feetype": qrs[0][4], "inused": qrs[0][5], "createdby": getUserName(qrs[0][6]),
                        "totalfees": qrs[0][7], "startdate": str(qrs[0][8]) if qrs[0][8] else '00:00:00',
                        "userid": qrs[0][6], "issubtractfree": True if qrs[0][9] and int(qrs[0][9]) > 0 else False,
                        "issubtracttolerance": True if qrs[0][10] and int(qrs[0][10]) > 0 else False,
                        "feeday": qrs[0][11], "startnight": str(qrs[0][12]) if qrs[0][12] else '00:00:00',
                        "ischange": True if qrs[0][13] and int(qrs[0][13]) > 0 else False,"isvisible": True,"canlock": True if qrs[0][14] and int(qrs[0][14]) == 1 else False,"canunlock": True if qrs[0][14] and int(qrs[0][14]) == 2 else False}
                datajson = json.dumps(data, ensure_ascii=False).encode('utf8')
                return HttpResponse(datajson)
            else:
                HttpResponse("fail")
        except Exception as e:
            return HttpResponse("fail")
#post_sampleregis
@csrf_exempt
def post_sampleregis(request):
    if request.method == "POST":
        userid = request.user.id
        now = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        username = getUserName(userid)
        cursor = connections['secondary'].cursor()
        cursor.execute("begin")
        try:
            datas=request.POST['datas']
            jsdata = json.loads(str(datas))
            ctype=jsdata["cardtype"]
            vtype=jsdata["vehicletype"]
            activedate=jsdata["activedate"]
            sampleid = jsdata["sampleid"]
            sampleid1 = jsdata["sampleid1"]
            sampleid2 = jsdata["sampleid2"]
            atdate=datetime.strptime(activedate, "%Y-%m-%d")
            #getsampleactivetoupdate
            util = Utilities()
            qrs = util.QuerySecond('getsampleactivetoupdate', vtype,ctype)
            idupdate=-1
            if len(qrs)>0:
                idupdate=qrs[0][0]
                if atdate.date()<=datetime.strptime(str(qrs[0][1]),"%Y-%m-%d").date():
                    rp={"result":"fail","data":"Ngày hiệu lực phải lớn hơn %s"%(qrs[0][1].strftime("%Y-%m-%d"))}
                    jsdata=json.dumps(rp, ensure_ascii=False).encode('utf8')
                    return HttpResponse(jsdata)
                expireddate = atdate + timedelta(days=-1)
                expireddate = expireddate.strftime("%Y-%m-%d")
                qr = "update sampleactive set expireddate='%s' where id='%s'" % (
                    expireddate,idupdate)
                cursor.execute(qr)
            if sampleid2 >0 and sampleid1>0 :
                qr = "insert into sampleactive(activedate,cardtype,vehicletype,samplefeeid,samplefeeid1,samplefeeid2,usercreate) values('%s','%s','%s','%s','%s','%s','%s')" % (
                    activedate, ctype, vtype, sampleid, sampleid1,sampleid2, userid)
            elif sampleid2 >0:
                qr = "insert into sampleactive(activedate,cardtype,vehicletype,samplefeeid,samplefeeid2,usercreate) values('%s','%s','%s','%s','%s','%s')" % (
                    activedate, ctype, vtype, sampleid, sampleid2, userid)
            elif sampleid1 > 0:
                qr = "insert into sampleactive(activedate,cardtype,vehicletype,samplefeeid,samplefeeid1,samplefeeid2,usercreate) values('%s','%s','%s','%s','%s','%s','%s')" % (
                    activedate, ctype, vtype, sampleid, sampleid1, sampleid1, userid)
            else:
                qr = "insert into sampleactive(activedate,cardtype,vehicletype,samplefeeid,samplefeeid2,usercreate) values('%s','%s','%s','%s','%s','%s')" % (
                    activedate, ctype, vtype, sampleid,sampleid, userid)
            cursor.execute(qr)
            qr = "insert into historyaccess(target,useraction,content,actiondate,userid,username) values('Biểu phí','Thêm','%s','%s','%s','%s')" % (
                "Loại thẻ: %s, Loại xe: %s, Ngày hiệu lực: %s"%(getCardName(ctype),getVehicleName(vtype),activedate), now, userid, username)
            cursor.execute(qr)
            cursor.execute("commit")
            rp = {"result": "ok", "data": "ok"}
            jsdata = json.dumps(rp, ensure_ascii=False).encode('utf8')
            return HttpResponse(jsdata)
        except Exception as e:
            rp = {"result": "fail", "data": "Dữ liệu nhập không hợp lệ."}
            jsdata = json.dumps(rp, ensure_ascii=False).encode('utf8')
            return HttpResponse(jsdata)
@csrf_exempt
def post_sampleregissimilar(request):
    if request.method == "POST":
        userid = request.user.id
        now = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        username = getUserName(userid)
        cursor = connections['secondary'].cursor()
        cursor.execute("begin")
        try:
            util = Utilities()
            datas=request.POST['datas']
            jsdata = json.loads(str(datas))
            dateactive=jsdata["activedate"]
            atdate = datetime.strptime(dateactive, "%Y-%m-%d")
            sampleid=jsdata["sample"]["sampleselected"]["id"]
            sample1id=jsdata["sample1"]["sampleselected"]["id"]
            sample2id = jsdata["sample2"]["sampleselected"]["id"]
            cardtypes=[]
            for ct in jsdata["cardtypes"]:
                if ct["chosen"]:
                    vhc=[]
                    for v in ct["vehicles"]:
                        if v["chosen"]:
                            vhc.append({"id":v["id"]})
                    if vhc and len(vhc)>0:
                        cardtypes.append({"id":ct["id"],"vehicles":vhc})
            for c in cardtypes:
                cid=c["id"]
                for v in c["vehicles"]:
                    vid=v["id"]
                    qrs = util.QuerySecond('getsampleactivetoupdate', vid, cid)
                    idupdate = -1
                    if len(qrs) > 0:
                        idupdate = qrs[0][0]
                        if atdate.date() <= datetime.strptime(str(qrs[0][1]), "%Y-%m-%d").date():
                           continue
                        else:
                            expireddate = atdate + timedelta(days=-1)
                            expireddate = expireddate.strftime("%Y-%m-%d")
                            qr = "update sampleactive set expireddate='%s' where id='%s'" % (
                                expireddate, idupdate)
                            cursor.execute(qr)
                    if sample2id >0 and sample1id > 0:
                        qr = "insert into sampleactive(activedate,cardtype,vehicletype,samplefeeid,samplefeeid1,samplefeeid2,usercreate) values('%s','%s','%s','%s','%s','%s','%s')" % (
                            dateactive, cid, vid, sampleid, sample1id,sample2id, userid)
                    elif  sample2id>0:
                        qr = "insert into sampleactive(activedate,cardtype,vehicletype,samplefeeid,samplefeeid2,usercreate) values('%s','%s','%s','%s','%s','%s')" % (
                            dateactive, cid, vid, sampleid, sample2id, userid)
                    elif sample1id > 0:
                        qr = "insert into sampleactive(activedate,cardtype,vehicletype,samplefeeid,samplefeeid1,samplefeeid2,usercreate) values('%s','%s','%s','%s','%s','%s','%s')" % (
                            dateactive, cid, vid, sampleid, sample1id, sample1id,userid)
                    else:
                        qr = "insert into sampleactive(activedate,cardtype,vehicletype,samplefeeid,samplefeeid2,usercreate) values('%s','%s','%s','%s','%s','%s')" % (
                            dateactive, cid, vid, sampleid,sampleid, userid)
                    cursor.execute(qr)
                    qr = "insert into historyaccess(target,useraction,content,actiondate,userid,username) values('Biểu phí','Thêm','%s','%s','%s','%s')" % (
                        "Loại thẻ: %s, Loại xe: %s, Ngày hiệu lực: %s" % (getCardName(cid),getVehicleName(vid), dateactive), now, userid,
                        username)
                    cursor.execute(qr)
            cursor.execute("commit")
            rp = {"result": "ok", "data": "ok"}
            jsdata = json.dumps(rp, ensure_ascii=False).encode('utf8')
            return HttpResponse(jsdata)
        except Exception as e:
            rp = {"result": "fail", "data": "Dữ liệu nhập không hợp lệ."}
            jsdata = json.dumps(rp, ensure_ascii=False).encode('utf8')
            return HttpResponse(jsdata)
@csrf_exempt
def post_redemptionregissimilar(request):
    if request.method == "POST":
        userid = request.user.id
        now = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        username = getUserName(userid)
        cursor = connections['secondary'].cursor()
        cursor.execute("begin")
        try:
            util = Utilities()
            datas=request.POST['datas']
            jsdata = json.loads(str(datas))
            dateactive=jsdata["activedate"]
            atdate = datetime.strptime(dateactive, "%Y-%m-%d")
            sampleid=jsdata["sample"]["sampleselected"]["id"]
            sample1id=sampleid
            groups=[]
            for ct in jsdata["groups"]:
                if ct["chosen"]:
                    vhc=[]
                    for v in ct["vehicles"]:
                        if v["chosen"]:
                            vhc.append({"id":v["id"]})
                    if vhc and len(vhc)>0:
                        groups.append({"id":ct["id"],"vehicles":vhc})
            for c in groups:
                cid=c["id"]
                for v in c["vehicles"]:
                    vid=v["id"]
                    qrs = util.QuerySecond('gtredemptionactive', cid, vid)
                    idupdate = -1
                    if len(qrs) > 0:
                        idupdate = qrs[0][0]
                        if atdate.date() <= datetime.strptime(str(qrs[0][1]), "%Y-%m-%d").date():
                           continue
                        else:
                            expireddate = atdate + timedelta(days=-1)
                            expireddate = expireddate.strftime("%Y-%m-%d")
                            qr = "update redemptionactive set expireddate='%s' where id='%s'" % (
                                expireddate, idupdate)
                            cursor.execute(qr)
                    qr = "insert into redemptionactive(activedate,grouptenant,samplefeeid,usercreate,vehicletypeid,sampleid1) values('%s','%s','%s','%s','%s','%s')" % (
                        dateactive, cid, sampleid, userid, vid, sample1id)
                    cursor.execute(qr)
                    qr = "insert into historyaccess(target,useraction,content,actiondate,userid,username) values('Biểu khấu trừ phí','Thêm','%s','%s','%s','%s')" % (
                        "Nhóm cửa hàng: %s, Loại xe: %s, Ngày hiệu lực: %s" % (getGroupTanant(cid),getVehicleName(vid), dateactive), now, userid,
                        username)
                    cursor.execute(qr)
            cursor.execute("commit")
            rp = {"result": "ok", "data": "ok"}
            jsdata = json.dumps(rp, ensure_ascii=False).encode('utf8')
            return HttpResponse(jsdata)
        except Exception as e:
            rp = {"result": "fail", "data": "Dữ liệu nhập không hợp lệ."}
            jsdata = json.dumps(rp, ensure_ascii=False).encode('utf8')
            return HttpResponse(jsdata)
@csrf_exempt
def post_changepermission(request):
    if request.method == "POST":
        userid = request.user.id
        now = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        username = getUserName(userid)
        cursor = connections['secondary'].cursor()
        cursor.execute("begin")
        try:
            datas = request.POST['datas']
            jsdata = json.loads(str(datas))
            for ag in jsdata["listpermission"]:
                agid=ag["id"]
                menuid=ag["menuid"]
                groupid = ag["groupid"]
                users=ag["userpermission"]
                if ag["isactive"]:
                    isadd=1 if ag["isadd"] else 0
                    isedit=1 if ag["isedit"] else 0
                    if ag["iseditall"]:
                        isedit=2
                    isdel=1 if ag["isdel"] else 0
                    if ag["isdelall"]:
                        isdel=2
                    if agid==-1:
                        qr = "insert into historyaccess(target,useraction,content,actiondate,userid,username) values('Phân quyền - nhóm','Thêm','%s','%s','%s','%s')" % (
                            "Chức năng: %s, Nhóm: %s" % (getMenuName(menuid), getGroupName(groupid)), now, userid, username)
                        cursor.execute(qr)
                        qr = "insert into groupuserpermission(menuid,groupid,isadd,isedit,isdel) values('%s','%s','%s','%s','%s')" % (
                        menuid, groupid, isadd, isedit, isdel)
                    else:
                        qr = "insert into historyaccess(target,useraction,content,actiondate,userid,username) values('Phân quyền - nhóm','Cập nhật','%s','%s','%s','%s')" % (
                            "Chức năng: %s, Nhóm: %s" % (getMenuName(menuid), getGroupName(groupid)), now, userid,
                            username)
                        cursor.execute(qr)
                        qr="update groupuserpermission set isadd='%s',isedit='%s',isdel='%s' where id='%s'"%(isadd,isedit,isdel,agid)
                    cursor.execute(qr)
                    for u in users:
                        usid = u["userid"]
                        uid=u["id"]
                        if u["isactive"]:
                            isaddu = 1 if u["isadd"] else 0
                            if isaddu>isadd:
                                isaddu=isadd
                            iseditu = 1 if u["isedit"] else 0
                            if u["iseditall"]:
                                iseditu = 2
                            if iseditu>isedit:
                                iseditu-isedit
                            isdelu = 1 if u["isdel"] else 0
                            if u["isdelall"]:
                                isdelu = 2
                            if isdelu>isdel:
                                isdelu=isdel
                            if uid == -1:

                                qr = "insert into historyaccess(target,useraction,content,actiondate,userid,username) values('Phân quyền - nhân viên','Thêm','%s','%s','%s','%s')" % (
                                    "Chức năng: %s, Nhân viên: %s" % (getMenuName( menuid), getUserName(usid)), now, userid,
                                    username)
                                cursor.execute(qr)
                                qr = "insert into userpermission(menuid,userid,isadd,isedit,isdel) values('%s','%s','%s','%s','%s')" % (
                                    menuid, usid, isaddu, iseditu, isdelu)
                            else:
                                qr = "insert into historyaccess(target,useraction,content,actiondate,userid,username) values('Phân quyền - nhân viên','Cập nhật','%s','%s','%s','%s')" % (
                                    "Chức năng: %s, Nhân viên: %s" % (getMenuName( menuid),getUserName(usid)), now, userid,
                                    username)
                                cursor.execute(qr)
                                qr = "update userpermission set isadd='%s',isedit='%s',isdel='%s' where id='%s'" % (
                                isaddu, iseditu, isdelu, uid)
                            cursor.execute(qr)
                        elif uid>-1:

                            qr = "delete from userpermission where id='%s'" % (uid)
                            cursor.execute(qr)
                            qr = "insert into historyaccess(target,useraction,content,actiondate,userid,username) values('Phân quyền - nhân viên','Xóa','%s','%s','%s','%s')" % (
                                "Chức năng: %s, Nhân viên: %s" % (getMenuName(menuid), getUserName(usid)), now, userid,
                                username)
                            cursor.execute(qr)
                else:
                    for u in users:
                        usid = u["userid"]
                        uid=u["id"]
                        if uid > -1:
                            qr = "delete from userpermission where id='%s'" % (uid)
                            cursor.execute(qr)
                            qr = "insert into historyaccess(target,useraction,content,actiondate,userid,username) values('Phân quyền - nhân viên','Xóa','%s','%s','%s','%s')" % (
                                "Chức năng: %s, Nhân viên: %s" % (getMenuName(menuid), getUserName(usid)), now, userid,
                                username)
                            cursor.execute(qr)
                    if agid>-1:
                        qr = "delete from groupuserpermission where id='%s'" % (agid)
                        cursor.execute(qr)
                        qr = "insert into historyaccess(target,useraction,content,actiondate,userid,username) values('Phân quyền - nhân viên','Xóa','%s','%s','%s','%s')" % (
                            "Chức năng: %s, Nhóm: %s" % (getMenuName(menuid), getGroupName(groupid)), now, userid,
                            username)
                        cursor.execute(qr)
            cursor.execute("commit")
            rp = {"result": "ok", "data": "ok"}
            jsdata = json.dumps(rp, ensure_ascii=False).encode('utf8')
            return HttpResponse(jsdata)
        except Exception as e:
            cursor.execute("rollback")
            rp = {"result": "fail", "data": "Dữ liệu nhập không hợp lệ."}
            jsdata = json.dumps(rp, ensure_ascii=False).encode('utf8')
            return HttpResponse(jsdata)
@csrf_exempt
def post_changepermissionroot(request):
    if request.method == "POST":
        userid = request.user.id
        now = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        username = getUserName(userid)
        cursor = connections['secondary'].cursor()
        cursor.execute("begin")
        try:
            datas = request.POST['datas']
            jsdata = json.loads(str(datas))
            #insert into userroot(`userid`,`levelroot`,`usercreate`) values('%s','%s','%s')%()
            for g in jsdata:
                for u in g["group"]["users"]:
                    uid = u["userid"]
                    if u["rootlevel"]:
                        qr="select u.id from userroot u where u.userid='%s';"%(uid)
                        cursor.execute(qr)
                        rows = cursor.fetchall()
                        if rows and len(rows)>0:
                            qr="update userroot set levelroot=1 where userid='%s'"%(uid)
                        else:
                            qr="insert into userroot(userid,usercreate,levelroot) values('%s','%s',1)"%(uid,userid)
                        cursor.execute(qr)
                    else:
                        qr="delete from userroot where userid='%s'"%(uid)
                        cursor.execute(qr)
            cursor.execute("commit")
            rp = {"result": "ok", "data": "ok"}
            jsdata = json.dumps(rp, ensure_ascii=False).encode('utf8')
            return HttpResponse(jsdata)
        except Exception as e:
            cursor.execute("rollback")
            rp = {"result": "fail", "data": "Dữ liệu nhập không hợp lệ."}
            jsdata = json.dumps(rp, ensure_ascii=False).encode('utf8')
            return HttpResponse(jsdata)
@csrf_exempt
def post_redemptionregis(request):
    if request.method == "POST":
        userid = request.user.id
        now = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        username = getUserName(userid)
        cursor = connections['secondary'].cursor()
        cursor.execute("begin")
        try:
            datas = request.POST['datas']
            jsdata = json.loads(str(datas))
            tenantgroupid = jsdata["tenantgroupid"]
            activedate = jsdata["activedate"]
            sampleid = jsdata["sampleid"]
            sampleid1 = jsdata["sampleid1"]
            vehicleid=jsdata["vehicleid"]
            atdate = datetime.strptime(activedate, "%Y-%m-%d")
            # getsampleactivetoupdate
            util = Utilities()
            qrs = util.QuerySecond('gtredemptionactive', tenantgroupid, vehicleid)
            idupdate = -1
            if len(qrs) > 0:
                idupdate = qrs[0][0]
                if atdate.date() <= datetime.strptime(str(qrs[0][1]), "%Y-%m-%d").date():
                    rp = {"result": "fail",
                          "data": "Ngày hiệu lực phải lớn hơn %s" % (qrs[0][1].strftime("%Y-%m-%d"))}
                    jsdata = json.dumps(rp, ensure_ascii=False).encode('utf8')
                    return HttpResponse(jsdata)
                expireddate = atdate + timedelta(days=-1)
                expireddate = expireddate.strftime("%Y-%m-%d")
                qr = "update redemptionactive set expireddate='%s' where id='%s'" % (
                    expireddate, idupdate)
                cursor.execute(qr)
            qr = "insert into redemptionactive(activedate,grouptenant,samplefeeid,usercreate,vehicletypeid,sampleid1) values('%s','%s','%s','%s','%s','%s')" % (
                activedate, tenantgroupid, sampleid, userid,vehicleid,sampleid1)
            cursor.execute(qr)
            qr = "insert into historyaccess(target,useraction,content,actiondate,userid,username) values('Biểu khấu trừ phí','Thêm','%s','%s','%s','%s')" % (
               "Nhóm cửa hàng: %s, Loại xe: %s, Ngày hiệu lực: %s" % (getGroupTanant(tenantgroupid),
                getVehicleName(vehicleid), activedate), now, userid, username)
            cursor.execute(qr)
            cursor.execute("commit")
            rp = {"result": "ok", "data": "ok"}
            jsdata = json.dumps(rp, ensure_ascii=False).encode('utf8')
            return HttpResponse(jsdata)
        except Exception as e:
            cursor.execute("rollback")
            rp = {"result": "fail", "data": "Dữ liệu nhập không hợp lệ."}
            jsdata = json.dumps(rp, ensure_ascii=False).encode('utf8')
            return HttpResponse(jsdata)

@csrf_exempt
def post_callredemtion(request):
    if request.method == "POST":
        try:
            datas = request.POST['datas']
            jsdata = json.loads(str(datas))
            checkintime=jsdata["checkintime"]
            vehicleid=jsdata["vehicleid"]
            redemtiontime = jsdata["redemtiontime"]
            activetime = datetime.strptime(redemtiontime, "%Y%m%d%H%M%S")
            citime=datetime.strptime(checkintime, "%Y%m%d%H%M%S")
            groups = jsdata["groups"]
            details=[]
            rdfee=0;
            for g in groups:
                activeid = findredemptionactiveid(vehicleid,g["group"]["id"],activetime.date())
                detail = callredemtion(activeid,g["billamount"],g["group"]["groupname"],activetime, citime)
                rdfee =rdfee+int(detail["redemptionfee"])
                details.append(detail)
            rp = {"result": "ok", "data": {"redemtionresult":rdfee,"details":details}}
            jsdata = json.dumps(rp, ensure_ascii=False).encode('utf8')
            return HttpResponse(jsdata)
        except Exception as e:
            rp = {"result": "fail", "data": "Dữ liệu nhập không hợp lệ."}
            jsdata = json.dumps(rp, ensure_ascii=False).encode('utf8')
            return HttpResponse(jsdata)
#post_changegrouptenant
@csrf_exempt
def post_changegrouptenant(request):
    if request.method == "POST":
        userid = request.user.id
        now = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        username = getUserName(userid)
        cursor = connections['secondary'].cursor()
        cursor.execute("begin")
        try:
            datas=request.POST['datas']
            jsdata = json.loads(str(datas))
            for tn in jsdata:
                id=tn["id"]
                selectedindex=tn["selectedindex"]
                gsid=tn["groupselected"]["id"]
                gid=tn["group"][selectedindex]["id"]
                if gsid>0 and gsid!=gid:
                    qr = "update tenants set tenantgroupid='%s' where id='%s';" % (gsid,id)
                    cursor.execute(qr)
                    qr = "insert into historyaccess(target,useraction,content,actiondate,userid,username) values('Cửa hàng - đổi nhóm đối tác','Cập nhật','%s','%s','%s','%s')" % (
                        "Nhóm đối tác: %s, Cửa hàng: %s" % (
                            gsid, id), now, userid, username)
                    cursor.execute(qr)
            cursor.execute("commit")
            rp = {"result": "ok", "data": "ok"}
            jsdata = json.dumps(rp, ensure_ascii=False).encode('utf8')
            return HttpResponse(jsdata)
        except Exception as e:
            rp = {"result": "fail", "data": "Dữ liệu nhập không hợp lệ."}
            jsdata = json.dumps(rp, ensure_ascii=False).encode('utf8')
            return HttpResponse(jsdata)
@csrf_exempt
def post_sample24h(request):
    if request.method == "POST":
        userid = request.user.id
        username=getUserName(userid)
        now = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        cursor = connections['secondary'].cursor()
        id=request.POST['id']

        after24hfee = 0 if request.POST['after24hfee'] == '' else int(request.POST['after24hfee'])
        after24htype = 1 if request.POST['after24htype'] == 'true' else 0
        exceptfee = 1 if request.POST['except'] == 'true' else 0
        blocks = request.POST['blocks']
        jsdata = json.loads(str(blocks))
        desdata = []
        for it in jsdata:
            desdata.append({"blockhour": it["blockhour"], "blockfee": it["blockfee"]})
        jsdata = json.dumps(desdata)
        qr = "update fee24detail set blockhours='%s',blockfee='%s',affterfee='%s',canrepeat='%s',exceptfee='%s' where id='%s'" % (
            jsdata, jsdata,after24hfee,after24htype,exceptfee,id)
        cursor.execute("begin")
        try:
            cursor.execute(qr)
            util = Utilities()
            qrs = util.QuerySecond('getsamplefee',id,0)
            if qrs and len(qrs)>0:
                callname=qrs[0][1]
            else:
                callname=''
            qr = "insert into historyaccess(target,useraction,content,actiondate,userid,username) values('Công thức phí - dạng 24h','Cập nhật','%s','%s','%s','%s')" % (
                callname, now, userid, username)
            cursor.execute(qr)
            cursor.execute("commit")
            return HttpResponse("ok")
        except Exception as e:
            cursor.execute("rollback")
            HttpResponse("fail")
@csrf_exempt
def post_complex(request):
    if request.method == "POST":
        userid = request.user.id
        username = getUserName(userid)
        now = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        cursor = connections['secondary'].cursor()
        cursor.execute("begin")
        try:
            datas=request.POST['datas']
            jsdata = json.loads(str(datas))
            samplefeeid=-1
            ##check validate cycles
            # sttt=None
            # ettt=None
            # for dt in jsdata:
            #     if samplefeeid==-1:
            #         samplefeeid = dt["samplefeeid"]
            #         util = Utilities()
            #         qrs = util.QuerySecond('getsamplefee', samplefeeid, 0)
            #         if len(qrs)<1:
            #             cursor.execute("rollback")
            #             rp = {"result": "fail", "data": "Dữ liệu nhập không hợp lệ."}
            #             jsdata = json.dumps(rp, ensure_ascii=False).encode('utf8')
            #             return HttpResponse(jsdata)
            #         starttime="2018-04-25 %s"%(str(qrs[0][8]) if qrs[0][8] else '00:00:00')
            #         sttt=datetime.strptime(starttime, "%Y-%m-%d %H:%M:%S")
            #         ettt=sttt+timedelta(days=1)+timedelta(seconds=-1)
            #     if sttt and ettt:
            #         cycletimes = dt["cycletimes"]
            #         cycles1 = [x for x in cycletimes if x["cycle"] == 1]
            #         cycles2 = [x for x in cycletimes if x["cycle"] == 2]
            #         cycles3 = [x for x in cycletimes if x["cycle"] == 3]
            #         lsttime=[]
            #         for cc in cycles1:
            #             fr = "2018-04-25 %s" % (cc["fromtime"])
            #             fromtime = datetime.strptime(fr, "%Y-%m-%d %H:%M:%S")
            #             to = "2018-04-25 %s" % (cc["totime"])
            #             totime = datetime.strptime(to, "%Y-%m-%d %H:%M:%S")
            #             if totime < fromtime:
            #                 totime = totime + timedelta(days=1)
            #             lsttime.append({"from":fromtime,"to":totime})
            #         l=len(lsttime)
            #         if l>0:
            #             if lsttime[0]["from"]!=sttt or lsttime[l-1]["to"]!=ettt:
            #                 cursor.execute("rollback")
            #                 rp = {"result": "fail", "data": "Dữ liệu nhập không hợp lệ."}
            #                 jsdata = json.dumps(rp, ensure_ascii=False).encode('utf8')
            #                 return HttpResponse(jsdata)
            #             id=0
            #             for t in lsttime:
            #                 if id+1<l:
            #                     if lsttime[id]["to"]+timedelta(seconds=1)!=lsttime[id+1]["from"]:
            #                         cursor.execute("rollback")
            #                         rp = {"result": "fail", "data": "Dữ liệu nhập không hợp lệ."}
            #                         jsdata = json.dumps(rp, ensure_ascii=False).encode('utf8')
            #                         return HttpResponse(jsdata)
            #                 id=id+1
            #         lsttime = []
            #         for cc in cycles3:
            #             fr = "2018-04-25 %s" % (cc["fromtime"])
            #             fromtime = datetime.strptime(fr, "%Y-%m-%d %H:%M:%S")
            #             to = "2018-04-25 %s" % (cc["totime"])
            #             totime = datetime.strptime(to, "%Y-%m-%d %H:%M:%S")
            #             if totime < fromtime:
            #                 totime = totime + timedelta(days=1)
            #             lsttime.append({"from": fromtime, "to": totime})
            #         l = len(lsttime)
            #         if l > 0:
            #             if lsttime[0]["from"] != sttt or lsttime[l-1]["to"] != ettt:
            #                 cursor.execute("rollback")
            #                 rp = {"result": "fail", "data": "Dữ liệu nhập không hợp lệ."}
            #                 jsdata = json.dumps(rp, ensure_ascii=False).encode('utf8')
            #                 return HttpResponse(jsdata)
            #             id = 0
            #             for t in lsttime:
            #                 if id + 1 < l:
            #                     if lsttime[id]["to"] + timedelta(seconds=1) != lsttime[id + 1]["from"]:
            #                         cursor.execute("rollback")
            #                         rp = {"result": "fail", "data": "Dữ liệu nhập không hợp lệ."}
            #                         jsdata = json.dumps(rp, ensure_ascii=False).encode('utf8')
            #                         return HttpResponse(jsdata)
            #                 id = id + 1
            #         lsttime = []
            #         for cc in cycles2:
            #             fr = "2018-04-25 %s" % (cc["fromtime"])
            #             fromtime = datetime.strptime(fr, "%Y-%m-%d %H:%M:%S")
            #             to = "2018-04-25 %s" % (cc["totime"])
            #             totime = datetime.strptime(to, "%Y-%m-%d %H:%M:%S")
            #             if totime < fromtime:
            #                 totime = totime + timedelta(days=1)
            #             lsttime.append({"from": fromtime, "to": totime})
            #         l = len(lsttime)
            #         if l > 0:
            #             id = 0
            #             for t in lsttime:
            #                 if id + 1 < l:
            #                     if lsttime[id]["to"]  >= lsttime[id + 1]["from"]:
            #                         cursor.execute("rollback")
            #                         rp = {"result": "fail", "data": "Dữ liệu nhập không hợp lệ."}
            #                         jsdata = json.dumps(rp, ensure_ascii=False).encode('utf8')
            #                         return HttpResponse(jsdata)
            #                 id = id + 1
            # ##
            for dt in jsdata:
                samplefeeid=dt["samplefeeid"]
                datetypeid=dt["id"]
                callname=dt["callname"]
                cycletimes=dt["cycletimes"]
                for cc in cycletimes:
                    ccid=cc["id"]
                    fromtime=cc["fromtime"]
                    totime = cc["totime"]
                    formula=cc["formulaselected"]["id"]
                    qr = "update `cycletime` set formula='%s',fromtime='%s',totime='%s' where id='%s'" % (
                        formula, fromtime, totime, ccid)
                    cursor.execute(qr)
                totalfee=0
                cycles=[x for x in cycletimes if x["cycle"]==1]
                for cc in cycles:
                    fr="2018-04-25 %s"%(cc["fromtime"])
                    fromtime=datetime.strptime(fr, "%Y-%m-%d %H:%M:%S")
                    to="2018-04-25 %s"%(cc["totime"])
                    totime = datetime.strptime(to, "%Y-%m-%d %H:%M:%S")
                    if totime < fromtime:
                        totime=totime+timedelta(days=1)
                    totalseconds=(totime-fromtime).total_seconds()
                    dm=divmod(totalseconds,3600)
                    totalhours=int(dm[0])+(1 if dm[1]>0 else 0)
                    fml=cc["formulaselected"]
                    totalfee=totalfee+get_fee_from_formula(fml,totalhours)
                qr="update typeofdate set callname='%s',feefullday='%s' where id='%s'"%(callname,totalfee,datetypeid)
                cursor.execute(qr)
            qr="update samplefee set inused='1' where id='%s'"%(samplefeeid)
            cursor.execute(qr)
            util = Utilities()
            qrs = util.QuerySecond('getsamplefee', samplefeeid, 0)
            if qrs and len(qrs) > 0:
                callname = qrs[0][1]
            else:
                callname = ''
            qr = "insert into historyaccess(target,useraction,content,actiondate,userid,username) values('Công thức phí - dạng phức hợp','Cập nhật','%s','%s','%s','%s')" % (
                callname, now, userid, username)
            cursor.execute(qr)
            cursor.execute("commit")
            rp = {"result": "ok", "data": "ok."}
            jsdata = json.dumps(rp, ensure_ascii=False).encode('utf8')
            return HttpResponse(jsdata)
        except Exception as e:
            cursor.execute("rollback")
            rp = {"result": "fail", "data": "Dữ liệu nhập không hợp lệ."}
            jsdata = json.dumps(rp, ensure_ascii=False).encode('utf8')
            return HttpResponse(jsdata)
@csrf_exempt
def post_redemption(request):
    if request.method == "POST":
        userid = request.user.id
        username = getUserName(userid)
        now = datetime.now().strftime("%Y-%m-%d %H:%M:%S")

        cursor = connections['secondary'].cursor()
        cursor.execute("begin")
        try:
            datas=request.POST['datas']
            jsdata = json.loads(str(datas))
            samplefeeid=-1
            for dt in jsdata:
                samplefeeid=dt["samplefeeid"]
                datetypeid=dt["id"]
                callname=dt["callname"]
                cycletimes=dt["cycletimes"]
                for cc in cycletimes:
                    ccid=cc["id"]
                    fromtime=cc["fromtime"]
                    totime = cc["totime"]
                    formula=cc["formulaselected"]["id"]
                    qr = "update `cycleredemption` set formulabill='%s',fromtime='%s',totime='%s' where id='%s'" % (
                        formula, fromtime, totime, ccid)
                    cursor.execute(qr)
                qr="update `datetyperedemption` set `callname`='%s' where id='%s'"%(callname,datetypeid)
                cursor.execute(qr)
            qr = "update samplefee set inused='1' where id='%s'" % (samplefeeid)
            cursor.execute(qr)
            util = Utilities()

            qrs = util.QuerySecond('getsamplefee', samplefeeid, 0)
            if qrs and len(qrs) > 0:
                callname = qrs[0][1]
            else:
                callname = ''
            qr = "insert into historyaccess(target,useraction,content,actiondate,userid,username) values('Công thức khấu trừ','Cập nhật','%s','%s','%s','%s')" % (
                callname, now, userid, username)
            cursor.execute(qr)
            cursor.execute("commit")
            return HttpResponse("ok")
        except Exception as e:
            cursor.execute("rollback")
            HttpResponse("fail")
@csrf_exempt
def post_formulafee(request):
    if request.method == "POST":
        cursor = connections['secondary'].cursor()
        try:
            userid = request.user.id
            username = getUserName(userid)
            now = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
            feetypes = request.POST['feetypes']
            callname=request.POST['callname']
            util = Utilities()
            qrss = util.QuerySecond('checkexistfee', callname)
            if not qrss or len(qrss) < 1 or qrss[0][0] == 1:
                return HttpResponse("failname");
            detail=''
            fullfee=0
            desdata = []
            if feetypes=='2':
               detail= request.POST['detail']
               jsdd=json.loads(str(detail))

               for it in jsdd:
                   desdata.append({"hours": it["hours"], "money": it["money"],"des": it["desselected"]["des"], "isonly":it["isonly"]})
               detail = json.dumps(desdata, ensure_ascii=False).encode('utf8')
            else:
                fullfee=request.POST['fullfee']
            qr = "insert into `feeformula`(`feetype`,`callname`,`detail`,`fullfee`,`usercreate`) values('%s','%s','%s','%s','%s')" % (
                feetypes, callname, detail, fullfee, userid)
            cursor.execute("begin")
            cursor.execute(qr)
            id = cursor.lastrowid
            qr = "insert into historyaccess(target,useraction,content,actiondate,userid,username) values('Khai báo phí','Thêm','%s','%s','%s','%s')" % (
                callname, now, userid, username)
            cursor.execute(qr)
            cursor.execute("commit")
            dtjs = {"id": id, "feetype": feetypes, "callname": callname, "detail": desdata if detail!="" else "",
                    "fullfee": fullfee, "createdby": getUserName(userid),"userid":userid,"isvisible":True,"ischange":True}
            datajson = json.dumps(dtjs, ensure_ascii=False).encode('utf8')
            return HttpResponse(datajson)
        except Exception as e:
            cursor.execute("rollback")
            HttpResponse("fail")
@csrf_exempt
def post_formulabill(request):
    if request.method == "POST":
        cursor = connections['secondary'].cursor()
        try:
            userid = request.user.id
            username = getUserName(userid)
            now = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
            callname = request.POST['callname']
            util = Utilities()
            qrss = util.QuerySecond('checkexistbill', callname)
            if not qrss or len(qrss) < 1 or qrss[0][0] == 1:
                return HttpResponse("failname");
            detail = request.POST['detail']
            jsdd = json.loads(str(detail))
            desdata=[]
            for it in jsdd:
                desdata.append(
                    {"billamount": it["billamount"], "deductionamount": it["deductionamount"]})
                detail = json.dumps(desdata, ensure_ascii=False).encode('utf8')
            qr = "insert into billformula(callname,detail,usercreate) values('%s','%s','%s')"%(callname,detail,userid)
            cursor.execute("begin")
            cursor.execute(qr)
            id = cursor.lastrowid
            qr = "insert into historyaccess(target,useraction,content,actiondate,userid,username) values('Định mức khấu trừ','Thêm','%s','%s','%s','%s')" % (
                callname, now, userid, username)
            cursor.execute(qr)
            cursor.execute("commit")
            dtjs = {"id": id, "callname": callname, "detail": desdata if detail != "" else "",
                    "createdby": getUserName(userid),"ischange":True,"userid":userid,"isvisible":True}
            datajson = json.dumps(dtjs, ensure_ascii=False).encode('utf8')
            return HttpResponse(datajson)
        except Exception as e:
            cursor.execute("rollback")
            HttpResponse("fail")
@csrf_exempt
def post_changestate(request):
    if request.method == "POST":
        cursor = connections['secondary'].cursor()
        try:
            userid = request.user.id
            username = getUserName(userid)
            now = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
            id = request.POST['id']
            type = 3 if request.POST['type'] and request.POST['type']=="lock" else 2
            content = "Khóa" if request.POST['type'] and request.POST['type'] == "lock" else "Mở khóa"
            feetype="Công thức khấu trừ" if request.POST['feetype'] and request.POST['feetype'] =="4" else "Công thức phí"
            fname=request.POST['fname']
            cursor.execute("begin")
            qr = "update samplefee set inused=%s where id=%s"%(type,id)
            cursor.execute(qr)
            qr = "insert into historyaccess(target,useraction,content,actiondate,userid,username) values('%s','%s','%s','%s','%s','%s')" % (
                feetype,content,fname, now, userid, username)
            cursor.execute(qr)
            cursor.execute("commit")
            dtjs={"result":"ok","data":"ok"}
            datajson = json.dumps(dtjs, ensure_ascii=False).encode('utf8')
            return HttpResponse(datajson)
        except Exception as e:
            cursor.execute("rollback")
            dtjs = {"result": "fail", "data": "Không thể xóa. Vui lòng thử lại!"}
            datajson = json.dumps(dtjs, ensure_ascii=False).encode('utf8')
            return HttpResponse(datajson)
@csrf_exempt
def post_copysample(request):
    if request.method == "POST":
        cursor = connections['secondary'].cursor()
        util = Utilities()
        last_id = -1
        try:
            userid = request.user.id
            username = getUserName(userid)
            now = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
            id = request.POST['id']
            copyname=request.POST['copyname']
            qrs = util.QuerySecond('getsamplefee', id, 0)
            if qrs and len(qrs)==1:
                cursor.execute("begin")
                feetype=qrs[0][4]
                callname =copyname if copyname and copyname !='undefined' and  copyname !='' else  "%s_%s" % (qrs[0][1], now)
                totalfee = qrs[0][7] if qrs[0][7] else 0
                feebyday = qrs[0][11] if qrs[0][11] else 0
                startdate = str(qrs[0][8]) if qrs[0][8] else '00:00:00'
                startnight = str(qrs[0][12]) if qrs[0][12] else '00:00:00'
                freetime = qrs[0][2] if qrs[0][2] else 0
                tolerancetime = qrs[0][3] if qrs[0][3] else 0
                subtractfree = qrs[0][9] if qrs[0][9] else 0
                subtracttolerance = qrs[0][10] if qrs[0][10] else 0
                qrss=util.QuerySecond('checkexistsample', callname)
                if not qrss or len(qrss)<1 or qrss[0][0]==1:
                    return HttpResponse("failname");
                if feetype==1:
                    qr = "insert into samplefee(`callname`,`feetype`,`totalfees`,`inused`,`createbyid`,`startdate`,`startnight`,`feeday`,`freetime`) values('%s','1','%s','1','%s','%s','%s','%s','%s')" % (
                        callname, totalfee, userid, startdate, startnight, feebyday, freetime)
                    cursor.execute(qr)
                    last_id = cursor.lastrowid
                    qr = "insert into historyaccess(target,useraction,content,actiondate,userid,username) values('Công thức phí - dạng ngày đêm','Sao chép','%s','%s','%s','%s')" % (
                        callname, now, userid, username)
                    cursor.execute(qr)

                    cursor.execute("commit")
                elif feetype==2:
                    qr = "insert into samplefee(`callname`,`feetype`,`inused`,`createbyid`,`freetime`) values('%s','2','1','%s','%s')" % (
                    callname, userid,freetime)
                    cursor.execute(qr)
                    last_id = cursor.lastrowid
                    qrd = util.QuerySecond('getdetailfee24h', id)
                    if qrd and len(qrd)==1:
                        block=qrd[0][2] if qrd[0][2] else '[]'
                        after24hfee=qrd[0][4] if qrd[0][4] else 0
                        after24htype=qrd[0][5] if qrd[0][5] else 0
                        exceptfee=qrd[0][6] if qrd[0][6] else 0
                        qr = "insert into fee24detail(`samplefeeid`,`blockhours`,`blockfee`,`affterfee`,`canrepeat`,`exceptfee`) values('%s','%s','%s','%s','%s','%s')" % (
                            last_id, block, block, after24hfee, after24htype, exceptfee)
                        cursor.execute(qr)
                    qr = "insert into historyaccess(target,useraction,content,actiondate,userid,username) values('Công thức phí - dạng 24h','Sao chép','%s','%s','%s','%s')" % (
                        callname, now, userid, username)
                    cursor.execute(qr)
                    cursor.execute("commit")
                elif feetype==3:
                    qr = "insert into samplefee(`callname`,`feetype`,`startdate`,`freetime`,`tolerancetime`,`inused`,`createbyid`,`subtractfree`,`subtracttolerance`) values('%s','3','%s','%s','%s','1','%s','%s','%s')" % (
                        callname, startdate, freetime, tolerancetime, userid, subtractfree, subtracttolerance)
                    cursor.execute(qr)
                    last_id = cursor.lastrowid
                    qrd = util.QuerySecond('getdatetypecomplex', id)
                    for rd in qrd:
                        name=rd[1] if rd[1] else ''
                        dayresult=rd[2] if rd[1] else ''
                        dtid=rd[0] if rd[0] else -1
                        qr = "insert into `typeofdate`(`samplefeeid`,`weekmap`,`callname`) values('%s','%s','%s')" % (
                            last_id, dayresult, name)
                        cursor.execute(qr)
                        last_id_dt = cursor.lastrowid
                        qrdd = util.QuerySecond('getcyclecomplex', dtid)
                        for rc in qrdd:
                            f_time=str(rc[1]) if rc[1] else '00:00:00'
                            t_time = str(rc[2]) if rc[2] else '00:00:00'
                            fm=rc[4] if rc[4] else -1
                            ct=rc[5] if rc[4] else -1
                            qr = "insert into cycletime(fromtime,totime,formula,dateetypeid,cycletype) values('%s','%s','%s','%s','%s')" % (f_time,t_time,fm,last_id_dt,ct)
                            cursor.execute(qr)
                    qr = "insert into historyaccess(target,useraction,content,actiondate,userid,username) values('Công thức phí - dạng phức hợp','Sao chép','%s','%s','%s','%s')" % (
                        callname, now, userid, username)
                    cursor.execute(qr)
                    cursor.execute("commit")
                elif feetype==4:
                    qr = "insert into samplefee(`callname`,`feetype`,`startdate`,`inused`,`createbyid`) values('%s','4','%s','1','%s')" % (
                        callname, startdate, userid)
                    cursor.execute(qr)
                    last_id = cursor.lastrowid
                    qrd = util.QuerySecond('getdatetyperedemption',id)
                    for rd in qrd:
                        name=rd[1] if rd[1] else ''
                        dayresult=rd[2] if rd[1] else ''
                        dtid=rd[0] if rd[0] else -1
                        qr = "insert into `datetyperedemption`(`redemptfeeid`,`weekmap`,`callname`) values('%s','%s','%s')" % (
                            last_id, dayresult, name)
                        cursor.execute(qr)
                        last_id_dt = cursor.lastrowid
                        qrdd = util.QuerySecond('getcycleredemption', dtid)
                        for rc in qrdd:
                            f_time=str(rc[1]) if rc[1] else '00:00:00'
                            t_time = str(rc[2]) if rc[2] else '00:00:00'
                            fm=rc[3] if rc[4] else -1
                            ct=rc[5] if rc[4] else -1
                            qr = "insert into cycleredemption(fromtime,totime,formulabill,datetypeid,cycletype) values('%s','%s','%s','%s','%s')" % (f_time,t_time,fm,last_id_dt,ct)
                            cursor.execute(qr)
                    qr = "insert into historyaccess(target,useraction,content,actiondate,userid,username) values('Công thức khấu trừ','Sao chép','%s','%s','%s','%s')" % (
                        callname, now, userid, username)
                    cursor.execute(qr)
                    cursor.execute("commit")
        except Exception as e:
            cursor.execute("rollback")
            last_id=-1
        try:
            qrs = util.QuerySecond('getsamplefee',last_id,0)
            if len(qrs)>0:
                data = {"id": qrs[0][0], "feename": qrs[0][1], "freetime": qrs[0][2], "tolerancetime": qrs[0][3],
                        "feetype": qrs[0][4], "inused": qrs[0][5], "createdby": getUserName(qrs[0][6]),
                        "totalfees": qrs[0][7], "startdate": str(qrs[0][8]) if qrs[0][8] else '00:00:00',
                        "userid": qrs[0][6], "issubtractfree": True if qrs[0][9] and int(qrs[0][9]) > 0 else False,
                        "issubtracttolerance": True if qrs[0][10] and int(qrs[0][10]) > 0 else False,
                        "feeday": qrs[0][11], "startnight": str(qrs[0][12]) if qrs[0][12] else '00:00:00',
                        "ischange": True if qrs[0][13] and int(qrs[0][13]) > 0 else False,"isvisible": True,"canlock": True if qrs[0][14] and int(qrs[0][14]) == 1 else False,"canunlock": True if qrs[0][14] and int(qrs[0][14]) == 2 else False}
                datajson = json.dumps(data, ensure_ascii=False).encode('utf8')
                return HttpResponse(datajson)
            else:
                HttpResponse("fail")
        except Exception as e:
            return HttpResponse("fail")
@csrf_exempt
def post_copyfromfee(request):
    if request.method == "POST":
        cursor = connections['secondary'].cursor()
        util = Utilities()
        last_id = -1
        try:
            userid = request.user.id
            username = getUserName(userid)
            now = datetime.now().strftime("%Y-%m-%d %H:%M:%S")

            data = request.POST['data']
            jsdata = json.loads(str(data))
            if "copyname" in jsdata and jsdata["copyname"] !='undefined' and  jsdata["copyname"] !='':
                callname=jsdata["copyname"]
            else:
                callname="%s_%s"%( jsdata["callname"],now)
            qrss = util.QuerySecond('checkexistfee', callname)
            if not qrss or len(qrss) < 1 or qrss[0][0] == 1:
                return HttpResponse("failname");
            feetypes=jsdata["feetype"]
            fullfee=0
            detail=''
            if str(feetypes)=='2':
                jsdd=jsdata["detail"]
                desdata=[]
                for it in jsdd:
                    desdata.append({"hours": it["hours"], "money": it["money"], "des": it["des"],
                                    "isonly": it["isonly"]})
                detail = json.dumps(desdata, ensure_ascii=False).encode('utf8')
            else:
                fullfee = jsdata["fullfee"]
            qr = "insert into `feeformula`(`feetype`,`callname`,`detail`,`fullfee`,`usercreate`) values('%s','%s','%s','%s','%s')" % (
                feetypes, callname, detail, fullfee, userid)
            cursor.execute("begin")
            cursor.execute(qr)
            id = cursor.lastrowid
            qr = "insert into historyaccess(target,useraction,content,actiondate,userid,username) values('Khai báo phí','Thêm','%s','%s','%s','%s')" % (
                callname, now, userid, username)
            cursor.execute(qr)
            cursor.execute("commit")
            dtjs = {"id": id, "feetype": feetypes, "callname": callname, "detail": desdata if detail!="" else "",
                    "fullfee": fullfee, "createdby": getUserName(userid),"userid":userid,"isvisible":True,"ischange":True}
            datajson = json.dumps(dtjs, ensure_ascii=False).encode('utf8')
            return HttpResponse(datajson)
        except Exception as e:
            cursor.execute("rollback")
            HttpResponse("fail")
@csrf_exempt
def post_copyfrombill(request):
    if request.method == "POST":
        cursor = connections['secondary'].cursor()
        util = Utilities()
        last_id = -1
        try:
            userid = request.user.id
            username = getUserName(userid)
            now = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
            data = request.POST['data']
            jsdata = json.loads(str(data))
            if "copyname" in jsdata and jsdata["copyname"] !='undefined' and  jsdata["copyname"] !='':
                callname=jsdata["copyname"]
            else:
                callname="%s_%s"%( jsdata["callname"],now)
            qrss = util.QuerySecond('checkexistbill', callname)
            if not qrss or len(qrss) < 1 or qrss[0][0] == 1:
                return HttpResponse("failname");
            jsdd = jsdata["detail"]
            desdata = []
            for it in jsdd:
                desdata.append({"billamount": it["billamount"], "deductionamount": it["deductionamount"]})
            detail = json.dumps(desdata, ensure_ascii=False).encode('utf8')
            qr = "insert into billformula(callname,detail,usercreate) values('%s','%s','%s')" % (
            callname, detail, userid)
            cursor.execute("begin")
            cursor.execute(qr)
            id = cursor.lastrowid
            qr = "insert into historyaccess(target,useraction,content,actiondate,userid,username) values('Định mức khấu trừ','Thêm','%s','%s','%s','%s')" % (
                callname, now, userid, username)
            cursor.execute(qr)
            cursor.execute("commit")
            dtjs = {"id": id, "callname": callname, "detail": desdata if detail != "" else "",
                    "createdby": getUserName(userid), "ischange": True, "userid": userid, "isvisible": True}
            datajson = json.dumps(dtjs, ensure_ascii=False).encode('utf8')
            return HttpResponse(datajson)
        except Exception as e:
            cursor.execute("rollback")
            HttpResponse("fail")
@csrf_exempt
def post_removeitem(request):
    if request.method == "POST":
        cursor = connections['secondary'].cursor()
        util = Utilities()
        try:
            userid = request.user.id
            username = getUserName(userid)
            now = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
            id = request.POST['id']
            tablename = request.POST['tablename']
            qrs = util.QuerySecond('getinforemove', tablename, id)
            cursor.execute("begin")
            if tablename=="samplefee":
                qrd = util.QuerySecond('getdatetypecomplex', id)
                for rd in qrd:
                    dtid = rd[0] if rd[0] else -1
                    qrdd = util.QuerySecond('getcyclecomplex', dtid)
                    for rc in qrdd:
                        ctid = rc[0] if rc[0] else -1
                        qr = "delete from `cycletime` where id = '%s'" % (ctid)
                        cursor.execute(qr)
                    qr = "delete from `typeofdate` where id = '%s'" % (dtid)
                    cursor.execute(qr)
                qrd = util.QuerySecond('getdatetyperedemption', id)
                for rd in qrd:
                    dtid = rd[0] if rd[0] else -1
                    qrdd = util.QuerySecond('getcycleredemption', dtid)
                    for rc in qrdd:
                        ctid = rc[0] if rc[0] else -1
                        qr = "delete from `cycleredemption` where id = '%s'" % (ctid)
                        cursor.execute(qr)
                    qr = "delete from `datetyperedemption` where id = '%s'" % (dtid)
                    cursor.execute(qr)
            qr = "delete from `%s` where id = '%s'" % ( tablename,id)
            cursor.execute(qr)
            content = "ID: %s" % (id)
            target = "Bảng dữ liệu: %s" % (tablename)
            if qrs and len(qrs)==1:
                content=qrs[0][1]
                target=qrs[0][0]
            qr = "insert into historyaccess(target,useraction,content,actiondate,userid,username) values('%s','Xóa','%s','%s','%s','%s')" % (
                target,content, now, userid, username)
            cursor.execute(qr)
            cursor.execute("commit")
            dtjs={"result":"ok","data":"ok"}
            datajson = json.dumps(dtjs, ensure_ascii=False).encode('utf8')
            return HttpResponse(datajson)
        except Exception as e:
            cursor.execute("rollback")
            dtjs = {"result": "fail", "data": "Không thể xóa. Vui lòng thử lại!"}
            datajson = json.dumps(dtjs, ensure_ascii=False).encode('utf8')
            return HttpResponse(datajson)
@csrf_exempt
def post_removeitem1(request):
    if request.method == "POST":
        cursor = connections['secondary'].cursor()
        util = Utilities()
        try:
            userid = request.user.id
            username = getUserName(userid)
            now = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
            id = request.POST['id']
            tablename = request.POST['tablename']
            idupdate= request.POST['idupdate']
            qrs = util.QuerySecond('getinforemove', tablename, id)
            cursor.execute("begin")
            qr = "delete from `%s` where id = '%s'" % ( tablename,id)
            cursor.execute(qr)
            qr="update `%s` set `expireddate` = null where id='%s'"% ( tablename,idupdate)
            cursor.execute(qr)

            content = "ID: %s" % (id)
            target = "Bảng dữ liệu: %s" % (tablename)
            if qrs and len(qrs) == 1:
                if tablename=='redemptionactive':
                    content = "Nhóm cửa hàng: %s, Loại xe: %s, Ngày hiệu lực: %s" % (getGroupTanant(qrs[0][0]),
                                                                                           getVehicleName(qrs[0][1]),
                                                                                           qrs[0][2].strftime(
                                                                                               "%Y-%m-%d"))
                    target = "Biểu khấu trừ phí"
                else:
                    content = "Loại thẻ: %s,Loại xe: %s, Ngày hiệu lực: %s" % (getCardName(qrs[0][0]),
                                                                                           getVehicleName(qrs[0][1]),
                                                                                           qrs[0][2].strftime(
                                                                                               "%Y-%m-%d"))
                    target = "Biểu phí"
            qr = "insert into historyaccess(target,useraction,content,actiondate,userid,username) values('%s','Xóa','%s','%s','%s','%s')" % (
                target, content, now, userid, username)
            cursor.execute(qr)
            cursor.execute("commit")
            dtjs={"result":"ok","data":"ok"}
            datajson = json.dumps(dtjs, ensure_ascii=False).encode('utf8')
            return HttpResponse(datajson)
        except Exception as e:
            cursor.execute("rollback")
            dtjs = {"result": "fail", "data": "Không thể xóa. Vui lòng thử lại!"}
            datajson = json.dumps(dtjs, ensure_ascii=False).encode('utf8')
            return HttpResponse(datajson)
@csrf_exempt
def post_tenantgroup(request):
    if request.method == "POST":
        cursor = connections['secondary'].cursor()
        try:
            userid = request.user.id
            username = getUserName(userid)
            now = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
            callname = request.POST['callname']
            qr = "insert into tenantgroup(groupname) values('%s')"%(callname)
            cursor.execute("begin")
            cursor.execute(qr)
            id = cursor.lastrowid
            qr = "insert into historyaccess(target,useraction,content,actiondate,userid,username) values('Nhóm đối tác','Thêm','%s','%s','%s','%s')" % (
                callname, now, userid, username)
            cursor.execute(qr)
            cursor.execute("commit")
            dtjs = {"id": id, "groupname": callname, "ischange":True, "isvisible":True, "detail":[]}
            datajson = json.dumps(dtjs, ensure_ascii=False).encode('utf8')
            return HttpResponse(datajson)
        except Exception as e:
            cursor.execute("rollback")
            HttpResponse("fail")
@csrf_exempt
def post_specialdate(request):
    if request.method == "POST":
        cursor = connections['secondary'].cursor()
        try:
            userid = request.user.id
            username = getUserName(userid)
            now = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
            callname = request.POST['callname']
            dateactive = request.POST['dateactive']
            percentupordown = request.POST['percentupordown']
            if percentupordown=='undefined':
                percentupordown='0'
            qr = "insert into specialdate(callname,dateactive,percentupordown,usercreate) values('%s','%s','%s','%s')" % (
            callname, dateactive, int(percentupordown) if percentupordown and percentupordown !="" else 0 , userid)
            cursor.execute("begin")
            cursor.execute(qr)
            id = cursor.lastrowid
            qr = "insert into historyaccess(target,useraction,content,actiondate,userid,username) values('Ngày đặc biệt','Thêm','%s','%s','%s','%s')" % (
                "Tên gọi: %s, Ngày hiệu lực: %s"%(callname,dateactive), now, userid, username)
            cursor.execute(qr)
            cursor.execute("commit")
            dtjs = {"id": id,"dateactive":dateactive, "callname": callname,"percentupordown":percentupordown,
                "createdby":getUserName(userid),"userid":userid,"isvisible":True,"ischange":True}
            datajson = json.dumps(dtjs, ensure_ascii=False).encode('utf8')
            return HttpResponse(datajson)
        except Exception as e:
            cursor.execute("rollback")
            HttpResponse("fail")
def checkandsaveinvalid(excel_file):
    try:
        import os
        import tempfile
        import xlrd
        fd, tmp = tempfile.mkstemp()
        with os.fdopen(fd, 'w') as out:
            out.write(excel_file.read())
        book = xlrd.open_workbook(tmp)
        sh = book.sheet_by_index(0)
        nr = sh.nrows
        invalid=False
        ldate = []
        for i in range(1, nr):
            try:
                frcomment=''
                fr=datetime.strptime(sh.row(i)[0].value,"%d/%m/%Y %H:%M:%S")
            except Exception as ee:
                frcomment = '"%s" không đúng theo định dạng: "dd/MM/YYYY HH:mm:ss"'%(sh.row(i)[0].value)
                fr = ''
                invalid=True
            try:
                tocomment=''
                to=datetime.strptime(sh.row(i)[1].value,"%d/%m/%Y %H:%M:%S")
            except Exception as ee:
                invalid = True
                tocomment = '"%s" không đúng theo định dạng: "dd/MM/YYYY HH:mm:ss"'%(sh.row(i)[1].value)
                to = ''
            ldate.append({"from": fr, "to": to, "fromcomment": frcomment, "tocomment": tocomment})
        os.remove(tmp)
        # save invalid file
        if invalid:
            folder_name = 'templates/report'
            file_name = 'invalidimport.xlsx'
            file_path = '%s/%s' % (folder_name, file_name)
            if not os.path.exists(folder_name):
                os.mkdir(folder_name)
            if os.path.isfile(file_path):
                os.remove(file_path)
            workbook = Workbook(file_path, {'constant_memory': True})
            sheet = workbook.add_worksheet("invalid")
            bold = workbook.add_format({'bold': True})
            wrap = workbook.add_format()
            wrap.set_text_wrap()
            border = workbook.add_format()
            border.set_border()
            bold_border = workbook.add_format({'bold': True, 'border': 1})
            number_border_format = workbook.add_format({'num_format': '#,###0', 'border': 1})
            number_bold_border_format = workbook.add_format({'num_format': '#,###0', 'border': 1, 'bold': True})
            sheet.set_column(0, 1, 20)
            curdata = [u"Thời điểm Check-in", u"Thời điểm Check-out"]
            sheet.write_row(0, 0, curdata, bold_border)
            i = 0
            for it in ldate:

                if it["fromcomment"]!='':
                    sheet.write(i+1,0,it["from"],border)
                    sheet.write_comment(i+1,0, it["fromcomment"])
                else:
                    sheet.write(i + 1, 0, it["from"].strftime("%d/%m/%Y %H:%M:%S"), border)
                if it["tocomment"]!='':
                    sheet.write(i + 1, 1, it["to"], border)
                    sheet.write_comment(i+1,1, it["tocomment"])
                else:
                    sheet.write(i + 1, 1, it["to"].strftime("%d/%m/%Y %H:%M:%S"), border)
                i = i + 1
            workbook.close()
            return 0,ldate
        else:
            return 1, ldate
    except Exception as e:
        return -1, None
@csrf_exempt
def post_tocalandreport(request):
    if request.method == "POST":
        try:
            excel_file = request.FILES['file']
            sample = request.POST['sample']
            jsdd = json.loads(str(sample))
            feeid=jsdd["id"]
            callname=jsdd["callname"]
            import os
            # import tempfile
            # import xlrd
            # fd, tmp = tempfile.mkstemp()
            # with os.fdopen(fd, 'w') as out:
            #     out.write(excel_file.read())
            # book = xlrd.open_workbook(tmp)
            # sh = book.sheet_by_index(0)
            # nr = sh.nrows
            # ldate=[]
            # for i in range(1,nr):
            #     ldate.append({"from":datetime.strptime(sh.row(i)[0].value,"%Y-%m-%d %H:%M:%S"),"to":datetime.strptime(sh.row(i)[1].value,"%Y-%m-%d %H:%M:%S")})
            # os.remove(tmp)
            #call fee and report
            checkvalue,ldate = checkandsaveinvalid(excel_file)
            if checkvalue==1:
                lrp=[]
                total_fee=0
                for d in ldate:
                    fee=callfullfeecomplex(feeid,d["from"],d["to"])[1]
                    total_fee=total_fee+fee
                    lrp.append({"from":d["from"].strftime("%d/%m/%Y %H:%M:%S"),"to":d["to"].strftime("%d/%m/%Y %H:%M:%S"),"fee":fee})

                #report
                folder_name = 'templates/report'
                file_name = 'calculatefee.xlsx'
                file_path = '%s/%s' % (folder_name, file_name)
                if not os.path.exists(folder_name):
                    os.mkdir(folder_name)
                if os.path.isfile(file_path):
                    os.remove(file_path)
                workbook = Workbook(file_path, {'constant_memory': True})
                sheet = workbook.add_worksheet("CalculateFeeList")
                bold = workbook.add_format({'bold': True})
                wrap = workbook.add_format()
                wrap.set_text_wrap()
                border = workbook.add_format()
                border.set_border()
                bold_border = workbook.add_format({'bold': True, 'border': 1})
                number_border_format = workbook.add_format({'num_format': '#,###0', 'border': 1})
                number_bold_border_format = workbook.add_format({'num_format': '#,###0', 'border': 1, 'bold': True})
                sheet.set_column(1, 3, 20)
                sheet.write(1, 0, u"Kết quả cho mẫu phí: %s"%(callname), bold)
                curdata=[u"STT",u"Check-In-Time",u"Check-Out_Time",u"Phí"]
                sheet.write_row(2, 0, curdata, bold_border)
                i=0
                for it in lrp:
                    curdata=[i+1,it["from"],it["to"],it["fee"]]
                    sheet.write_row(3+i, 0, curdata, number_border_format)
                    i=i+1
                sheet.merge_range(3+i, 0, 3+i, 2,u"Tổng phí",bold_border)
                sheet.write(3+i, 3, total_fee, number_bold_border_format)
                workbook.close()
                return HttpResponse("ok")
            elif checkvalue==0:
                return HttpResponse("faildata")
            else:
                return HttpResponse("fail")
        except Exception as e:
            return HttpResponse("fail")
### calfee function 2018-04-23
def callredemtion(activeid,billamount,groupname,redemtiontime, checkintime):
    try:
        billamount=int(billamount)
        fml=getbillformulas()
        util = Utilities()
        if checkintime.date()==redemtiontime.date():
            type=0
        else:
            type=1
        #cùng ngày
        qrc = util.QuerySecond('findsampleredemtion', activeid, 0)
        if len(qrc)<1:
            return {"groupname":groupname,"callname": "Không xác định", "redemptiontype": 0,"redemptionfee":0, "details": [{"formula": "Không xác định", "deductionamount": 0, "billamount": billamount,
             "redemtiontime": redemtiontime.strftime("%d-%m-%Y %H:%M:%S")}]};
        starttime = qrc[0][8] if qrc[0][8] else time(0, 0, 0, 0)
        startdate=checkintime.replace(hour=starttime.hour, minute=starttime.minute, second=starttime.second)
        if(startdate>checkintime):
            startdate=startdate+timedelta(days=-1)
        enddate=startdate+timedelta(days=1,seconds=-1)
        if(enddate>=redemtiontime):#cùng ngày
            feeid = qrc[0][0]
            starttime = qrc[0][8] if qrc[0][8] else time(0, 0, 0, 0)
            tmptime = redemtiontime
            atime = get_time(redemtiontime)
            if atime < starttime:
                tmptime = redemtiontime + timedelta(days=-1)
            dow = tmptime.weekday()
            weekofdate = wodate(dow)
            weekofdatename = wodatename(dow)
            qrs = util.QuerySecond('getdatetyperedemtion', feeid)
            specialday = [x for x in qrs if x[2].find("0") >= 0]
            pc, ispecial = IsSpecialDay(tmptime)
            datetypeid = -1
            if ispecial and len(specialday) == 1:
                datetypeid = specialday[0][0]
            else:
                myday = [x for x in qrs if x[2].find(weekofdate) >= 0]
                if len(myday) == 1:
                    datetypeid = myday[0][0]

            fmlId = -1
            dtnm = util.QuerySecond('getcycleredemption', datetypeid)
            ddd=[d for d in dtnm if int(d[5])==1]
            dd=[d for d in dtnm if int(d[5])==2]
            key=True
            if dd and len(dd)>0:
                for it in dd:
                    ftime=startdate.replace(hour=it[1].hour,minute=it[1].minute,second=it[1].second)
                    ttime=startdate.replace(hour=it[2].hour,minute=it[2].minute,second=it[2].second)
                    if ttime<ftime:
                        ttime=ttime+timedelta(days=1)
                    if (checkintime >= ftime and checkintime <= ttime and redemtiontime >= ftime and redemtiontime <= ttime):
                        fmlId = int(it[3])
                        key=False
                        break
            if key and len(ddd) > 0:
                for it in ddd:
                    if (atime >= it[1] and atime <= it[2]) or (it[2] < it[1] and (atime >= it[1] or atime <= it[2])):
                        fmlId = int(it[3])
                        break
            formula = [f for f in fml if f["id"] == fmlId]
            deductionamount = 0
            if len(formula) > 0:
                l = len(formula[0]["detail"])
                index = 1
                for dt in formula[0]["detail"]:
                    billcp = int(dt["billamount"])
                    if l == 1:
                        deductionamount = int(dt["deductionamount"]) if billcp <= billamount else 0
                        return {"groupname": groupname, "callname": qrs[0][1], "redemptiontype": 1,
                                "redemptionfee": deductionamount, "details": [
                                {"formula": formula, "deductionamount": deductionamount, "billamount": billamount,
                                 "redemtiontime": redemtiontime.strftime("%d-%m-%Y %H:%M:%S")}]};
                    if index == 1 and billcp > int(billamount):
                        return {"groupname": groupname, "callname": qrs[0][1], "redemptiontype": 1,
                                "redemptionfee": 0, "details": [
                                {"formula": formula, "deductionamount": 0, "billamount": billamount,
                                 "redemtiontime": redemtiontime.strftime("%d-%m-%Y %H:%M:%S")}]};
                    billcp = int(formula[0]["detail"][index]["billamount"])
                    if index == l - 1 or billcp > int(billamount):
                        if billcp > int(billamount):
                            deductionamount = int(dt["deductionamount"]) if dt["deductionamount"] else 0
                        else:
                            deductionamount = int(formula[0]["detail"][index]["deductionamount"]) if \
                            formula[0]["detail"][index]["deductionamount"] else 0
                        return {"groupname": groupname, "callname": qrs[0][1], "redemptiontype": 1,
                                "redemptionfee": deductionamount, "details": [
                                {"formula": formula, "deductionamount": deductionamount, "billamount": billamount,
                                 "redemtiontime": redemtiontime.strftime("%d-%m-%Y %H:%M:%S")}]};
                    index = index + 1
            return {"groupname": groupname, "callname": qrs[0][1], "redemptiontype": 1, "redemptionfee": deductionamount,
                    "details": [
                        {"formula": "Không xác định", "deductionamount": deductionamount, "billamount": billamount,
                         "redemtiontime": redemtiontime.strftime("%d-%m-%Y %H:%M:%S")}]};
        else:#Khác ngày
            qr = util.QuerySecond('findsampleredemtion', activeid, 1)
            if len(qr)<1:
                return {"groupname":groupname,"callname": "Không xác định", "redemptiontype": 0,"redemptionfee":0, "details": [{"formula": "Không xác định", "deductionamount": 0, "billamount": billamount,
                 "redemtiontime": redemtiontime.strftime("%d-%m-%Y %H:%M:%S")}]};
            feeid=qr[0][0]
            starttime = qr[0][8] if qr[0][8] else time(0, 0, 0, 0)
            tmptime=redemtiontime
            atime=get_time(redemtiontime)
            if atime<starttime:
                tmptime=redemtiontime+timedelta(days=-1)
            dow=tmptime.weekday()
            weekofdate = wodate(dow)
            weekofdatename=wodatename(dow)
            qrs = util.QuerySecond('getdatetyperedemtion', feeid)
            specialday = [x for x in qrs if x[2].find("0") >= 0]
            pc, ispecial = IsSpecialDay(tmptime)
            datetypeid = -1
            if ispecial and len(specialday) == 1:
                datetypeid = specialday[0][0]
            else:
                myday = [x for x in qrs if x[2].find(weekofdate) >= 0]
                if len(myday) == 1:
                    datetypeid = myday[0][0]
            dtnm = util.QuerySecond('getcycleredemption', datetypeid)
            ddd = [d for d in dtnm if int(d[5]) == 1]
            fmlId=-1
            if len(ddd)>0:
                for it in dtnm:
                    if (atime >= it[1] and atime<=it[2]) or (it[2]<it[1] and (atime>=it[1] or atime<=it[2])):
                        fmlId=int(it[3])
                        break
            formula=[f for f in fml if f["id"]==fmlId]
            deductionamount=0
            if len(formula)>0:
                l=len(formula[0]["detail"])
                index=1
                for dt in formula[0]["detail"]:
                    billcp=int(dt["billamount"])
                    if l==1:
                        deductionamount=int(dt["deductionamount"]) if billcp<=billamount else 0
                        return {"groupname": groupname, "callname": qr[0][1], "redemptiontype": 1,
                                "redemptionfee": deductionamount, "details": [
                                {"formula": formula, "deductionamount": deductionamount, "billamount": billamount,
                                 "redemtiontime": redemtiontime.strftime("%d-%m-%Y %H:%M:%S")}]};
                    if index==1 and billcp>int(billamount):
                        return {"groupname": groupname, "callname": qr[0][1], "redemptiontype": 1,
                                "redemptionfee": 0, "details": [
                                {"formula": formula, "deductionamount": 0, "billamount": billamount,
                                 "redemtiontime": redemtiontime.strftime("%d-%m-%Y %H:%M:%S")}]};
                    billcp = int(formula[0]["detail"][index]["billamount"])
                    if index==l-1 or billcp>int(billamount):
                        if billcp>int(billamount):
                            deductionamount=int(dt["deductionamount"]) if dt["deductionamount"] else 0
                        else:
                            deductionamount = int(formula[0]["detail"][index]["deductionamount"]) if formula[0]["detail"][index]["deductionamount"] else 0
                        return {"groupname":groupname,"callname": qr[0][1], "redemptiontype": 1, "redemptionfee": deductionamount, "details": [
                            {"formula": formula, "deductionamount": deductionamount, "billamount": billamount,
                             "redemtiontime": redemtiontime.strftime("%d-%m-%Y %H:%M:%S")}]};
                    index=index+1
            return {"groupname":groupname,"callname": qr[0][1], "redemptiontype": 1, "redemptionfee": deductionamount, "details": [
                {"formula": "Không xác định", "deductionamount": deductionamount, "billamount": billamount,
                 "redemtiontime": redemtiontime.strftime("%d-%m-%Y %H:%M:%S")}]};
    except Exception as e:
        return {"groupname":groupname,"callname": "Không xác định", "redemptiontype": 0, "redemptionfee": 0,
                "details": [{"formula": "Không xác định", "deductionamount": 0, "billamount": billamount,
                             "redemtiontime": redemtiontime.strftime("%d-%m-%Y %H:%M:%S")}]};
def downloadtemplate (request):
    import os
    folder_name = 'templates/report'
    file_name = 'TemplateImport.xlsx'
    file_path = '%s/%s' % (folder_name, file_name)
    if not os.path.exists(folder_name):
        os.mkdir(folder_name)
    if os.path.isfile(file_path):
        os.remove(file_path)
    workbook = Workbook(file_path, {'constant_memory': True})
    sheet = workbook.add_worksheet('Calculation_Template')
    wrap = workbook.add_format()
    wrap.set_text_wrap()
    border = workbook.add_format()
    border.set_border()
    bold_border = workbook.add_format({'bold': True, 'border': 1})
    nomal=workbook.add_format({'border': 1})
    sheet.set_column(0, 1, 20)
    sheet.write(0, 0, "Thời điểm check-in", bold_border)
    sheet.write(0, 1, "Thời điểm check-out", bold_border)
    noww=datetime.now()
    ci=(noww.replace(hour=5, minute=0, second=0) +timedelta(days=-1)).strftime("%d/%m/%Y %H:%M:%S")
    co=noww.strftime("%d/%m/%Y %H:%M:%S")
    sheet.write(1, 0, ci, nomal)
    sheet.write(1, 1, co, nomal)
    workbook.close()
    with open(file_path, 'r') as f:
        response = HttpResponse(f.read(), content_type="application/vnd.ms-excel")
        response['Content-Disposition'] = "attachment; filename=%s" % file_name
        return response
def downloadfeeresult(request):
    import os
    folder_name = 'templates/report'
    file_name = 'calculatefee.xlsx'
    file_path = '%s/%s' % (folder_name, file_name)
    if os.path.exists(file_path):
        with open(file_path, 'r') as f:
            response = HttpResponse(f.read(), content_type="application/vnd.ms-excel")
            response['Content-Disposition'] = "attachment; filename=%s" % file_name
            return response
    raise Http404;
def downloadinvalidimport(request):
    import os
    folder_name = 'templates/report'
    file_name = 'invalidimport.xlsx'
    file_path = '%s/%s' % (folder_name, file_name)
    if os.path.exists(file_path):
        with open(file_path, 'r') as f:
            response = HttpResponse(f.read(), content_type="application/vnd.ms-excel")
            response['Content-Disposition'] = "attachment; filename=%s" % file_name
            return response
    raise Http404;
def callfullfee(qrs,fromdate,todate):
    try:
        # util = Utilities()
        # qrs = util.QuerySecond('getsamplefee',feeid,1)
        if len(qrs)<1:
            return {"callname": "Phí lược ngày đêm", "feetype": 1,
                    "details": {"from": fromdate.strftime("%d-%m-%Y %H:%M:%S"), "to": todate.strftime("%d-%m-%Y %H:%M:%S"),
                                "fee": 0}}, 0
        else:
            freetime = int(qrs[0][2]) if qrs[0][2] else 0
            totalseconds = (todate - fromdate).total_seconds()
            if totalseconds <= (freetime * 60):
                return {"callname(miễn phí: %s)"%(freetime * 60): qrs[0][1], "feetype": 1,
                         "details": {"from": fromdate.strftime("%d-%m-%Y %H:%M:%S"), "to": todate.strftime("%d-%m-%Y %H:%M:%S"),
                                "fee": 0}}, 0
            starttime = qrs[0][8] if qrs[0][8] else time(0, 0, 0, 0)
            beginnight = qrs[0][12] if qrs[0][12] else time(0, 0, 0, 0)
            nightfee=int(qrs[0][7]) if qrs[0][7] else 0
            dayfee=int(qrs[0][11]) if qrs[0][11] else 0
            fee=0
            days=0
            tmpday=fromdate.replace(hour=starttime.hour,minute=starttime.minute,second=starttime.second)
            if fromdate>=tmpday:
                tmpday = tmpday + timedelta(days=1)
            while tmpday < todate:
                days=days+1
                tmpday=tmpday+timedelta(days=1)
            if days>0:
                fee=nightfee*days
            elif (starttime<beginnight and todate.time() <= beginnight and todate.time()>=starttime\
                    or starttime>beginnight and (todate.time()>=starttime or todate.time()<=beginnight))\
                and (starttime<beginnight and fromdate.time() <= beginnight and fromdate.time()>=starttime\
                    or starttime>beginnight and (fromdate.time()>=starttime or fromdate.time()<=beginnight)):
                fee=dayfee
            else:
                fee= nightfee
            return {"callname": "Phí lược ngày đêm %s" % (qrs[0][1]), "feetype": 1,
                    "details": {"from": fromdate.strftime("%d-%m-%Y %H:%M:%S"), "to": todate.strftime("%d-%m-%Y %H:%M:%S"),
                                "fee": fee}}, fee
    except Exception as e:
        return {"callname": "Phí lược ngày đêm", "feetype": 1,
                "details": {"from": fromdate.strftime("%d-%m-%Y %H:%M:%S"), "to": todate.strftime("%d-%m-%Y %H:%M:%S"),
                            "fee": 0}}, 0
def callfullfeenn(fid, fromdate, tofdate):
    try:
        util = Utilities()
        qrs = util.QuerySecond('getsamplefeedetailnn', fid)
        if qrs and len(qrs)==1:
            callname="Định mức phí Nhà nước: %s"%(qrs[0][1])
            startday=qrs[0][2]
            startnight = qrs[0][3]
            freetime=int(qrs[0][4]) if qrs[0][4] else 0
            fee24h=int(qrs[0][5]) if qrs[0][5] else 0
            maxfee = int(qrs[0][6]) if qrs[0][6] else 0
            optioncase=int(qrs[0][7]) if qrs[0][7] else 1
            formuladay=qrs[0][9]
            formuladaydetail=json.loads(str(qrs[0][10]))
            formulanight = qrs[0][12]
            formulanightdetail = json.loads(str(qrs[0][13]))
            if not startday or not  startnight or not formuladaydetail or not formulanightdetail or len(formuladaydetail)!=len(formulanightdetail):
                return {"callname": callname, "feetype": 5, "from": fromdate.strftime("%d-%m-%Y %H:%M:%S"),"details":[],
                        "to": tofdate.strftime("%d-%m-%Y %H:%M:%S"), "fee": 0,"feedetail":"Cấu hình phí sai"}, 0
            totalseconds = (tofdate - fromdate).total_seconds()
            if freetime*60>=totalseconds:
                return {"callname": callname, "feetype": 5, "from": fromdate.strftime("%d-%m-%Y %H:%M:%S"),"details":[],
                        "to": tofdate.strftime("%d-%m-%Y %H:%M:%S"), "fee": 0, "feedetail": "Thời gian miễn phí %s phút "%(freetime)}, 0
            if len(formuladaydetail)==2:
                begintime=fromdate.time()
                formuladetail = formuladaydetail if (startday < startnight and begintime >= startday and begintime < startnight) or (
                startday > startnight and ((begintime >= startday and begintime <= time(hour=23, minute=59, second=59)) or (
                begintime >= time(hour=0, minute=0, second=0) and begintime < startnight))) else formulanightdetail
                breakday=fromdate+timedelta(hours=int(formuladetail[0]["hours"]) if formuladetail[0]["hours"] else 0)
                if breakday>=tofdate:
                    totalfee=int(formuladetail[0]["money"]) if formuladetail[0]["money"] else 0
                    details = [{"from": fromdate.strftime("%d-%m-%Y %H:%M:%S"), "to": tofdate.strftime("%d-%m-%Y %H:%M:%S"),
                                "fee": totalfee, "formula": formuladetail,"note":"block %sh"%(formuladetail[0]["hours"])}]

                else:
                    fromnew=breakday+timedelta(seconds=1)
                    totalseconds = (tofdate - fromnew).total_seconds()
                    dm = divmod(totalseconds, 3600)
                    totalhours = dm[0] + (1 if dm[1] > 0 else 0)
                    firstfee = int(formuladetail[0]["money"]) if formuladetail[0]["money"] else 0
                    lastfee=int(formuladetail[1]["money"])*totalhours if formuladetail[1]["money"] else 0
                    totalfee=firstfee+lastfee
                    details = [{"from": fromdate.strftime("%d-%m-%Y %H:%M:%S"), "to": breakday.strftime("%d-%m-%Y %H:%M:%S"),
                                "fee": firstfee, "formula": formuladetail,"note":"block %sh"%(formuladetail[0]["hours"])},
                               {"from": fromnew.strftime("%d-%m-%Y %H:%M:%S"),
                                "to": tofdate.strftime("%d-%m-%Y %H:%M:%S"),
                                "fee": lastfee, "formula": formuladetail,"note":"%sh * %s/h"%(int(totalhours),formuladetail[1]["money"])}
                               ]
                if maxfee > 0 and totalfee > maxfee:
                    return {"callname": callname, "feetype": 5, "feedetail": "Lấy theo phí thiết lập lớn nhất",
                            "from": fromdate.strftime("%d-%m-%Y %H:%M:%S"), "details": details,
                            "to": tofdate.strftime("%d-%m-%Y %H:%M:%S"), "fee": totalfee}, maxfee
                return {"callname": callname, "feetype": 5, "details": details,
                        "from": fromdate.strftime("%d-%m-%Y %H:%M:%S"), "feedetail": "",
                        "to": tofdate.strftime("%d-%m-%Y %H:%M:%S"), "fee": totalfee}, totalfee
            if len(formuladaydetail) == 1:
                startdatefrom=fromdate.replace(hour=startday.hour,minute=startday.minute,second=startday.second)
                startdateto = fromdate.replace(hour=startnight.hour, minute=startnight.minute, second=startnight.second)
                if startdateto<startdatefrom:
                    startdateto+timedelta(days=1)
                dayseconds=(startdateto-startdatefrom).total_seconds()
                dm = divmod(dayseconds, 3600)
                dayhours = dm[0] + (1 if dm[1] >= 1800 else 0)
                nighthours=24-dayhours
                dhours=int(formuladaydetail[0]["hours"])
                nhours=int(formulanightdetail[0]["hours"])
                dm1=divmod(dayhours, dhours)
                dm2 = divmod(nighthours, nhours)
                if dm1[0]==0 or dm1[1]>0 or dm2[0]==0 or dm2[1]>0:
                    return {"callname": callname, "feetype": 5, "from": fromdate.strftime("%d-%m-%Y %H:%M:%S"),
                            "details": [],
                            "to": tofdate.strftime("%d-%m-%Y %H:%M:%S"), "fee": 0, "feedetail": "Cấu hình phí sai - Không khớp block giờ với mốc ngày và mốc đêm"}, 0
                blocksday=dm1[0]
                blocksnight = dm2[0]
                if fee24h <=0:
                    fee24h=blocksday*formuladaydetail[0]["money"]+blocksnight*formulanightdetail[0]["money"]
                dm=divmod(totalseconds, 86400)
                totaldays=int(dm[0])
                partday=int(dm[1])
                formdatepart=tofdate+timedelta(seconds=0-partday)
                detail=[]
                feefirst=0
                feelast=0
                totalfee=0
                if totaldays>0:
                    feefirst=totaldays*fee24h
                    detail.append(
                        {"from": fromdate.strftime("%d-%m-%Y %H:%M:%S"), "to": (formdatepart+timedelta(seconds=-1)).strftime("%d-%m-%Y %H:%M:%S"),
                         "fee": feefirst, "formula": [], "note": "%s * %s /24h" % (totaldays, fee24h)}
                    )
                while formdatepart<= tofdate:
                    chk=getisday(formdatepart.time(),startday,startnight)
                    caseday=True
                    if chk:
                        fromtemp=formdatepart+timedelta(hours=dhours)
                        if fromtemp>tofdate:
                            fromtemp=tofdate
                        if optioncase!=1:
                            chk1=getisday(fromtemp.time(),startday,startnight)
                            if not chk1:
                                tmp=formdatepart+timedelta(hours=nhours)
                                if tmp>tofdate:
                                    tmp=tofdate
                                chk2 = getisday(tmp.time(), startday, startnight)
                                if not chk2:
                                    cmpdate=formdatepart.replace(hour=startnight.hour, minute=startnight.minute, second=startnight.second)
                                    if cmpdate <formdatepart:
                                        cmpdate=cmpdate+timedelta(days=1)
                                    if (cmpdate-formdatepart).total_seconds()<(tmp-cmpdate).total_seconds():
                                        caseday = False
                                        fromtemp=tmp
                    else:
                        fromtemp = formdatepart + timedelta(hours=nhours)
                        caseday = False
                        if fromtemp>tofdate:
                            fromtemp=tofdate
                        if optioncase !=1:
                            chk1 = getisday(fromtemp.time(), startday, startnight)
                            if chk1:
                                tmp = formdatepart + timedelta(hours=dhours)
                                if tmp > tofdate:
                                    tmp = tofdate
                                chk2 = getisday(tmp.time(), startday, startnight)
                                if chk2:
                                    cmpdate = formdatepart.replace(hour=startday.hour, minute=startday.minute,
                                                                   second=startday.second)
                                    if cmpdate < formdatepart:
                                        cmpdate = cmpdate + timedelta(days=1)
                                    if (cmpdate - formdatepart).total_seconds() < (tmp - cmpdate).total_seconds():
                                        caseday = True
                                        fromtemp = tmp
                    if caseday:
                            detail.append(
                                {"from": formdatepart.strftime("%d-%m-%Y %H:%M:%S"),
                                 "to": fromtemp.strftime("%d-%m-%Y %H:%M:%S"),
                                 "fee": int(formuladaydetail[0]["money"]), "formula": formuladaydetail, "note": "%s * block %sh" % (int(formuladaydetail[0]["money"]), dhours)}
                            )
                            feelast=feelast+int(formuladaydetail[0]["money"])
                    else:
                        detail.append(
                            {"from": formdatepart.strftime("%d-%m-%Y %H:%M:%S"),
                             "to": fromtemp.strftime("%d-%m-%Y %H:%M:%S"),
                             "fee": int(formulanightdetail[0]["money"]), "formula": formulanightdetail,
                             "note": "%s * block %sh" % (int(formulanightdetail[0]["money"]), nhours)}
                        )
                        feelast = feelast + int(formulanightdetail[0]["money"])
                    formdatepart=fromtemp+timedelta(seconds=1)
                totalfee=feefirst+feelast
                if maxfee > 0 and totalfee > maxfee:
                    return {"callname": callname, "feetype": 5, "from": fromdate.strftime("%d-%m-%Y %H:%M:%S"),
                            "to": tofdate.strftime("%d-%m-%Y %H:%M:%S"), "fee": totalfee,
                            "feedetail": "Phí không được vượt quá %s"%(maxfee), "details": detail}, maxfee
                return {"callname": callname, "feetype": 5, "from": fromdate.strftime("%d-%m-%Y %H:%M:%S"),
                        "to": tofdate.strftime("%d-%m-%Y %H:%M:%S"), "fee": totalfee,
                        "feedetail": "","details":detail}, totalfee
        else:
            return {"callname": "Định mức phí Nhà nước", "feetype": 5, "from": fromdate.strftime("%d-%m-%Y %H:%M:%S"), "details": [],
                    "to": tofdate.strftime("%d-%m-%Y %H:%M:%S"), "fee": 0, "feedetail": "Không tìm thấy công thức", "details":[]}, 0
    except Exception as e:
        return {"callname": "Định mức phí Nhà nước", "feetype": 5, "from": fromdate.strftime("%d-%m-%Y %H:%M:%S"),
                "to": tofdate.strftime("%d-%m-%Y %H:%M:%S"), "fee": 0, "feedetail": "Trường hợp ngoại lệ","details":[]}, 0
def getisday(begintime,startday,startnight):
    isday = True if (startday < startnight and begintime >= startday and begintime < startnight) or (startday > startnight and ((begintime >= startday and begintime <= time(hour=23, minute=59, second=59)) or (begintime >= time(hour=0, minute=0,second=0) and begintime < startnight))) else False
    return isday
def callfullfee24h(qrs, fromdate, tofdate):
    try:
        #util = Utilities()
        # qrs = util.QuerySecond('getsamplefee',feeid,2)
        if len(qrs)<1:
            return {"callname": "Phí lược 24h", "feetype": 2, "from": fromdate.strftime("%d-%m-%Y %H:%M:%S"),
                    "to": tofdate.strftime("%d-%m-%Y %H:%M:%S"), "fee": 0}, 0
        if qrs[0][2]:
            total_second=(tofdate-fromdate).total_seconds()
            if(total_second<=qrs[0][2]*60):
                return {"callname": "Phí lược 24h: %s miễn phí %s phút"%(qrs[0][1],qrs[0][2]), "feetype": 2, "from": fromdate.strftime("%d-%m-%Y %H:%M:%S"),
                        "to": tofdate.strftime("%d-%m-%Y %H:%M:%S"), "fee": 0}, 0
        detail={"callname":"Phí lược 24h: %s"%(qrs[0][1]),"feetype":2, "from": fromdate.strftime("%d-%m-%Y %H:%M:%S"),
                    "to": tofdate.strftime("%d-%m-%Y %H:%M:%S"), "fee": 0}
        util = Utilities()
        qrdt=util.QuerySecond('getdetailfee24h',qrs[0][0])
        if len(qrdt)<1:
            return {"callname":"Phí lược 24h","feetype":2, "from": fromdate.strftime("%d-%m-%Y %H:%M:%S"),
                    "to": tofdate.strftime("%d-%m-%Y %H:%M:%S"), "fee": 0} ,0
        blocks=json.loads(str(qrdt[0][2])) if qrdt[0][2] else [{"blockhour":24,"blockfee":"0"}]
        afterfee=int(qrdt[0][4]) if qrdt[0][4] else 0
        canrepeat = int(qrdt[0][5]) if qrdt[0][5] else 0
        exceptfee= int(qrdt[0][6]) if qrdt[0][6] else 0
        feeall24h=0
        l=len(blocks)
        if exceptfee==1:
            feeall24h=int(blocks[l-1]["blockfee"]) if blocks[l-1]["blockfee"] else 0
        else:
            for bl in blocks:
                feeall24h=feeall24h + (int(bl["blockfee"]) if bl["blockfee"] else 0)
        totalseconds=(tofdate-fromdate).total_seconds()
        dm=divmod(totalseconds,3600)
        totalhours=dm[0] + (1 if dm[1]>0 else 0)
        dm=divmod(totalhours,24)
        ndays=dm[0]
        hours=dm[1]
        partfee=0
        if hours>0:
            if exceptfee == 1:
                for bl in blocks:
                    if hours<=int(bl["blockhour"]) if bl["blockhour"] else 24:
                        partfee=int(bl["blockfee"]) if bl["blockfee"] else 0
                        break
            else:
                for bl in blocks:
                    if hours<=int(bl["blockhour"]) if bl["blockhour"] else 24:
                        partfee=partfee+(int(bl["blockfee"]) if bl["blockfee"] else 0)
                        break
                    else:
                        partfee = partfee + (int(bl["blockfee"]) if bl["blockfee"] else 0)
        totalfee=0
        if canrepeat==1:
            if ndays>0:
                totalfee=feeall24h*ndays+partfee
                if hours>0:
                    detail = "%s:%s ngày %s giờ = %s * %s + %s" % (qrs[0][1],int(ndays),int(hours),int(ndays),int(feeall24h),int(partfee))
                else:
                    detail = "%s:%s ngày = %s * %s" % (qrs[0][1], int(ndays), int(ndays), int(feeall24h))
            else:
                totalfee=partfee
                detail = "%s:%s giờ = %s" % (qrs[0][1], int(hours), int(partfee))
        else:
            if totalhours>24:
                if exceptfee==1:
                    totalfee = int(qrdt[0][4]) if qrdt[0][4] else 0
                    detail = "%s:phí sau 24h = %s" % (qrs[0][1], int(totalfee))
                else:
                    total1=int(qrdt[0][4]) if qrdt[0][4] else 0
                    totalfee =  feeall24h + total1
                    detail = "%s:phí 24h %s + sau 24h %s= %s" % (qrs[0][1], int(feeall24h),int(total1), int(totalfee))
            elif totalhours==24:
                totalfee = feeall24h
                detail = "%s:phí 24h = %s" % (qrs[0][1], int(totalfee))
            else:
                totalfee=partfee
                detail = "%s:phí %sh = %s" % (qrs[0][1], int(hours), int(totalfee))
        return {"callname":'%s = %s'%(detail,int(totalfee)),"feetype":2, "from": fromdate.strftime("%d-%m-%Y %H:%M:%S"),
                    "to": tofdate.strftime("%d-%m-%Y %H:%M:%S"), "fee": int(totalfee)} ,int(totalfee)
    except Exception as e:
        return {"callname": "Phí lược 24h", "feetype": 2, "from": fromdate.strftime("%d-%m-%Y %H:%M:%S"),
                    "to": tofdate.strftime("%d-%m-%Y %H:%M:%S"), "fee": 0}, 0
def wodate(dow):
    if dow==0:
        return "2"
    if dow == 1:
        return "3"
    if dow == 2:
        return "4"
    if dow == 3:
        return "5"
    if dow == 4:
        return "6"
    if dow == 5:
        return "7"
    if dow == 6:
        return "1"
    return dow
def wodatename(dow):
    if dow==0:
        return "T2"
    if dow == 1:
        return "T3"
    if dow == 2:
        return "T4"
    if dow == 3:
        return "T5"
    if dow == 4:
        return "T6"
    if dow == 5:
        return "T7"
    if dow == 6:
        return "CN"
    return "None"
def IsSpecialDay(myday):
    util = Utilities()
    qr = util.QuerySecond('getspecialdatefee', myday.strftime("%Y-%m-%d"))
    if qr and len(qr)>0:
        pc={"DateActive":qr[0][1].strftime("%d-%m-%Y"),"CallName":qr[0][2],"Percent":qr[0][3]}
        return pc,True
    return None,False
def get_fee_from_formula(fml,totalhours):
    if fml:
        if fml["feetype"]==1:
            return int(fml["fullfee"]) if fml["fullfee"] else 0
        if fml["feetype"] == 0:
            return int(fml["fullfee"])*int(totalhours) if fml["fullfee"] else 0
        if fml["feetype"] == 2:
            blocks=fml["detail"]
            activehours=totalhours
            fee=0
            i=0
            l=len(blocks)
            for bl in blocks:
                if activehours>0:
                    money=int(bl["money"]) if bl["money"] else 0
                    hour=int(bl["hours"]) if bl["hours"] else activehours
                    des=bl["des"]
                    if i!=0 and i==l-1:
                        fee = fee + money * activehours
                        activehours = 0
                    else:
                        if des=="trên block":
                            fee = fee + money
                        else:
                            h = hour if hour <= activehours else activehours
                            fee = fee + h * money
                        activehours=activehours-hour
                i=i+1
            return fee
    return  0
#2019Jan28
def get_fee_from_formula_caseA(fml, fromtime, totime, checkintime,fromcheckin):
    if fml:
        # ttseconds = (fromtime - checkintime).total_seconds()
        # dm = divmod(ttseconds, 3600)
        # totalhourscompare = int(dm[0]) + (1 if dm[1] > 0 else 0)
        # fromtime1=checkintime+timedelta(hours=totalhourscompare)
        # ttseconds = (totime - fromtime1).total_seconds()
        # dm = divmod(ttseconds, 3600)
        # totalhours = int(dm[0]) + (1 if dm[1] > 0 else 0)
        if fromcheckin>0:
            tmpbegin = checkintime + timedelta(hours=fromcheckin+1, seconds=1)
            if tmpbegin > fromtime+timedelta(seconds=1):
                if(tmpbegin-fromtime).total_seconds()>3600:
                    tmpbegin = tmpbegin-timedelta(seconds=3600)
                fromtime = tmpbegin
        if totime<=fromtime:
            fee = 0
            return {"fedetail": [{"from": fromtime.strftime("%d-%m-%Y %H:%M:%S"),
                                  "to": totime.strftime("%d-%m-%Y %H:%M:%S"), "des": "Không khớp thời gian thực", "fee": fee}]}, fee
        ttseconds = (totime - fromtime).total_seconds()
        dm = divmod(ttseconds, 3600)
        totalhours = int(dm[0]) + (1 if dm[1] > 0 else 0)
        ttseconds = (fromtime - checkintime).total_seconds()
        dm = divmod(ttseconds, 3600)
        totalhourscompare = int(dm[0]) + (1 if dm[1] > 0 else 0)
        begintime=fromtime
        if fml["feetype"]==1:
            fee=int(fml["fullfee"]) if fml["fullfee"] else 0
            return {"fedetail": [{"from": fromtime.strftime("%d-%m-%Y %H:%M:%S"),
                                 "to": totime.strftime("%d-%m-%Y %H:%M:%S"), "des": "trên block", "fee": fee}]}, fee
        if fml["feetype"] == 0:
            fee = int(fml["fullfee"]) if fml["fullfee"] else 0
            return {"fedetail": [{"from": fromtime.strftime("%d-%m-%Y %H:%M:%S"),
                                 "to": totime.strftime("%d-%m-%Y %H:%M:%S"),
                                 "des": "%s*%s=%s" % (int(totalhours), fee, int(totalhours) * fee),
                                 "fee": int(totalhours) * fee}]}, int(totalhours) * fee
        if fml["feetype"] == 2:
            blocks=fml["detail"]
            activehours=totalhours
            fee=0
            i=0
            l=len(blocks)
            houraccross=0
            lstdetail=[]
            for bl in blocks:
                if activehours>0:
                    money=int(bl["money"]) if bl["money"] else 0
                    hour=int(bl["hours"]) if bl["hours"] else activehours
                    des=bl["des"]
                    repeat = bl["isonly"]
                    if i!=0 and i==l-1:
                        if repeat:
                            endtime=begintime+timedelta(seconds=int(activehours)*3600-1)
                            if endtime>totime:
                                endtime=totime
                            fee = fee + money * activehours
                            detail = {"from": begintime.strftime("%d-%m-%Y %H:%M:%S"),
                                      "to": endtime.strftime("%d-%m-%Y %H:%M:%S"),
                                      "des": "%sh*%s" % (int(activehours), money), "fee": int(activehours) * money}
                            lstdetail.append(detail)
                        activehours = -1
                    else:
                        h = hour if hour <= activehours else activehours
                        houraccross = houraccross + hour
                        if des=="trên block":
                            if repeat:
                                endtime = begintime + timedelta(seconds=int(h)*3600-1)
                                if endtime>totime:
                                    endtime=totime
                                fee = fee + money
                                detail = {"from": begintime.strftime("%d-%m-%Y %H:%M:%S"),
                                          "to": endtime.strftime("%d-%m-%Y %H:%M:%S"),
                                          "des": "trên block", "fee": money}
                                lstdetail.append(detail)
                                activehours = activehours - h
                                begintime=endtime+timedelta(seconds=1)
                            else:
                                if houraccross>totalhourscompare:
                                    if i!=0:
                                        fee = fee + money
                                        if houraccross-totalhourscompare<h:
                                            h=houraccross-totalhourscompare
                                        endtime = begintime + timedelta(seconds=int(h)*3600-1)
                                        if endtime>totime:
                                            endtime=totime
                                        detail = {"from": begintime.strftime("%d-%m-%Y %H:%M:%S"),
                                                  "to": endtime.strftime("%d-%m-%Y %H:%M:%S"),
                                                  "des": "trên block", "fee": money}
                                        lstdetail.append(detail)
                                        activehours = activehours - h
                                        begintime = endtime + timedelta(seconds=1)
                                    elif totalhourscompare==0:
                                        fee = fee + money
                                        if houraccross - totalhourscompare < h:
                                            h = houraccross - totalhourscompare
                                        endtime = begintime + timedelta(seconds=int(h) * 3600 - 1)
                                        if endtime > totime:
                                            endtime = totime
                                        detail = {"from": begintime.strftime("%d-%m-%Y %H:%M:%S"),
                                                  "to": endtime.strftime("%d-%m-%Y %H:%M:%S"),
                                                  "des": "trên block", "fee": money}
                                        lstdetail.append(detail)
                                        activehours = activehours - h
                                        begintime = endtime + timedelta(seconds=1)
                        else:
                            if repeat:
                                endtime = begintime + timedelta(seconds=int(h)*3600-1)
                                if endtime>totime:
                                    endtime=totime
                                fee = fee + h * money
                                detail = {"from": begintime.strftime("%d-%m-%Y %H:%M:%S"),
                                          "to": endtime.strftime("%d-%m-%Y %H:%M:%S"),
                                          "des": "%sh*%s" % (int(h), money), "fee": int(h) * money}
                                lstdetail.append(detail)
                                activehours = activehours - h
                                begintime = endtime + timedelta(seconds=1)
                            else:
                                if houraccross>totalhourscompare:
                                    if i!=0:
                                        if houraccross-totalhourscompare<h:
                                            h=houraccross-totalhourscompare
                                        endtime = begintime + timedelta(seconds=int(h)*3600-1)
                                        if endtime>totime:
                                            endtime=totime
                                        fee = fee + h * money
                                        detail = {"from": begintime.strftime("%d-%m-%Y %H:%M:%S"),
                                                  "to": endtime.strftime("%d-%m-%Y %H:%M:%S"),
                                                  "des": "%sh*%s" % (int(h), money), "fee": int(h) * money}
                                        lstdetail.append(detail)
                                        activehours=activehours - h
                                        begintime = endtime + timedelta(seconds=1)
                                    elif totalhourscompare==0:
                                        if houraccross-totalhourscompare<h:
                                            h=houraccross-totalhourscompare
                                        endtime = begintime + timedelta(seconds=int(h)*3600-1)
                                        if endtime>totime:
                                            endtime=totime
                                        fee = fee + h * money
                                        detail = {"from": begintime.strftime("%d-%m-%Y %H:%M:%S"),
                                                  "to": endtime.strftime("%d-%m-%Y %H:%M:%S"),
                                                  "des": "%sh*%s" % (int(h), money), "fee": int(h) * money}
                                        lstdetail.append(detail)
                                        activehours=activehours - h
                                        begintime = endtime + timedelta(seconds=1)
                i=i+1
            return {"fedetail":lstdetail},fee
    return {"fedetail":"Không xác định"}, 0
##CaseA before 2019Jan28
def get_fee_from_formula_caseB(fml, fromtime, totime, checkintime):
    if fml:
        ttseconds=(totime-fromtime).total_seconds()
        dm=divmod(ttseconds,3600)
        totalhours=int(dm[0])+(1 if dm[1]>0 else 0)
        ttseconds = (fromtime - checkintime).total_seconds()
        dm = divmod(ttseconds, 3600)
        totalhourscompare = int(dm[0]) + (1 if dm[1] > 0 else 0)
        begintime=fromtime
        if fml["feetype"]==1:
            fee=int(fml["fullfee"]) if fml["fullfee"] else 0
            return {"fedetail": [{"from": fromtime.strftime("%d-%m-%Y %H:%M:%S"),
                                 "to": totime.strftime("%d-%m-%Y %H:%M:%S"), "des": "trên block", "fee": fee}]}, fee
        if fml["feetype"] == 0:
            fee = int(fml["fullfee"]) if fml["fullfee"] else 0
            return {"fedetail": [{"from": fromtime.strftime("%d-%m-%Y %H:%M:%S"),
                                 "to": totime.strftime("%d-%m-%Y %H:%M:%S"),
                                 "des": "%s*%s=%s" % (int(totalhours), fee, int(totalhours) * fee),
                                 "fee": int(totalhours) * fee}]}, int(totalhours) * fee
        if fml["feetype"] == 2:
            blocks=fml["detail"]
            activehours=totalhours
            fee=0
            i=0
            l=len(blocks)
            houraccross=0
            lstdetail=[]
            for bl in blocks:
                if activehours>0:
                    money=int(bl["money"]) if bl["money"] else 0
                    hour=int(bl["hours"]) if bl["hours"] else activehours
                    des=bl["des"]
                    repeat = bl["isonly"]
                    if i!=0 and i==l-1:
                        if repeat:
                            endtime=begintime+timedelta(seconds=int(activehours)*3600-1)
                            if endtime>totime:
                                endtime=totime
                            fee = fee + money * activehours
                            detail = {"from": begintime.strftime("%d-%m-%Y %H:%M:%S"),
                                      "to": endtime.strftime("%d-%m-%Y %H:%M:%S"),
                                      "des": "%sh*%s" % (int(activehours), money), "fee": int(activehours) * money}
                            lstdetail.append(detail)
                        activehours = -1
                    else:
                        h = hour if hour <= activehours else activehours
                        houraccross = houraccross + hour
                        if des=="trên block":
                            if repeat:
                                endtime = begintime + timedelta(seconds=int(h)*3600-1)
                                if endtime>totime:
                                    endtime=totime
                                fee = fee + money
                                detail = {"from": begintime.strftime("%d-%m-%Y %H:%M:%S"),
                                          "to": endtime.strftime("%d-%m-%Y %H:%M:%S"),
                                          "des": "trên block", "fee": money}
                                lstdetail.append(detail)
                                activehours = activehours - h
                                begintime=endtime+timedelta(seconds=1)
                            else:
                                if houraccross>totalhourscompare:
                                    fee = fee + money
                                    if houraccross-totalhourscompare<h:
                                        h=houraccross-totalhourscompare
                                    endtime = begintime + timedelta(seconds=int(h)*3600-1)
                                    if endtime>totime:
                                        endtime=totime
                                    detail = {"from": begintime.strftime("%d-%m-%Y %H:%M:%S"),
                                              "to": endtime.strftime("%d-%m-%Y %H:%M:%S"),
                                              "des": "trên block", "fee": money}
                                    lstdetail.append(detail)
                                    activehours = activehours - h
                                    begintime = endtime + timedelta(seconds=1)
                        else:

                            if repeat:
                                endtime = begintime + timedelta(seconds=int(h)*3600-1)
                                if endtime>totime:
                                    endtime=totime
                                fee = fee + h * money
                                detail = {"from": begintime.strftime("%d-%m-%Y %H:%M:%S"),
                                          "to": endtime.strftime("%d-%m-%Y %H:%M:%S"),
                                          "des": "%sh*%s" % (int(h), money), "fee": int(h) * money}
                                lstdetail.append(detail)
                                activehours = activehours - h
                                begintime = endtime + timedelta(seconds=1)
                            else:
                                if houraccross>totalhourscompare:
                                    if houraccross-totalhourscompare<h:
                                        h=houraccross-totalhourscompare
                                    endtime = begintime + timedelta(seconds=int(h)*3600-1)
                                    if endtime>totime:
                                        endtime=totime
                                    fee = fee + h * money
                                    detail = {"from": begintime.strftime("%d-%m-%Y %H:%M:%S"),
                                              "to": endtime.strftime("%d-%m-%Y %H:%M:%S"),
                                              "des": "%sh*%s" % (int(h), money), "fee": int(h) * money}
                                    lstdetail.append(detail)
                                    activehours=activehours - h
                                    begintime = endtime + timedelta(seconds=1)
                i=i+1
            return {"fedetail":lstdetail},fee
    return {"fedetail":"Không xác định"}, 0
##CaseA before 2019Jan28
def findcycle(daylist, fdate,tdate):
    begintime=get_time(fdate)
    end=None
    index=None
    for d in daylist:
        if d[1]<d[2]:
            if begintime>=d[1] and begintime<=d[2]:
                end=fdate.replace(hour=d[2].hour, minute=d[2].minute, second=d[2].second)
                if end>tdate:
                    end=tdate
                index=d[4]
                break
        else:
            if begintime<=d[2]:
                end = fdate.replace(hour=d[2].hour, minute=d[2].minute, second=d[2].second)
                if end > tdate:
                    end = tdate
                index = d[4]
                break
            elif begintime>=d[1]:
                end = fdate.replace(hour=d[2].hour, minute=d[2].minute, second=d[2].second)+timedelta(days=1)
                if end > tdate:
                    end = tdate
                index = d[4]
                break
    if end:
        tt_sc = (end - fdate).total_seconds()
        dm = divmod(tt_sc, 3600)
        tt_hours = int(dm[0]) + (1 if dm[1] > 0 else 0)
        return tt_hours,end,index
    return 0,None,None
def callfullfeecomplex(feeid, fromdate, todate):
    fml = getformulas()
    try:

        util = Utilities()
        qr = util.QuerySecond('getsamplefee', feeid, 0)
        if len(qr)<1:
            return {"callname": "Không xác định", "feetype": 0, "details": [
                {"from": fromdate.strf("%d-%m-%Y %H:%M:%S"), "to": todate.strftime("%d-%m-%Y %H:%M:%S"), "fee": 0}]}, 0
        feetype=int(qr[0][4]) if qr[0][4] else 0
        if feetype==0:
            return {"callname": "Không xác định", "feetype": 0, "details": [
                {"from": fromdate.strf("%d-%m-%Y %H:%M:%S"), "to": todate.strftime("%d-%m-%Y %H:%M:%S"), "fee": 0}]}, 0
        if feetype==1:
            return callfullfee(qr,fromdate,todate)
        if feetype==2:
            return  callfullfee24h(qr,fromdate,todate)
        if feetype == 5:
            return callfullfeenn(feeid, fromdate, todate)
        totalseconds = (todate - fromdate).total_seconds()
        freetime=int(qr[0][2]) if qr[0][2] else 0
        subtractfree=int(qr[0][9]) if qr[0][9] and freetime>0 else 0

        if totalseconds<=(freetime*60):
            return {"callname": qr[0][1],"feetype":-1, "details": ("Tổng thời gian: %s giây <= Thời gian miễn phí %s giây"%(int(totalseconds),int(freetime*60)))}, 0
        # in case similar Sedona
        freedes=''
        if subtractfree<1 and freetime>0:
            tdateee=fromdate+timedelta(minutes=freetime)+timedelta(seconds=-1)
            freedes = "Từ %s đến %s không tính phí %s phút => phí = 0đ" % (
            fromdate.strftime("%d-%m-%Y %H:%M:%S"), tdateee.strftime("%d-%m-%Y %H:%M:%S"), freetime)
            fromdate=fromdate+timedelta(minutes=freetime)
        #
        tolerancetime=int(qr[0][3]) if qr[0][3] else 0
        subtracttolerance = int(qr[0][10]) if qr[0][10] and tolerancetime > 0 else 0
        toldes=''
        if subtracttolerance>0 and tolerancetime > 0:
            tdateee=todate-timedelta(minutes=tolerancetime)+timedelta(seconds=1)
            toldes = "Từ %s đến %s áp dụng dung sai %s phút => phí = 0đ" % (
                tdateee.strftime("%d-%m-%Y %H:%M:%S"), todate.strftime("%d-%m-%Y %H:%M:%S"),tolerancetime)
            todate=todate-timedelta(minutes=tolerancetime)
            tolerancetime=0
        starttime=qr[0][8] if qr[0][8] else time(0,0,0,0)
        datebegin=fromdate.replace(hour=starttime.hour,minute=starttime.minute,second=starttime.second)
        if datebegin>fromdate:
            datebegin=datebegin+timedelta(days=-1)
        dateend=datebegin+timedelta(days=1)+timedelta(seconds=-1)
        dow=datebegin.weekday()
        dayfirst = None
        daylast = None
        if todate<=dateend:
            dayfirst = {"from": fromdate, "to": todate,
                        "fee": 0,"feedetail":"", "weekofdate": wodate(dow), "weekofdatename": wodatename(dow),"pc":None,"ispecial":False}
        else:
            dayfirst = {"from": fromdate, "to": dateend,
                        "fee": 0,"feedetail":"", "weekofdate": wodate(dow), "weekofdatename": wodatename(dow),"pc":None,"ispecial":False}

        qrs = util.QuerySecond('getdatetypecomplex', feeid)
        totalfee=0
        specialday = [x for x in qrs if x[2].find("0") >= 0]
        # first
        if dayfirst:
            pc, ispecial = IsSpecialDay(dayfirst["from"])
            datetypeid = -1
            timecompare = get_time(dayfirst["to"])
            if ispecial and len(specialday) == 1:
                datetypeid = specialday[0][0]
            else:
                pc=None
                ispecial=False
                myday = [x for x in qrs if x[2].find(dayfirst["weekofdate"]) >= 0]
                if len(myday) == 1:
                    datetypeid = myday[0][0]
            dtnm = util.QuerySecond('getcyclecomplex', datetypeid)
            dtgold = [x for x in dtnm if x[5] == 2]
            beginfirst = dayfirst["from"]
            endfirst = dayfirst["to"]
            while (todate-fromdate).total_seconds() < 24*3600 and beginfirst<=endfirst and dtgold:
                tt_hours, end, index = findcycle(dtgold, beginfirst, endfirst)
                if end is None or end < todate:
                    break
                formula=fml[getindex(index, fml)]
                ttfee = get_fee_from_formula(formula, tt_hours)
                days = {"first": {"from": fromdate.strftime("%d-%m-%Y %H:%M:%S"),
                                  "to": todate.strftime("%d-%m-%Y %H:%M:%S"), "fee": ttfee,
                                  "weekofdatename": "Thuộc khung giờ vàng %s"%(dayfirst["weekofdatename"])},"minddle":"","last":""}
                return {"callname": qr[0][1], "feetype": 3, "details": days,
                        "formula": formula}, ttfee
            dtfirst = [x for x in dtnm if x[5] == 3]
            if len(dtfirst) < 1:
                dtfirst = [x for x in dtnm if x[5] == 1]
            detail=[]
            beginfirst=dayfirst["from"]
            endfirst=dayfirst["to"]
            while beginfirst<=endfirst and dtfirst:
                tt_hours,end,index=findcycle(dtfirst,beginfirst,endfirst)
                tt_sc=(beginfirst-fromdate).total_seconds()
                dm=divmod(tt_sc,3600)
                tt_hours1=int(dm[0])+(1 if dm[1]>0 else 0)
                if end is None:
                    break
                if (end - beginfirst).total_seconds() <= (tolerancetime * 60):
                    break
                detail.append({"fomurla": fml[getindex(index, fml)], "fromsub": beginfirst.strftime("%d-%m-%Y %H:%M:%S"),
                               "tosub": end.strftime("%d-%m-%Y %H:%M:%S"), "totalhours": tt_hours,"totalhours1":tt_hours1,"fedetail":"","fee":0})
                beginfirst=end+timedelta(seconds=1)
            dayfirst["from"]=dayfirst["from"].strftime("%d-%m-%Y %H:%M:%S")
            dayfirst["to"] = dayfirst["to"].strftime("%d-%m-%Y %H:%M:%S")
            dayfirst["pc"]=pc
            dayfirst["ispecial"]=ispecial
            ff=0
            for dt in detail:
                frsub=datetime.strptime(dt["fromsub"],"%d-%m-%Y %H:%M:%S")
                tosub =datetime.strptime(dt["tosub"],"%d-%m-%Y %H:%M:%S")
                fromcheckin = (int)(dt["totalhours1"])
                fmm = dt["fomurla"]
                bl = fmm["detail"]
                hour = int(bl[0]["hours"]) if bl !='' and bl[0]["hours"] else 0
                if fromcheckin > 0 and fromcheckin < hour:
                    chk=fromdate+timedelta(hours=fromcheckin)
                    if fmm["feetype"] == 2 and frsub<chk:
                        frsub = fromdate + timedelta(hours=hour, seconds=1)
                det,fullfee=get_fee_from_formula_caseB(dt["fomurla"],frsub,tosub,fromdate)
                #dt["fomurla"]["fullfee"]=fullfee
                dt["fedetail"]=det
                dt["fee"]=fullfee
                ff+=fullfee
            dayfirst["feedetail"]=detail
            if dayfirst["pc"] and dayfirst["ispecial"]:
                tmpff= ff + int((ff* dayfirst["pc"]["Percent"])/100)
                ff=tmpff if tmpff>0 else 0
                dayfirst["weekofdatename"] = u"%s (%s- tăng giảm %s phần trăm)" % (
                    dayfirst["weekofdatename"], dayfirst["pc"]["CallName"], dayfirst["pc"]["Percent"])
            dayfirst["fee"]=ff
            totalfee = totalfee+ff
        # first

        daysmiddle = []
        while dateend < todate:
            dateend = dateend + timedelta(days=1)
            datebegin = datebegin + timedelta(days=1)
            dow = datebegin.weekday()
            if dateend >= todate:
                daylast = {"from": datebegin, "to": todate,
                           "fee": 0, "feedetail": "", "weekofdate": wodate(dow), "weekofdatename": wodatename(dow),"pc":None,"ispecial":False}
            else:
                daysmiddle.append({"from": datebegin, "to": dateend,
                                   "fee": 0, "feedetail": "trọn ngày %s" % (wodatename(dow)),
                                   "weekofdate": wodate(dow), "weekofdatename": wodatename(dow),"pc":None,"ispecial":False})

        for md in daysmiddle:
            # pc,ispecial=IsSpecialDay(md["from"])
            # if ispecial and len(specialday)==1:
            #     md["fee"]=int(specialday[0][4]) + int(specialday[0][4])*(int(pc) if pc else 0) if specialday[0][4] else 0
            #     md["from"]=("%s: ngày đặc biệt"%(md["from"].strftime("%d-%m-%Y %H:%M:%S")))
            #     md["to"] = ("%s: ngày đặc biệt" % (md["to"].strftime("%d-%m-%Y %H:%M:%S")))
            # else:
            #     myday=[x for x in qrs if x[2].find(md["weekofdate"])>=0]
            #     if len(myday)==1:
            #         md["from"]=md["from"].strftime("%d-%m-%Y %H:%M:%S")
            #         md["to"] = md["to"].strftime("%d-%m-%Y %H:%M:%S")
            #         md["fee"] = int(myday[0][4]) if myday[0][4] else 0
            # totalfee=totalfee+md["fee"]
            ##
            pc, ispecial = IsSpecialDay(md["from"])
            datetypeid = -1
            timecompare = get_time(md["from"])
            if ispecial and len(specialday) == 1:
                datetypeid = specialday[0][0]
            else:
                pc = None
                ispecial = False
                myday = [x for x in qrs if x[2].find(md["weekofdate"]) >= 0]
                if len(myday) == 1:
                    datetypeid = myday[0][0]
            dtnm = util.QuerySecond('getcyclecomplex', datetypeid)
            dtlast = [x for x in dtnm if x[5] == 1]
            detail = []
            beginfirst = md["from"]
            endfirst = md["to"]
            while beginfirst <= endfirst and dtlast:
                tt_hours, end, index = findcycle(dtlast, beginfirst, endfirst)
                tt_sc = (beginfirst - fromdate).total_seconds()
                dm = divmod(tt_sc, 3600)
                tt_hours1 = int(dm[0]) + (1 if dm[1] > 0 else 0)
                if end is None:
                    break
                if (end - beginfirst).total_seconds() <= (tolerancetime * 60):
                    break
                detail.append(
                    {"fomurla": fml[getindex(index, fml)], "fromsub": beginfirst.strftime("%d-%m-%Y %H:%M:%S"),
                     "tosub": end.strftime("%d-%m-%Y %H:%M:%S"), "totalhours": tt_hours, "totalhours1": tt_hours1,
                     "fedetail": "","fee":0})
                beginfirst = end + timedelta(seconds=1)
            md["from"] = md["from"].strftime("%d-%m-%Y %H:%M:%S")
            md["to"] = md["to"].strftime("%d-%m-%Y %H:%M:%S")
            md["pc"] = pc
            md["ispecial"] = ispecial
            ff = 0
            for dt in detail:
                frsub = datetime.strptime(dt["fromsub"], "%d-%m-%Y %H:%M:%S")
                tosub = datetime.strptime(dt["tosub"], "%d-%m-%Y %H:%M:%S")
                fromcheckin = (int)(dt["totalhours1"])
                fmm = dt["fomurla"]
                bl = fmm["detail"]
                hour = int(bl[0]["hours"]) if bl !='' and bl[0]["hours"] else 0
                if fromcheckin > 0 and fromcheckin < hour:
                    chk = fromdate + timedelta(hours=fromcheckin)
                    if fmm["feetype"] == 2 and frsub < chk:
                        frsub = fromdate + timedelta(hours=hour, seconds=1)
                det, fullfee = get_fee_from_formula_caseB(dt["fomurla"], frsub, tosub, fromdate)
                dt["fee"] = fullfee
                dt["fedetail"] = det
                ff += fullfee
            md["feedetail"] = detail
            if md["pc"] and md["ispecial"]:
                tmpff = ff + int((ff * md["pc"]["Percent"]) / 100)
                ff = tmpff if tmpff > 0 else 0
                md["weekofdatename"]=u"%s (%s - tăng giảm %s phần trăm)"%(md["weekofdatename"],md["pc"]["CallName"],md["pc"]["Percent"])
            md["fee"] = ff
            totalfee = totalfee + ff
            ##
        # last
        if daylast:
            pc, ispecial = IsSpecialDay(daylast["from"])
            datetypeid = -1
            timecompare = get_time(daylast["from"])
            if ispecial and len(specialday) == 1:
                datetypeid = specialday[0][0]
            else:
                pc = None
                ispecial = False
                myday = [x for x in qrs if x[2].find(daylast["weekofdate"]) >= 0]
                if len(myday) == 1:
                    datetypeid = myday[0][0]
            dtnm = util.QuerySecond('getcyclecomplex', datetypeid)
            dtlast = [x for x in dtnm if x[5] == 1]
            detail = []
            beginfirst = daylast["from"]
            endfirst = daylast["to"]
            while beginfirst <= endfirst and dtlast:
                tt_hours, end, index = findcycle(dtlast, beginfirst, endfirst)
                tt_sc = (beginfirst - fromdate).total_seconds()
                dm = divmod(tt_sc, 3600)
                tt_hours1 = int(dm[0]) + (1 if dm[1] > 0 else 0)
                if end is None:
                    break
                if (end-beginfirst).total_seconds()<=(tolerancetime*60):
                    break
                detail.append(
                    {"fomurla": fml[getindex(index, fml)], "fromsub": beginfirst.strftime("%d-%m-%Y %H:%M:%S"),
                     "tosub": end.strftime("%d-%m-%Y %H:%M:%S"), "totalhours": tt_hours,"totalhours1":tt_hours1,"fedetail":"","fee":0})
                beginfirst = end + timedelta(seconds=1)
            daylast["from"] = daylast["from"].strftime("%d-%m-%Y %H:%M:%S")
            daylast["to"] = daylast["to"].strftime("%d-%m-%Y %H:%M:%S")
            daylast["pc"] = pc
            daylast["ispecial"] = ispecial
            ff = 0
            for dt in detail:
                frsub = datetime.strptime(dt["fromsub"], "%d-%m-%Y %H:%M:%S")
                tosub = datetime.strptime(dt["tosub"], "%d-%m-%Y %H:%M:%S")
                fromcheckin = (int)(dt["totalhours1"])
                fmm = dt["fomurla"]
                bl = fmm["detail"]
                hour = int(bl[0]["hours"]) if bl !='' and bl[0]["hours"] else 0
                if fromcheckin > 0 and fromcheckin < hour:
                    chk = fromdate + timedelta(hours=fromcheckin)
                    fmm = dt["fomurla"]
                    if fmm["feetype"] == 2 and frsub < chk:
                        frsub = fromdate + timedelta(hours=hour, seconds=1)
                det,fullfee = get_fee_from_formula_caseB(dt["fomurla"], frsub,tosub,fromdate)
                #dt["fomurla"]["fullfee"] = fullfee
                dt["fedetail"] = det
                dt["fee"] = fullfee
                ff += fullfee
            daylast["feedetail"] = detail
            if daylast["pc"] and daylast["ispecial"]:
                tmpff = ff + int((ff * daylast["pc"]["Percent"]) / 100)
                ff = tmpff if tmpff > 0 else 0
                daylast["weekofdatename"] = u"%s (%s- tăng giảm %s phần trăm)" % (
                    daylast["weekofdatename"], daylast["pc"]["CallName"], daylast["pc"]["Percent"])
            daylast["fee"] = ff
            totalfee = totalfee + ff
        # last
        days = {"freedes":freedes,"tolerancedes":toldes,"first": dayfirst if dayfirst else '', "minddle": daysmiddle if daysmiddle else '', "last": daylast if daylast else ''}
        return {"callname": qr[0][1],"feetype":3, "details":days}  , totalfee
    except Exception as e:
        data = {"callname": "Không xác định", "feetype": 0, "details": [
            {"from": fromdate.strftime("%d-%m-%Y %H:%M:%S"), "to": todate.strftime("%d-%m-%Y %H:%M:%S"), "fee": 0}]}
        return data, 0
#2019Jan28
def callactivefee(activeid,fromdate,todate,expiratedate, notregis):
    util = Utilities()
    fr=fromdate.strftime("%Y%m%d%H%M%S")
    to = todate.strftime("%Y%m%d%H%M%S")
    exp=expiratedate.strftime("%Y%m%d%H%M%S")
    fromdate=datetime.strptime(fr,"%Y%m%d%H%M%S")
    todate=datetime.strptime(to,"%Y%m%d%H%M%S")
    expiratedate=datetime.strptime(exp,"%Y%m%d%H%M%S")
    qr = util.QuerySecond('getsampleactivebyid', activeid)
    if len(qr)<1:
        return {"callname":"không xác định","type":0},0
    if notregis:
        sampleid= qr[0][8] if qr[0][8] else -1
    else:
        sampleid= qr[0][5] if qr[0][5] else -1
    if (not notregis) and (qr[0][6]) and (expiratedate<todate):
        if expiratedate<=fromdate:
            callfee = callfullfeecomplex(qr[0][6], fromdate, todate)
            return {"callname":"Hết hạn","type":2,"detail":{"nomal":"","expired":callfee[0]}},callfee[1]
        else:
            callfee = callfullfeecomplex(sampleid, fromdate, expiratedate+timedelta(seconds=-1))
            callfee1 = callfullfeecomplex(qr[0][6], expiratedate, todate)
            return {"callname": "Thông thường + Hết hạn", "type": 3, "detail": {"nomal":callfee[0],"expired":callfee1[0]}}, int(callfee[1]+callfee1[1])
    else:
        callfee = callfullfeecomplex(sampleid, fromdate, todate)
        fee = recalculate_fee(callfee[1], fromdate, todate, qr[0][3])
        return {"callname": "Thông thường", "type": 1, "detail": {"nomal": callfee[0], "expired": ""}}, fee
def recalculate_fee(fee, fromdate, todate, vehicle_tye_id):
    tt_sc = (todate - fromdate).total_seconds()
    dm = divmod(tt_sc, 3600)
    tt_hours1 = int(dm[0]) + (1 if dm[1] > 0 else 0)
    if tt_hours1 <= 10:
        if vehicle_tye_id == 1000001:
            if fee > 6000:
            	return 6000
            else:
            	return fee
        elif vehicle_tye_id == 2000101:
            if fee > 20000:
            	return 20000
            else:
            	return fee
        elif vehicle_tye_id == 4000301:
            if fee > 3000:
            	return 3000
            else:
            	return fee
    elif tt_hours1 > 10 and tt_hours1 <= 24:
        if vehicle_tye_id == 1000001:
            if fee > 10000:
            	return 10000
            else:
            	return fee
        elif vehicle_tye_id == 2000101:
            if fee > 40000:
            	return 40000
            else:
            	return fee
        elif vehicle_tye_id == 4000301:
            if fee > 5000:
            	return 5000
            else:
            	return fee
    else:
        return fee
def findactiveid(vehicleid,cardid,outdate):
    try:
        util =  Utilities()
        qr = util.QuerySecond('getactiveid', vehicleid, cardid, outdate)
        if len(qr)>0 and qr[0][0]:
            return int(qr[0][0])
        return -1
    except Exception as e:
        return -1
def findredemptionactiveid(vehicleid,groupid,redemtiondate):
    try:
        util =  Utilities()
        qr = util.QuerySecond('geredemtiontactiveid', vehicleid, groupid, redemtiondate)
        if len(qr)>0 and qr[0][0]:
            return int(qr[0][0])
        return -1
    except Exception as e:
        return -1
def get_parking_fee_info(card_id, vehicle_type, card_type, check_in_time, to_time):
    try:
        vehicle_type_id = VEHICLE_TYPE_ENCODE_DICT[vehicle_type]
        expiredtime = to_time.astimezone(timezone(TIME_ZONE))
        intime=check_in_time.astimezone(timezone(TIME_ZONE))
        outtime= to_time.astimezone(timezone(TIME_ZONE))
        activeid=findactiveid(vehicle_type_id,card_type,outtime.date())
        vehicle_registration = VehicleRegistration.objects.filter(card__card_id=card_id)
        if vehicle_registration:
            vehicle_registration = vehicle_registration[0]
            tmptime = get_now_utc().astimezone(timezone(TIME_ZONE)).replace(hour=0, minute=0, second=0)
            expired_date = vehicle_registration.expired_date + timedelta(days=1) if vehicle_registration.expired_date  else None
            cancel_date = vehicle_registration.cancel_date if vehicle_registration.cancel_date else None
            if expired_date and cancel_date:
                if expired_date < cancel_date:
                    tmptime = tmptime.replace(year=expired_date.year, month=expired_date.month, day=expired_date.day)
                else:
                    tmptime = tmptime.replace(year=cancel_date.year, month=cancel_date.month, day=cancel_date.day)
            elif expired_date:
                tmptime = tmptime.replace(year=expired_date.year, month=expired_date.month, day=expired_date.day)
            elif cancel_date:
                tmptime = tmptime.replace(year=cancel_date.year, month=cancel_date.month, day=cancel_date.day)
            else:
                tmptime = to_time.astimezone(timezone(TIME_ZONE))
            if tmptime < expiredtime:
                expiredtime = tmptime
            callfee=callactivefee(activeid,intime,outtime,expiredtime,False)
            parking_fee, parking_fee_detail=callfee[1],u"Dynamic tool fee"
            customer = vehicle_registration.customer
            update_renewal_status_info(vehicle_registration)
            #
            # pre_check = is_vehicle_registration_available(card_id)
            # if pre_check[0]:
            #     if pre_check[2]:
            #         parking_fee = 0
            #         parking_fee_detail = u""
            pre_checkout_data = {
                "parking_fee": parking_fee,
                "parking_fee_detail": parking_fee_detail,

                "customer_name": customer.customer_name,
                "customer_id": customer.customer_id,
                "customer_type": customer.customer_type.name if customer.customer_type else '',

                "birthday": customer.customer_birthday,
                "phone": customer.customer_phone,
                "mobile": customer.customer_mobile,
                "email": customer.customer_email,
                "apartment": customer.apartment.address if customer.apartment else '',
                "building": customer.building.name if customer.building else '',
                "company": customer.company.name if customer.company else '',

                "order_register_name": customer.order_register_name,
                "order_register_address": customer.order_register_address,
                "order_tax_code": customer.order_tax_code,

                "messaging_sms_phone": customer.messaging_sms_phone,
                "messaging_email": customer.messaging_email,
                "messaging_address": customer.messaging_address,
		
		"vehicle_type_from_card": vehicle_type_id,

                "vehicle_registration_info": {
                    "status": vehicle_registration.status,
                    "enum_status": dict(VEHICLE_STATUS_CHOICE),
                    "total_remain_duration": get_total_remain_duration(vehicle_registration),
                    "level_fee": vehicle_registration.level_fee.__unicode__() if vehicle_registration else '',

                    "registration_date": vehicle_registration.registration_date.astimezone(timezone(TIME_ZONE)).strftime(
                        "%d/%m/%Y %H:%M"),
                    "first_renewal_effective_date": vehicle_registration.first_renewal_effective_date if vehicle_registration.first_renewal_effective_date else None,
                    "last_renewal_date": vehicle_registration.last_renewal_date if vehicle_registration.last_renewal_date else None,
                    "last_renewal_effective_date": vehicle_registration.last_renewal_effective_date if vehicle_registration.last_renewal_effective_date else None,

                    "start_date": vehicle_registration.start_date if vehicle_registration.start_date else None,
                    "expired_date": vehicle_registration.expired_date if vehicle_registration.expired_date else None,

                    "pause_date": vehicle_registration.pause_date if vehicle_registration.pause_date else None,
                    "cancel_date": vehicle_registration.cancel_date if vehicle_registration.cancel_date else None,

                    "vehicle_driver_name": vehicle_registration.vehicle_driver_name,
                    "vehicle_driver_id": vehicle_registration.vehicle_driver_id,
                    "vehicle_driver_phone": vehicle_registration.vehicle_driver_phone,

                    "vehicle_type": vehicle_registration.vehicle_type.name if vehicle_registration.vehicle_type else '',
                    "vehicle_number": vehicle_registration.vehicle_number,
                    "vehicle_brand": vehicle_registration.vehicle_brand,
                    "vehicle_paint": vehicle_registration.vehicle_paint,
                }
            }
        else:
            callfee = callactivefee(activeid, intime, outtime, expiredtime,True)
            parking_fee, parking_fee_detail = callfee[1], u"Dynamic tool fee"
            pre_checkout_data = {
                "parking_fee": parking_fee,
                "parking_fee_detail": parking_fee_detail
            }
        return pre_checkout_data
    except Exception as e:
        return {"parking_fee": 0,
                "parking_fee_detail": ""}
def callfeetestcomplex(request,feeid,checkin,checkout):
    cin=datetime.strptime(checkin, "%Y%m%d%H%M%S")
    cout=datetime.strptime(checkout, "%Y%m%d%H%M%S")
    callfee=callfullfeecomplex(feeid,cin,cout)
    data={"detail":callfee[0],"fee":callfee[1]}
    datajson = json.dumps(data, ensure_ascii=False).encode('utf8')
    return HttpResponse(datajson)
def callfeetestactive(request,feeid,checkin,checkout,expireddate,notregis):
    cin=datetime.strptime(checkin, "%Y%m%d%H%M%S")
    cout=datetime.strptime(checkout, "%Y%m%d%H%M%S")
    epr=datetime.strptime(expireddate, "%Y%m%d%H%M%S")
    notRegis= True if notregis =='1' else False
    callfee=callactivefee(feeid,cin,cout,epr,notRegis)
    data={"detail":callfee[0],"fee":callfee[1]}
    datajson = json.dumps(data, ensure_ascii=False).encode('utf8')
    return HttpResponse(datajson)
def callredemtion1(vehicletype,checkintime,redemtiontime,billdata):
    try:
        # util = Utilities()
        # qr = util.QuerySecond('gettenantgroup')
        tg=ClaimPromotionGroupTenant.objects.all()
        group=[]
        for g in tg:
            gid=g.id
            groupname=g.groupname
            companies=[]
            # qrs = util.QuerySecond('gettennants', gid)
            tn=ClaimPromotionTenant.objects.filter(group_tenant=gid)
            for t in tn:
                companies.append({"company":t.name})
            group.append({"groupid":gid,"groupname":groupname,"companies":companies,"billamount":0})
        for d in billdata:
            ba=int(d["bill_amount"]) if d["bill_amount"] or d["bill_amount"]!='' else 0
            gr=[]
            for g in group:
                baa = g["billamount"]
                for c in g["companies"]:
                    if c["company"]==d["company_info"]:
                        g["billamount"]=baa+ba
        activegroup=[x for x in group if x["billamount"]>0]
        activetime = datetime.strptime(redemtiontime, "%Y%m%d%H%M%S")
        citime = datetime.strptime(checkintime, "%Y%m%d%H%M%S")
        rdfee=0
        for g in activegroup:
            activeid = findredemptionactiveid(vehicletype, g["groupid"], activetime.date())
            detail = callredemtion(activeid, g["billamount"], g["groupname"], activetime, citime)
            rdfee = rdfee + int(detail["redemptionfee"])
        return rdfee
    except Exception as e:
        return 0
##
##
###
