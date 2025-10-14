# -*- coding: utf-8 -*-

# Standard
from operator import itemgetter
from os import path, mkdir, remove
from datetime import datetime, timedelta, date
from math import ceil
import os

# Django
##pdf
# from django.core.files.storage import  FileSystemStorage
# from django.template.loader import render_to_string
# from weasyprint import HTML
from io import  BytesIO
from reportlab.pdfgen import canvas
from reportlab.lib import colors
from reportlab.lib.pagesizes import letter, mm, inch, A0, A2, A3, A4
from reportlab.platypus import Spacer, Image, Paragraph, BaseDocTemplate, SimpleDocTemplate, Table, TableStyle
from reportlab.lib.styles import getSampleStyleSheet,ParagraphStyle,TA_CENTER
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.pdfbase import pdfmetrics

##pdf
from django.contrib import messages
from django.core.urlresolvers import reverse
from django.db.models import Q, Sum
from django.http import HttpResponseRedirect, HttpResponse
from django.shortcuts import redirect, render
from django.utils.timezone import localtime
from django.contrib.auth.models import Group

# 3rd party
from xlsxwriter import Workbook
from pytz import utc
# Local
from models import CustomerType, get_setting, VehicleRegistration, VEHICLE_TYPE, VehicleType, ParkingFeeSession,UserProfile, \
    Customer, TicketPaymentDetail, DepositPaymentDetail, DepositPayment, Receipt, TicketPayment, PauseResumeHistory, \
    Card, ClaimPromotion, ClaimPromotionBill, ClaimPromotionV2, ClaimPromotionBillV2, load_nghia_vehicle_type, CardType, \
    ParkingSession, get_storaged_vehicle_type, FeeAdjustment, load_vehicle_type_name, admin_log
from django.contrib.auth.models import User
__author__ = 'S factors'

from django.contrib.auth.decorators import login_required
### bắt đầu thêm mới: 2018-01-14
from django.db import connections
import json
### kết thúc thêm mới: 2018-01-14
VEHICLE_REGISTRATION_STATUS_CHOICE = (
    ('status_active', u'Hoạt động'),
    ('status_expired', u'Hết hạn'),
    ('status_cancel', u'Hủy đậu xe'),
    ('status_pause', u'Tạm ngừng'),
)
def notallowreport(uid):
    # util = Utilities()
    # dts = util.Query("getpermissionbyuser",uid)
    # if dts and len(dts)>2:
    #     return False
    # return  True
    return False

def add_worksheet(sheet_name, workbook, TEMPLATE, data, custom_param, sheet_protect=True):  # Them sheet vao workbook
    LOGO_EXCEL_REPORT_PATH = 'parking/static/image/' + get_setting('logo_report',
                                                                   u'Mẫu logo dùng trong báo cáo',
                                                                   'logo_report.png')
    LOGO_EXCEL_REPORT_PATH_RIGHT = 'parking/static/image/' + get_setting('logo_repor',
                                                                         u'Mẫu logo dùng trong báo cáo',
                                                                         'logo_report_right.png')

    TITLE = TEMPLATE['TITLE']
    HEADER = TEMPLATE['HEADER']
    STAT = TEMPLATE['STAT']

    sheet = workbook.add_worksheet(sheet_name)
    sheet.insert_image(0, 0, LOGO_EXCEL_REPORT_PATH)
    sheet.insert_image(0, 8, LOGO_EXCEL_REPORT_PATH)

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
def add_worksheet_new_card_status(sheet_name, workbook, TEMPLATE, queryset, sheet_protect=True):  # Them sheet vao workbook
    # LOGO_EXCEL_REPORT_PATH = 'parking/static/image/logo_report.png'
    LOGO_EXCEL_REPORT_PATH = 'parking/static/image/' + get_setting('logo_report',
                                                                   u'Mẫu logo dùng trong báo cáo',
                                                                   'logo_report.png')
    LOGO_EXCEL_REPORT_PATH_RIGHT = 'parking/static/image/' + get_setting('logo_repor',
                                                                         u'Mẫu logo dùng trong báo cáo',
                                                                         'logo_report_right.png')
    TITLE = TEMPLATE['TITLE']
    HEADER = TEMPLATE['HEADER']
    STAT = TEMPLATE['STAT']

    sheet = workbook.add_worksheet(sheet_name)
    sheet.insert_image(0, 0, LOGO_EXCEL_REPORT_PATH)
    sheet.insert_image(0, 8, LOGO_EXCEL_REPORT_PATH_RIGHT)
    bold = workbook.add_format({'bold': True})
    border = workbook.add_format()
    border.set_border()
    bold_boder = workbook.add_format({'bold': True, 'border': 1})
    text_wrap = workbook.add_format({'border': 1})
    text_wrap.set_text_wrap()

    for r in TITLE:  # Viet tieu de: (dong r[0], cot r[1])
        sheet.write(r[0], r[1], r[2], bold)
        if len(r) == 4:
            sheet.write(r[0], r[1] + 1, r[3])

    for r in STAT:  # Viet dong thong ke theo loai xe: (dong r[0], cot r[1])
        sheet.write(r[0], r[1], r[2], bold)
        sum_r = queryset.filter(vehicle_type_id=r[3]).count()
        sheet.write(r[0], r[1] + 1, sum_r)

    for i, column in enumerate(HEADER):  # Viet table header (dong 5)
        sheet.write(5, i - 1, HEADER[i][0], bold_boder)
        sheet.set_column(i - 1, i - 1, 15)  # Default width
        if len(column) > 1 and 'format' in column[1]:
            if 'width' in column[1]['format']:
                width = column[1]['format']['width']
                sheet.set_column(i - 1, i - 1, width)

    # Write data
    fields = [f[1]['db_field'] for f in HEADER if len(f) > 1 and 'db_field' in f[1]]

    values = (queryset.values(*fields))  # Dict values list

    start_row = 6

    border_format = workbook.add_format({'border': 1})
    date_format = workbook.add_format({'border': 1, 'num_format': 'dd/mm/yyyy'})

    for stt, raw_row_data in enumerate(values):  # Prepare data
        # temp_row = []
        if raw_row_data['status']==1:
            raw_row_data['status'] = 'Đang Dùng'
        elif raw_row_data['status'] == 2:
            raw_row_data['status'] = 'Khóa'
        for col, column in enumerate(HEADER):
            item = ''
            # column_order = column[1]
            if column[0] == u'STT':
                item = stt + 1
                sheet.write(start_row + stt, col - 1, item, border_format)

            elif len(column) > 1 and 'db_field' in column[1]:
                db_field = column[1]['db_field']
                item = raw_row_data[db_field]

                if item is not None:
                    if 'custom_function' in column[1]:
                        item = column[1]['custom_function'](item)
                    # Check format
                    wrote = False
                    if 'format' in column[1]:
                        if 'date' in column[1]['format']:
                            item = item.strftime("%d/%m/%Y")
                        if 'text_wrap' in column[1]['format']:
                            sheet.write(start_row + stt, col - 1, item, text_wrap)
                            wrote = True
                    if not wrote:
                        sheet.write(start_row + stt, col - 1, item, border_format)
                else:
                    sheet.write(start_row + stt, col - 1, '', border_format)
            else:
                sheet.write(start_row + stt, col - 1, item, border_format)

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

def add_worksheet_new_admin_log(sheet_name, workbook, TEMPLATE, queryset, sheet_protect=True):  # Them sheet vao workbook
    # LOGO_EXCEL_REPORT_PATH = 'parking/static/image/logo_report.png'
    LOGO_EXCEL_REPORT_PATH = 'parking/static/image/' + get_setting('logo_report',
                                                                   u'Mẫu logo dùng trong báo cáo',
                                                                   'logo_report.png')
    LOGO_EXCEL_REPORT_PATH_RIGHT = 'parking/static/image/' + get_setting('logo_repor',
                                                                         u'Mẫu logo dùng trong báo cáo',
                                                                         'logo_report_right.png')
    TITLE = TEMPLATE['TITLE']
    HEADER = TEMPLATE['HEADER']
    STAT = TEMPLATE['STAT']

    sheet = workbook.add_worksheet(sheet_name)
    sheet.insert_image(0, 0, LOGO_EXCEL_REPORT_PATH)
    sheet.insert_image(0, 8, LOGO_EXCEL_REPORT_PATH_RIGHT)
    bold = workbook.add_format({'bold': True, 'locked':1})
    border = workbook.add_format({ 'locked':1})
    border.set_border({ 'locked':1})
    bold_boder = workbook.add_format({'bold': True, 'border': 1, 'locked':1})
    text_wrap = workbook.add_format({'border': 1, 'locked':1})
    text_wrap.set_text_wrap()

    for r in TITLE:  # Viet tieu de: (dong r[0], cot r[1])
        sheet.write(r[0], r[1], r[2], bold)
        if len(r) == 4:
            sheet.write(r[0], r[1] + 1, r[3])

    for r in STAT:  # Viet dong thong ke theo loai xe: (dong r[0], cot r[1])
        sheet.write(r[0], r[1], r[2], bold)
        sum_r = queryset.filter(vehicle_type_id=r[3]).count()
        sheet.write(r[0], r[1] + 1, sum_r)

    for i, column in enumerate(HEADER):  # Viet table header (dong 5)
        sheet.write(5, i - 1, HEADER[i][0], bold_boder)
        sheet.set_column(i - 1, i - 1, 15)  # Default width
        if len(column) > 1 and 'format' in column[1]:
            if 'width' in column[1]['format']:
                width = column[1]['format']['width']
                sheet.set_column(i - 1, i - 1, width)

    # Write data
    fields = [f[1]['db_field'] for f in HEADER if len(f) > 1 and 'db_field' in f[1]]

    values = (queryset.values(*fields))  # Dict values list

    start_row = 6

    border_format = workbook.add_format({'border': 1})
    date_format = workbook.add_format({'border': 1, 'num_format': 'dd/mm/yyyy'})

    for stt, raw_row_data in enumerate(values):  # Prepare data
        # temp_row = []
        if raw_row_data['action_flag'] == 1:
            raw_row_data['action_flag'] = 'Tạo mới '
        elif raw_row_data['action_flag'] == 2:
            raw_row_data['action_flag'] = 'Cập nhật'
        else:
            raw_row_data['action_flag'] = 'Xóa'

        for col, column in enumerate(HEADER):
            item = ''
            # column_order = column[1]
            if column[0] == u'STT':
                item = stt + 1
                sheet.write(start_row + stt, col - 1, item, border_format)

            elif len(column) > 1 and 'db_field' in column[1]:
                db_field = column[1]['db_field']
                item = raw_row_data[db_field]

                if item is not None:
                    if 'custom_function' in column[1]:
                        item = column[1]['custom_function'](item)
                    # Check format
                    wrote = False
                    if 'format' in column[1]:
                        if 'date' in column[1]['format']:
                            item = item.strftime("%d/%m/%Y")
                        if 'text_wrap' in column[1]['format']:
                            sheet.write(start_row + stt, col - 1, item, text_wrap)
                            wrote = True
                    if not wrote:
                        sheet.write(start_row + stt, col - 1, item, border_format)
                else:
                    sheet.write(start_row + stt, col - 1, '', border_format)
            else:
                sheet.write(start_row + stt, col - 1, item, border_format)

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

def add_worksheet_new(sheet_name, workbook, TEMPLATE, queryset, sheet_protect=True):  # Them sheet vao workbook
    # LOGO_EXCEL_REPORT_PATH = 'parking/static/image/logo_report.png'
    LOGO_EXCEL_REPORT_PATH = 'parking/static/image/' + get_setting('logo_report',
                                                                   u'Mẫu logo dùng trong báo cáo',
                                                                   'logo_report.png')
    LOGO_EXCEL_REPORT_PATH_RIGHT = 'parking/static/image/' + get_setting('logo_repor',
                                                                         u'Mẫu logo dùng trong báo cáo',
                                                                         'logo_report_right.png')
    TITLE = TEMPLATE['TITLE']
    HEADER = TEMPLATE['HEADER']
    STAT = TEMPLATE['STAT']

    sheet = workbook.add_worksheet(sheet_name)
    sheet.insert_image(0, 0, LOGO_EXCEL_REPORT_PATH)
    sheet.insert_image(0, 8, LOGO_EXCEL_REPORT_PATH_RIGHT)
    bold = workbook.add_format({'bold': True})
    border = workbook.add_format()
    border.set_border()
    bold_boder = workbook.add_format({'bold': True, 'border': 1})
    text_wrap = workbook.add_format({'border': 1})
    text_wrap.set_text_wrap()

    for r in TITLE:  # Viet tieu de: (dong r[0], cot r[1])
        sheet.write(r[0], r[1], r[2], bold)
        if len(r) == 4:
            sheet.write(r[0], r[1] + 1, r[3])

    for r in STAT:  # Viet dong thong ke theo loai xe: (dong r[0], cot r[1])
        sheet.write(r[0], r[1], r[2], bold)
        sum_r = queryset.filter(vehicle_type_id=r[3]).count()
        sheet.write(r[0], r[1] + 1, sum_r)

    for i, column in enumerate(HEADER):  # Viet table header (dong 5)
        sheet.write(5, i - 1, HEADER[i][0], bold_boder)
        sheet.set_column(i - 1, i - 1, 15)  # Default width
        if len(column) > 1 and 'format' in column[1]:
            if 'width' in column[1]['format']:
                width = column[1]['format']['width']
                sheet.set_column(i - 1, i - 1, width)

    # Write data
    fields = [f[1]['db_field'] for f in HEADER if len(f) > 1 and 'db_field' in f[1]]

    values = (queryset.values(*fields))  # Dict values list

    start_row = 6

    border_format = workbook.add_format({'border': 1})
    date_format = workbook.add_format({'border': 1, 'num_format': 'dd/mm/yyyy'})

    for stt, raw_row_data in enumerate(values):  # Prepare data
        # temp_row = []
        for col, column in enumerate(HEADER):
            item = ''
            # column_order = column[1]
            if column[0] == u'STT':
                item = stt + 1
                sheet.write(start_row + stt, col - 1, item, border_format)

            elif len(column) > 1 and 'db_field' in column[1]:
                db_field = column[1]['db_field']
                item = raw_row_data[db_field]

                if item is not None:
                    if 'custom_function' in column[1]:
                        item = column[1]['custom_function'](item)
                    # Check format
                    wrote = False
                    if 'format' in column[1]:
                        if 'date' in column[1]['format']:
                            item = item.strftime("%d/%m/%Y")
                        if 'text_wrap' in column[1]['format']:
                            sheet.write(start_row + stt, col - 1, item, text_wrap)
                            wrote = True
                    if not wrote:
                        sheet.write(start_row + stt, col - 1, item, border_format)
                else:
                    sheet.write(start_row + stt, col - 1, '', border_format)
            else:
                sheet.write(start_row + stt, col - 1, item, border_format)

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

def getUserName(id):
    user = UserProfile.objects.filter(user__id=id)
    if user and len(user)>0:
        return user[0].fullname
    return  ''
def get_messaging_info(customer_id):
    customer = Customer.objects.filter(id=customer_id)
    if customer:
        customer = customer[0]
        info = []
        s = u"{0}{1}"
        if customer.messaging_via_sms:
            info.append(s.format(u"Qua SMS: ", customer.messaging_sms_phone))
        if customer.messaging_via_phone:
            info.append(s.format(u"Gọi điện: ", customer.messaging_phone))
        if customer.messaging_via_email:
            info.append(s.format(u"Email: ", customer.messaging_email))
        if customer.messaging_via_apart_mail:
            info.append(s.format(u"Thư căn hộ: ", customer.messaging_address))
        if customer.messaging_via_wiper_mail:
            info.append(s.format(u"Thư gắn gạt nước xe", ""))
        return u"; ".join(info)
    return ''


def get_total_status(vehicle_id):
    try:
        vehicle = VehicleRegistration.objects.get(id=vehicle_id)
        today = date.today()

        if vehicle.cancel_date:
            return u'Hủy'
        if vehicle.pause_date:
            return u'Tạm ngừng'
        if not vehicle.expired_date or vehicle.expired_date < today:
            return u'Hết hạn'
        return u'Hoạt động'
    except Exception as e:
        return u''


def get_order_info(customer_id):
    customer = Customer.objects.filter(id=customer_id)
    if customer:
        customer = customer[0]
        info = []
        s = u"{0}{1}"
        if customer.order_register_name:
            info.append(s.format(u"Tên: ", customer.order_register_name))
        if customer.order_register_address:
            info.append(s.format(u"Địa chỉ: ", customer.order_register_address))
        if customer.order_tax_code:
            info.append(s.format(u"MST: ", customer.order_tax_code))
        return u"; ".join(info)
    return ''

def get_cell_name(col, row): # col is 1 based
    excelCol = str()
    div = col
    while div:
        (div, mod) = divmod(div-1, 26) # will return (x, 0 .. 25)
        excelCol = chr(mod + 65) + excelCol

    return excelCol+str(row)

@login_required(redirect_field_name='', login_url='/admin/')
def render_report_expired_vehicle_registration_collection(
        request):  # Bao cao nhac phi (ve sau chuyen thanh bao cao khach hang)
    # Template Bao cao Nhac phi
    userid = request.user.id
    if notallowreport(userid):
        return render(request, 'admin/nopermission.html')
    TEMPLATE_SHEET_EXPIRED_VEHICLE_REGISTRATION_COLLECTION = {
        'TITLE': [
            [2, 5, u'DANH SÁCH CÁC XE NHẮC PHÍ'],
            [3, 4, u'Ngày lập'],
        ],
        'HEADER': (
            (0,),
            (u'STT', {'format': {'width': 5}}),
            (u'Ngày đăng ký', {'db_field': 'first_renewal_effective_date', 'format': {'date': ''}}),
            (u'Ngày đóng phí', {'db_field': 'last_renewal_date', 'format': {'date': ''}}),
            (u'Ngày hiệu lực', {'db_field': 'last_renewal_effective_date', 'format': {'date': ''}}),
            (u'Hạn hiện tại', {'db_field': 'expired_date', 'format': {'date': ''}}),
            (u'Tòa nhà', {'db_field': 'customer__building__name',}),
            (u'Căn hộ', {'db_field': 'customer__apartment__address',}),
            (u'Tên khách hàng', {'db_field': 'customer__customer_name', 'format': {'width': 20}}),
            (u'Tên công ty', {'db_field': 'customer__company__name',}),
            (u'Loại khách hàng', {'db_field': 'customer__customer_type__name', 'format': {'width': 12}}),
            (u'Tên chủ phương tiện ', {'db_field': 'vehicle_driver_name', 'format': {'width': 30}}),
            (u'Số thẻ ', {'db_field': 'card__card_label'}),
            (u'Loại xe', {'db_field': 'vehicle_type__name'}),
            (u'Số xe', {'db_field': 'vehicle_number', 'format': {'width': 25}}),
            (u'SĐT', {'db_field': 'customer__customer_mobile',}),
            (u'Email', {'db_field': 'customer__customer_email', 'format': {'width': 20}}),
            (u'Hình thức nhắc phí', {'db_field': 'customer_id', 'custom_function': get_messaging_info,
                                     'format': {'width': 20, 'text_wrap': ''}}),
            (u'Ghi chú', {'format': {'width': 20}}),
        ),
        'STAT': [],
    }

    SPECIAL_CRITERIA_DATA = (
        ('collection', u'Nhắc phí'),
        ('expired', u'Hết hạn'),
        ('order', 'Xuất hóa đơn')
    )

    special_criteria_data = list()

    for criterion in SPECIAL_CRITERIA_DATA:
        special_criteria_data.append({'value': criterion[0], 'name': criterion[1]})

    vehicle_type_data = list()

    for type in VEHICLE_TYPE:
        vehicle_type_data.append({"value": str(type[0]), "name": type[1]})

    customer_type_data = list()

    customer_types = CustomerType.objects.all()
    customer_type_data.append({"value": "", "name": "Tất cả"})
    for customer_type in customer_types:
        customer_type_data.append({"value": customer_type.name, "name": customer_type.name})

    vehicle_registration_status_data = list()
    for status in VEHICLE_REGISTRATION_STATUS_CHOICE:
        vehicle_registration_status_data.append({'value': status[0], 'name': status[1], 'checked': "true"})

    num_parking_fee_collection_day_before_expired = get_setting('num_parking_fee_collection_day_before_expired',
                                                                u'Số ngày nhắc phí trước khi hết hạn', 10)

    if 'btn_REPORT' in request.POST:
        is_protected_report = False if 'unprotected_report' in request.POST else True

        current_user = request.user
        if not is_protected_report and not current_user.has_perm('parking.export_unprotected_excel'):
            messages.error(request, u'Tài khoản hiện tại không có quyền này!', fail_silently=True)
            return redirect(reverse('render_report_expired_vehicle_registration_collection'))
        now = datetime.today()

        TEMPLATE_SHEET_EXPIRED_VEHICLE_REGISTRATION_COLLECTION['TITLE'][1].append(
            datetime.today().strftime("%d/%m/%Y %H:%M"))  # Tu
        # TEMPLATE_SHEET_EXPIRED_VEHICLE_REGISTRATION_COLLECTION['TITLE'][2].append(to_time)  # Den

        queryset = VehicleRegistration.objects.all()
        file_name = 'GPMS_BC_KH_Nhacphi.xlsx'
        worksheet_name = 'Nhac phi'

        try:
            if 'special_criteria' in request.POST:
                special_criterion = request.POST['special_criteria']
                num_parking_fee_collection_day_before_expired = request.POST[
                                                                    'num_parking_fee_collection_day_before_expired'] \
                                                                or 10

                today = date.today()

                if special_criterion == 'collection':  # Nhac phi; nhung xe gan het han (mac dinh 10 ngay hoac duoc
                    # set trong Parking Setting)
                    # rs = rs.filter(expired_date__lte=__to_time, expired_date__gte=__from_time)
                    queryset = queryset.filter(
                        expired_date__lte=today + timedelta(days=int(num_parking_fee_collection_day_before_expired)))
                elif special_criterion == 'expired':  # Sap het han/ da het han
                    # rs = rs.filter(Q(expired_date__lte=__to_time, expired_date__gte=__from_time) | Q(status=3))
                    file_name = 'GPMS_BCKH_Hethan.xlsx'
                    worksheet_name = 'Het han'
                    TEMPLATE_SHEET_EXPIRED_VEHICLE_REGISTRATION_COLLECTION['TITLE'][0][2] = u'DANH SÁCH CÁC XE HẾT HẠN'
                    queryset = queryset.filter(Q(expired_date__lte=today) | Q(status=3))
                    # elif special_criterion == 'new_cancel_pause':  # Dang ky moi/huy/tam ngung
                    #     file_name = 'GPMS_BCKH_Dangkymoi_huy_tamngung.xlsx'
                    #     worksheet_name = 'Dang ky moi, huy, tam ngung'
                    #     TEMPLATE_SHEET_EXPIRED_VEHICLE_REGISTRATION_COLLECTION['TITLE'][0][2] = u'DANH SÁCH CÁC XE
                    # ĐĂNG KÝ MỚI/HỦY/TẠM NGỪNG'
                    #     queryset = queryset.filter(Q(registration_date=today) | Q(status=0) | Q(status=2))
        except:
            messages.error(request, u'Không phù hợp!', fail_silently=True)
            return HttpResponseRedirect('')

        building_query = request.POST['building_query'].strip()
        apartment_query = request.POST['apartment_query'].strip()
        company_name = request.POST['company_name'].strip()
        vehicle_number = request.POST['vehicle_number'].strip()
        customer_name = request.POST['customer_name'].strip()

        customer_type = request.POST['customer_type']
        vehicle_type = request.POST['vehicle_type']

        for item in vehicle_type_data:
            if item['value'] == vehicle_type:
                item['selected'] = "true"

        if int(vehicle_type) != 100000000:
            queryset = queryset.filter(vehicle_type_id=vehicle_type)
        if len(vehicle_number) > 0:
            queryset = queryset.filter(vehicle_number__icontains=vehicle_number)
        if len(customer_name) > 0:
            queryset = queryset.filter(customer__customer_name__icontains=customer_name)
        if len(company_name) > 0:
            queryset = queryset.filter(customer__company__name__icontains=company_name)
        if len(building_query) > 0:
            queryset = queryset.filter(Q(customer__building__name__icontains=building_query) | Q(
                customer__building__address__icontains=building_query))
        if len(apartment_query) > 0:
            queryset = queryset.filter(Q(customer__apartment__address__icontains=apartment_query)
                                       | Q(customer__apartment__owner_name__icontains=apartment_query)
                                       | Q(customer__apartment__owner_phone__icontains=apartment_query))

        if len(customer_type) > 0:
            queryset = queryset.filter(customer__customer_type__name=customer_type)

        # Prepare data
        HEADER = TEMPLATE_SHEET_EXPIRED_VEHICLE_REGISTRATION_COLLECTION['HEADER']

        folder_name = 'templates/report'
        file_path = '%s/%s' % (folder_name, file_name)

        if not path.exists(folder_name):
            mkdir(folder_name)
        if path.isfile(file_path):
            remove(file_path)

        workbook = Workbook(file_path, {'constant_memory': True})

        # add_worksheet(worksheet_name, workbook, TEMPLATE_SHEET_EXPIRED_VEHICLE_REGISTRATION_COLLECTION, rs, {},
        # is_protected_report) # D
        add_worksheet_new(worksheet_name, workbook, TEMPLATE_SHEET_EXPIRED_VEHICLE_REGISTRATION_COLLECTION, queryset,
                          is_protected_report)  # D

        workbook.close()

        with open(file_path, 'r') as f:
            response = HttpResponse(f.read(), content_type="application/vnd.ms-excel")
            response['Content-Disposition'] = "attachment; filename=%s" % file_name

            return response

    return render(request, 'admin/rp-expiredvehicleregistrationcollection.html',
                  {'customer_type_data': customer_type_data,
                   'vehicle_type_data': vehicle_type_data,
                   'special_criteria_data': special_criteria_data,
                   'num_parking_fee_collection_day_before_expired': num_parking_fee_collection_day_before_expired,
                   })


def get_pause_resume_request_type(request_type):
    if request_type == 1:
        return 'Phuc hoi'
    else:
        return 'Tam ngung'

##2018May21
@login_required(redirect_field_name='', login_url='/admin/')
def render_report_barier_forced(request):  # Bao cao cuong buc Barier
    userid = request.user.id
    if notallowreport(userid):
        return render(request, 'admin/nopermission.html')
    util = Utilities()
    now = datetime.now()
    from_time = datetime(now.year, now.month, now.day).replace(day=1, hour=0, minute=0,
                                                               second=0)  # TG bat dau: Dau thang hien tai
    to_time = from_time + timedelta(days=31)
    to_time = now.replace(hour=23, minute=59,second=59)
    from_time = from_time.strftime("%d/%m/%Y %H:%M:%S")
    to_time = to_time.strftime("%d/%m/%Y %H:%M:%S")
    username = ''
    lane = ''
    teminal = ''
    status = ''
    action = ''
    host = 'http://' + request.get_host() + ':9191/images/'
    #host = 'http://172.16.0.10:9191/images/'
    if 'btn_REPORT' in request.POST:
        from_time = request.POST['from_time']
        to_time = request.POST['to_time']
        status = request.POST['lane']
        action=request.POST['action']
        teminal = request.POST['teminal']
        try:
            from_time = datetime.strptime(from_time, '%d/%m/%Y %H:%M:%S').strftime('%Y-%m-%d %H:%M:%S')
            to_time = datetime.strptime(to_time, '%d/%m/%Y %H:%M:%S').strftime('%Y-%m-%d %H:%M:%S')
        except Exception as e:
            print e.message
            messages.error(request, u'Khoảng thời gian không phù hợp!', fail_silently=True)
            return HttpResponseRedirect('')
        qrres = util.Query("forcedbarierget", status, teminal, action, '', from_time, to_time)
        datares = []
        # for r in qrres:
        #     datares.append({'user': r[1], 'teminal': r[2], 'lane': r[3],
        #                     'time': r[7].strftime("%d-%m-%Y %H:%M:%S") if r[7] else '', 'front': r[5],
        #                     'back': r[6], })
        folder_name = 'templates/report'
        file_name = 'GPMS_BC_CuongbucBarier.xlsx'
        file_path = '%s/%s' % (folder_name, file_name)
        if not os.path.exists(folder_name):
            os.mkdir(folder_name)
        if os.path.isfile(file_path):
            os.remove(file_path)
        workbook = Workbook(file_path, {'constant_memory': True})
        sheet = workbook.add_worksheet("Barier_Forced_Report")
        LOGO_EXCEL_REPORT_PATH = 'parking/static/image/' + get_setting('logo_report', u'Mẫu logo dùng trong báo cáo',
                                                                       'logo_report.png')
        LOGO_EXCEL_REPORT_PATH_RIGHT = 'parking/static/image/' + get_setting('logo_repor', u'Mẫu logo dùng trong báo cáo',
                                                                       'logo_report_right.png')
        sheet.insert_image(0, 0, LOGO_EXCEL_REPORT_PATH)
        sheet.insert_image(0, 6, LOGO_EXCEL_REPORT_PATH_RIGHT)
        bold = workbook.add_format({'bold': True})
        wrap = workbook.add_format()
        wrap.set_text_wrap()
        border = workbook.add_format()
        border.set_border()
        bold_border = workbook.add_format({'bold': True, 'border': 1})
        link_border = workbook.add_format({'color': 'green','border': 1})
        number_border_format = workbook.add_format({'num_format': '#,###0', 'border': 1})
        number_bold_border_format = workbook.add_format({'num_format': '#,###0', 'border': 1, 'bold': True})
        sheet.set_column(1, 1, 30)
        sheet.set_column(2, 4, 20)
        sheet.set_column(5, 6, 40)
        statusaction=u"Tất cả"
        if action=='OUT':
            statusaction="Check-out"
        elif  action=='IN':
            statusaction="Check-in"
        elif  action==u'ngoại lệ':
            statusaction=u"Check-out ngoại lệ"
        else:
            statusaction = u"Tất cả"
        sheet.write(2, 2, u"Máy trạm: %s" % (("Tất cả" if teminal == '' else teminal)), bold)
        sheet.write(3, 2, u"Nhân viên: %s" % (("Tất cả" if status == '' else status)), bold)
        sheet.write(4, 2, u"Hành động: %s" % (statusaction), bold)
        currData=u"Báo cáo cưỡng bức Barier Từ ngày "+ from_time + u" Đến ngày " + to_time
        sheet.write(5,2,currData,bold)

        currData=[u"STT",u"Địa chỉ máy trạm",u"Làn",u"Ngày cưỡng bức",u"Nhân viên cưỡng bức", u"Ghi chú",u"Hình mặt trước",u"Hình mặt sau"]
        sheet.write_row(6, 0, currData, bold_border)
        i=0
        for r in qrres:
            currData = [i + 1, r[2], r[3], r[7].strftime("%d-%m-%Y %H:%M:%S") if r[7] else '', r[1],r[8], r[5], r[6]]
            sheet.write_row(7+i, 0, currData, border)
            link='=HYPERLINK("%s%s","%s-%s")'%(host,r[5],r[7].strftime("%d-%m-%Y %H:%M:%S") if r[7] else '',"Front")
            sheet.write_formula(7+i,6,link,link_border)
            link = '=HYPERLINK("%s%s","%s-%s")' % (host, r[6], r[7].strftime("%d-%m-%Y %H:%M:%S") if r[7] else '', "Back")
            sheet.write_formula(7 + i, 7, link,link_border)
            i=i+1
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
        workbook.close()
        with open(file_path, 'r') as f:
            response = HttpResponse(f.read(), content_type="application/vnd.ms-excel")
            response['Content-Disposition'] = "attachment; filename=%s" % file_name
            return response
    if 'btn_FILTER' in request.POST:
        from_time = request.POST['from_time']
        to_time = request.POST['to_time']
        status = request.POST['lane']
        teminal = request.POST['teminal']
        action = request.POST['action']
        try:
            fdate = datetime.strptime(from_time, '%d/%m/%Y %H:%M:%S').strftime('%Y-%m-%d %H:%M:%S')
            tdate = datetime.strptime(to_time, '%d/%m/%Y %H:%M:%S').strftime('%Y-%m-%d %H:%M:%S')
        except Exception as e:
            print e.message
            messages.error(request, u'Khoảng thời gian không phù hợp!', fail_silently=True)
            return HttpResponseRedirect('')

    qrres = util.Query("getPCAddress")
    pcAddress=[]
    statuslane=[]
    for r in qrres:
        pcAddress.append({'name':r[0],'value':r[1]})
    qrres = util.Query("getUser")
    for r in qrres:
        statuslane.append({'name':r[0],'value':r[1]})

    actions=[{'name':u'-Tất cả-','value':u''},{'name':u'Check-in','value':u'IN'},{'name':u'Check-out','value':u'OUT'},{'name':u'Check-out ngoại lệ','value':u'ngoại lệ'}]
    fdate=datetime.strptime(from_time,'%d/%m/%Y %H:%M:%S').strftime('%Y-%m-%d %H:%M:%S')
    tdate=datetime.strptime(to_time,'%d/%m/%Y %H:%M:%S').strftime('%Y-%m-%d %H:%M:%S')
    qrres = util.Query("forcedbarierget",status,teminal,action,'',fdate,tdate)
    datares=[]
    for r in qrres:
        datares.append({'user': r[1], 'teminal': r[2], 'lane': r[3],
                        'time':  r[7].strftime("%d-%m-%Y %H:%M:%S") if r[7] else '' , 'front': r[5],
                        'back': r[6],'note':r[8]})
    #host='http://'+request.get_host()+'/images/'
    return render(request, 'admin/rp-barierforced.html', {'pcAddress': pcAddress, 'statuslane': statuslane,
                                                          'from_time': from_time,
                                                          'to_time': to_time,
                                                          'username':username,'lane':lane,'teminal':teminal,'status':status,
                                                          'datadetail':datares,'hostname':host, 'actions':actions,'action':action
                                                        })
@login_required(redirect_field_name='', login_url='/admin/')
def render_report_blacklist(request):  # Bao cao cuong buc Barier
    userid = request.user.id
    if notallowreport(userid):
        return render(request, 'admin/nopermission.html')
    util = Utilities()
    now = datetime.now()
    from_time = datetime(now.year, now.month, now.day).replace(day=1, hour=0, minute=0,
                                                               second=0)  # TG bat dau: Dau thang hien tai
    to_time = from_time + timedelta(days=31)
    to_time = now.replace(hour=23, minute=59,second=59)
    from_time = from_time.strftime("%d/%m/%Y %H:%M:%S")
    to_time = to_time.strftime("%d/%m/%Y %H:%M:%S")
    host = 'http://' + request.get_host() + ':9191/images/'
    #host = 'http://172.16.0.10:9191/images/'
    if 'btn_REPORT' in request.POST:
        from_time = request.POST['from_time']
        to_time = request.POST['to_time']
        try:
            from_time = datetime.strptime(from_time, '%d/%m/%Y %H:%M:%S').strftime('%Y-%m-%d %H:%M:%S')
            to_time = datetime.strptime(to_time, '%d/%m/%Y %H:%M:%S').strftime('%Y-%m-%d %H:%M:%S')
        except Exception as e:
            print e.message
            messages.error(request, u'Khoảng thời gian không phù hợp!', fail_silently=True)
            return HttpResponseRedirect('')
        qrres = util.Query("getblacklistreport", from_time, to_time)
        folder_name = 'templates/report'
        file_name = 'GPMS_BC_Blacklist.xlsx'
        file_path = '%s/%s' % (folder_name, file_name)
        if not os.path.exists(folder_name):
            os.mkdir(folder_name)
        if os.path.isfile(file_path):
            os.remove(file_path)
        workbook = Workbook(file_path, {'constant_memory': True})
        sheet = workbook.add_worksheet("Blacklist_Report")
        LOGO_EXCEL_REPORT_PATH = 'parking/static/image/' + get_setting('logo_report', u'Mẫu logo dùng trong báo cáo',
                                                                       'logo_report.png')
        LOGO_EXCEL_REPORT_PATH_RIGHT = 'parking/static/image/' + get_setting('logo_repor', u'Mẫu logo dùng trong báo cáo',
                                                                       'logo_report_right.png')
        sheet.insert_image(0, 0, LOGO_EXCEL_REPORT_PATH)
        sheet.insert_image(0, 6, LOGO_EXCEL_REPORT_PATH_RIGHT)
        bold = workbook.add_format({'bold': True})
        wrap = workbook.add_format()
        wrap.set_text_wrap()
        border = workbook.add_format()
        border.set_border()
        bold_border = workbook.add_format({'bold': True, 'border': 1})
        link_border = workbook.add_format({'color': 'green','border': 1})
        number_border_format = workbook.add_format({'num_format': '#,###0', 'border': 1})
        number_bold_border_format = workbook.add_format({'num_format': '#,###0', 'border': 1, 'bold': True})

        sheet.set_column(2, 4, 20)
        sheet.set_column(5, 6, 40)
        sheet.set_column(7, 9, 30)
        sheet.write(3, 2, u"Báo cáo biển số đen", bold)
        currData=u"Từ ngày "+ from_time + u" Đến ngày " + to_time
        sheet.write(4,2,currData)

        currData=[u"STT",u"Trạng thái",u"Ngày ghi nhận",u"Loại xe",u"Biển số", u"Nội dung",u"Máy trạm",u"Nhân viên",u"Hình mặt trước",u"Hình mặt sau"]
        sheet.write_row(5, 0, currData, bold_border)
        i=0
        for r in qrres:
            currData = [i + 1, r[10], r[1], r[3], r[2], r[7], r[4],r[5]]
            sheet.write_row(6+i, 0, currData, border)
            jsdata = json.loads(r[9])
            front=jsdata["front"]
            back=jsdata["back"]
            link='=HYPERLINK("%s%s","%s-%s")'%(host,front,r[1],"Front")
            sheet.write_formula(6+i,8,link,link_border)
            link = '=HYPERLINK("%s%s","%s-%s")' % (host, back, r[1], "Back")
            sheet.write_formula(6 + i, 9, link,link_border)
            i=i+1
        # sheet.protect('ndhoang', options={
        #     'format_cells': True,
        #     'format_columns': True,
        #     'format_rows': True,
        #     'select_locked_cells': True,
        #     'sort': True,
        #     'autofilter': True,
        #     'pivot_tables': True,
        #     'select_unlocked_cells': True,
        # })
        workbook.close()
        with open(file_path, 'r') as f:
            response = HttpResponse(f.read(), content_type="application/vnd.ms-excel")
            response['Content-Disposition'] = "attachment; filename=%s" % file_name
            return response
    if 'btn_FILTER' in request.POST:
        from_time = request.POST['from_time']
        to_time = request.POST['to_time']
        try:
            fdate = datetime.strptime(from_time, '%d/%m/%Y %H:%M:%S').strftime('%Y-%m-%d %H:%M:%S')
            tdate = datetime.strptime(to_time, '%d/%m/%Y %H:%M:%S').strftime('%Y-%m-%d %H:%M:%S')
        except Exception as e:
            print e.message
            messages.error(request, u'Khoảng thời gian không phù hợp!', fail_silently=True)
            return HttpResponseRedirect('')


    fdate=datetime.strptime(from_time,'%d/%m/%Y %H:%M:%S').strftime('%Y-%m-%d %H:%M:%S')
    tdate=datetime.strptime(to_time,'%d/%m/%Y %H:%M:%S').strftime('%Y-%m-%d %H:%M:%S')
    qrres = util.Query("getblacklistreport",fdate,tdate)
    datares=[]
    for r in qrres:
        jsdata = json.loads(r[9])
        front = jsdata["front"]
        back = jsdata["back"]
        datares.append(
            {'user': r[5], 'teminal': r[4], 'vehiclenumber': r[2], "vehicletype": r[3], "content": r[7], "state": r[10],
             'time': r[1], 'front': front, 'back': back, })
    #host='http://'+request.get_host()+'/images/'
    return render(request, 'admin/rp-blacklist.html', {
                                                          'from_time': from_time,
                                                          'to_time': to_time,
                                                          'datadetail':datares,'hostname':host
                                                        })
##2018May21
@login_required(redirect_field_name='', login_url='/admin/')
def render_report_vehicle_registration_status(request):  # Bao cao tinh trang xe
    userid = request.user.id
    if notallowreport(userid):
        return render(request, 'admin/nopermission.html')
    TEMPLATE_SHEET_EXPIRED_VEHICLE_REGISTRATION_COLLECTION = {
        'TITLE': [
            [2, 5, u'DANH SÁCH CÁC XE NHẮC PHÍ'],
            [3, 4, u'Ngày lập'],
        ],
        'HEADER': (
            (0,),
            (u'STT', {'format': {'width': 5}}),
            (u'Ngày đăng ký', {'db_field': 'first_renewal_effective_date', 'format': {'date': ''}}),
            (u'Ngày đóng phí', {'db_field': 'last_renewal_date', 'format': {'date': ''}}),
            (u'Ngày hiệu lực', {'db_field': 'last_renewal_effective_date', 'format': {'date': ''}}),
            (u'Hạn hiện tại', {'db_field': 'expired_date', 'format': {'date': ''}}),
            (u'Tòa nhà', {'db_field': 'customer__building__name',}),
            (u'Căn hộ', {'db_field': 'customer__apartment__address',}),
            (u'Tên khách hàng', {'db_field': 'customer__customer_name', 'format': {'width': 20}}),
            (u'Tên công ty', {'db_field': 'customer__company__name',}),
            (u'Loại khách hàng', {'db_field': 'customer__customer_type__name', 'format': {'width': 12}}),
            (u'Tên chủ phương tiện ', {'db_field': 'vehicle_driver_name', 'format': {'width': 30}}),
            (u'Số thẻ ', {'db_field': 'card__card_label'}),
            (u'Loại xe', {'db_field': 'vehicle_type__name'}),
            (u'Số xe', {'db_field': 'vehicle_number', 'format': {'width': 25}}),
            (u'SĐT', {'db_field': 'customer__customer_mobile',}),
            (u'Email', {'db_field': 'customer__customer_email', 'format': {'width': 20}}),
            (u'Hình thức nhắc phí', {'db_field': 'customer_id', 'custom_function': get_messaging_info,
                                     'format': {'width': 20, 'text_wrap': ''}}),
            (u'Ghi chú', {'format': {'width': 20}}),
        ),
        'STAT': [],
    }

    TEMPLATE_SHEET_ACTIVE = {
        'HEADER': (
            (0,),
            (u'STT',),
            (u'Ngày đăng ký', {'db_field': 'first_renewal_effective_date', 'format': {'date': ''}}),
            (u'Ngày đóng phí', {'db_field': 'last_renewal_date', 'format': {'date': ''}}),
            (u'Hạn hiện tại', {'db_field': 'expired_date', 'format': {'date': ''}}),
            (u'Tòa nhà', {'db_field': 'customer__building__name'}),
            (u'Căn hộ', {'db_field': 'customer__apartment__address'}),
            (u'Tên khách hàng', {'db_field': 'customer__customer_name', 'format': {'width': 30}}),
            (u'Tên công ty', {'db_field': 'customer__company__name'}),
            (u'Loại khách hàng', {'db_field': 'customer__customer_type__name'}),
            (u'Tên chủ phương tiện ', {'db_field': 'vehicle_driver_name', 'format': {'width': 30}}),
            (u'Số thẻ ', {'db_field': 'card__card_label'}),
            (u'Loại xe', {'db_field': 'vehicle_type__name'}),
            (u'Số xe', {'db_field': 'vehicle_number'}),
            (u'Màu xe', {'db_field': 'vehicle_paint'}),
            (u'Hiệu xe', {'db_field': 'vehicle_brand'}),
            (u'SĐT', {'db_field': 'customer__customer_mobile'}),
            (u'Email', {'db_field': 'customer__customer_email', 'format': {'width': 30}}),
            (u'Thông tin xuất hóa đơn',
             {'db_field': 'customer_id', 'custom_function': get_order_info, 'format': {'width': 30}, 'text_wrap': ''}),
            (u'Hình thức nhắc phí', {'db_field': 'customer_id', 'custom_function': get_messaging_info,
                                     'format': {'width': 30, 'text_wrap': ''}}),
            (u'Ghi chú',),
        ),
        'TITLE': (
            (2, 5, u'DANH SÁCH CÁC XE ĐANG HOẠT ĐỘNG'),
            [3, 5, u'Ngày lập'],
            # [3, 8, u'Đến'],
            (4, 5, u'Tổng số xe'),
        ),
        'STAT': [(4, 6 + i * 2, type[1], type[0]) for i, type in enumerate(VEHICLE_TYPE)]
        # Dong 4, cot 6 + i*2, ten loai xe, ma loai xe
    }

    TEMPLATE_SHEET_EXPIRED = {
        'HEADER': (
            (0,),
            (u'STT',),
            (u'Ngày đăng ký', {'db_field': 'first_renewal_effective_date', 'format': {'date': ''}}),
            (u'Ngày đóng phí', {'db_field': 'last_renewal_date', 'format': {'date': ''}}),
            (u'Hạn hiện tại', {'db_field': 'expired_date', 'format': {'date': ''}}),
            (u'Tòa nhà', {'db_field': 'customer__building__name'}),
            (u'Căn hộ', {'db_field': 'customer__apartment__address'}),
            (u'Tên khách hàng', {'db_field': 'customer__customer_name', 'format': {'width': 30}}),
            (u'Tên công ty', {'db_field': 'customer__company__name'}),
            (u'Loại khách hàng', {'db_field': 'customer__customer_type__name'}),
            (u'Tên chủ phương tiện ', {'db_field': 'vehicle_driver_name', 'format': {'width': 30}}),
            (u'Số thẻ ', {'db_field': 'card__card_label'}),
            (u'Loại xe', {'db_field': 'vehicle_type__name'}),
            (u'Số xe', {'db_field': 'vehicle_number'}),
            (u'Màu xe', {'db_field': 'vehicle_paint'}),
            (u'Hiệu xe', {'db_field': 'vehicle_brand'}),
            (u'SĐT', {'db_field': 'customer__customer_mobile'}),
            (u'Email', {'db_field': 'customer__customer_email', 'format': {'width': 30}}),
            (u'Thông tin xuất hóa đơn',
             {'db_field': 'customer_id', 'custom_function': get_order_info, 'format': {'width': 30}, 'text_wrap': ''}),
            (u'Hình thức nhắc phí', {'db_field': 'customer_id', 'custom_function': get_messaging_info,
                                     'format': {'width': 30, 'text_wrap': ''}}),
            (u'Ghi chú',),
        ),
        'TITLE': (
            (2, 5, u'DANH SÁCH CÁC XE ĐANG NỢ PHÍ'),
            [3, 5, u'Ngày lập'],
            # [3, 8, u'Đến'],
            (4, 5, u'Tổng số xe'),
        ),
        'STAT': [(4, 6 + i * 2, type[1], type[0]) for i, type in enumerate(VEHICLE_TYPE)]
    }

    TEMPLATE_SHEET_PAUSE = {
        'HEADER': (
            (0,),
            (u'STT',),
            (u'Ngày đăng ký',
             {'db_field': 'vehicle_registration__first_renewal_effective_date', 'format': {'date': ''}}),
            (u'Ngày đóng phí', {'db_field': 'vehicle_registration__last_renewal_date', 'format': {'date': ''}}),
            (u'Hạn hiện tại', {'db_field': 'vehicle_registration__expired_date', 'format': {'date': ''}}),
            # (u'Ngày tạm ngừng', {'db_field': 'pause_date', 'format': {'date': ''}}),
            (u'Tòa nhà', {'db_field': 'vehicle_registration__customer__building__name'}),
            (u'Căn hộ', {'db_field': 'vehicle_registration__customer__apartment__address'}),
            (u'Tên khách hàng', {'db_field': 'vehicle_registration__customer__customer_name', 'format': {'width': 30}}),
            (u'Tên công ty', {'db_field': 'vehicle_registration__customer__company__name'}),
            (u'Loại khách hàng', {'db_field': 'vehicle_registration__customer__customer_type__name'}),
            (u'Tên chủ phương tiện ', {'db_field': 'vehicle_driver_name', 'format': {'width': 30}}),
            (u'Số thẻ ', {'db_field': 'vehicle_registration__card__card_label'}),
            (u'Loại xe', {'db_field': 'vehicle_registration__vehicle_type__name'}),

            (u'Số xe', {'db_field': 'vehicle_registration__vehicle_number'}),
            (u'Màu xe', {'db_field': 'vehicle_registration__vehicle_paint'}),
            (u'Hiệu xe', {'db_field': 'vehicle_registration__vehicle_brand'}),
            (u'Ngày yêu cầu', {'db_field': 'request_date', 'format': {'date': ''}}),
            (u'Ngày hiệu lực trở lại', {'db_field': 'start_date', 'format': {'date': ''}}),
            (u'Loại yêu cầu', {'db_field': 'request_type', 'custom_function': get_pause_resume_request_type}),
            (u'Hạn lúc tạm ngưng', {'db_field': 'expired_date', 'format': {'date': ''}}),
            (u'Thời gian còn lại', {'db_field': 'remain_duration'}),
            (u'SĐT', {'db_field': 'vehicle_registration__customer__customer_mobile'}),
            (u'Email', {'db_field': 'vehicle_registration__customer__customer_email', 'format': {'width': 30}}),
            (u'Thông tin xuất hóa đơn',
             {'db_field': 'vehicle_registration__customer_id', 'custom_function': get_order_info,
              'format': {'width': 30}, 'text_wrap': ''}),
            (u'Hình thức nhắc phí',
             {'db_field': 'vehicle_registration__customer_id', 'custom_function': get_messaging_info,
              'format': {'width': 30, 'text_wrap': ''}}),
            (u'Ghi chú', {'db_field': 'request_notes'}),
        ),
        'TITLE': (
            (2, 5, u'DANH SÁCH CÁC XE ĐANG TẠM NGỪNG'),
            [3, 5, u'Ngày lập'],
            # [3, 8, u'Đến'],
            (4, 5, u'Tổng số xe'),
        ),
        'STAT': []  # [(4, 6 + i*2, type[1], type[0]) for i, type in enumerate(VEHICLE_TYPE)]
    }

    TEMPLATE_SHEET_CANCEL = {
        'HEADER': (
            (0,),
            (u'STT',),
            (u'Ngày đăng ký', {'db_field': 'first_renewal_effective_date', 'format': {'date': ''}}),
            (u'Ngày đóng phí', {'db_field': 'last_renewal_date', 'format': {'date': ''}}),
            (u'Hạn hiện tại', {'db_field': 'expired_date', 'format': {'date': ''}}),
            (u'Ngày hủy', {'db_field': 'cancel_date', 'format': {'date': ''}}),
            (u'Tòa nhà', {'db_field': 'customer__building__name'}),
            (u'Căn hộ', {'db_field': 'customer__apartment__address'}),
            (u'Tên khách hàng', {'db_field': 'customer__customer_name', 'format': {'width': 30}}),
            (u'Tên công ty', {'db_field': 'customer__company__name'}),
            (u'Loại khách hàng', {'db_field': 'customer__customer_type__name'}),
            (u'Tên chủ phương tiện ', {'db_field': 'vehicle_driver_name', 'format': {'width': 30}}),
            (u'Số thẻ ', {'db_field': 'card__card_label'}),
            (u'Loại xe', {'db_field': 'vehicle_type__name'}),
            (u'Số xe', {'db_field': 'vehicle_number'}),
            (u'Màu xe', {'db_field': 'vehicle_paint'}),
            (u'Hiệu xe', {'db_field': 'vehicle_brand'}),
            (u'SĐT', {'db_field': 'customer__customer_mobile'}),
            (u'Email', {'db_field': 'customer__customer_email', 'format': {'width': 30}}),
            (u'Thông tin xuất hóa đơn',
             {'db_field': 'customer_id', 'custom_function': get_order_info, 'format': {'width': 30}, 'text_wrap': ''}),
            (u'Hình thức nhắc phí', {'db_field': 'customer_id', 'custom_function': get_messaging_info,
                                     'format': {'width': 30, 'text_wrap': ''}}),
            (u'Ghi chú',),
        ),
        'TITLE': (
            (2, 5, u'DANH SÁCH CÁC XE HỦY ĐẬU XE'),
            [3, 5, u'Ngày lập'],
            # [3, 8, u'Đến'],
            (4, 5, u'Tổng số xe'),
        ),
        'STAT': [(4, 6 + i * 2, type[1], type[0]) for i, type in enumerate(VEHICLE_TYPE)]
    }

    TEMPLATE_SHEET_TOTAL = {
        'HEADER': (
            (0,),
            (u'STT',),
            (u'Ngày đăng ký', {'db_field': 'first_renewal_effective_date', 'format': {'date': ''}}),
            (u'Ngày đóng phí', {'db_field': 'last_renewal_date', 'format': {'date': ''}}),
            (u'Hạn hiện tại', {'db_field': 'expired_date', 'format': {'date': ''}}),
            (u'Ngày tạm ngừng', {'db_field': 'pause_date', 'format': {'date': ''}}),
            (u'Ngày hủy', {'db_field': 'cancel_date', 'format': {'date': ''}}),
            (u'Tình trạng', {'db_field': 'id', 'custom_function': get_total_status,}),
            (u'Tòa nhà', {'db_field': 'customer__building__name'}),
            (u'Căn hộ', {'db_field': 'customer__apartment__address'}),
            (u'Tên khách hàng', {'db_field': 'customer__customer_name', 'format': {'width': 30}}),
            (u'Tên công ty', {'db_field': 'customer__company__name'}),
            (u'Loại khách hàng', {'db_field': 'customer__customer_type__name'}),
            (u'Tên chủ phương tiện ', {'db_field': 'vehicle_driver_name', 'format': {'width': 30}}),
            (u'Số thẻ ', {'db_field': 'card__card_label'}),
            (u'Loại xe', {'db_field': 'vehicle_type__name'}),
            (u'Số xe', {'db_field': 'vehicle_number'}),
            (u'Màu xe', {'db_field': 'vehicle_paint'}),
            (u'Hiệu xe', {'db_field': 'vehicle_brand'}),
            (u'SĐT', {'db_field': 'customer__customer_mobile'}),
            (u'Email', {'db_field': 'customer__customer_email', 'format': {'width': 30}}),
            (u'Thông tin xuất hóa đơn',
             {'db_field': 'customer_id', 'custom_function': get_order_info, 'format': {'width': 30}, 'text_wrap': ''}),
            (u'Hình thức nhắc phí', {'db_field': 'customer_id', 'custom_function': get_messaging_info,
                                     'format': {'width': 30, 'text_wrap': ''}}),
            (u'Ghi chú',),
        ),
        'TITLE': (
            (2, 5, u'DANH SÁCH ĐẬU XE'),
            [3, 5, u'Ngày lập'],
            # [3, 8, u'Đến'],
            (4, 5, u'Tổng số xe'),
        ),
        'STAT': [(4, 6 + i * 2, type[1], type[0]) for i, type in enumerate(VEHICLE_TYPE)]
    }

    def thread_add_worksheet_data(workbook, rs, request_post, is_protected_report):
        today = date.today()
        bool(rs)
        if 'status_active' in request_post:  # Han hien tai > ngay hom nay
            TEMPLATE_SHEET_ACTIVE['TITLE'][1].append(today.strftime("%d/%m/%Y"))
            # TEMPLATE_SHEET_ACTIVE['TITLE'][2].append(to_time)  # Den
            # rs_active = rs.filter(  # status=1,
            #                         expired_date__gte=today)

            add_worksheet_new('Hoat dong', workbook, TEMPLATE_SHEET_ACTIVE,
                              rs.filter(status__in=[1, 3], expired_date__gte=today), is_protected_report)

        if 'status_expired' in request_post:
            TEMPLATE_SHEET_EXPIRED['TITLE'][1].append(today.strftime("%d/%m/%Y"))
            # rs_expired = rs.filter(  # status=3,
            #                          expired_date__lte=today)
            add_worksheet_new('No phi', workbook, TEMPLATE_SHEET_EXPIRED, rs.filter(expired_date__lte=today),
                              is_protected_report)

        if 'status_cancel' in request_post:
            TEMPLATE_SHEET_CANCEL['TITLE'][1].append(today.strftime("%d/%m/%Y"))
            # rs_cancel = rs.filter(status=0)
            add_worksheet_new('Huy dau xe', workbook, TEMPLATE_SHEET_CANCEL, rs.filter(status=0), is_protected_report)

        if 'status_pause' in request_post:
            TEMPLATE_SHEET_PAUSE['TITLE'][1].append(today.strftime("%d/%m/%Y"))
            # rs_pause = rs.filter(status=2)
            # new_data = PauseResumeHistory.objects.all().order_by('vehicle_registration')
            add_worksheet_new('Tam ngung', workbook, TEMPLATE_SHEET_PAUSE,
                              PauseResumeHistory.objects.all().order_by('vehicle_registration'), is_protected_report)

        if 'status_total' in request_post:
            TEMPLATE_SHEET_TOTAL['TITLE'][1].append(today.strftime("%d/%m/%Y"))
            add_worksheet_new('Tinh trang tong', workbook, TEMPLATE_SHEET_TOTAL, rs, is_protected_report)

        num_parking_fee_collection_day_before_expired = request_post[
                                                            'num_parking_fee_collection_day_before_expired'] or 10
        today = date.today()
        TEMPLATE_SHEET_EXPIRED_VEHICLE_REGISTRATION_COLLECTION['TITLE'][1].append(today.strftime("%d/%m/%Y"))

        if 'criterion_collection' in request_post:  # Nhac phi; nhung xe gan het han (mac dinh 10 ngay hoac duoc set
            # trong Parking Setting)
            rs2 = rs.filter(
                expired_date__lte=today + timedelta(days=int(num_parking_fee_collection_day_before_expired)))
            add_worksheet_new('Nhac phi', workbook, TEMPLATE_SHEET_EXPIRED_VEHICLE_REGISTRATION_COLLECTION, rs2,
                              is_protected_report)
        if 'criterion_expired' in request_post:  # Sap het han/ da het han
            TEMPLATE_SHEET_EXPIRED_VEHICLE_REGISTRATION_COLLECTION['TITLE'][0][2] = u'DANH SÁCH CÁC XE HẾT HẠN'
            rs3 = rs.filter(Q(expired_date__lte=today) | Q(status=3))
            add_worksheet_new('Het han', workbook, TEMPLATE_SHEET_EXPIRED_VEHICLE_REGISTRATION_COLLECTION, rs3,
                              is_protected_report)

    vehicle_type_data = list()

    for type in VEHICLE_TYPE:
        if type[0] == 100000000:
            vehicle_type_data.append({"value": str(type[0]), "name": type[1], "selected": "true"})
        else:
            vehicle_type_data.append({"value": str(type[0]), "name": type[1]})

    customer_type_data = list()
    customer_types = CustomerType.objects.all()
    customer_type_data.append({"value": "", "name": "Tất cả"})
    for customer_type in customer_types:
        customer_type_data.append({"value": customer_type.name, "name": customer_type.name})

    vehicle_registration_status_data = list()
    for status in VEHICLE_REGISTRATION_STATUS_CHOICE:
        vehicle_registration_status_data.append({'value': status[0], 'name': status[1], 'checked': "true"})
    vehicle_registration_status_data.append({'value': 'status_total', 'name': 'Báo cáo tổng', 'checked': "true"})

    num_parking_fee_collection_day_before_expired = get_setting('num_parking_fee_collection_day_before_expired',
                                                                u'Số ngày nhắc phí trước khi hết hạn', 10)

    SPECIAL_CRITERIA_DATA = (
        ('criterion_collection', u'Nhắc phí'),
        ('criterion_expired', u'Hết hạn'),
    )

    special_criteria_data = list()

    for criterion in SPECIAL_CRITERIA_DATA:
        special_criteria_data.append({'value': criterion[0], 'name': criterion[1]})

    if 'btn_REPORT' in request.POST:
        is_protected_report = False if 'unprotected_report' in request.POST else True

        current_user = request.user
        if not is_protected_report and not current_user.has_perm('parking.export_unprotected_excel'):
            messages.error(request, u'Tài khoản hiện tại không có quyền này!', fail_silently=True)
            return redirect(reverse('render_report_vehicle_registration_status'))

        building_query = request.POST['building_query'].strip()
        apartment_query = request.POST['apartment_query'].strip()
        company_name = request.POST['company_name'].strip()
        vehicle_number = request.POST['vehicle_number'].strip()
        customer_name = request.POST['customer_name'].strip()

        customer_type = request.POST['customer_type']
        vehicle_type = request.POST['vehicle_type']

        for item in vehicle_type_data:
            if item['value'] == vehicle_type:
                item['selected'] = "true"

        for status in vehicle_registration_status_data:
            if status['value'] in request.POST:
                status['checked'] = 'true'
            else:
                status['checked'] = 'false'

        rs = VehicleRegistration.objects.all().select_related('customer__customer_name',
                                                              'customer__aparment',
                                                              'customer__building',
                                                              'customer__company',
                                                              'customer__order_register_name',
                                                              'customer__order_register_address',
                                                              'vehicle_registration__card__card_label'
                                                              'customer__order_tax_code',
                                                              'vehicle_type__name',
                                                              )

        if int(vehicle_type) != 100000000:
            rs = rs.filter(vehicle_type_id=vehicle_type)
        if len(vehicle_number) > 0:
            rs = rs.filter(vehicle_number__icontains=vehicle_number)
        if len(customer_name) > 0:
            rs = rs.filter(customer__customer_name__icontains=customer_name)
        if len(company_name) > 0:
            rs = rs.filter(customer__company__name__icontains=company_name)
        if len(building_query) > 0:
            rs = rs.filter(Q(customer__building__name__icontains=building_query) | Q(
                customer__building__address__icontains=building_query))
        if len(apartment_query) > 0:
            rs = rs.filter(Q(customer__apartment__address__icontains=apartment_query)
                           | Q(customer__apartment__owner_name__icontains=apartment_query)
                           | Q(customer__apartment__owner_phone__icontains=apartment_query))
        if customer_type:
            rs = rs.filter(customer__customer_type__name=customer_type)

        folder_name = 'templates/report'
        file_name = 'GPMS_BC_TinhTrangXe.xlsx'
        file_path = '%s/%s' % (folder_name, file_name)

        if not os.path.exists(folder_name):
            os.mkdir(folder_name)

        if os.path.isfile(file_path):
            os.remove(file_path)

        workbook = Workbook(file_path, {'constant_memory': True})

        from threading import Thread
        t = Thread(target=thread_add_worksheet_data, args=(workbook, rs, request.POST, is_protected_report))
        t.start()
        t.join(20)

        workbook.close()
        with open(file_path, 'r') as f:
            response = HttpResponse(f.read(), content_type="application/vnd.ms-excel")
            response['Content-Disposition'] = "attachment; filename=%s" % file_name
            return response

    return render(request, 'admin/rp-vehicleregistrationstatus.html', {'customer_type_data': customer_type_data,
                                                                       'vehicle_type_data': vehicle_type_data,
                                                                       'vehicle_registration_status_data':
                                                                           vehicle_registration_status_data,
                                                                       'special_criteria_data': special_criteria_data,
                                                                       'num_parking_fee_collection_day_before_expired': num_parking_fee_collection_day_before_expired,
                                                                       })
## 2018-03-20
@login_required(redirect_field_name='', login_url='/admin/')
def render_report_user_list(request):  # Bao cao danh sach nguoi dung
    userid = request.user.id
    if notallowreport(userid):
        return render(request, 'admin/nopermission.html')
    folder_name = 'templates/report'
    file_name = 'user_list.xlsx'
    file_path = '%s/%s' % (folder_name, file_name)
    if not os.path.exists(folder_name):
        os.mkdir(folder_name)
    if os.path.isfile(file_path):
        os.remove(file_path)
    workbook = Workbook(file_path, {'constant_memory': True})


    sheet = workbook.add_worksheet("Users_List")
    bold = workbook.add_format({'bold': True})
    wrap = workbook.add_format()
    wrap.set_text_wrap()
    border = workbook.add_format()
    border.set_border()
    bold_border = workbook.add_format({'bold': True, 'border': 1})
    number_border_format = workbook.add_format({'num_format': '#,###0', 'border': 1})
    number_bold_border_format = workbook.add_format({'num_format': '#,###0', 'border': 1, 'bold': True})
    LOGO_EXCEL_REPORT_PATH = 'parking/static/image/' + get_setting('logo_report', u'Mẫu logo dùng trong báo cáo',
                                                                   'logo_report.png')
    LOGO_EXCEL_REPORT_PATH_RIGHT = 'parking/static/image/' + get_setting('logo_repor', u'Mẫu logo dùng trong báo cáo',
                                                                         'logo_report_right.png')
    sheet.insert_image(0, 0, LOGO_EXCEL_REPORT_PATH)
    sheet.insert_image(0, 6, LOGO_EXCEL_REPORT_PATH_RIGHT)
    util = Utilities()
    sheet.set_column(1, 4, 20)
    sheet.write(5, 2, u"PARKING USER LIST PHASE 2", bold)
    curdata = ["No.", "User Name", "Full Name", "User Group", "Efective date", "Remark", "Web admin"]
    sheet.write_row(6, 0, curdata, bold_border)
    qr = util.Query("getuserlist")
    i = 0
    for r in qr:
        curdata = [i + 1, r[0], r[1], r[2] if r[2] else u'', r[3].strftime("%d/%m/%Y %H:%M:%S") if r[3] else u'', r[4],
                   r[5]]
        sheet.write_row(7 + i, 0, curdata, border)
        i = i + 1
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
    workbook.close()
    with open(file_path, 'r') as f:
        response = HttpResponse(f.read(), content_type="application/vnd.ms-excel")
        response['Content-Disposition'] = "attachment; filename=%s" % file_name
        return response
    # if 'btn_REPORT' in request.POST:
    #
    # return render(request, 'admin/rp-userlist.html')
## 2018-03-20
def findactiveid(vehicleid,cardid,outdate):
    try:
        util =  Utilities()
        qr = util.QuerySecond('getactiveid', vehicleid, cardid, outdate)
        if len(qr)>0 and qr[0][0]:
            return int(qr[0][0])
        return -1
    except Exception as e:
        return -1
def is_vehicle_registration_available(card_id):
    vehicle_registration = VehicleRegistration.objects.filter(card__card_id=card_id)
    if vehicle_registration:
        vehicle_registration = vehicle_registration[0]
        vehicle_registration_info={"vehicle_registration_info": {
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
            }}
        start_date = vehicle_registration.start_date if vehicle_registration.start_date else None
        expired_date = vehicle_registration.expired_date if vehicle_registration.expired_date else None
        #pause_date = vehicle_registration.pause_date if vehicle_registration.pause_date else None
        cancel_date = vehicle_registration.cancel_date if vehicle_registration.cancel_date else None
        return True,start_date,expired_date,cancel_date
    else:  # Khach vang lai
        return False, None, None, None
def listinsertreport(vehicleid, cardid, indate, outdate):#
    try:
        callname = ""#
        freetime = ""
        totorancetime = ""
        totorancesub = ""
        aid=findactiveid(vehicleid,cardid,outdate)
        if aid>0:
            util =  Utilities()
            qr = util.QuerySecond('getsemplefeebyactive', aid)
            vhcinfo=is_vehicle_registration_available(cardid)
            if qr and len(qr)>0:
                if vhcinfo[0]:
                    cin=datetime.strptime(indate,'%d/%m/%Y %H:%M:%S')
                    chtime=expired_date + timedelta(days=1) if expired_date else ci + timedelta(days=-1)
                    if cancel_date and cancel_date<chtime:
                        chtime=cancel_date
                    if chtime<=cin:
                        if len(qr)>1:
                            callname = str(qr[1][1])
                            freetime = str(qr[1][2]) if qr[1][2] and int(qr[1][2]) > 0 else '0'
                            totorancetime = str(qr[1][3]) if qr[0][3] and int(qr[1][3]) > 0 else '0'
                            totorancesub = 'Yes' if qr[1][9] and int(qr[1][9]) > 0 else 'No'
                        else:
                            callname = str(qr[0][1])
                            freetime = str(qr[0][2]) if qr[0][2] and int(qr[0][2]) > 0 else '0'
                            totorancetime = str(qr[0][3]) if qr[0][3] and int(qr[0][3]) > 0 else '0'
                            totorancesub = 'Yes' if qr[0][9] and int(qr[0][9]) > 0 else 'No'
                    elif chtime>=datetime.strptime(outdate,'%Y-%m-%d'):
                        callname = str(qr[0][1])
                        freetime = str(qr[0][2]) if qr[0][2] and int(qr[0][2]) > 0 else '0'
                        totorancetime = str(qr[0][3]) if qr[0][3] and int(qr[0][3]) > 0 else '0'
                        totorancesub = 'Yes' if qr[0][9] and int(qr[0][9]) > 0 else 'No'
                    else:
                        for q in qr:
                            if callname != "":
                                callname = callname + ", " + str(q[1])
                            else:
                                callname = str(q[1])
                            if freetime != "":
                                freetime = freetime + ", " + (str(q[2]) if q[2] and int(q[2]) > 0 else '0')
                            else:
                                freetime = str(q[2]) if q[2] and int(q[2]) > 0 else '0'
                            if totorancetime != "":
                                totorancetime = totorancetime + ", " + (str(q[3]) if q[3] and int(q[3]) > 0 else '0')
                            else:
                                totorancetime = str(q[3]) if q[3] and int(q[3]) > 0 else '0'
                            if totorancesub != "":
                                totorancesub = totorancesub + (", Yes" if q[9] and int(q[9]) > 0 else ', No')
                            else:
                                totorancesub = 'Yes' if q[9] and int(q[9]) > 0 else 'No'
                else:
                    callname = str(qr[0][1])
                    freetime = str(qr[0][2]) if qr[0][2] and int(qr[0][2]) > 0 else '0'
                    totorancetime = str(qr[0][3]) if qr[0][3] and int(qr[0][3]) > 0 else '0'
                    totorancesub = 'Yes' if qr[0][9] and int(qr[0][9]) > 0 else 'No'
                return callname,totorancetime,freetime,totorancesub
            else:
                return '', '', '', ''
        else:
            return '', '', '', ''
    except Exception as e:
        return '','','',''
@login_required(redirect_field_name='', login_url='/admin/')
def render_report_parking_fee(request):  # Bao cao phi vang lai
    userid = request.user.id
    if notallowreport(userid):
        return render(request, 'admin/nopermission.html')
    def add_worksheet_stat(sheet_name, workbook, TEMPLATE, sheet_protect=True):

        TITLE = TEMPLATE['TITLE']

        sheet = workbook.add_worksheet(sheet_name)
        LOGO_EXCEL_REPORT_PATH = 'parking/static/image/' + get_setting('logo_report', u'Mẫu logo dùng trong báo cáo',
                                                                       'logo_report.png')
        LOGO_EXCEL_REPORT_PATH_RIGHT = 'parking/static/image/' + get_setting('logo_repor',
                                                                             u'Mẫu logo dùng trong báo cáo',
                                                                             'logo_report_right.png')
        sheet.insert_image(0, 0, LOGO_EXCEL_REPORT_PATH)


        bold = workbook.add_format({'bold': True})
        wrap = workbook.add_format()
        wrap.set_text_wrap()
        border = workbook.add_format()
        border.set_border()
        bold_border = workbook.add_format({'bold': True, 'border': 1})
        number_border_format = workbook.add_format({'num_format': '#,###0', 'border': 1})
        number_bold_border_format = workbook.add_format({'num_format': '#,###0', 'border': 1, 'bold': True})

        for r in TITLE:  # Viet tieu de: (dong r[0], cot r[1])
            sheet.write(r[0], r[1], r[2], bold)
            if len(r) == 4:
                sheet.write(r[0], r[1] + 1, r[3])

        for i in xrange(1, len(TEMPLATE['HEADER'])):  # Viet table TEMPLATE[HEADER] (dong 5)
            sheet.write(5, i - 1, TEMPLATE['HEADER'][i][0], bold_border)

        len_header_sub = len(TEMPLATE['HEADER_SUB'])
        sheet.insert_image(0, len_header_sub+5, LOGO_EXCEL_REPORT_PATH_RIGHT)
        i = 1
        # print "@@@ TEMPLATE HEADER SUB", TEMPLATE['HEADER_SUB']

        for i in xrange(0, len_header_sub):
            sheet.write(5 + i + 1, 0, TEMPLATE['HEADER_SUB'][i][0], border)
            sheet.set_column(0, 0, 30)
            len_header_sub_i = len(TEMPLATE['HEADER_SUB'][i])
            # print ">> i ne", i
            for j in xrange(1, len_header_sub_i):
                # print "     j ne", j
                sheet.write(5 + i + 1, j, TEMPLATE['HEADER_SUB'][i][j], number_border_format)

        header_total_starting_row = 7 + i
        for x in xrange(1, len(TEMPLATE['HEADER_TOTAL'])):
            if x == 1:
                sheet.write(header_total_starting_row, x - 1, TEMPLATE['HEADER_TOTAL'][x][0], bold_border)
            else:
                sheet.write_row(header_total_starting_row, x - 1, TEMPLATE['HEADER_TOTAL'][x],
                                number_bold_border_format)

        format_cols = [0]
        for l in xrange(1, len(TEMPLATE['HEADER2'])):  # Viet table TEMPLATE[HEADER] (dong 5)
            sheet.write(7 + i + 2, l - 1, TEMPLATE['HEADER2'][l][0], bold_border)
            if len(TEMPLATE['HEADER2'][l]) > 1:
                format_cols.append(workbook.add_format(TEMPLATE['HEADER2'][l][1]))

        len_header2_sub = len(TEMPLATE['HEADER2_SUB'])

        header_2_sub_starting_row = 7 + i + 3

        for k in xrange(0, len_header2_sub):
            sheet.write(header_2_sub_starting_row + k, 0, TEMPLATE['HEADER2_SUB'][k][0], border)
            len_header2_sub_k = len(TEMPLATE['HEADER2_SUB'][k])

            for j in xrange(1, len_header2_sub_k):
                sheet.write(header_2_sub_starting_row + k, j, TEMPLATE['HEADER2_SUB'][k][j], format_cols[j])
                sheet.set_column(j, j, 15)  # Default width

        # row_tong_cong = len_header_sub + len_header2_sub + 7 + 1
        row_tong_cong = header_2_sub_starting_row + len_header2_sub

        for i in xrange(1, len(TEMPLATE['HEADER3'])):
            if i == 1:
                sheet.write(row_tong_cong, i - 1, TEMPLATE['HEADER3'][i][0], bold_border)
            else:
                sheet.write_row(row_tong_cong, i - 1, TEMPLATE['HEADER3'][i], number_bold_border_format)
        # Chu Ky
        sheet.write(row_tong_cong +2, 0, 'Handed Over by :')
        sheet.write(row_tong_cong + 4, 0, 'Taken Over by :', bold)
        sheet.write(row_tong_cong + 6, 0, 'Date Handed Over :')
        sheet.write(row_tong_cong + 8, 0, 'Checked by :')


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

    def add_worksheet_stat2(sheet_name, workbook, TEMPLATE, data, sheet_protect=True):

        TITLE = TEMPLATE['TITLE']

        sheet = workbook.add_worksheet(sheet_name)
        LOGO_EXCEL_REPORT_PATH = 'parking/static/image/' + get_setting('logo_report', u'Mẫu logo dùng trong báo cáo',
                                                                       'logo_report.png')
        LOGO_EXCEL_REPORT_PATH_RIGHT = 'parking/static/image/' + get_setting('logo_repor',
                                                                             u'Mẫu logo dùng trong báo cáo',
                                                                             'logo_report_right.png')
        sheet.insert_image(0, 0, LOGO_EXCEL_REPORT_PATH)


        bold = workbook.add_format({'bold': True})
        wrap = workbook.add_format()
        wrap.set_text_wrap()
        border = workbook.add_format()
        border.set_border()
        bold_border = workbook.add_format({'bold': True, 'border': 1})

        number_border_format = workbook.add_format({'num_format': '#,###0', 'border': 1})

        for r in TITLE:  # Viet tieu de: (dong r[0], cot r[1])
            sheet.write(r[0], r[1], r[2], bold)
            if len(r) == 4:
                sheet.write(r[0], r[1] + 1, r[3])

        # for i in xrange(0, len(TEMPLATE['VEHICLE_TYPE'])):
        #     col = (i + 1) * 5 - 1 - 2
        #     # sheet.write(4, col , TEMPLATE['VEHICLE_TYPE'][i][0], bold_border)
        #     sheet.merge_range(5, col, 5, col + 5 - 1, TEMPLATE['VEHICLE_TYPE'][i][0], bold_border)

        for i in xrange(1, len(TEMPLATE['HEADER'])):  # Viet table TEMPLATE[HEADER] (dong 5)
            sheet.write(6, i - 1, TEMPLATE['HEADER'][i][0], bold_border)
            sheet.set_column(i - 1, i - 1, 10)

        for j in xrange(0, len(TEMPLATE['DETAIL'])):
            col = 2 + j
            sheet.write(6, 2 + j, TEMPLATE['DETAIL'][j][0], bold_border)
            sheet.set_column(col, col, 20)
        max_column_num = 2 + len(TEMPLATE['DETAIL'])
        sheet.insert_image(0, max_column_num, LOGO_EXCEL_REPORT_PATH_RIGHT)
        for i in xrange(0, len(data)):
            row = 7 + i
            sheet.write_row(row, 0, [''] * max_column_num, border)

            sheet.write(row, 0, data[i][1], border)
            sheet.write(row, 1, data[i][2], border)
            sheet.write(row, 2, data[i][11], border)

            for j in xrange(0, 8):
                if j == 4:
                    sheet.write(row, j + 3, data[i][j + 3], number_border_format)
                else:
                    sheet.write(row, j + 3, data[i][j + 3], border)

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

    # Template Bao cao thu phi dau xe

    number_border_format = {'num_format': '#,###0', 'border': 1}
    TEMPLATE_SHEET_PARKING_FEE = {
        'HEADER': [
            (0,),
            (u'Số lượt xe vào',),
            (u'Số lượt',),
        ],

        'HEADER_SUB': [],

        'HEADER_TOTAL': [
            (0,),
            (u'Tổng số lượt xe vào',),
        ],

        'HEADER2': [
            (0,),
            (u'Số lượt xe ra', number_border_format),
            (u'Số lượt',number_border_format),
            (u'Thành tiền',number_border_format),
            (u'Điều chỉnh',number_border_format),
            (u'Thành tiền phải nộp',number_border_format),
            (u'Ghi chú',),
        ],

        'HEADER2_SUB': [],

        'HEADER3': [
            (0,),
            (u'Tổng số lượt xe ra - doanh thu',),
        ],

        'TITLE': [
            (2, 1, u'BÁO CÁO TỔNG PHÍ ĐẬU XE VÃNG LAI'),
            [3, 1, u'Từ'],
            [3, 3, u'Đến'],
        ],
    }

    vehicle_type_data = list()
    vehicle_types = VehicleType.objects.all()
    for type in vehicle_types:
        vehicle_type_data.append({"value": type.id, "name": type.name})

    now = datetime.now()
    from_time = datetime(now.year, now.month, now.day).replace(day=1)  # TG bat dau: Dau thang hien tai

    month = now.month
    year = now.year + month / 12

    try:
        month = (month + 1) % 12
        if month == 0: month = 12
    except:
        pass

    # to_time = from_time.replace(year=year, month=month, day=1, hour=23) + timedelta(
    #     days=-1)  # TG ket thuc: Cuoi thang hien tai
    # to_time = from_time.replace(hour=23, minute=59, second=59)  # Cuoi ngay
    from_time = now.replace(hour=00, minute=00, second=00)  # Bat dau ngay
    to_time = now.replace(hour=23, minute=59, second=59)  # Cuoi ngay

    if 'btn_REPORT' in request.POST:
        is_protected_report = False if 'unprotected_report' in request.POST else True

        current_user = request.user
        if not is_protected_report and not current_user.has_perm('parking.export_unprotected_excel'):
            messages.error(request, u'Tài khoản hiện tại không có quyền này!', fail_silently=True)
            return redirect(reverse('render_report_parking_fee'))

        from_time = request.POST['from_time']
        to_time = request.POST['to_time']
        TEMPLATE_SHEET_PARKING_FEE['TITLE'][1].append(from_time)  # Tu
        TEMPLATE_SHEET_PARKING_FEE['TITLE'][2].append(to_time)  # Den

        rs1 = ParkingFeeSession.objects.all()  # Xe vao
        rs2 = rs1[:]

        rs3 = ParkingSession.objects.all()  # Checkout ngoai le
        adjustments = FeeAdjustment.objects.all()

        try:
            _from_time = datetime.strptime(from_time, "%d/%m/%Y %H:%M:%S")  # Datetime
            _to_time = datetime.strptime(to_time, "%d/%m/%Y %H:%M:%S")
            rs1 = rs1.filter(session_type='IN', calculation_time__lte=_to_time,
                             calculation_time__gte=_from_time)  # Xe vao
            rs2 = rs2.filter(session_type='OUT', calculation_time__lte=_to_time,
                             calculation_time__gte=_from_time)  # Xe ra
            rs3 = rs3.filter(check_out_time__gte=_from_time, check_out_time__lte=_to_time,
                             check_out_exception_id__isnull=False) # Xe ra ngoai le
            adjustments = adjustments.filter(time__lte=_to_time, time__gte=_from_time)  # phi dieu chinh
        except:
            messages.error(request, u'Khoảng thời gian không phù hợp!', fail_silently=True)
            return HttpResponseRedirect('')

        # tong_doanh_thu_luot_ra = rs2.aggregate(Sum('parking_fee'))['parking_fee__sum'] or 0

        tong_doanh_thu_luot_ra = 0
        tong_luot_ra = 0
        tong_luot_vao = 0
        tong_dieu_chinh_phi  = 0
        for type in vehicle_types:
            vehicle_type_id = type.id
            luot_vao = rs1[:]
            luot_ra = rs2[:]
            luot_ra_ngoai_le = rs3[:]
            if vehicle_type_id == 100000000:
                continue

            luot_vao = luot_vao.filter(vehicle_type_id=vehicle_type_id)
            luot_ra = luot_ra.filter(vehicle_type_id=vehicle_type_id)
            luot_ra_ngoai_le = luot_ra_ngoai_le.filter(vehicle_type=get_storaged_vehicle_type(vehicle_type_id))
            adjustments_by_vehicle = adjustments.filter(vehicle_type=vehicle_type_id)

            # luot_vao_ve_thang = luot_vao.filter(is_vehicle_registration=True)
            # luot_vao_vang_lai = luot_vao.filter(is_vehicle_registration=False)
            # luot_ra_ve_thang = luot_ra.filter(is_vehicle_registration=True)
            # luot_ra_vang_lai = luot_ra.filter(is_vehicle_registration=False)


            # print "@@@@Loai xe", type, type.name, type.id
            #
            # TEMPLATE_SHEET_PARKING_FEE['HEADER_SUB'].append([u'%s tháng' % type.name, luot_vao_ve_thang.count()])
            # TEMPLATE_SHEET_PARKING_FEE['HEADER_SUB'].append([u'%s vãng lai' % type.name, luot_vao_vang_lai.count()])
            #
            tong = (luot_ra.aggregate(Sum('parking_fee'))['parking_fee__sum'] or 0) + \
                    (luot_ra_ngoai_le.aggregate(Sum('check_out_exception__parking_fee'))['check_out_exception__parking_fee__sum'] or 0)
            tong_doanh_thu_luot_ra += tong
            # tong_vang_lai = luot_ra_vang_lai.aggregate(Sum('parking_fee'))[
            #                                                       'parking_fee__sum'] or 0
            #
            # TEMPLATE_SHEET_PARKING_FEE['HEADER2_SUB'].append([u'%s tháng' % type.name, luot_ra_ve_thang.count(),
            # tong_ve_thang])
            # TEMPLATE_SHEET_PARKING_FEE['HEADER2_SUB'].append([u'%s vãng lai' % type.name, luot_ra_vang_lai.count(),
            #  tong_vang_lai])
            #
            # tong_doanh_thu_luot_ra += tong_ve_thang + tong_vang_laiTEMPLATE_SHEET_PARKING_FEE['HEADER_SUB'].append(
            # [u'%s tháng' % type.name, luot_vao_ve_thang.count()])
            adjust_fee_by_vehicle = adjustments_by_vehicle.aggregate(Sum('fee'))['fee__sum'] or 0
            tong_dieu_chinh_phi += adjust_fee_by_vehicle

            remark = ''
            if len(adjustments_by_vehicle) > 0:
                remark = adjustments_by_vehicle[0].remark
            luot_ra_theo_type = luot_ra.count()  + luot_ra_ngoai_le.count()
            luot_vao_theo_type = luot_vao.count()
            tong_luot_ra += luot_ra_theo_type
            tong_luot_vao += luot_vao_theo_type

            TEMPLATE_SHEET_PARKING_FEE['HEADER_SUB'].append([u'%s' % type.name, luot_vao.count()])
            ##old before 2018-01-16
            # TEMPLATE_SHEET_PARKING_FEE['HEADER2_SUB'].append([u'%s ' % type.name, luot_ra_theo_type, tong,adjust_fee_by_vehicle, tong - adjust_fee_by_vehicle,
            #                                                   remark])
            ##old before 2018-01-16
            ##new 2018-01-16
            TEMPLATE_SHEET_PARKING_FEE['HEADER2_SUB'].append(
                [u'%s ' % type.name, luot_ra_theo_type, tong, adjust_fee_by_vehicle, tong + adjust_fee_by_vehicle,
                 remark])
            ##new 2018-01-16
        TEMPLATE_SHEET_PARKING_FEE['HEADER_TOTAL'].append((tong_luot_vao,))
        ##old before 2018-01-16
        #TEMPLATE_SHEET_PARKING_FEE['HEADER3'].append((tong_luot_ra, tong_doanh_thu_luot_ra,tong_dieu_chinh_phi, tong_doanh_thu_luot_ra - tong_dieu_chinh_phi,''))
        ##old before 2018-01-16
        ##new 2018-01-16
        TEMPLATE_SHEET_PARKING_FEE['HEADER3'].append((tong_luot_ra, tong_doanh_thu_luot_ra, tong_dieu_chinh_phi,
                                                      tong_doanh_thu_luot_ra + tong_dieu_chinh_phi, ''))
        # print "@@@@@ DATA ", TEMPLATE_SHEET_PARKING_FEE


        folder_name = 'templates/report'
        file_name = 'GPMS_BC_TongPhiVangLai.xlsx'
        file_path = '%s/%s' % (folder_name, file_name)

        if not os.path.exists(folder_name):
            os.mkdir(folder_name)
        if os.path.isfile(file_path):
            os.remove(file_path)

        workbook = Workbook(file_path, {'constant_memory': True})
        add_worksheet_stat('Tong phi vang lai', workbook, TEMPLATE_SHEET_PARKING_FEE, is_protected_report)  # D
        workbook.close()

        with open(file_path, 'r') as f:
            response = HttpResponse(f.read(), content_type="application/vnd.ms-excel")
            response['Content-Disposition'] = "attachment; filename=%s" % file_name
            return response

    if 'btn_REPORT_DETAIL' in request.POST:
        # TEMPLATE_SHEET_PARKING_FEE_DETAIL = {
        #     'HEADER': [
        #         (0,),
        #         (u'STT',),
        #         (u'Ngày thu',),
        #     ],
        #
        #     'VEHICLE_TYPE': [],
        #
        #     'DETAIL': [
        #         (u'Loại xe', 'vehicle_type'),
        #         (u'Số thẻ', 'card_id'),
        #         (u'Biển số xe', 'vehicle_number'),
        #         (u'Nhân viên cho vào', 'check_in_operator_id__userprofile__fullname'),
        #         (u'Giờ vào',),
        #         (u'Nhân viên cho ra', 'check_out_operator_id__userprofile__fullname'),
        #         (u'Giờ ra',),
        #         (u'Check -Out Ngoại lệ',),
        #         (u'Thành tiền', 'parking_fee'),
        #     ],
        #
        #     'TITLE': [
        #         (2, 5, u'BÁO CÁO CHI TIẾT VÃNG LAI'),
        #         [3, 5, u'Từ'],
        #         [3, 8, u'Đến'],
        #     ],
        # }

        is_protected_report = False if 'unprotected_report' in request.POST else True

        current_user = request.user
        if not is_protected_report and not current_user.has_perm('parking.export_unprotected_excel'):
            messages.error(request, u'Tài khoản hiện tại không có quyền này!', fail_silently=True)
            return redirect(reverse('render_report_parking_fee'))

        from_time = request.POST['from_time']
        to_time = request.POST['to_time']

        # TEMPLATE_SHEET_PARKING_FEE_DETAIL['TITLE'][1].append(from_time)  # Tu
        # TEMPLATE_SHEET_PARKING_FEE_DETAIL['TITLE'][2].append(to_time)  # Den

        # vehicle_type_list = list()
        # vehicle_types = VehicleType.objects.all()
        #
        # for type in vehicle_types:
        #     vehicle_type_id = type.id
        #     if vehicle_type_id == 100000000:
        #         continue
        #
        #     TEMPLATE_SHEET_PARKING_FEE_DETAIL['VEHICLE_TYPE'].append((type.name, {'merge_cell': 5}))
        #     vehicle_type_list.append(type.id)

        _from_time = datetime.strptime(from_time, "%d/%m/%Y %H:%M:%S")  # Datetime
        _to_time = datetime.strptime(to_time, "%d/%m/%Y %H:%M:%S")
        fdate=_from_time.strftime("%Y-%m-%d %H:%M:%S")
        tdate = _to_time.strftime("%Y-%m-%d %H:%M:%S")
        util = Utilities()
        qr = util.Query('getparkingsessionreportdetail',fdate, tdate)
        # data = list()
        #
        # rs1 = ParkingFeeSession.objects \
        #     .filter(payment_date__gte=_from_time, payment_date__lte=_to_time, session_type='OUT') \
        #     .select_related('parking_session').values('id','vehicle_type_id', 'parking_fee',
        #                                               'parking_session__vehicle_number',
        #                                               'payment_date',
        #                                               'parking_session__check_in_operator_id__userprofile__fullname',
        #                                               'parking_session__check_in_time',
        #                                               'parking_session__check_out_operator_id__userprofile__fullname',
        #                                               'parking_session__check_out_time',
        #                                               'parking_session__card__card_label',
        #                                               'parking_session__check_out_exception_id__notes',
        #                                               'vehicle_type__name')
        #
        # rs2 = ParkingSession.objects \
        #         .filter(check_out_time__gte=_from_time, check_out_time__lte=_to_time, check_out_exception_id__isnull=False) \
        #         .select_related('check_out_exception').values('vehicle_type', 'check_out_exception__parking_fee',
        #                                                       'vehicle_number',
        #                                                       'check_in_operator_id__userprofile__fullname',
        #                                                       'check_in_time',
        #                                                       'check_out_operator_id__userprofile__fullname',
        #                                                       'check_out_time',
        #                                                       'check_out_exception_id__notes',
        #                                                       'card__card_label')
        #
        #
        # index = 0
        # for i, r in enumerate(rs1):
        #
        #
        #     index += 1
        #     data.append([
        #         vehicle_type_list.index(r['vehicle_type_id']) + 1,
        #         index,
        #         localtime(r['payment_date']).strftime("%d/%m/%Y") if r['payment_date'] else '',
        #         r['parking_session__card__card_label'],
        #         r['parking_session__vehicle_number'],
        #         r['parking_session__check_in_operator_id__userprofile__fullname'],
        #         localtime(r['parking_session__check_in_time']).strftime("%d/%m/%Y %H:%M:%S") if r[
        #             'parking_session__check_in_time'] else '',
        #         r['parking_session__check_out_operator_id__userprofile__fullname'],
        #         localtime(r['parking_session__check_out_time']).strftime("%d/%m/%Y %H:%M:%S") if r[
        #             'parking_session__check_in_time'] else '',
        #         r['parking_session__check_out_exception_id__notes'],
        #         unicode(r['parking_fee']),
        #         r['vehicle_type__name']
        #     ])
        #
        # for i, r in enumerate(rs2):
        #     index += 1
        #     data.append([
        #         vehicle_type_list.index(VEHICLE_TYPE_FULL_ID_BY_DECODED_ID_DICT[r['vehicle_type']]) + 1,
        #         index,
        #         localtime(r['check_out_time']).strftime("%d/%m/%Y") if r['check_out_time'] else '',
        #         r['card__card_label'],
        #         r['vehicle_number'],
        #         r['check_in_operator_id__userprofile__fullname'],
        #         localtime(r['check_in_time']).strftime("%d/%m/%Y %H:%M:%S") if r[
        #             'check_in_time'] else '',
        #         r['check_out_operator_id__userprofile__fullname'],
        #         localtime(r['check_out_time']).strftime("%d/%m/%Y %H:%M:%S") if r[
        #             'check_in_time'] else '',
        #         r['check_out_exception_id__notes'],
        #         unicode(r['check_out_exception__parking_fee']),
        #         VEHICLE_TYPE_NAME_BY_DECODED_ID[r['vehicle_type']]
        #     ])
        #

        # rs1 = ParkingFeeSession.objects.all()
        # rs2 = rs1.filter(session_type='OUT')
        # rs1 = rs1.filter(session_type='IN')
        #
        # len_rs2 = rs2.count()
        # for i in xrange(len_rs2):
        #     row = [vehicle_type_list.index(rs2[i].vehicle_type_id) + 1, i, rs2[i].payment_date.strftime("%d/%m/%Y"),
        #            rs2[i].card_id, rs2[i].vehicle_number]
        #
        #     session_in = rs1.filter(parking_session_id=rs2[i].parking_session_id)
        #     check_in_time = ''
        #     if session_in:
        #         check_in_time = session_in[0].payment_date.strftime("%d/%m/%Y %H:%M")
        #     row.append(check_in_time)
        #     row.append(rs2[i].payment_date.strftime("%d/%m/%Y %H:%M"))
        #     row.append(unicode(rs2[i].parking_fee))
        #
        #     data.append(row)

        folder_name = 'templates/report'
        file_name = 'GPMS_BC_ChiTietVangLai.xlsx'
        file_path = '%s/%s' % (folder_name, file_name)

        if not os.path.exists(folder_name):
            os.mkdir(folder_name)
        if os.path.isfile(file_path):
            os.remove(file_path)

        workbook = Workbook(file_path, {'constant_memory': True})
        sheet = workbook.add_worksheet('Chi tiet vang lai')
        LOGO_EXCEL_REPORT_PATH = 'parking/static/image/' + get_setting('logo_report', u'Mẫu logo dùng trong báo cáo',
                                                                       'logo_report.png')
        LOGO_EXCEL_REPORT_PATH_RIGHT = 'parking/static/image/' + get_setting('logo_repor',
                                                                             u'Mẫu logo dùng trong báo cáo',
                                                                             'logo_report_right.png')
        sheet.insert_image(0, 0, LOGO_EXCEL_REPORT_PATH)
        sheet.insert_image(0, 14, LOGO_EXCEL_REPORT_PATH_RIGHT)



        bold = workbook.add_format({'bold': True})
        wrap = workbook.add_format()
        wrap.set_text_wrap()
        border = workbook.add_format()
        border.set_border()
        bold_border = workbook.add_format({'bold': True, 'border': 1})

        number_border_format = workbook.add_format({'num_format': '#,###0', 'border': 1})
        sheet.set_column(1, 14, 20)
        sheet.write(3, 4, "BÁO CÁO CHI TIẾT VÃNG LAI", bold_border)
        row_data=[u'Từ:', _from_time.strftime("%d/%m/%Y %H:%M:%S"),u'','đến:',_to_time.strftime("%Y-%m-%d %H:%M:%S")]
        sheet.write_row(4,3,row_data,bold)
        sheet.merge_range(6, 10, 6, 13, u'Công thức', bold_border)
        row_data = [u'Stt', u'Ngày thu', u'Loại xe', u'Số thẻ', u'Biển số xe', u'Nhân viên cho vào',
                    u'Giờ vào', u'Nhân viên cho ra', u'Giờ ra', u"Check-Out ngoại lệ", u'Tên công thức',
                    u'Dung sai (phút)', u'Thời gian miễn phí (phút)', u'Từ đầu', u'Thành tiền']
        sheet.write_row(7, 0, row_data, bold_border)
        # add_worksheet_stat2('Chi tiet vang lai', workbook, TEMPLATE_SHEET_PARKING_FEE_DETAIL, data, is_protected_report)
        startrow=7
        index=1
        for q in qr:
            tmmdt=listinsertreport(q[9], q[10], q[5], q[11])
            row_data = [index, q[0], q[1], q[2], q[3], q[4], q[5], q[6], q[7], q[8],
                        tmmdt[0],tmmdt[1],tmmdt[2],tmmdt[3], q[13]]
            sheet.write_row(startrow+index, 0, row_data, number_border_format)
            index=index+1
        if is_protected_report:
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
        workbook.close()

        with open(file_path, 'r') as f:
            response = HttpResponse(f.read(), content_type="application/vnd.ms-excel")
            response['Content-Disposition'] = "attachment; filename=%s" % file_name
            return response

    return render(request, 'admin/rp-parkingfee.html', {'vehicle_type_data': vehicle_type_data,
                                                        'from_time': from_time.strftime("%d/%m/%Y %H:%M:%S"),
                                                        'to_time': to_time.strftime("%d/%m/%Y %H:%M:%S"),
                                                        })
@login_required(redirect_field_name='', login_url='/admin/')
def render_ParkingVehicleNumberState(request):  # Bao cao phi vang lai
    userid = request.user.id
    if notallowreport(userid):
        return render(request, 'admin/nopermission.html')
    numbertypedata = list()
    numbertypedata.append({"value": -1, "name": u"-Tất cả-"})
    numbertypedata.append({"value": 0, "name": u"Tự động"})
    numbertypedata.append({"value": 1, "name": u"Nhập biển số"})
    now = datetime.now()
    from_time = now.replace(hour=00, minute=00, second=00)  # Bat dau ngay
    to_time = now.replace(hour=23, minute=59, second=59)  # Cuoi ngay
    if 'btn_REPORT' in request.POST:
        is_protected_report = False if 'unprotected_report' in request.POST else True
        current_user = request.user
        if not is_protected_report and not current_user.has_perm('parking.export_unprotected_excel'):
            messages.error(request, u'Tài khoản hiện tại không có quyền này!', fail_silently=True)
            return redirect(reverse('render_ParkingVehicleNumberState'))

        from_time = request.POST['from_time']
        to_time = request.POST['to_time']
        numbertype=int(request.POST['numbertype'])
        _from_time = datetime.strptime(from_time, "%d/%m/%Y %H:%M:%S")  # Datetime
        _to_time = datetime.strptime(to_time, "%d/%m/%Y %H:%M:%S")
        fdate=_from_time.strftime("%Y-%m-%d %H:%M:%S")
        tdate = _to_time.strftime("%Y-%m-%d %H:%M:%S")
        util = Utilities()
        qr = util.Query('getdrivernumberbysession',fdate, tdate,numbertype)

        folder_name = 'templates/report'
        file_name = 'GPMS_BC_TrinhTrangGhiNhanBSX.xlsx'
        file_path = '%s/%s' % (folder_name, file_name)

        if not os.path.exists(folder_name):
            os.mkdir(folder_name)
        if os.path.isfile(file_path):
            os.remove(file_path)

        workbook = Workbook(file_path, {'constant_memory': True})

        sheet = workbook.add_worksheet('Trinh trang bien so xe')
        LOGO_EXCEL_REPORT_PATH = 'parking/static/image/' + get_setting('logo_report', u'Mẫu logo dùng trong báo cáo',
                                                                       'logo_report.png')
        LOGO_EXCEL_REPORT_PATH_RIGHT = 'parking/static/image/' + get_setting('logo_repor',
                                                                             u'Mẫu logo dùng trong báo cáo',
                                                                             'logo_report_right.png')
        sheet.insert_image(0, 0, LOGO_EXCEL_REPORT_PATH)
        sheet.insert_image(0, 8, LOGO_EXCEL_REPORT_PATH_RIGHT)

        bold = workbook.add_format({'bold': True})
        wrap = workbook.add_format()
        wrap.set_text_wrap()
        border = workbook.add_format()
        border.set_border()
        bold_border = workbook.add_format({'bold': True, 'border': 1})

        number_border_format = workbook.add_format({'num_format': '#,###0', 'border': 1})
        sheet.set_column(1, 14, 20)
        sheet.write(3, 4, "BÁO CÁO TRÌNH TRẠNG GHI NHẬN BIỂN SỐ XE", bold)
        row_data=[u'Từ:', _from_time.strftime("%d/%m/%Y %H:%M:%S"),u'','đến:',_to_time.strftime("%Y-%m-%d %H:%M:%S")]
        sheet.write_row(4,3,row_data,bold)
        row_data = [u'Stt', u'Loại xe', u'Số thẻ', u'Biển số xe', u'Nhân viên cho vào',
                    u'Giờ vào', u'Nhân viên cho ra', u'Giờ ra', u'Cách ghi nhận biển số xe']
        sheet.write_row(6, 0, row_data, bold_border)
        # add_worksheet_stat2('Chi tiet vang lai', workbook, TEMPLATE_SHEET_PARKING_FEE_DETAIL, data, is_protected_report)
        startrow=6
        index=1
        for q in qr:
            row_data = [index, q[1], q[0], q[6], q[4], q[2], q[5], q[3], q[7]]
            sheet.write_row(startrow+index, 0, row_data, border)
            index=index+1
        if is_protected_report:
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
        workbook.close()

        with open(file_path, 'r') as f:
            response = HttpResponse(f.read(), content_type="application/vnd.ms-excel")
            response['Content-Disposition'] = "attachment; filename=%s" % file_name
            return response

    return render(request, 'admin/rp-parkingnumberstate.html', {'numbertypedata': numbertypedata,
                                                        'from_time': from_time.strftime("%d/%m/%Y %H:%M:%S"),
                                                        'to_time': to_time.strftime("%d/%m/%Y %H:%M:%S"),
                                                        })
@login_required(redirect_field_name='', login_url='/admin/')
def render_report_parking_intheyard(request):
    userid = request.user.id
    if notallowreport(userid):
        return render(request, 'admin/nopermission.html')
    report_time = datetime.now()
    vehicle_type_data = list()
    card_type_data = list()
    card_type_data.append({"value": -1, "name": u"-Tất cả-"})
    card_types=CardType.objects.all()
    for type in card_types:
        card_type_data.append({"value": type.id, "name": type.name})
    vehicle_type_data.append({"value": 0, "name": u"-Tất cả-"})
    vehicle_types = VehicleType.objects.all()
    for type in vehicle_types:
        vehicle_type_data.append({"value": type.id, "name": type.name})
    if 'btn_REPORT' in request.POST:
        is_protected_report = False if 'unprotected_report' in request.POST else True
        current_user = request.user
        if not is_protected_report and not current_user.has_perm('parking.export_unprotected_excel'):
            messages.error(request, u'Tài khoản hiện tại không có quyền này!', fail_silently=True)
            return redirect(reverse('render_report_parking_intheyard'))
        report_time = request.POST['report_time']
        try:
            _report_time = datetime.strptime(report_time, "%d/%m/%Y %H:%M:%S")  # Datetime
        except:
            messages.error(request, u'Khoảng thời gian không phù hợp!', fail_silently=True)
            return HttpResponseRedirect('')
        rdate = _report_time.strftime("%Y-%m-%d %H:%M:%S")
        vehecletype = request.POST['vehecletype']
        cardLable = request.POST['cardtype']
        filtertype = request.POST['filtertype']
        durationday = request.POST['durationday']
        util = Utilities()
        qr = util.Query('getVehicleInParking', rdate, vehecletype,cardLable,durationday,filtertype)
        folder_name = 'templates/report'
        file_name = 'GPMS_BC_XeTonBai.xlsx'
        file_path = '%s/%s' % (folder_name, file_name)

        if not os.path.exists(folder_name):
            os.mkdir(folder_name)
        if os.path.isfile(file_path):
            os.remove(file_path)

        workbook = Workbook(file_path, {'constant_memory': True})
        LOGO_EXCEL_REPORT_PATH = 'parking/static/image/' + get_setting('logo_report', u'Mẫu logo dùng trong báo cáo',
                                                                       'logo_report.png')
        LOGO_EXCEL_REPORT_PATH_RIGHT = 'parking/static/image/' + get_setting('logo_repor',
                                                                             u'Mẫu logo dùng trong báo cáo',
                                                                             'logo_report_right.png')

        sheet = workbook.add_worksheet('Xe ton bai')
        sheet.insert_image(0, 0, LOGO_EXCEL_REPORT_PATH)
        sheet.insert_image(0,6, LOGO_EXCEL_REPORT_PATH_RIGHT)
        bold = workbook.add_format({'bold': True})
        wrap = workbook.add_format()
        wrap.set_text_wrap()
        border = workbook.add_format()
        border.set_border()
        bold_border = workbook.add_format({'bold': True, 'border': 1})

        number_border_format = workbook.add_format({'num_format': '#,###0', 'border': 1})
        sheet.set_column(1, 14, 20)
        sheet.write(3, 4, "BÁO CÁO XE TỒN BÃI", bold_border)
        row_data = [u'Thời điểm báo cáo:', _report_time.strftime("%d/%m/%Y %H:%M:%S")]
        sheet.write_row(4, 3, row_data, bold)
        row_data = [u'Stt', u'Loại xe',u'Loại thẻ', u'Số thẻ', u'Biển số xe',
                    u'Giờ vào',  u'Giờ ra']
        sheet.write_row(6, 0, row_data, bold_border)
        startrow = 7
        for q in qr:
            row_data = [q[0], q[4],q[3], q[2], q[5], q[7], q[8]]
            sheet.write_row(startrow , 0, row_data, number_border_format)
            startrow = startrow + 1
        if is_protected_report:
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
        workbook.close()

        with open(file_path, 'r') as f:
            response = HttpResponse(f.read(), content_type="application/vnd.ms-excel")
            response['Content-Disposition'] = "attachment; filename=%s" % file_name
            return response
    return render(request, 'admin/rp_parkingintheyard.html', {'vehecletype': vehicle_type_data,'durationday':0,'cardtype':card_type_data,
                                                        'report_time': report_time.strftime("%d/%m/%Y %H:%M:%S")
                                                        })
@login_required(redirect_field_name='', login_url='/admin/')
def render_report_parking_fee_new(request):  # Bao cao phi vang lai
    userid = request.user.id
    if notallowreport(userid):
        return render(request, 'admin/nopermission.html')
    def add_worksheet_stat(sheet_name, workbook, TEMPLATE, sheet_protect=True):
        LOGO_EXCEL_REPORT_PATH = 'parking/static/image/' + get_setting('logo_report', u'Mẫu logo dùng trong báo cáo',
                                                                       'logo_report.png')
        LOGO_EXCEL_REPORT_PATH_RIGHT = 'parking/static/image/' + get_setting('logo_repor',
                                                                             u'Mẫu logo dùng trong báo cáo',
                                                                             'logo_report_right.png')
        TITLE = TEMPLATE['TITLE']

        sheet = workbook.add_worksheet(sheet_name)
        sheet.insert_image(0, 0, LOGO_EXCEL_REPORT_PATH)
        sheet.insert_image(0, len(TEMPLATE['HEADER'])+5, LOGO_EXCEL_REPORT_PATH_RIGHT)

        bold = workbook.add_format({'bold': True})
        wrap = workbook.add_format()
        wrap.set_text_wrap()
        border = workbook.add_format()
        border.set_border()
        bold_border = workbook.add_format({'bold': True, 'border': 1})
        number_border_format = workbook.add_format({'num_format': '#,###0', 'border': 1})
        number_bold_border_format = workbook.add_format({'num_format': '#,###0', 'border': 1, 'bold': True})

        for r in TITLE:  # Viet tieu de: (dong r[0], cot r[1])
            sheet.write(r[0], r[1], r[2], bold)
            if len(r) == 4:
                sheet.write(r[0], r[1] + 1, r[3])

        for i in xrange(1, len(TEMPLATE['HEADER'])):  # Viet table TEMPLATE[HEADER] (dong 5)
            sheet.write(5, i - 1, TEMPLATE['HEADER'][i][0], bold_border)

        len_header_sub = len(TEMPLATE['HEADER_SUB'])

        i = 1
        # print "@@@ TEMPLATE HEADER SUB", TEMPLATE['HEADER_SUB']

        for i in xrange(0, len_header_sub):
            sheet.write(5 + i + 1, 0, TEMPLATE['HEADER_SUB'][i][0], border)
            sheet.set_column(0, 0, 30)
            len_header_sub_i = len(TEMPLATE['HEADER_SUB'][i])
            # print ">> i ne", i
            for j in xrange(1, len_header_sub_i):
                # print "     j ne", j
                sheet.write(5 + i + 1, j, TEMPLATE['HEADER_SUB'][i][j], number_border_format)

        header_total_starting_row = 7 + i
        for x in xrange(1, len(TEMPLATE['HEADER_TOTAL'])):
            if x == 1:
                sheet.write(header_total_starting_row, x - 1, TEMPLATE['HEADER_TOTAL'][x][0], bold_border)
            else:
                sheet.write_row(header_total_starting_row, x - 1, TEMPLATE['HEADER_TOTAL'][x],
                                number_bold_border_format)

        format_cols = [0]
        for l in xrange(1, len(TEMPLATE['HEADER2'])):  # Viet table TEMPLATE[HEADER] (dong 5)
            sheet.write(7 + i + 2, l - 1, TEMPLATE['HEADER2'][l][0], bold_border)
            if len(TEMPLATE['HEADER2'][l]) > 1:
                format_cols.append(workbook.add_format(TEMPLATE['HEADER2'][l][1]))

        len_header2_sub = len(TEMPLATE['HEADER2_SUB'])

        header_2_sub_starting_row = 7 + i + 3

        for k in xrange(0, len_header2_sub):
            sheet.write(header_2_sub_starting_row + k, 0, TEMPLATE['HEADER2_SUB'][k][0], border)
            len_header2_sub_k = len(TEMPLATE['HEADER2_SUB'][k])

            for j in xrange(1, len_header2_sub_k):
                sheet.write(header_2_sub_starting_row + k, j, TEMPLATE['HEADER2_SUB'][k][j], format_cols[j])
                sheet.set_column(j, j, 15)  # Default width

        # row_tong_cong = len_header_sub + len_header2_sub + 7 + 1
        row_tong_cong = header_2_sub_starting_row + len_header2_sub

        for i in xrange(1, len(TEMPLATE['HEADER3'])):
            if i == 1:
                sheet.write(row_tong_cong, i - 1, TEMPLATE['HEADER3'][i][0], bold_border)
            else:
                sheet.write_row(row_tong_cong, i - 1, TEMPLATE['HEADER3'][i], number_bold_border_format)
        # Chu Ky
        sheet.write(row_tong_cong +2, 0, 'Handed Over by :')
        sheet.write(row_tong_cong + 4, 0, 'Taken Over by :', bold)
        sheet.write(row_tong_cong + 6, 0, 'Date Handed Over :')
        sheet.write(row_tong_cong + 8, 0, 'Checked by :')


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

    def add_worksheet_stat2(sheet_name, workbook, TEMPLATE, data, sheet_protect=True):
        LOGO_EXCEL_REPORT_PATH = 'parking/static/image/' + get_setting('logo_report', u'Mẫu logo dùng trong báo cáo',
                                                                       'logo_report.png')
        LOGO_EXCEL_REPORT_PATH_RIGHT = 'parking/static/image/' + get_setting('logo_repor',
                                                                             u'Mẫu logo dùng trong báo cáo',
                                                                             'logo_report_right.png')
        TITLE = TEMPLATE['TITLE']

        sheet = workbook.add_worksheet(sheet_name)
        sheet.insert_image(0, 0, LOGO_EXCEL_REPORT_PATH)
        sheet.insert_image(0, len(TEMPLATE['HEADER'])+5, LOGO_EXCEL_REPORT_PATH_RIGHT)
        bold = workbook.add_format({'bold': True})
        wrap = workbook.add_format()
        wrap.set_text_wrap()
        border = workbook.add_format()
        border.set_border()
        bold_border = workbook.add_format({'bold': True, 'border': 1})

        number_border_format = workbook.add_format({'num_format': '#,###0', 'border': 1})

        for r in TITLE:  # Viet tieu de: (dong r[0], cot r[1])
            sheet.write(r[0], r[1], r[2], bold)
            if len(r) == 4:
                sheet.write(r[0], r[1] + 1, r[3])

        # for i in xrange(0, len(TEMPLATE['VEHICLE_TYPE'])):
        #     col = (i + 1) * 5 - 1 - 2
        #     # sheet.write(4, col , TEMPLATE['VEHICLE_TYPE'][i][0], bold_border)
        #     sheet.merge_range(5, col, 5, col + 5 - 1, TEMPLATE['VEHICLE_TYPE'][i][0], bold_border)

        for i in xrange(1, len(TEMPLATE['HEADER'])):  # Viet table TEMPLATE[HEADER] (dong 5)
            sheet.write(6, i - 1, TEMPLATE['HEADER'][i][0], bold_border)
            sheet.set_column(i - 1, i - 1, 10)

        for j in xrange(0, len(TEMPLATE['DETAIL'])):
            col = 2 + j
            sheet.write(6, 2 + j, TEMPLATE['DETAIL'][j][0], bold_border)
            sheet.set_column(col, col, 20)
        max_column_num = 2 + len(TEMPLATE['DETAIL'])

        for i in xrange(0, len(data)):
            row = 7 + i
            sheet.write_row(row, 0, [''] * max_column_num, border)

            sheet.write(row, 0, data[i][1], border)
            sheet.write(row, 1, data[i][2], border)
            sheet.write(row, 2, data[i][11], border)

            for j in xrange(0, 8):
                if j == 4:
                    sheet.write(row, j + 3, data[i][j + 3], number_border_format)
                else:
                    sheet.write(row, j + 3, data[i][j + 3], border)

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

    # Template Bao cao thu phi dau xe
    userid = request.user.id
    username = getUserName(userid)
    util = Utilities()
    qr = util.QuerySecond('getrootbyuser', userid)
    if qr and len(qr) > 0:
        isroot = True
    else:
        isroot = False
    number_border_format = {'num_format': '#,###0', 'border': 1}
    TEMPLATE_SHEET_PARKING_FEE = {
        'HEADER': [
            (0,),
            (u'Số lượt xe vào',),
            (u'Số lượt',),
        ],

        'HEADER_SUB': [],

        'HEADER_TOTAL': [
            (0,),
            (u'Tổng số lượt xe vào',),
        ],

        'HEADER2': [
            (0,),
            (u'Số lượt xe ra', number_border_format),
            (u'Số lượt',number_border_format),
            (u'Thành tiền',number_border_format),
            (u'Điều chỉnh',number_border_format),
            (u'Thành tiền phải nộp',number_border_format),
            (u'Ghi chú',),
        ],

        'HEADER2_SUB': [],

        'HEADER3': [
            (0,),
            (u'Tổng số lượt xe ra - doanh thu',),
        ],

        'TITLE': [
            (2, 1, u'BÁO CÁO TỔNG PHÍ ĐẬU XE VÃNG LAI'),
            [3, 1, u'Từ'],
            [3, 3, u'Đến'],
        ],
    }

    vehicle_type_data = list()
    vehicle_types = VehicleType.objects.all()
    for type in vehicle_types:
        vehicle_type_data.append({"value": type.id, "name": type.name})

    now = datetime.now()
    from_time = datetime(now.year, now.month, now.day).replace(day=1)  # TG bat dau: Dau thang hien tai

    month = now.month
    year = now.year + month / 12

    try:
        month = (month + 1) % 12
        if month == 0: month = 12
    except:
        pass

    # to_time = from_time.replace(year=year, month=month, day=1, hour=23) + timedelta(
    #     days=-1)  # TG ket thuc: Cuoi thang hien tai
    # to_time = from_time.replace(hour=23, minute=59, second=59)  # Cuoi ngay
    from_time = now.replace(hour=00, minute=00, second=00)  # Bat dau ngay
    to_time = now.replace(hour=23, minute=59, second=59)  # Cuoi ngay
    if 'btn_REPORT' in request.POST:
        # TEMPLATE_SHEET_PARKING_FEE_DETAIL = {
        #     'HEADER': [
        #         (0,),
        #         (u'STT',),
        #         (u'Ngày thu',),
        #     ],
        #
        #     'VEHICLE_TYPE': [],
        #
        #     'DETAIL': [
        #         (u'Loại xe', 'vehicle_type'),
        #         (u'Số thẻ', 'card_id'),
        #         (u'Biển số xe', 'vehicle_number'),
        #         (u'Nhân viên cho vào', 'check_in_operator_id__userprofile__fullname'),
        #         (u'Giờ vào',),
        #         (u'Nhân viên cho ra', 'check_out_operator_id__userprofile__fullname'),
        #         (u'Giờ ra',),
        #         (u'Check -Out Ngoại lệ',),
        #         (u'Thành tiền', 'parking_fee'),
        #     ],
        #
        #     'TITLE': [
        #         (2, 5, u'BÁO CÁO CHI TIẾT VÃNG LAI'),
        #         [3, 5, u'Từ'],
        #         [3, 8, u'Đến'],
        #     ],
        # }

        # is_protected_report = False if 'unprotected_report' in request.POST else True

        current_user = request.user
        # if not is_protected_report and not current_user.has_perm('parking.export_unprotected_excel'):
        #     messages.error(request, u'Tài khoản hiện tại không có quyền này!', fail_silently=True)
        #     return redirect(reverse('render_report_parking_fee'))

        from_time = request.POST['from_time']
        to_time = request.POST['to_time']

        # TEMPLATE_SHEET_PARKING_FEE_DETAIL['TITLE'][1].append(from_time)  # Tu
        # TEMPLATE_SHEET_PARKING_FEE_DETAIL['TITLE'][2].append(to_time)  # Den

        # vehicle_type_list = list()
        # vehicle_types = VehicleType.objects.all()
        #
        # for type in vehicle_types:
        #     vehicle_type_id = type.id
        #     if vehicle_type_id == 100000000:
        #         continue
        #
        #     TEMPLATE_SHEET_PARKING_FEE_DETAIL['VEHICLE_TYPE'].append((type.name, {'merge_cell': 5}))
        #     vehicle_type_list.append(type.id)

        _from_time = datetime.strptime(from_time, "%d/%m/%Y %H:%M:%S")  # Datetime
        _to_time = datetime.strptime(to_time, "%d/%m/%Y %H:%M:%S")
        fdate=_from_time.strftime("%Y-%m-%d %H:%M:%S")
        tdate = _to_time.strftime("%Y-%m-%d %H:%M:%S")
        util = Utilities()
        qr = util.Query('getparkingsessionreportdetail',fdate, tdate)
        # data = list()
        #
        # rs1 = ParkingFeeSession.objects \
        #     .filter(payment_date__gte=_from_time, payment_date__lte=_to_time, session_type='OUT') \
        #     .select_related('parking_session').values('id','vehicle_type_id', 'parking_fee',
        #                                               'parking_session__vehicle_number',
        #                                               'payment_date',
        #                                               'parking_session__check_in_operator_id__userprofile__fullname',
        #                                               'parking_session__check_in_time',
        #                                               'parking_session__check_out_operator_id__userprofile__fullname',
        #                                               'parking_session__check_out_time',
        #                                               'parking_session__card__card_label',
        #                                               'parking_session__check_out_exception_id__notes',
        #                                               'vehicle_type__name')
        #
        # rs2 = ParkingSession.objects \
        #         .filter(check_out_time__gte=_from_time, check_out_time__lte=_to_time, check_out_exception_id__isnull=False) \
        #         .select_related('check_out_exception').values('vehicle_type', 'check_out_exception__parking_fee',
        #                                                       'vehicle_number',
        #                                                       'check_in_operator_id__userprofile__fullname',
        #                                                       'check_in_time',
        #                                                       'check_out_operator_id__userprofile__fullname',
        #                                                       'check_out_time',
        #                                                       'check_out_exception_id__notes',
        #                                                       'card__card_label')
        #
        #
        # index = 0
        # for i, r in enumerate(rs1):
        #
        #
        #     index += 1
        #     data.append([
        #         vehicle_type_list.index(r['vehicle_type_id']) + 1,
        #         index,
        #         localtime(r['payment_date']).strftime("%d/%m/%Y") if r['payment_date'] else '',
        #         r['parking_session__card__card_label'],
        #         r['parking_session__vehicle_number'],
        #         r['parking_session__check_in_operator_id__userprofile__fullname'],
        #         localtime(r['parking_session__check_in_time']).strftime("%d/%m/%Y %H:%M:%S") if r[
        #             'parking_session__check_in_time'] else '',
        #         r['parking_session__check_out_operator_id__userprofile__fullname'],
        #         localtime(r['parking_session__check_out_time']).strftime("%d/%m/%Y %H:%M:%S") if r[
        #             'parking_session__check_in_time'] else '',
        #         r['parking_session__check_out_exception_id__notes'],
        #         unicode(r['parking_fee']),
        #         r['vehicle_type__name']
        #     ])
        #
        # for i, r in enumerate(rs2):
        #     index += 1
        #     data.append([
        #         vehicle_type_list.index(VEHICLE_TYPE_FULL_ID_BY_DECODED_ID_DICT[r['vehicle_type']]) + 1,
        #         index,
        #         localtime(r['check_out_time']).strftime("%d/%m/%Y") if r['check_out_time'] else '',
        #         r['card__card_label'],
        #         r['vehicle_number'],
        #         r['check_in_operator_id__userprofile__fullname'],
        #         localtime(r['check_in_time']).strftime("%d/%m/%Y %H:%M:%S") if r[
        #             'check_in_time'] else '',
        #         r['check_out_operator_id__userprofile__fullname'],
        #         localtime(r['check_out_time']).strftime("%d/%m/%Y %H:%M:%S") if r[
        #             'check_in_time'] else '',
        #         r['check_out_exception_id__notes'],
        #         unicode(r['check_out_exception__parking_fee']),
        #         VEHICLE_TYPE_NAME_BY_DECODED_ID[r['vehicle_type']]
        #     ])
        #

        # rs1 = ParkingFeeSession.objects.all()
        # rs2 = rs1.filter(session_type='OUT')
        # rs1 = rs1.filter(session_type='IN')
        #
        # len_rs2 = rs2.count()
        # for i in xrange(len_rs2):
        #     row = [vehicle_type_list.index(rs2[i].vehicle_type_id) + 1, i, rs2[i].payment_date.strftime("%d/%m/%Y"),
        #            rs2[i].card_id, rs2[i].vehicle_number]
        #
        #     session_in = rs1.filter(parking_session_id=rs2[i].parking_session_id)
        #     check_in_time = ''
        #     if session_in:
        #         check_in_time = session_in[0].payment_date.strftime("%d/%m/%Y %H:%M")
        #     row.append(check_in_time)
        #     row.append(rs2[i].payment_date.strftime("%d/%m/%Y %H:%M"))
        #     row.append(unicode(rs2[i].parking_fee))
        #
        #     data.append(row)

        folder_name = 'templates/report'
        file_name = 'BC_PhiVangLai.xlsx'
        file_path = '%s/%s' % (folder_name, file_name)

        if not os.path.exists(folder_name):
            os.mkdir(folder_name)
        if os.path.isfile(file_path):
            os.remove(file_path)

        workbook = Workbook(file_path, {'constant_memory': True})
        LOGO_EXCEL_REPORT_PATH = 'parking/static/image/' + get_setting('logo_report', u'Mẫu logo dùng trong báo cáo',
                                                                       'logo_report.png')
        LOGO_EXCEL_REPORT_PATH_RIGHT = 'parking/static/image/' + get_setting('logo_repor',
                                                                             u'Mẫu logo dùng trong báo cáo',
                                                                             'logo_report_right.png')

        sheet = workbook.add_worksheet('BC_PhiVangLai')
        sheet.insert_image(0, 0, LOGO_EXCEL_REPORT_PATH)
        sheet.insert_image(0, 15, LOGO_EXCEL_REPORT_PATH_RIGHT)

        bold = workbook.add_format({'bold': True})
        wrap = workbook.add_format()
        wrap.set_text_wrap()
        border = workbook.add_format()
        border.set_border()
        bold_border = workbook.add_format({'bold': True, 'border': 1})
        bold_border_center = workbook.add_format({'bold': True, 'border': 1, 'align': 'center'})
        number_border_format = workbook.add_format({'num_format': '#,###0', 'border': 1})
        sheet.set_column(1, 14, 20)
        sheet.merge_range(3, 3, 3, 6, u'BÁO CÁO CHI TIẾT VÃNG LAI', bold)
        sheet.merge_range(4, 3, 4, 6, "Từ: %s đến: %s"%(_from_time.strftime("%d/%m/%Y %H:%M:%S"),_to_time.strftime("%d/%m/%Y %H:%M:%S")), bold)

        sheet.merge_range(6, 10, 6, 13, u'Công thức', bold_border_center)
        row_data = [u'Stt', u'Ngày thu', u'Loại xe', u'Số thẻ', u'Biển số xe', u'Nhân viên Check-in',
                    u'Giờ Check-in', u'Nhân viên Check-out', u'Giờ Check-out', u"Check-Out ngoại lệ",
                    u'Dung sai (phút)', u'Thời gian miễn phí (phút)', u'Từ đầu',u'Tên công thức', u'Thành tiền']
        sheet.write_row(7, 0, row_data, bold_border)
        # add_worksheet_stat2('Chi tiet vang lai', workbook, TEMPLATE_SHEET_PARKING_FEE_DETAIL, data, is_protected_report)
        startrow=7
        index=1
        for q in qr:
            tmmdt=listinsertreport(q[9], q[10], q[5], q[11])
            row_data = [index, q[0], q[1], q[2], q[3], q[4], q[5], q[6], q[7], q[8],
                        tmmdt[1],tmmdt[2],tmmdt[3],tmmdt[0], q[13]]
            sheet.write_row(startrow+index, 0, row_data, number_border_format)
            index=index+1
        # if is_protected_report:
        #     sheet.protect('ndhoang', options={
        #         'format_cells': True,
        #         'format_columns': True,
        #         'format_rows': True,
        #         'select_locked_cells': True,
        #         'sort': True,
        #         'autofilter': True,
        #         'pivot_tables': True,
        #         'select_unlocked_cells': True,
        #     })
        workbook.close()

        with open(file_path, 'r') as f:
            response = HttpResponse(f.read(), content_type="application/vnd.ms-excel")
            response['Content-Disposition'] = "attachment; filename=%s" % file_name
            return response

    return render(request, 'admin/rp_phivanglai.html', {'vehicle_type_data': vehicle_type_data,
                                                        'from_time': from_time.strftime("%d/%m/%Y %H:%M:%S"),
                                                        'to_time': to_time.strftime("%d/%m/%Y %H:%M:%S"),'username':username,"isroot":isroot
                                                        })
@login_required(redirect_field_name='', login_url='/admin/')
def render_report_daily_compact(request):  # Bao cao compact
    userid = request.user.id
    if notallowreport(userid):
        return render(request, 'admin/nopermission.html')
    def convertduration(duration):
        try:
            rm = divmod(duration, 86400)
            days=rm[0]
            seconds=rm[1]
            rm=divmod(seconds, 3600)
            hours=rm[0]
            seconds = rm[1]
            rm = divmod(seconds, 60)
            minutes=rm[0]
            seconds = rm[1]
            return "%s - %s:%s:%s"%(days,hours,minutes,seconds)
        except:
            return ''
    number_border_format = {'num_format': '#,###0', 'border': 1}
    now = datetime.now()
    from_time = datetime(now.year, now.month, now.day).replace(day=1)  # TG bat dau: Dau thang hien tai

    month = now.month
    year = now.year + month / 12

    try:
        month = (month + 1) % 12
        if month == 0: month = 12
    except:
        pass
    from_time = now.replace(hour=00, minute=00, second=00)  # Bat dau ngay
    to_time = now.replace(hour=23, minute=59, second=59)  # Cuoi ngay
    if 'btn_REPORT' in request.POST:
        from_time = request.POST['from_time']
        to_time = request.POST['to_time']
        _from_time = datetime.strptime(from_time, "%d/%m/%Y %H:%M:%S")  # Datetime
        _to_time = datetime.strptime(to_time, "%d/%m/%Y %H:%M:%S")
        fdate=_from_time.strftime("%Y-%m-%d %H:%M:%S")
        tdate = _to_time.strftime("%Y-%m-%d %H:%M:%S")
        util = Utilities()
        qr = util.Query('getreportcompact',fdate, tdate)
        folder_name = 'templates/report'
        file_name = 'Daily_Record_Compact.xlsx'
        file_path = '%s/%s' % (folder_name, file_name)
        if not os.path.exists(folder_name):
            os.mkdir(folder_name)
        if os.path.isfile(file_path):
            os.remove(file_path)
        workbook = Workbook(file_path, {'constant_memory': True})
        LOGO_EXCEL_REPORT_PATH = 'parking/static/image/' + get_setting('logo_report', u'Mẫu logo dùng trong báo cáo',
                                                                       'logo_report.png')
        LOGO_EXCEL_REPORT_PATH_RIGHT = 'parking/static/image/' + get_setting('logo_repor',
                                                                             u'Mẫu logo dùng trong báo cáo',
                                                                             'logo_report_right.png')

        sheet = workbook.add_worksheet('Daily_Record_Compact')
        sheet.insert_image(0, 0, LOGO_EXCEL_REPORT_PATH)
        sheet.insert_image(0, 7, LOGO_EXCEL_REPORT_PATH_RIGHT)
        bold = workbook.add_format({'bold': True})
        wrap = workbook.add_format()
        wrap.set_text_wrap()
        border = workbook.add_format()
        border.set_border()
        bold_border = workbook.add_format({'bold': True, 'border': 1})
        bold_border_center = workbook.add_format({'bold': True, 'border': 1, 'align': 'center'})
        number_border_format = workbook.add_format({'num_format': '#,###0', 'border': 1})
        sheet.set_column(1, 14, 20)
        sheet.merge_range(3, 3, 3, 6, u'Daily Record Compact Report', bold)
        sheet.merge_range(4, 3, 4, 6, "From: %s To: %s"%(_from_time.strftime("%d/%m/%Y %H:%M:%S"),_to_time.strftime("%d/%m/%Y %H:%M:%S")), bold)
        row_data = [ u'Type', u'Card Type', u'Card Code', u'Check-in time', u'Claimed',
                    u'Check-out time', u'Parking Duration', u'Redemption']
        sheet.write_row(7, 0, row_data, bold_border)
        startrow=7
        index=1
        for q in qr:
            row_data = [ q[0], q[1], q[2], q[3], q[4], q[5], convertduration(q[6]), q[7]]
            sheet.write_row(startrow+index, 0, row_data, number_border_format)
            index=index+1
        workbook.close()
        with open(file_path, 'r') as f:
            response = HttpResponse(f.read(), content_type="application/vnd.ms-excel")
            response['Content-Disposition'] = "attachment; filename=%s" % file_name
            return response

    return render(request, 'admin/rp_compact.html', {'from_time': from_time.strftime("%d/%m/%Y %H:%M:%S"),
                                                        'to_time': to_time.strftime("%d/%m/%Y %H:%M:%S"),
                                                        })
@login_required(redirect_field_name='', login_url='/admin/')
def render_report_ticket_payment(request):  # Bao cao phi ve thang
    userid = request.user.id
    if notallowreport(userid):
        return render(request, 'admin/nopermission.html')
    vehicle_type_data = list()

    vehicle_types = VehicleType.objects.all()
    for type in vehicle_types:
        vehicle_type_data.append({"value": type.id, "name": type.name})

    now = datetime.now()
    from_time = datetime(now.year, now.month, now.day).replace(day=1)  # TG bat dau: Dau thang hien tai

    month = now.month
    year = now.year + month / 12

    try:
        month = (month + 1) % 12
        if month == 0: month = 12
    except:
        pass
    to_time = from_time.replace(year=year, month=month, day=1, hour=23) + timedelta(
        days=-1)  # TG ket thuc: Cuoi thang hien tai

    customer_type_data = list()

    customer_types = CustomerType.objects.all()
    customer_type_data.append({"value": "", "name": "Tất cả"})
    for customer_type in customer_types:
        customer_type_data.append({"value": customer_type.name, "name": customer_type.name})

    payment_method_data = [{"value": "", "name": "Tất cả"},
                           {"value": "TM", "name": "Tiền mặt"},
                           {"value": "CK", "name": "Chuyển khoản"}]

    if 'btn_REPORT_DETAIL' in request.POST:
        def add_worksheet_stat2(sheet_name, workbook, TEMPLATE, data, custom_param, sheet_protect=True):
            LOGO_EXCEL_REPORT_PATH = 'parking/static/image/' + get_setting('logo_report',
                                                                           u'Mẫu logo dùng trong báo cáo',
                                                                           'logo_report.png')
            LOGO_EXCEL_REPORT_PATH_RIGHT = 'parking/static/image/' + get_setting('logo_repor',
                                                                                 u'Mẫu logo dùng trong báo cáo',
                                                                                 'logo_report_right.png')
            TITLE = TEMPLATE['TITLE']
            TOP = TEMPLATE['TOP']
            VEHICLE_TYPE = TEMPLATE['VEHICLE_TYPE']
            HEADER = TEMPLATE['HEADER']
            DETAIL = TEMPLATE['DETAIL']

            sheet = workbook.add_worksheet(sheet_name)
            sheet.insert_image(0, 0, LOGO_EXCEL_REPORT_PATH)
            sheet.insert_image(0, len(HEADER), LOGO_EXCEL_REPORT_PATH_RIGHT)

            bold = workbook.add_format({'bold': True})
            wrap = workbook.add_format()
            wrap.set_text_wrap()
            border = workbook.add_format()
            border.set_border()
            bold_border = workbook.add_format({'bold': True, 'border': 1})
            bold_center_border = workbook.add_format({'align': 'center', 'bold': True, 'border': 1})
            bold_center_border.set_pattern(1)  # This is optional when using a solid fill.
            bold_center_border.set_bg_color('#ffffff')
            number_border_format = workbook.add_format({'num_format': '#,###0', 'border': 1})
            bold_number_border_format = workbook.add_format({'num_format': '#,###0', 'border': 1, 'bold': True})

            cancelled_cell = workbook.add_format({'bg_color': 'red', 'border': 1})

            for r in TITLE:  # Viet tieu de: (dong r[0], cot r[1])
                sheet.write(r[0], r[1], r[2], bold)
                if len(r) == 4:
                    sheet.write(r[0], r[1] + 1, r[3])

            sheet.write_row(5, 0, [''] * 13, bold_center_border)

            for r in TOP:
                col = r[1]['col']
                end_col = col + r[1]['h_merge_range'] - 1
                sheet.merge_range(5, col, 5, end_col, r[0], bold_center_border)

            next_col = TOP[2][1]['col']

            for type in VEHICLE_TYPE:
                sheet.merge_range(6, next_col, 6, next_col + 1, type, bold_center_border)
                next_col += 2

            # sheet.write_row(6, 0, [''] * 12, border)

            sheet.write_row(6, 0, [''] * TOP[1][1]['col'], bold_center_border)

            sheet.merge_range(6, 1, 7, 1, u"Số", )  # So
            sheet.write(6, 1, u"Số", bold_center_border)
            sheet.set_column(1, 1, 15)  # Default width
            sheet.merge_range(6, 2, 7, 2, u"Ngày nộp", )  # Ngay nop
            sheet.write(6, 2, u"Ngày nộp", bold_center_border)
            sheet.set_column(2, 2, 15)  # Default width
            sheet.merge_range(6, 3, 7, 3, u"Ngày nộp", )  # Ngay nop
            sheet.write(6, 3, u"Phiếu thu bị huỷ", bold_center_border)
            sheet.set_column(3, 3, 50)  # Default width
            sheet.set_tab_color('red')
            sheet.write_row(6, TOP[1][1]['col'], VEHICLE_TYPE, bold_center_border)  # Coc loai xe
            sheet.write_row(7, TOP[1][1]['col'], [''] * len(VEHICLE_TYPE), bold_center_border)  # Coc loai xe

            for i in xrange(len(HEADER)):
                if i not in [1, 2, 3]:
                    sheet.write(7, i, HEADER[i][0], bold_center_border)
                    sheet.set_column(i, i, 20)  # Default width

            next_col = TOP[2][1]['col']

            for type in VEHICLE_TYPE:  # So thang dong phi, Thanh tien
                sheet.write_row(7, next_col, DETAIL, bold_center_border)
                next_col += 2
            sheet.write(7, next_col, u'Tổng cộng', bold_center_border)

            num_line = len(data)
            for i in xrange(num_line):
                if data[i][3].find(u'HUỶ') != -1:
                    sheet.write_row(8 + i, 0, data[i], cancelled_cell)  # Ghi toan dong du lieu (xuat phat tu dong 6)
                else:
                    sheet.write_row(8 + i, 0, data[i],
                                    number_border_format)  # Ghi toan dong du lieu (xuat phat tu dong 6)
                sheet.set_column(0, len(data[i]), 15)
                if num_line - i == 1:
                    sheet.write_row(8 + i, 0, data[i],
                                    bold_number_border_format)  # Ghi toan dong du lieu (xuat phat tu dong 6)
            # sheet.set_row(k+6, (count_newline + 1) * 15)

            sheet.write(8 + num_line + 1, TOP[1][1]['col'], u"Ngày lập báo cáo", bold)
            sheet.write(8 + num_line + 1, TOP[1][1]['col'] + 1, now.strftime("%d/%m/%Y %H:%M:%S"))

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

        TEMPLATE_SHEET_PARKING_FEE_DETAIL = {
            'TOP': [
                (u'Phiếu thu', {'col': 1, 'h_merge_range': 3}),
                [u'Cọc', {'col': 13, 'h_merge_range': 0}],
                [u'Phí gửi xe tháng', ],
            ],

            'HEADER': [
                (u'STT',),
                (u'Số', 'ticket_payment__receipt_number',),
                (u'Ngày nộp', 'ticket_payment__payment_date'),
                (u'Phiếu thu bị huỷ', 'ticket_payment_id'),
                (u'Tòa nhà', 'ticket_payment__customer__building__name'),
                (u'Mã căn hộ', 'ticket_payment__customer__apartment__address'),
                (u'Tên khách hàng', 'ticket_payment__customer__customer_name'),
                (u'Công ty', 'ticket_payment__customer__company__name'),
                (u'Loại khách hàng', 'ticket_payment__customer__customer_type__name'),
                (u'Phương thức', 'ticket_payment__payment_method'),
                (u'Số xe', 'vehicle_number'),
                (u'Ngày hiệu lực', 'effective_date'),
                (u'Hạn hiện tại', 'expired_date'),
            ],

            'HEADER2': [
                (u'STT',),
                (u'Số', 'deposit_payment__receipt_number',),
                (u'Ngày nộp', 'deposit_payment__payment_date'),
                (u'Phiếu thu bị huỷ', 'deposit_payment_id'),
                (u'Tòa nhà', 'deposit_payment__customer__building__name'),
                (u'Mã căn hộ', 'deposit_payment__customer__apartment__address'),
                (u'Tên khách hàng', 'deposit_payment__customer__customer_name'),
                (u'Công ty', 'deposit_payment__customer__company__name'),
                (u'Loại khách hàng', 'deposit_payment__customer__customer_type__name'),
                (u'Phương thức', 'deposit_payment__payment_method'),
                (u'Số xe', 'vehicle_number'),
                (u'Ngày hiệu lực', 'deposit_payment__payment_date'),
                (u'Hạn hiện tại',),
            ],

            'VEHICLE_TYPE': [],

            'DETAIL': [
                (u'TG đóng thêm'),
                (u'Số tiền'),
            ],

            'TITLE': [
                (2, 5, u'BÁO CÁO TỔNG PHÍ GIỮ XE THÁNG'),
                [3, 5, u'Từ'],
                [3, 8, u'Đến'],
            ],

            'STAT': [],  # [(4, 6 + i*2, type[1], type[0]) for i, type in enumerate(VEHICLE_TYPE)]
        }

        is_protected_report = False if 'unprotected_report' in request.POST else True

        current_user = request.user
        if not is_protected_report and not current_user.has_perm('parking.export_unprotected_excel'):
            messages.error(request, u'Tài khoản hiện tại không có quyền này!', fail_silently=True)
            return redirect(reverse('render_report_ticket_payment'))

        from_time = request.POST['from_time']
        to_time = request.POST['to_time']
        TEMPLATE_SHEET_PARKING_FEE_DETAIL['TITLE'][1].append(from_time)  # Tu
        TEMPLATE_SHEET_PARKING_FEE_DETAIL['TITLE'][2].append(to_time)  # Den

        customer_type = request.POST['customer_type'].strip()
        payment_method = request.POST['payment_method'].strip()

        vehicle_type_list = list()
        vehicle_types = VehicleType.objects.all()
        for type in vehicle_types:
            TEMPLATE_SHEET_PARKING_FEE_DETAIL['VEHICLE_TYPE'].append(type.name)
            vehicle_type_list.append(type.id)
        len_vehicle_type_list = len(vehicle_type_list)

        TEMPLATE_SHEET_PARKING_FEE_DETAIL['TOP'][1][1]['h_merge_range'] = len_vehicle_type_list
        TEMPLATE_SHEET_PARKING_FEE_DETAIL['TOP'][2].append(
            {'col': TEMPLATE_SHEET_PARKING_FEE_DETAIL['TOP'][1][1]['col'] + len_vehicle_type_list,
             'h_merge_range': 2 * len_vehicle_type_list})
        data = list()

        rs = TicketPaymentDetail.objects.all().select_related('ticket_payment__payment_date',
                                                              'ticket_payment__customer__customer_type__name',
                                                              'ticket_payment__payment_method',
                                                              'ticket_payment__receipt_number',
                                                              'ticket_payment__customer__building__name',
                                                              'ticket_payment__customer__apartment__address',
                                                              'ticket_payment__customer__customer_name',
                                                              'ticket_payment__customer__company__name',
                                                              'vehicle_registration__level_fee__fee',
                                                              'vehicle_registration__vehicle_type',
                                                              )  # Chi tiet thanh toan ve thang
        rs2 = DepositPaymentDetail.objects.all().select_related('deposit_payment__payment_date'
                                                                'deposit_payment__customer__customer_type__name',
                                                                'deposit_payment__payment_method',
                                                                'deposit_payment__receipt_number',
                                                                'deposit_payment__customer__building__name',
                                                                'deposit_payment__customer__apartment__address',
                                                                'deposit_payment__customer__customer_name',
                                                                'deposit_payment__customer__company__name',
                                                                'deposit_action_fee__fee',
                                                                'vehicle_registration__vehicle_type',
                                                                )

        try:
            _from_time = datetime.strptime(from_time, "%d/%m/%Y").replace(tzinfo=utc)
            _to_time = datetime.strptime(to_time, "%d/%m/%Y").replace(hour=23, minute=59, second=59, tzinfo=utc)

            rs = rs.filter(ticket_payment__payment_date__lte=_to_time,
                           ticket_payment__payment_date__gte=_from_time)  # Xe vao
            rs2 = rs2.filter(deposit_payment__payment_date__lte=_to_time,
                             deposit_payment__payment_date__gte=_from_time)  # Xe vao
        except:
            messages.error(request, u'Khoảng thời gian không phù hợp!', fail_silently=True)
            return HttpResponseRedirect('')

        if len(customer_type) > 0:
            rs = rs.filter(ticket_payment__customer__customer_type__name=customer_type)
            rs2 = rs2.filter(deposit_payment__customer__customer_type__name=customer_type)

        if len(payment_method) > 0:
            rs = rs.filter(ticket_payment__payment_method=payment_method)
            rs2 = rs2.filter(deposit_payment__payment_method=payment_method)

        list_val = [w[1] for w in TEMPLATE_SHEET_PARKING_FEE_DETAIL['HEADER'] if
                    len(w) == 2]  # Co field query hoac custom function
        list_val2 = [w[1] for w in TEMPLATE_SHEET_PARKING_FEE_DETAIL['HEADER2'] if
                     len(w) == 2]  # Co field query hoac custom function

        rs_value_list = (rs.values(*list_val))  # Query data
        rs2_value_list = (rs2.values(*list_val2))

        stt = 0
        total_sum = 0.0

        all_receipt = Receipt.objects.all()
        bool(all_receipt)

        for k, r in enumerate(rs_value_list):
            temp_data = list()
            count_newline = 0

            cancelled_receipts = None
            for j in range(0, len(TEMPLATE_SHEET_PARKING_FEE_DETAIL['HEADER'])):
                column = TEMPLATE_SHEET_PARKING_FEE_DETAIL['HEADER'][j]
                if column[0] == u'STT':  # Cell so thu tu tang dan
                    stt += 1
                    temp_data.append(stt)
                elif len(column) == 2 and column[1] in r:
                    # Nhung cot co dinh nghia query field
                    temp_value = r[column[1]]
                    if isinstance(temp_value, date):  # Du lieu cell kieu datetime.date
                        temp_value = temp_value.strftime("%d/%m/%Y")

                    if column[1] == 'ticket_payment_id':
                        cancelled_receipts = all_receipt.filter(type=0, ref_id=int(temp_value),
                                                                cancel=True).values_list('receipt_number',
                                                                                         'action_date', 'notes')
                        if cancelled_receipts:
                            temp_value = " ,".join(str(w[0]) for w in cancelled_receipts)
                        else:
                            temp_value = ""

                    if temp_value and column[0] == u'Số':
                        temp_value = int(temp_value)
                    temp_data.append(temp_value)

                    count_newline = max(count_newline, temp_value.count('\n')) if (
                        isinstance(temp_value, str) or isinstance(temp_value, unicode)) else count_newline

                else:  # Cot rong
                    temp_data.append('')  # Cell trong

            temp_data += [''] * len(vehicle_type_list)
            row = rs[k]

            vehicle_type_id = -1
            if row.vehicle_registration:
                if row.vehicle_registration.vehicle_type:
                    vehicle_type_id = row.vehicle_registration.vehicle_type.id
            month_duration = row.duration / 30
            day_duration = row.day_duration

            payment_detail_fee = row.payment_detail_fee or 0

            current_row_sum = 0.0

            for id in vehicle_type_list:
                if id == vehicle_type_id:
                    temp_data.append(u"%s tháng %s ngày" % (month_duration, day_duration))
                    temp_data.append(payment_detail_fee)
                    current_row_sum += payment_detail_fee

                    # final_ticket_payment_sum += sum
                else:
                    temp_data.append('')
                    temp_data.append('')

            total_sum += current_row_sum
            temp_data.append(current_row_sum)

            data.append(temp_data)

            if cancelled_receipts:
                for cancelled_receipt in cancelled_receipts:
                    stt += 1
                    temp_data = [stt, cancelled_receipt[0],
                                 cancelled_receipt[1].strftime("%d/%m/%Y %H:%M:%S") if cancelled_receipt[1] else "",
                                 u"HUỶ%s" % (": " + cancelled_receipt[2] if cancelled_receipt[2] else "")]
                    temp_data += [''] * (10 + 3 * len(vehicle_type_list))
                    data.append(temp_data)

                    # for i in xrange(len(data)):
                    #     row = data[i]
                    #     if row[3]:
                    #         try:

                    # except:
                    #     continue

        for k, r in enumerate(rs2_value_list):  # Coc
            temp_data = list()
            count_newline = 0
            cancelled_receipts = None
            for j in range(0, len(TEMPLATE_SHEET_PARKING_FEE_DETAIL['HEADER2'])):
                column = TEMPLATE_SHEET_PARKING_FEE_DETAIL['HEADER2'][j]
                if column[0] == u'STT':  # Cell so thu tu tang dan
                    stt += 1
                    temp_data.append(stt)
                elif len(column) == 2 and column[1] in r:
                    # Nhung cot co dinh nghia query field
                    temp_value = r[column[1]]
                    if isinstance(temp_value, date):  # Du lieu cell kieu datetime.date
                        temp_value = temp_value.strftime("%d/%m/%Y %H:%M:%S")

                    if column[1] == 'deposit_payment_id':
                        cancelled_receipts = all_receipt.filter(type=1, ref_id=int(temp_value),
                                                                cancel=True).values_list('receipt_number',
                                                                                         'action_date', 'notes')
                        if cancelled_receipts:
                            temp_value = " ,".join(str(w[0]) for w in cancelled_receipts)
                        else:
                            temp_value = ""

                    temp_data.append(temp_value)

                    count_newline = max(count_newline, temp_value.count('\n')) if (
                        isinstance(temp_value, str) or isinstance(temp_value, unicode)) else count_newline
                else:  # Cot rong
                    temp_data.append('')  # Cell trong

            # temp_data += [''] * len(vehicle_type_list)
            row = rs2[k]
            vehicle_type_id = -1
            if row.vehicle_registration:
                if row.vehicle_registration.vehicle_type:
                    vehicle_type_id = row.vehicle_registration.vehicle_type.id

            deposit_action_fee = row.deposit_action_fee.fee if row.deposit_action_fee else 0

            current_row_sum = 0.0
            for id in vehicle_type_list:
                if id == vehicle_type_id:
                    temp_data.append(deposit_action_fee)
                    current_row_sum += deposit_action_fee
                    # final_deposit_sum += sum
                else:
                    temp_data.append('')

            total_sum += current_row_sum
            temp_data += [''] * len(vehicle_type_list) * 2
            temp_data.append(current_row_sum)
            data.append(temp_data)

            if cancelled_receipts:
                for cancelled_receipt in cancelled_receipts:
                    stt += 1
                    temp_data = [stt, cancelled_receipt[0],
                                 cancelled_receipt[1].strftime("%d/%m/%Y %H:%M:%S") if cancelled_receipt[1] else "",
                                 u"HUỶ%s" % (": " + cancelled_receipt[2] if cancelled_receipt[2] else "")]
                    temp_data += [''] * (10 + 3 * len(vehicle_type_list))
                    data.append(temp_data)

        data = sorted(data, key=itemgetter(1))
        len_data = len(data)
        for i in xrange(len_data):
            data[i][0] = i + 1

        # New modify
        # final_ticket_payment_sum = rs.aggregate(Sum('payment_detail_fee'))
        # final_deposit_sum = rs2.aggregate(Sum('deposit_payment_detail_fee'))

        # STAT SUM
        temp_sum_data = [''] * len(TEMPLATE_SHEET_PARKING_FEE_DETAIL['HEADER'])
        temp_sum_data += [''] * (len(vehicle_type_list))
        temp_sum_data += [''] * (len(vehicle_type_list) * 2)

        temp_sum_data.append(total_sum)
        data.append(temp_sum_data)

        # temp_sum_data.append(final_deposit_sum['deposit_payment_detail_fee__sum'])
        # temp_sum_data.append(u'TONG PHI THANG')
        # temp_sum_data.append(final_ticket_payment_sum['payment_detail_fee__sum'])

        folder_name = 'templates/report'
        file_name = 'GPMS_BC_TongPhiVeThang.xlsx'
        file_path = '%s/%s' % (folder_name, file_name)

        if not os.path.exists(folder_name):
            os.mkdir(folder_name)
        if os.path.isfile(file_path):
            os.remove(file_path)

        workbook = Workbook(file_path, {'constant_memory': True})
        add_worksheet_stat2('Tong phi ve thang', workbook, TEMPLATE_SHEET_PARKING_FEE_DETAIL, data, {},
                            is_protected_report)  # D
        workbook.close()

        with open(file_path, 'r') as f:
            response = HttpResponse(f.read(), content_type="application/vnd.ms-excel")
            response['Content-Disposition'] = "attachment; filename=%s" % file_name
            return response

    return render(request, 'admin/rp-ticketpayment.html', {'vehicle_type_data': vehicle_type_data,
                                                           'customer_type_data': customer_type_data,
                                                           'payment_method_data': payment_method_data,
                                                           'from_time': from_time.strftime("%d/%m/%Y %H:%M:%S"),
                                                           'to_time': to_time.strftime("%d/%m/%Y %H:%M:%S"),
                                                           })


@login_required(redirect_field_name='', login_url='/admin/')
def render_report_parking_fee_ticket_payment(request):  # Bao cao tong doanh thu
    userid = request.user.id
    if notallowreport(userid):
        return render(request, 'admin/nopermission.html')
    def add_worksheet_stat(sheet_name, workbook, TEMPLATE, data, custom_param, sheet_protect=True):
        LOGO_EXCEL_REPORT_PATH = 'parking/static/image/' + get_setting('logo_report',
                                                                       u'Mẫu logo dùng trong báo cáo',
                                                                       'logo_report.png')
        LOGO_EXCEL_REPORT_PATH_RIGHT = 'parking/static/image/' + get_setting('logo_repor',
                                                                             u'Mẫu logo dùng trong báo cáo',
                                                                             'logo_report_right.png')
        TITLE = TEMPLATE['TITLE']

        sheet = workbook.add_worksheet(sheet_name)
        sheet.insert_image(0, 0, LOGO_EXCEL_REPORT_PATH)
        sheet.insert_image(0, 9, LOGO_EXCEL_REPORT_PATH_RIGHT)

        bold = workbook.add_format({'bold': True})
        border = workbook.add_format()
        border.set_border()
        bold_border = workbook.add_format({'bold': True, 'border': 1})

        number_border_format = workbook.add_format({'num_format': '#,###0', 'border': 1})
        bold_number_border = workbook.add_format({'num_format': '#,###0', 'border': 1, 'bold': True})

        for r in TITLE:  # Viet tieu de: (dong r[0], cot r[1])
            sheet.write(r[0], r[1], r[2], bold)
            if len(r) == 4:
                sheet.write(r[0], r[1] + 1, r[3])

        for i in xrange(1, len(TEMPLATE['HEADER'])):
            sheet.write(5, i - 1, TEMPLATE['HEADER'][i][0], bold_border)
        for i in xrange(1, len(TEMPLATE['HEADER2'])):
            sheet.write(6, i - 1, TEMPLATE['HEADER2'][i][0], bold_border)
            sheet.set_column(i - 1, i - 1, 10)  # Default width
        for r in xrange(3, 7):
            for i in range(1, len(TEMPLATE['HEADER' + str(r)])):
                sheet.write(6 + r - 2, i - 1, TEMPLATE['HEADER' + str(r)][i][0], number_border_format)
                if i == 1:
                    sheet.set_column(i - 1, i - 1, 25)

        for i in xrange(1, len(TEMPLATE['HEADER7'])):
            sheet.write(11, i - 1, TEMPLATE['HEADER7'][i][0], bold_number_border)

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

    # Template Bao cao thu phi dau xe
    TEMPLATE_SHEET_PARKING_FEE_TICKET_PAYMENT = {
        'HEADER': [
            (0,),
            (u'Loại xe',),
        ],

        'HEADER2': [
            (0,),
            (u'Loại phí',),
        ],

        'HEADER3': [
            (0,),
            (u'Xe tháng tiền mặt',),
        ],

        'HEADER4': [
            (0,),
            (u'Xe tháng chuyển khoản',),
        ],

        'HEADER5': [
            (0,),
            (u'Xe vãng lai',),
        ],

        'HEADER6': [
            (0,),
            (u'Tiền cọc',),
        ],

        'HEADER7': [
            (0,),
            (u'Tổng tiền thu phí đậu xe',),
        ],

        'TITLE': [
            (2, 5, u'BÁO CÁO TIỀN THU PHÍ ĐẬU XE'),
            [3, 5, u'Từ'],
            [3, 8, u'Đến'],
        ],
    }

    vehicle_type_data = list()

    vehicle_types = VehicleType.objects.all()
    for type in vehicle_types:
        vehicle_type_data.append({"value": type.id, "name": type.name})
        TEMPLATE_SHEET_PARKING_FEE_TICKET_PAYMENT['HEADER'].append((type.name,))
        TEMPLATE_SHEET_PARKING_FEE_TICKET_PAYMENT['HEADER'].append(('',))

        TEMPLATE_SHEET_PARKING_FEE_TICKET_PAYMENT['HEADER2'].append((u'Số lượng',))
        TEMPLATE_SHEET_PARKING_FEE_TICKET_PAYMENT['HEADER2'].append((u'Thành tiền',))

    TEMPLATE_SHEET_PARKING_FEE_TICKET_PAYMENT['HEADER2'].append((u'Tổng cộng',))

    now = datetime.now()
    from_time = datetime(now.year, now.month, now.day).replace(day=1)  # TG bat dau: Dau thang hien tai

    month = now.month

    year = now.year + month / 12

    try:
        month = (month + 1) % 12
        if month == 0: month = 12
    except:
        pass
    to_time = from_time.replace(year=year, month=month, day=1, hour=23) + timedelta(
        days=-1)  # TG ket thuc: Cuoi thang hien tai

    if 'btn_REPORT' in request.POST:
        is_protected_report = False if 'unprotected_report' in request.POST else True

        current_user = request.user
        if not is_protected_report and not current_user.has_perm('parking.export_unprotected_excel'):
            messages.error(request, u'Tài khoản hiện tại không có quyền này!', fail_silently=True)
            return redirect(reverse('render_report_parking_fee_ticket_payment'))

        from_time = request.POST['from_time']
        to_time = request.POST['to_time']

        TEMPLATE_SHEET_PARKING_FEE_TICKET_PAYMENT['TITLE'][1].append(from_time)
        TEMPLATE_SHEET_PARKING_FEE_TICKET_PAYMENT['TITLE'][2].append(to_time)

        rs1 = TicketPaymentDetail.objects.all().select_related('ticket_payment__payment_date',
                                                               'vehicle_registration__vehicle_type_id',
                                                               'ticket_payment__payment_method',
                                                               )  # Cac chi tiet thanh toan
        rs2 = ParkingFeeSession.objects.all()  # Cac luot thu phi xe vang lai
        rs3 = DepositPaymentDetail.objects.all().select_related('deposit_payment__payment_date',
                                                                'vehicle_registration__vehicle_type_id',
                                                                )

        try:
            _from_time = datetime.strptime(from_time, "%d/%m/%Y").replace(tzinfo=utc)  # Datetime
            _to_time = datetime.strptime(to_time, "%d/%m/%Y").replace(hour=23, minute=59, second=59, tzinfo=utc)

            rs1 = rs1.filter(ticket_payment__payment_date__lte=_to_time, ticket_payment__payment_date__gte=_from_time)
            rs2 = rs2.filter(payment_date__lte=_to_time, payment_date__gte=_from_time)
            rs3 = rs3.filter(deposit_payment__payment_date__lte=_to_time, deposit_payment__payment_date__gte=_from_time)
        except:
            messages.error(request, u'Khoảng thời gian không phù hợp!', fail_silently=True)
            return HttpResponseRedirect('')

        tong_ve_thang_tien_mat = tong_ve_thang_chuyen_khoan = tong_vang_lai = tong_coc_the = 0.0
        for type in vehicle_types:
            vehicle_type_id = type.id  # Loai xe
            ve_thang = rs1[:]
            vang_lai = rs2[:]
            coc_the = rs3[:]

            # if vehicle_type_id != 100000000:
            ve_thang = rs1.filter(vehicle_registration__vehicle_type_id=vehicle_type_id)
            vang_lai = rs2.filter(vehicle_type_id=vehicle_type_id)
            coc_the = rs3.filter(vehicle_registration__vehicle_type_id=vehicle_type_id)

            ve_thang_tien_mat = ve_thang.filter(ticket_payment__payment_method=u'TM')
            ve_thang_chuyen_khoan = ve_thang.filter(ticket_payment__payment_method=u'CK')
            ve_thang_tien_mat_so_luong = ve_thang_tien_mat.count()
            ve_thang_tien_mat_thanh_tien = ve_thang_tien_mat.aggregate(Sum('payment_detail_fee'))[
                                               'payment_detail_fee__sum'] or 0
            ve_thang_chuyen_khoan_so_luong = ve_thang_chuyen_khoan.count()
            ve_thang_chuyen_khoan_thanh_tien = ve_thang_chuyen_khoan.aggregate(Sum('payment_detail_fee'))[
                                                   'payment_detail_fee__sum'] or 0
            vang_lai_so_luong = vang_lai.count()
            vang_lai_thanh_tien = vang_lai.aggregate(Sum('parking_fee'))['parking_fee__sum'] or 0
            coc_the_so_luong = coc_the.count()
            coc_the_thanh_tien = coc_the.aggregate(Sum('deposit_payment_detail_fee'))[
                                     'deposit_payment_detail_fee__sum'] or 0

            TEMPLATE_SHEET_PARKING_FEE_TICKET_PAYMENT['HEADER3'].append((ve_thang_tien_mat_so_luong,))
            TEMPLATE_SHEET_PARKING_FEE_TICKET_PAYMENT['HEADER3'].append((ve_thang_tien_mat_thanh_tien,))
            TEMPLATE_SHEET_PARKING_FEE_TICKET_PAYMENT['HEADER4'].append((ve_thang_chuyen_khoan_so_luong,))
            TEMPLATE_SHEET_PARKING_FEE_TICKET_PAYMENT['HEADER4'].append((ve_thang_chuyen_khoan_thanh_tien,))
            TEMPLATE_SHEET_PARKING_FEE_TICKET_PAYMENT['HEADER5'].append((vang_lai_so_luong,))
            TEMPLATE_SHEET_PARKING_FEE_TICKET_PAYMENT['HEADER5'].append((vang_lai_thanh_tien,))
            TEMPLATE_SHEET_PARKING_FEE_TICKET_PAYMENT['HEADER6'].append((coc_the_so_luong,))
            TEMPLATE_SHEET_PARKING_FEE_TICKET_PAYMENT['HEADER6'].append((coc_the_thanh_tien,))

            tong_ve_thang_tien_mat += ve_thang_tien_mat_thanh_tien
            tong_ve_thang_chuyen_khoan += ve_thang_chuyen_khoan_thanh_tien
            tong_vang_lai += vang_lai_thanh_tien
            tong_coc_the += coc_the_thanh_tien

        TEMPLATE_SHEET_PARKING_FEE_TICKET_PAYMENT['HEADER3'].append((tong_ve_thang_tien_mat,))
        TEMPLATE_SHEET_PARKING_FEE_TICKET_PAYMENT['HEADER4'].append((tong_ve_thang_chuyen_khoan,))
        TEMPLATE_SHEET_PARKING_FEE_TICKET_PAYMENT['HEADER5'].append((tong_vang_lai,))
        TEMPLATE_SHEET_PARKING_FEE_TICKET_PAYMENT['HEADER6'].append((tong_coc_the,))

        TEMPLATE_SHEET_PARKING_FEE_TICKET_PAYMENT['HEADER7'].append((tong_ve_thang_tien_mat +
                                                                     tong_ve_thang_chuyen_khoan +
                                                                     tong_vang_lai +
                                                                     tong_coc_the,))
        folder_name = 'templates/report'
        file_name = 'GPMS_BC_ThuPhiDauxe.xlsx'
        file_path = '%s/%s' % (folder_name, file_name)

        if not os.path.exists(folder_name):
            os.mkdir(folder_name)

        if os.path.isfile(file_path):
            os.remove(file_path)

        workbook = Workbook(file_path, {'constant_memory': True})
        add_worksheet_stat('Tien thu phi dau xe', workbook, TEMPLATE_SHEET_PARKING_FEE_TICKET_PAYMENT, None, {},
                           is_protected_report)
        workbook.close()

        with open(file_path, 'r') as f:
            response = HttpResponse(f.read(), content_type="application/vnd.ms-excel")
            response['Content-Disposition'] = "attachment; filename=%s" % file_name
            return response

    return render(request, 'admin/rp-parkingfee-ticketpayment.html', {'vehicle_type_data': vehicle_type_data,
                                                                      'from_time': from_time.strftime("%d/%m/%Y"),
                                                                      'to_time': to_time.strftime("%d/%m/%Y"),
                                                                      })


@login_required(redirect_field_name='', login_url='/admin/')
def render_report_order_info(request):
    userid = request.user.id
    if notallowreport(userid):
        return render(request, 'admin/nopermission.html')
    if 'btn_REPORT' in request.POST:
        is_protected_report = False if 'unprotected_report' in request.POST else True

        current_user = request.user
        if not is_protected_report and not current_user.has_perm('parking.export_unprotected_excel'):
            messages.error(request, u'Tài khoản hiện tại không có quyền này!', fail_silently=True)
            return redirect(reverse('render_report_parking_fee_ticket_payment'))

        from_time = request.POST['from_time']
        to_time = request.POST['to_time']

        receipts = None
        try:
            _from_time = datetime.strptime(from_time, "%d/%m/%Y")  # Datetime
            _to_time = datetime.strptime(to_time, "%d/%m/%Y").replace(hour=23, minute=59, second=59)
            receipts = Receipt.objects.filter(cancel=False, type=0, action_date__lte=_to_time.replace(tzinfo=utc),
                                              action_date__gte=_from_time.replace(tzinfo=utc))
        except Exception as e:
            print e.message
            messages.error(request, u'Khoảng thời gian không phù hợp!', fail_silently=True)
            return HttpResponseRedirect('')

        if receipts:
            folder_name1 = 'templates/report'
            file_name1 = 'templates/GPMS_Mau_BC_Xuat_hoa_don.xls'
            file_name2 = 'GPMS_BC_Xuat_hoa_don.xls'
            file_path1 = '%s/%s' % (folder_name1, file_name1)
            file_path2 = '%s/%s' % (folder_name1, file_name2)

            from xlrd import open_workbook
            from xlwt import XFStyle, Borders
            rb = open_workbook(file_path1, formatting_info=True, on_demand=True)

            number_format = XFStyle()
            number_format.num_format_str = "#,###0"
            borders = Borders()
            borders.top = Borders.THIN
            borders.right = Borders.THIN
            borders.bottom = Borders.THIN
            borders.left = Borders.THIN
            number_format.borders = borders

            from xlutils.copy import copy
            wb = copy(rb)
            sheet1 = wb.get_sheet(0)

            sheet1.write(2, 4, from_time)
            sheet1.write(2, 6, to_time)

            value_list = ('receipt_number', 'action_date', 'type', 'ref_id')
            rs = receipts.values(*value_list)

            from json import loads
            order_report_template = get_setting('order_report_template', u'Mẫu nội dung xuất hoá đơn',
                                                u'{"ve_thang": "Công ty KV thu hộ phí giữ xe từ {valid_from} - {'
                                                u'valid_to}", "coc_the": "Thu hộ phí cọc thẻ"}',
                                                'Các truờng thông tin có thể sử dụng (đóng trong cặp dấu {}): {'
                                                'valid_from}, {valid_to} ')
            try:
                order_report_template = loads(order_report_template)
            except:
                pass

            stt = 0
            for row in rs:
                receipt_number = row['receipt_number']
                action_date = row['action_date'].strftime('%d/%m/%Y')
                ref_id = row['ref_id']
                payment = None
                fee = 0.0
                count_oto = count_xemay = 0
                detail = u''
                content = u''

                if row['type'] == 0 and ref_id:
                    valid_from = None
                    payment = TicketPayment.objects.filter(id=ref_id).select_related('customer__customer_name',
                                                                                     'customer__aparment',
                                                                                     'customer__building',
                                                                                     'customer__company',
                                                                                     'customer__order_register_name',
                                                                                     'customer__order_register_address',
                                                                                     'customer__order_tax_code',
                                                                                     )
                    if payment:
                        payment = payment[0]
                        payment_details = TicketPaymentDetail.objects.filter(ticket_payment=payment).select_related(
                            'vehicle_registration__vehicle_type')

                        for payment_detail in payment_details:
                            if valid_from is None and payment_detail.effective_date and payment_detail.expired_date:
                                if order_report_template:
                                    content = order_report_template['ve_thang'].format(
                                        valid_from=payment_detail.effective_date.strftime("%d/%m/%Y"),
                                        valid_to=payment_detail.expired_date.strftime("%d/%m/%Y"))
                                else:
                                    content = u'Thu hộ phí giữ xe từ %s đến %s' % (
                                        payment_detail.effective_date.strftime("%d/%m/%Y"),
                                        payment_detail.expired_date.strftime("%d/%m/%Y"))

                        fee = payment.payment_fee
                        count_oto = payment_details.filter(payment_detail_fee__gt=0,
                                                           vehicle_registration__vehicle_type__name=u'Ô tô').count()
                        count_xemay = payment_details.filter(payment_detail_fee__gt=0).count() - count_oto
                        detail = "OT: {0} xe\nXM: {1} xe".format(count_oto, count_xemay)
                elif row['type'] == 1 and ref_id:
                    payment = DepositPayment.objects.filter(id=ref_id)
                    if payment:
                        payment = payment[0]
                        payment_details = DepositPaymentDetail.objects.filter(deposit_payment=payment).select_related(
                            'vehicle_registration__vehicle_type')
                        fee = payment.payment_fee
                        count_oto = payment_details.filter(deposit_payment_detail_fee__gt=0,
                                                           vehicle_registration__vehicle_type__name=u'Ô tô').count()
                        count_xemay = payment_details.filter(deposit_payment_detail_fee__gt=0).count() - count_oto
                        detail = "OT: {0} xe.\nXM: {1} xe".format(count_oto, count_xemay)
                        if order_report_template:
                            content = order_report_template['coc_the']
                        else:
                            content = u'Thu hộ phí cọc xe'

                if payment:
                    customer = payment.customer
                    customer_name = customer.customer_name
                    address = []

                    if len(customer.order_register_name) > 0 or len(customer.order_register_address) > 0 or len(
                            customer.order_tax_code) > 0:
                        if customer.building:
                            address.append(u"Toà nhà %s" % customer.building.name)
                        if customer.apartment:
                            address.append(u"Căn hộ %s" % customer.apartment.address)
                        if customer.company:
                            address.append(u"Công ty %s" % customer.company.name)

                        address = u" - ".join(address)

                        order_register_name = order_register_address = order_tax_code = u''
                        if customer.order_register_name:
                            order_register_name = customer.order_register_name
                        if customer.order_register_address:
                            order_register_address = customer.order_register_address
                        if customer.order_tax_code:
                            order_tax_code = customer.order_tax_code

                        stt += 1
                        temp_data = [receipt_number, action_date, order_register_name, order_tax_code,
                                     order_register_address,
                                     content, detail, fee, stt, customer_name, address]

                        for i in xrange(len(temp_data)):
                            sheet1.write(4 + stt, i, temp_data[i], number_format)

            if is_protected_report:
                sheet1.protect = True
                sheet1.password = 'ndhoang'

            if not os.path.exists(folder_name1):
                os.mkdir(folder_name1)
            if os.path.isfile(file_path2):
                os.remove(file_path2)

            wb.save(file_path2)

            with open(file_path2, 'r') as f:
                response = HttpResponse(f.read(), content_type="application/vnd.ms-excel")
                response['Content-Disposition'] = "attachment; filename=%s" % file_name2
                return response
        # except:
        #     return render(request, 'admin/rp-order-info.html', {'from_time': from_time,
        #                                                         'to_time': to_time,
        #                                                         })
        else:
            messages.warning(request, u"Không có dữ liệu phù hợp", fail_silently=True)
            return render(request, 'admin/rp-order-info.html', {'from_time': from_time,
                                                                'to_time': to_time
                                                                })

    now = datetime.now()
    from_time = datetime(now.year, now.month, now.day).replace(day=1)  # TG bat dau: Dau thang hien tai

    month = now.month
    year = now.year + month / 12

    try:
        month = (month + 1) % 12
        if month == 0: month = 12
    except:
        pass
    to_time = from_time.replace(year=year, month=month, day=1, hour=23) + timedelta(
        days=-1)  # TG ket thuc: Cuoi thang hien tai

    return render(request, 'admin/rp-order-info.html', {'from_time': from_time.strftime("%d/%m/%Y"),
                                                        'to_time': to_time.strftime("%d/%m/%Y"),
                                                        })


def get_card_status_name(status):
    try:
        from common import CARD_STATUS
        return CARD_STATUS[status][1]
    except:
        return status

def get_card_status(status):
    try:
        from common import card
        return card[status][1]
    except:
        return status

def get_card_type_name(card_type):
    try:
        from models import CardType
        return CardType.objects.get(id=card_type).name
    except:
        return card_type


def get_action_user_name(action_user_id):
    try:
        from models import UserProfile
        return UserProfile.objects.get(user_id=action_user_id).fullname
    except:
        return action_user_id

def get_vehicle_driver_name(card_id):
    vehicle_driver_name = ""
    from models import VehicleRegistration
    query = VehicleRegistration.objects.filter(card__id=card_id, status=1).order_by('-card')
    if len(query) > 0:
        return query[0].vehicle_driver_name
    else:
        return vehicle_driver_name

def get_customer_name(card_id):
    customer_id = ""
    from models import VehicleRegistration
    query = VehicleRegistration.objects.filter(card__id=card_id, status=1).order_by('-card')
    if len(query) > 0:
        return query[0].customer_id
    else:
        return customer_id

# def get_customer_name(customer_id):
#     customer_name = ""
#     from models import Customer
#     query = Customer.objects.filter(id=customer_id)
#     if len(query) > 0:
#         return query[0].customer_name
#     else:
#         return customer_name

@login_required(redirect_field_name='', login_url='/admin/')
def render_report_card_status_change(request):
    userid = request.user.id
    if notallowreport(userid):
        return render(request, 'admin/nopermission.html')
    now = datetime.now()
    from_time = datetime(now.year, now.month, now.day).replace(day=1)  # TG bat dau: Dau thang hien tai

    month = now.month
    year = now.year + month / 12

    try:
        month = (month + 1) % 12
        if month == 0: month = 12
    except:
        pass
    to_time = from_time.replace(year=year, month=month, day=1, hour=23) + timedelta(days=-1)  # TG ket thuc: Cuoi thang hien tai

    if 'btn_REPORT' in request.POST:
        from_time = request.POST['from_time']
        to_time = request.POST['to_time']

        try:
            _from_time = datetime.strptime(from_time, "%d/%m/%Y")  # Datetime
            _to_time = datetime.strptime(to_time, "%d/%m/%Y").replace(hour=23, minute=59, second=59)

        except Exception as e:
            print e.message
            messages.error(request, u'Khoảng thời gian không phù hợp!', fail_silently=True)
            return HttpResponseRedirect('')

        new_data = Card.audit_log.all().filter(status=2, action_type='U', action_date__gte=_from_time, action_date__lte=_to_time)  # Loc nhung the bi khoa

        TEMPLATE_SHEET = {
            'HEADER': (
                (0,),
                (u'STT',),
                (u'Tên thẻ', {'db_field': 'card_label', 'format': {'width': 30}}),
                (u'Loại thẻ', {'db_field': 'card_type', 'custom_function': get_card_type_name}),
                (u'Trạng thái', {'db_field': 'status', 'custom_function': get_card_status_name}),
                (u'Ngày cập nhật', {'db_field': 'action_date', 'format': {'date': ''}}),
                (u'Nguời thực hiện',{'db_field': 'action_user_id', 'custom_function': get_action_user_name, 'format': {'width': 30}}),
                (u'Ghi chú', {'db_field': 'note', 'format': {'width': 50}}),

            ),

            'TITLE': (
                (2, 5, u'BÁO CÁO KHOÁ THẺ'),
                [3, 5, u'Ngày lập'],
            ),

            'STAT': []
        }

        folder_name = 'templates/report'
        file_name = 'GPMS_BC_KhoaThe.xlsx'
        file_path = '%s/%s' % (folder_name, file_name)

        if not os.path.exists(folder_name):
            os.mkdir(folder_name)

        if os.path.isfile(file_path):
            os.remove(file_path)

        workbook = Workbook(file_path, {'constant_memory': True})

        TEMPLATE_SHEET['TITLE'][1].append(datetime.today().strftime("%d/%m/%Y %H:%M"))
        add_worksheet_new('Khoa the', workbook, TEMPLATE_SHEET, new_data, False)

        workbook.close()
        with open(file_path, 'r') as f:
            response = HttpResponse(f.read(), content_type="application/vnd.ms-excel")
            response['Content-Disposition'] = "attachment; filename=%s" % file_name
            return response

    return render(request, 'admin/rp-cardstatus.html', {'from_time': from_time.strftime("%d/%m/%Y"),
                                                            'to_time': to_time.strftime("%d/%m/%Y")})


# @login_required(redirect_field_name='', login_url='/admin/')
def render_report_card(request):
    userid = request.user.id
    if notallowreport(userid):
        return render(request, 'admin/nopermission.html')
    if 'btn_REPORT' in request.POST:
        new_data = Card.objects.filter(vehicle_type = 100000000)
        #
        TEMPLATE_SHEET = {
            'HEADER': (
                (0,),
                (u'STT',),
                (u'Tên thẻ', {'db_field': 'card_label', 'format': {'width': 30}}),
                (u'Loại thẻ', {'db_field': 'card_type', 'custom_function': get_card_type_name}),
                (u'Tên chủ phương tiện', {'db_field': 'id', 'custom_function': get_vehicle_driver_name , 'format': {'width': 30}}),
                # (u'Công ty', {'db_field': 'id', 'custom_function': get_customer_name , 'format': {'width': 30}}),
                (u'Tên công ty', {'db_field': 'vehicleregistration__customer__customer_name', 'format': {'width': 30}}),
                (u'Trạng thái', {'db_field': 'status', 'custom_function': get_card_status}),

            ),

            'TITLE': (
                (2, 3, u'BÁO CÁO DANH SÁCH THẺ'),
                [3, 3, u'Ngày lập'],
            ),

            'STAT': []
        }

        folder_name = 'templates/report'
        file_name = 'Report-Card.xlsx'
        file_path = '%s/%s' % (folder_name, file_name)

        if not os.path.exists(folder_name):
            os.mkdir(folder_name)

        if os.path.isfile(file_path):
            os.remove(file_path)

        workbook = Workbook(file_path, {'constant_memory': True})

        TEMPLATE_SHEET['TITLE'][1].append(datetime.today().strftime("%d/%m/%Y %H:%M"))
        add_worksheet_new_card_status('BÁO CÁO DANH SÁCH THẺ', workbook, TEMPLATE_SHEET, new_data, False)

        workbook.close()
        with open(file_path, 'r') as f:
            response = HttpResponse(f.read(), content_type="application/vnd.ms-excel")
            response['Content-Disposition'] = "attachment; filename=%s" % file_name
            return response
    return render(request, 'admin/rp-Card.html')

def add_pdfdata_new_admin_log(data, TEMPLATE, queryset, rowHeights):  # Them sheet vao workbook
    HEADER = TEMPLATE['HEADER']
    tmpdata=[]
    pdfmetrics.registerFont(TTFont('Arial', 'Arial.ttf'))
    styleN = ParagraphStyle(
        name='Normal',
        fontName='Arial',
        fontSize=7,
    )
    styleT = ParagraphStyle(
        name='Normal',
        fontName='Arial',
        fontSize=7,
        textColor='green'
    )
    for i, column in enumerate(HEADER):  # Viet table header
        tmpdata.append(Paragraph(HEADER[i][0], styleT))
    data.append(tmpdata)
    rowHeights.append(40)
    # Write data
    fields = [f[1]['db_field'] for f in HEADER if len(f) > 1 and 'db_field' in f[1]]
    values = (queryset.values(*fields))  # Dict values list

    for stt, raw_row_data in enumerate(values):  # Prepare data
        # temp_row = []
        if raw_row_data['action_flag'] == 1:
            raw_row_data['action_flag'] = 'Tạo mới '
        elif raw_row_data['action_flag'] == 2:
            raw_row_data['action_flag'] = 'Cập nhật'
        else:
            raw_row_data['action_flag'] = 'Xóa'
        tmpdata = []

        for col, column in enumerate(HEADER):
            item = ''
            # column_order = column[1]
            if column[0] == u'STT':
                item = stt + 1
                tmpdata.append(item)
            elif len(column) > 1 and 'db_field' in column[1]:
                db_field = column[1]['db_field']
                item = raw_row_data[db_field]

                if item is not None:

                    if 'custom_function' in column[1]:
                        item = column[1]['custom_function'](item)


                    # Check format
                    wrote = False
                    if 'format' in column[1]:
                        if 'date' in column[1]['format']:
                            item = item.strftime("%d/%m/%Y")
                        if 'text_wrap' in column[1]['format']:
                            tmpdata.append(Paragraph(item, styleN))
                            wrote = True
                    if not wrote:
                        if len(item)>1000:
                            tmpdata.append(Paragraph(item[0:1000]+'...', styleN))
                        else:
                            tmpdata.append(Paragraph(item, styleN))
                else:
                    tmpdata.append('')
            else:
                tmpdata.append('')
        data.append(tmpdata)
        rowHeights.append(30)

class TransactionPDF(object):
    """"""

    # ----------------------------------------------------------------------
    def __init__(self):
        """Constructor"""
        self.width, self.height = letter
        self.styles = getSampleStyleSheet()
    # ----------------------------------------------------------------------
    # def coord(self, x, y, unit=1):
    #     """
    #     http://stackoverflow.com/questions/4726011/wrap-text-in-a-table-reportlab
    #     Helper class to help position flowables in Canvas objects
    #     """
    #     x, y = x * unit, self.height - y * unit
    #     return x, y
    #
    # # ----------------------------------------------------------------------
    def run(self, data):
        """
        Run the report
        """
        response = HttpResponse(content_type='application/pdf')
        response['Content-Disposition'] = 'inline; filename="Transactionhistory.pdf"'
        self.doc = SimpleDocTemplate(response,pagesize=A4,rightMargin=30, leftMargin=30, topMargin=30, bottomMargin=30)
        self.story = [Spacer(1, 1)]
        self.createLineItems(data)

        self.doc.build(self.story, onFirstPage=self.addPageNumber,onLaterPages=self.addPageNumber)
        return response
        #print "finished!"

    # ----------------------------------------------------------------------
    def addPageNumber(self, canvas, doc):
        """
        Create the document
        """
        page_num = canvas.getPageNumber()
        text = "Trang %s" % page_num
        canvas.drawRightString(200 * mm, 5 * mm, text)

    # ----------------------------------------------------------------------
    def createLineItems(self,data):
        """
        Create the line items
        """
        colwidths = (30, 60, 60, 80, 60, 100, 60, None)
        pdfmetrics.registerFont(TTFont('Arial', 'Arial.ttf'))
        GRID_STYLE = TableStyle([
            ('SPAN', (0, 0), (0, 2)),
            ('SPAN', (1, 0), (7, 0)),
            ('SPAN', (2, 0), (7, 0)),
            ('FONTSIZE', (0, 0), (1, 2), 12),
            ('INNERGRID', (0, 4), (-1, -1), 0.05, colors.green),
            ('BOX', (0, 3), (-1, -1), 0.05, colors.black),
            ('TEXTCOLOR', (0, 3), (7, 3), colors.green),
            ('FONTSIZE', (0, 3), (7, 3), 7),
            ('FONTSIZE', (0, 4), (6, -1), 7),
            ('FONTSIZE', (7, 4), (7, -1), 7),
            ('FONTNAME', (0, 0), (-1, -1), 'Arial'),
            ('ALIGN', (0, 0), (-1, -1), 'LEFT'),
            ('VALIGN', (0, 0), (-1, -1), 'MIDDLE'),
        ])
        table = Table(data,colWidths=colwidths,rowHeights=None,style=GRID_STYLE)
        self.story.append(table)

def render_report_Transaction_history(request):
    userid = request.user.id
    if notallowreport(userid):
        return render(request, 'admin/nopermission.html')

    now = datetime.now()
    from_time = datetime(now.year, now.month, now.day).replace(day=1)  # TG bat dau: Dau thang hien tai

    month = now.month
    year = now.year + month / 12

    try:
        month = (month + 1) % 12
        if month == 0: month = 12
    except:
        pass
    to_time = from_time.replace(year=year, month=month, day=1, hour=23) + timedelta(days=-1)  # TG ket thuc: Cuoi thang hien tai

    if 'btn_REPORT' in request.POST:
        PDF_report = True if 'PDF_report' in request.POST else False
        from_time = request.POST['from_time']
        to_time = request.POST['to_time']

        try:
            _from_time = datetime.strptime(from_time, "%d/%m/%Y")  # Datetime
            _to_time = datetime.strptime(to_time, "%d/%m/%Y").replace(hour=23, minute=59, second=59)
        except Exception as e:
            print e.message
            messages.error(request, u'Khoảng thời gian không phù hợp!', fail_silently=True)
            return HttpResponseRedirect('')
        newdata = admin_log.objects.filter(action_time__gte=_from_time, action_time__lte=_to_time)
        ##pdf
        if PDF_report:
            # return HttpResponseRedirect('')
            TEMPLATE_SHEET = {
                'HEADER': (
                    (u'STT',),
                    (u'Ngày cập nhật', {'db_field': 'action_time', 'format': {'date': ''}}),
                    (u'Mã nhân viên', {'db_field': 'user__userprofile__staff_id', 'format': {'width': 10}}),
                    (u'Tài Khoản thực hiện', {'db_field': 'user__userprofile__fullname', 'format': {'width': 30}}),
                    (u'Ngày sinh', {'db_field': 'user__userprofile__birthday', 'format': {'date': ''}}),
                    (u'Mục Tiêu Điều Chỉnh', {'db_field': 'object_repr', 'format': {'width': 30}}),
                    (u'Thao Tác', {'db_field': 'action_flag', 'format': {'width': 10}}),
                    (u'Nội Dung', {'db_field': 'change_message', 'format': {'width': 200}}),

                )
            }
            try:
                a = Image("parking/static/image/logo_report.png", 0.75 * inch, 0.5 * inch)
            except Exception as e:
                a = ''
            data = [[a,u'        REPORT TRANSACTION HISTORY    ngày lập: %s' % (datetime.today().strftime("%d/%m/%Y %H:%M:%S")),'','','','','',''],
                    ['',u'        Từ ngày: %s đến ngày: %s' % (_from_time.strftime("%d/%m/%Y %H:%M:%S"),_to_time.strftime("%d/%m/%Y %H:%M:%S")),'','','','','',''],
                    ['','','','','','','','']]

            rowHeights = []
            add_pdfdata_new_admin_log(data, TEMPLATE_SHEET, newdata, rowHeights)
            pdf = TransactionPDF()
            return pdf.run(data)

        ##End PDF
        else:##Excel
            TEMPLATE_SHEET = {
                'HEADER': (
                    (0,),
                    (u'STT',),
                    (u'Ngày cập nhật', {'db_field': 'action_time', 'format': {'date': ''}}),
                    (u'Mã nhân viên', {'db_field': 'user__userprofile__staff_id', 'format': {'width': 10}}),
                    (u'Tài Khoản thực hiện', {'db_field': 'user__userprofile__fullname', 'format': {'width': 30}}),
                    (u'Ngày sinh', {'db_field': 'user__userprofile__birthday', 'format': {'date': ''}}),
                    (u'Mục Tiêu Điều Chỉnh', {'db_field': 'object_repr', 'format': {'width': 30}}),
                    (u'Thao Tác', {'db_field': 'action_flag', 'format': {'width': 10}}),
                    (u'Nội Dung', {'db_field': 'change_message', 'format': {'width': 200}}),

                ),

                'TITLE': (
                    (2, 5, u'REPORT TRANSACTION HISTORY'),
                    [3, 5, u'Ngày lập'],
                ),

                'STAT': []
            }
            folder_name = 'templates/report'
            file_name = 'Transactionhistory.xlsx'
            file_path = '%s/%s' % (folder_name, file_name)

            if not os.path.exists(folder_name):
                os.mkdir(folder_name)

            if os.path.isfile(file_path):
                os.remove(file_path)

            workbook = Workbook(file_path, {'constant_memory': True})

            TEMPLATE_SHEET['TITLE'][1].append(datetime.today().strftime("%d/%m/%Y %H:%M"))
            add_worksheet_new_admin_log('TRANSACTION', workbook, TEMPLATE_SHEET, newdata, True)

            workbook.close()
            with open(file_path, 'r') as f:
                response = HttpResponse(f.read(), content_type="application/vnd.ms-excel")
                response['Content-Disposition'] = "attachment; filename=%s" % file_name
                return response
    return render(request, 'admin/rp_transactionhistory.html', {'from_time': from_time.strftime("%d/%m/%Y"),
                                                            'to_time': to_time.strftime("%d/%m/%Y")})
##report 2018July11
def add_worksheet_configfee_action(sheet_name, workbook, TEMPLATE, data, sheet_protect=True):  # Them sheet vao workbook
    # LOGO_EXCEL_REPORT_PATH = 'parking/static/image/logo_report.png'
    LOGO_EXCEL_REPORT_PATH = 'parking/static/image/' + get_setting('logo_report',
                                                                   u'Mẫu logo dùng trong báo cáo',
                                                                   'logo_report.png')
    LOGO_EXCEL_REPORT_PATH_RIGHT = 'parking/static/image/' + get_setting('logo_repor',
                                                                         u'Mẫu logo dùng trong báo cáo',
                                                                         'logo_report_right.png')
    TITLE = TEMPLATE['TITLE']
    HEADER = TEMPLATE['HEADER']
    STAT = TEMPLATE['STAT']

    sheet = workbook.add_worksheet(sheet_name)
    sheet.insert_image(0, 0, LOGO_EXCEL_REPORT_PATH)
    sheet.insert_image(0, 15, LOGO_EXCEL_REPORT_PATH_RIGHT)

    bold = workbook.add_format({'bold': True})
    border = workbook.add_format()
    border.set_border()
    bold_boder = workbook.add_format({'bold': True, 'border': 1})
    text_wrap = workbook.add_format({'border': 1})
    text_wrap.set_text_wrap()

    for r in TITLE:  # Viet tieu de: (dong r[0], cot r[1])

        if len(r) == 4:
            sheet.merge_range(r[0], r[1], r[0], r[1] + 2, "%s: %s"%(r[2],r[3]), bold)
        else:
            sheet.merge_range(r[0], r[1], r[0], r[1] + 2, r[2], bold)

    for r in STAT:  # Viet dong thong ke theo loai xe: (dong r[0], cot r[1])
        sheet.write(r[0], r[1], r[2], bold)
        sum_r = queryset.filter(vehicle_type_id=r[3]).count()
        sheet.write(r[0], r[1] + 1, sum_r)

    for i, column in enumerate(HEADER):  # Viet table header (dong 5)
        sheet.write(5, i - 1, HEADER[i][0], bold_boder)
        sheet.set_column(i - 1, i - 1, 15)  # Default width
        if len(column) > 1 and 'format' in column[1]:
            if 'width' in column[1]['format']:
                width = column[1]['format']['width']
                sheet.set_column(i - 1, i - 1, width)

    # Write data

    start_row = 6

    border_format = workbook.add_format({'border': 1})
    date_format = workbook.add_format({'border': 1, 'num_format': 'dd/mm/yyyy'})
    stt=1
    for d in data:  # Prepare data
        try:
            currdata=[stt,d[0],d[1],d[2],d[3],d[4]]
            sheet.write_row(start_row+stt-1,0,currdata,border_format)#
            stt=stt+1
        except Exception as e:
            a=1
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
def add_pdfdata_fee(data, TEMPLATE, newdata, rowHeights):  # Them sheet vao workbook
    HEADER = TEMPLATE['HEADER']
    tmpdata=[]
    pdfmetrics.registerFont(TTFont('Arial', 'Arial.ttf'))
    styleN = ParagraphStyle(
        name='Normal',
        fontName='Arial',
        fontSize=7,
    )
    styleT = ParagraphStyle(
        name='Normal',
        fontName='Arial',
        fontSize=7,
        textColor='green'
    )
    for i, column in enumerate(HEADER):  # Viet table header
        tmpdata.append(Paragraph(HEADER[i][0], styleT))
    data.append(tmpdata)
    rowHeights.append(40)
    # Write data
    stt = 1
    for d in newdata:  # Prepare data
        try:
            tmpdata = []
            tmpdata.append(stt)
            tmpdata.append(Paragraph(d[0], styleN))
            tmpdata.append(Paragraph(d[1], styleN))
            tmpdata.append(Paragraph(d[2], styleN))
            tmpdata.append(Paragraph(d[3], styleN))
            tmpdata.append(Paragraph(d[4], styleN))
            data.append(tmpdata)
            rowHeights.append(30)
            stt=stt+1
        except Exception as e:
            a=1
class TransactionFeePDF(object):
    """"""

    # ----------------------------------------------------------------------
    def __init__(self):
        """Constructor"""
        self.width, self.height = letter
        self.styles = getSampleStyleSheet()
    # ----------------------------------------------------------------------
    # def coord(self, x, y, unit=1):
    #     """
    #     http://stackoverflow.com/questions/4726011/wrap-text-in-a-table-reportlab
    #     Helper class to help position flowables in Canvas objects
    #     """
    #     x, y = x * unit, self.height - y * unit
    #     return x, y
    #
    # # ----------------------------------------------------------------------
    def run(self, data):
        """
        Run the report
        """

        response = HttpResponse(content_type='application/pdf')
        response['Content-Disposition'] = 'inline; filename="TransactionhistoryFee.pdf"'
        self.doc = SimpleDocTemplate(response, pagesize=A4, rightMargin=30, leftMargin=30, topMargin=30,
                                     bottomMargin=30, unicode="utf-8")
        self.story = [Spacer(1, 1)]
        self.createLineItems(data)
        self.doc.build(self.story, onFirstPage=self.addPageNumber, onLaterPages=self.addPageNumber)
        return response
        #print "finished!"

    # ----------------------------------------------------------------------
    def addPageNumber(self, canvas, doc):
        """
        Create the document
        """
        page_num = canvas.getPageNumber()
        text = "Trang %s" % page_num
        canvas.drawRightString(200 * mm, 5 * mm, text)

    # ----------------------------------------------------------------------
    def createLineItems(self,data):
        """
        Create the line items
        """
        colwidths = (30, 80, 80, 100, 60, None)
        pdfmetrics.registerFont(TTFont('Arial', 'Arial.ttf'))
        GRID_STYLE = TableStyle([
            ('SPAN', (0, 0), (0, 2)),
            ('SPAN', (1, 0), (5, 0)),
            ('SPAN', (2, 0), (5, 0)),
            ('FONTSIZE', (0, 0), (1, 2), 12),
            ('INNERGRID', (0, 4), (-1, -1), 0.05, colors.green),
            ('BOX', (0, 3), (-1, -1), 0.05, colors.black),
            ('TEXTCOLOR', (0, 3), (5, 3), colors.green),
            ('FONTSIZE', (0, 3), (5, 3), 7),
            ('FONTSIZE', (0, 4), (4, -1), 7),
            ('FONTSIZE', (5, 4), (5, -1), 7),
            ('FONTNAME', (0, 0), (-1, -1), 'Arial'),
            ('ALIGN', (0, 0), (-1, -1), 'LEFT'),
            ('VALIGN', (0, 0), (-1, -1), 'MIDDLE'),
        ])
        table = Table(data,colWidths=colwidths,rowHeights=None,style=GRID_STYLE)
        self.story.append(table)
@login_required(redirect_field_name='', login_url='/admin/')
def render_report_Configfee_history(request):
    userid = request.user.id
    if notallowreport(userid):
        return render(request, 'admin/nopermission.html')
    now = datetime.now()
    from_time = datetime(now.year, now.month, now.day).replace(day=1)  # TG bat dau: Dau thang hien tai

    month = now.month
    year = now.year + month / 12

    try:
        month = (month + 1) % 12
        if month == 0: month = 12
    except:
        pass
    userid = request.user.id
    username = getUserName(userid)
    util = Utilities()
    qr = util.QuerySecond('getrootbyuser', userid)
    if qr and len(qr) > 0:
        isroot = True
    else:
        isroot = False
    to_time = from_time.replace(year=year, month=month, day=1, hour=23) + timedelta(days=-1)  # TG ket thuc: Cuoi thang hien tai

    if 'btn_REPORT' in request.POST:
        PDF_report = True if 'PDF_report' in request.POST else False
        from_time = request.POST['from_time']
        to_time = request.POST['to_time']
        newdata=[]
        try:
            _from_time = datetime.strptime(from_time, "%d/%m/%Y %H:%M:%S").replace(hour=0, minute=0, second=0)  # Datetime
            _to_time = datetime.strptime(to_time, "%d/%m/%Y %H:%M:%S").replace(hour=23, minute=59, second=59)
            util = Utilities()
            newdata=util.QuerySecond('gethistoryaccess',_from_time.strftime('%Y-%m-%d %H:%M:%S'),_to_time.strftime('%Y-%m-%d %H:%M:%S'))
        except Exception as e:
            print e.message
            messages.error(request, u'Khoảng thời gian không phù hợp!', fail_silently=True)
            return HttpResponseRedirect('')

        if PDF_report:
            #<editor-fold desc="PDF">
            TEMPLATE_SHEET = {
                'HEADER': (
                    (u'STT',),
                    (u'Ngày tác động', {'db_field': 'action_time', 'format': {'date': ''}}),
                    # (u'Mã nhân viên', {'db_field': 'user__userprofile__staff_id', 'format': {'width': 10}}),
                    (u'Tác động bởi', {'db_field': 'user__userprofile__fullname', 'format': {'width': 30}}),
                    (u'Nơi tác động', {'db_field': 'object_repr', 'format': {'width': 30}}),
                    (u'Thao Tác', {'db_field': 'action_flag', 'format': {'width': 10}}),
                    (u'Mục tiêu tác động', {'db_field': 'change_message', 'format': {'width': 70}}),
                )
            }
            try:
                a = Image("parking/static/image/logo_report.png", 0.75 * inch, 0.5 * inch)
            except Exception as e:
                a = ''
            data = [[a,u'        BÁO CÁO LỊCH SỬ TÁC ĐỘNG    ngày lập: %s' % (datetime.today().strftime("%d/%m/%Y %H:%M:%S")),'','','',''],
                    ['',u'        Từ ngày: %s đến ngày: %s' % (_from_time.strftime("%d/%m/%Y %H:%M:%S"),_to_time.strftime("%d/%m/%Y %H:%M:%S")),'','','',''],
                    ['','','','','','']]

            rowHeights = []
            add_pdfdata_fee(data, TEMPLATE_SHEET, newdata, rowHeights)
            pdf = TransactionFeePDF()
            return pdf.run(data)
            #</editor-fold>
        else:

            TEMPLATE_SHEET = {
                'HEADER': (
                    (0,),
                    (u'STT',),
                    (u'Ngày tác động', {'db_field': 'action_time','format': {'date': ''}}),
                    # (u'Mã nhân viên', {'db_field': 'user__userprofile__staff_id', 'format': {'width': 10}}),
                    (u'Tác động bởi', {'db_field': 'user__userprofile__fullname', 'format': {'width': 30}}),
                    (u'Nơi tác động',{'db_field': 'object_repr', 'format': {'width': 30}}),
                    (u'Thao Tác', {'db_field': 'action_flag', 'format': {'width': 10}}),
                    (u'Mục tiêu tác động', {'db_field': 'change_message', 'format': {'width': 70}}),
                ),

                'TITLE': (
                    (2, 5, u'BÁO CÁO LỊCH SỬ TÁC ĐỘNG'),
                    [3, 5, u'Ngày lập'],
                ),

                'STAT': []
            }

            #folder_name="\\\\172.16.0.1\\SharingNATTest\\"
            folder_name = 'templates/report'
            file_name = 'BC_LichSuTacDong.xlsx'
            file_path = '%s/%s' % (folder_name, file_name)

            if not os.path.exists(folder_name):
                os.mkdir(folder_name)

            if os.path.isfile(file_path):
                os.remove(file_path)

            workbook = Workbook(file_path, {'constant_memory': True})

            TEMPLATE_SHEET['TITLE'][1].append(datetime.today().strftime("%d/%m/%Y %H:%M:%S"))

            add_worksheet_configfee_action('BC_LichSuTacDong', workbook, TEMPLATE_SHEET, newdata, True)

            workbook.close()
            with open(file_path, 'r') as f:
                response = HttpResponse(f.read(), content_type="application/vnd.ms-excel")
                response['Content-Disposition'] = "attachment; filename=%s" % file_name
                return response
    return render(request, 'admin/rp_configfeehistory.html', {'from_time': from_time.strftime("%d/%m/%Y %H:%M:%S"),
                                                            'to_time': to_time.strftime("%d/%m/%Y %H:%M:%S"),'username':username,"isroot":isroot})
@login_required(redirect_field_name='', login_url='/admin/')
def render_report_permission_configfee(request):
    userid = request.user.id
    if notallowreport(userid):
        return render(request, 'admin/nopermission.html')
    def add_worksheet_new(sheet_name, workbook):
        util=Utilities()
        pmsr=util.QuerySecond('getmenupermissionss')
        gpmt=util.QuerySecond('grouppermissionreport')
        groups = ()
        groupss=[]
        if selected_group_id == -2:
            groups = Group.objects.all()
            for group in groups:
                groupss.append({"id": group.id, "name": group.name})
            #groupss.append({"id": -1, "name": "Nhóm khác"})
        # elif selected_group_id == -1:
        #     groupss.append({"id": -1, "name": "Nhóm khác"})
        else:
            groups = Group.objects.filter(id=selected_group_id)
            for group in groups:
                groupss.append({"id": group.id, "name": group.name})
        LOGO_EXCEL_REPORT_PATH = 'parking/static/image/' + get_setting('logo_report',
                                                                       u'Mẫu logo dùng trong báo cáo',
                                                                       'logo_report.png')
        LOGO_EXCEL_REPORT_PATH_RIGHT = 'parking/static/image/' + get_setting('logo_repor',
                                                                             u'Mẫu logo dùng trong báo cáo',
                                                                             'logo_report_right.png')

        bold = workbook.add_format({'bold': True})
        border = workbook.add_format()
        border.set_border()
        bold_border = workbook.add_format({'bold': True, 'border': 1})#'align': 'center'
        nobold_border = workbook.add_format({'bold': False, 'border': 1})
        bold_border_center = workbook.add_format({'bold': True, 'border': 1,'align': 'center'})  # 'align': 'center'
        sheet = workbook.add_worksheet(sheet_name)
        sheet.insert_image(0, 0, LOGO_EXCEL_REPORT_PATH)
        sheet.insert_image(0, 7, LOGO_EXCEL_REPORT_PATH_RIGHT)
        sheet.set_column(1, 1, 30)
        sheet.set_column(2, 2, 20)
        sheet.set_column(3, 2+len(groups), 12)

        sheet.merge_range(4, 2, 4, 4, u'BÁO CÁO PHÂN QUYỀN THEO NHÓM', bold)
        sheet.merge_range(5, 2, 5, 4, "Ngày lập: %s"%(datetime.now().strftime("%d/%m/%Y")), bold)
        sheet.merge_range(6, 0, 7, 0, u'STT', bold_border_center)
        sheet.merge_range(6, 1, 7, 2, u'Chức năng', bold_border_center)
        if len(groupss)>1:
            sheet.merge_range(6, 3, 6, len(groupss)+2, u'Phân quyền', bold_border_center)
        else:
            sheet.write(6, 3, u'Phân quyền', bold_border_center)
        j=3
        for g in groupss:
            sheet.write(7, j, g["name"], bold_border)
            j=j+1
        i=8
        index=8
        id=pmsr[0][0]
        name=pmsr[0][1]
        l=len(pmsr)
        stt=1
        for r in pmsr:
            if r[0]!=id:
                if i-1>index:
                    sheet.merge_range(index, 0,i-1,0, stt, nobold_border)
                    sheet.merge_range(index, 1, i-1, 1, name, nobold_border)
                else:
                    sheet.write(index, 0, stt, nobold_border)
                    sheet.write(index, 1, name, nobold_border)
                stt=stt+1
                id=r[0]
                name=r[1]
                index=i
            elif l==i-7:
                if i > index:
                    sheet.merge_range(index, 0, i, 0, stt, nobold_border)
                    sheet.merge_range(index, 1, i, 1, r[1], nobold_border)
                else:
                    sheet.write(index, 0, stt, nobold_border)
                    sheet.write(index, 1, r[1], nobold_border)
                stt = stt + 1
            sheet.write(i, 2, r[2], nobold_border)
            j=3
            for g in groupss:
                val=[x for x in gpmt if x[2]==g["id"] and x[1]==r[0]]
                if val and len(val)>0:
                    if str(r[2])=="Thêm mới":
                        if int(val[0][3])>0:
                            sheet.write(i, j, u'X', nobold_border)
                        else:
                            sheet.write(i, j, u'', nobold_border)
                    elif str(r[2])=="Xóa":
                        if int(val[0][5])>0:
                            sheet.write(i, j, u'X', nobold_border)
                        else:
                            sheet.write(i, j, u'', nobold_border)
                    elif str(r[2])=="Xóa tất cả":
                        if int(val[0][5])>1:
                            sheet.write(i, j, u'X', nobold_border)
                        else:
                            sheet.write(i, j, u'', nobold_border)
                    elif str(r[2])=="Cập nhật":
                        if int(val[0][4])>0:
                            sheet.write(i, j, u'X', nobold_border)
                        else:
                            sheet.write(i, j, u'', nobold_border)
                    elif str(r[2])=="Cập nhật tất cả":
                        if int(val[0][4])>1:
                            sheet.write(i, j, u'X', nobold_border)
                        else:
                            sheet.write(i, j, u'', nobold_border)
                    else:
                        sheet.write(i, j, u'', nobold_border)
                else:
                    sheet.write(i, j, u'', nobold_border)
                j=j+1
            i=i+1
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
    def add_worksheet_user_new(sheet_name, workbook):
        util=Utilities()
        pmsr=util.QuerySecond('getmenupermissionss')
        gpmt=util.QuerySecond('getuserpermissionss')
        gpmtgg = util.QuerySecond('grouppermissionreport')
        groups = ()
        groupss=[]
        glen=0
        if selected_group_id == -2:
            groups = Group.objects.all()
            for group in groups:
                valg = [x for x in gpmtgg if x[2] == group.id]
                if valg and len(valg)>0:
                    uqr=util.Query('getuserbygroup',group.id)
                    userss=[]
                    ulen = 0
                    userss.append({"id": group.id, "fullname": "All"})
                    ulen = ulen + 1
                    glen = glen + 1
                    for u in uqr:
                        valu = [x for x in gpmt if x[2] == u[0]]
                        if valu and len(valu)>0:
                            userss.append({"id":u[0],"fullname":u[1]})
                            ulen = ulen+1
                            glen=glen+1
                    groupss.append({"id": group.id, "name": group.name, "users":userss,"userlen":ulen})
            #groupss.append({"id": -1, "name": "Nhóm khác"})
        # elif selected_group_id == -1:
        #     groupss.append({"id": -1, "name": "Nhóm khác"})
        else:
            groups = Group.objects.filter(id=selected_group_id)
            for group in groups:
                valg = [x for x in gpmtgg if x[2] == group.id]
                if valg and len(valg) > 0:
                    uqr = util.Query('getuserbygroup', group.id)
                    userss = []
                    ulen = 0
                    userss.append({"id": group.id, "fullname": "All"})
                    ulen = ulen + 1
                    glen = glen + 1
                    for u in uqr:
                        valu = [x for x in gpmt if x[2] == u[0]]
                        if valu and len(valu) > 0:
                            userss.append({"id": u[0], "fullname": u[1]})
                            ulen = ulen + 1
                            glen = glen + 1
                    groupss.append({"id": group.id, "name": group.name, "users":userss,"userlen":ulen})
        LOGO_EXCEL_REPORT_PATH = 'parking/static/image/' + get_setting('logo_report',
                                                                       u'Mẫu logo dùng trong báo cáo',
                                                                       'logo_report.png')
        LOGO_EXCEL_REPORT_PATH_RIGHT = 'parking/static/image/' + get_setting('logo_repor',
                                                                             u'Mẫu logo dùng trong báo cáo',
                                                                             'logo_report_right.png')

        bold = workbook.add_format({'bold': True})
        border = workbook.add_format()
        border.set_border()
        bold_border = workbook.add_format({'bold': True, 'border': 1})#'align': 'center'
        nobold_border = workbook.add_format({'bold': False, 'border': 1})
        bold_border_center = workbook.add_format({'bold': True, 'border': 1,'align': 'center'})  # 'align': 'center'
        sheet = workbook.add_worksheet(sheet_name)
        sheet.insert_image(0, 0, LOGO_EXCEL_REPORT_PATH)
        sheet.insert_image(0, 7, LOGO_EXCEL_REPORT_PATH_RIGHT)
        sheet.set_column(1, 1, 30)
        sheet.set_column(2, 2, 20)
        sheet.merge_range(4, 2, 4, 4, u'BÁO CÁO PHÂN QUYỀN THEO NGƯỜI DÙNG', bold)
        sheet.merge_range(5, 2, 5, 4, "Ngày lập: %s"%(datetime.now().strftime("%d/%m/%Y")), bold)
        sheet.merge_range(6, 0, 8, 0, u'STT', bold_border_center)
        sheet.merge_range(6, 1, 8, 2, u'Chức năng', bold_border_center)
        if glen>0:
            sheet.merge_range(6, 3, 6, glen + 2, u'Phân quyền', bold_border_center)
            sheet.set_column(3, glen + 2, 20)
        j=3
        grs = [x for x in groupss if x["userlen"] >0]
        for g in grs:
            ll=g["userlen"]
            if ll>0:
                if ll>1:
                    sheet.merge_range(7, j, 7, ll + j-1,  g["name"], bold_border_center)
                else:
                    sheet.write(7, j, g["name"], bold_border)
                k=j
                for u in g["users"]:
                    sheet.write(8, k, u["fullname"], bold_border)
                    k=k+1
                j=j+ll
        i=9
        index=9
        id=pmsr[0][0]
        name=pmsr[0][1]
        l=len(pmsr)
        stt=1
        for r in pmsr:
            if r[0]!=id:
                if i-1>index:
                    sheet.merge_range(index, 0,i-1,0, stt, nobold_border)
                    sheet.merge_range(index, 1, i-1, 1, name, nobold_border)
                else:
                    sheet.write(index, 0, stt, nobold_border)
                    sheet.write(index, 1, name, nobold_border)
                stt=stt+1
                id=r[0]
                name=r[1]
                index=i
            elif l==i-8:
                if i > index:
                    sheet.merge_range(index, 0, i, 0, stt, nobold_border)
                    sheet.merge_range(index, 1, i, 1, r[1], nobold_border)
                else:
                    sheet.write(index, 0, stt, nobold_border)
                    sheet.write(index, 1, r[1], nobold_border)
                stt = stt + 1
            sheet.write(i, 2, r[2], nobold_border)
            j=3

            for g in grs:
                for u in g["users"]:
                    if u["fullname"]=="All":
                        val = [x for x in gpmtgg if x[2] == u["id"] and x[1] == r[0]]
                    else:
                        val=[x for x in gpmt if x[2]==u["id"] and x[1]==r[0]]
                    if val and len(val)>0:
                        if str(r[2])=="Thêm mới":
                            if int(val[0][3])>0:
                                sheet.write(i, j, u'X', nobold_border)
                            else:
                                sheet.write(i, j, u'', nobold_border)
                        elif str(r[2])=="Xóa":
                            if int(val[0][5])>0:
                                sheet.write(i, j, u'X', nobold_border)
                            else:
                                sheet.write(i, j, u'', nobold_border)
                        elif str(r[2])=="Xóa tất cả":
                            if int(val[0][5])>1:
                                sheet.write(i, j, u'X', nobold_border)
                            else:
                                sheet.write(i, j, u'', nobold_border)
                        elif str(r[2])=="Cập nhật":
                            if int(val[0][4])>0:
                                sheet.write(i, j, u'X', nobold_border)
                            else:
                                sheet.write(i, j, u'', nobold_border)
                        elif str(r[2])=="Cập nhật tất cả":
                            if int(val[0][4])>1:
                                sheet.write(i, j, u'X', nobold_border)
                            else:
                                sheet.write(i, j, u'', nobold_border)
                        else:
                            sheet.write(i, j, u'', nobold_border)
                    else:
                        sheet.write(i, j, u'', nobold_border)
                    j=j+1
            i=i+1
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
    if 'btn_REPORT' in request.POST:
        selected_group_id = int(request.POST['group'])

        folder_name = 'templates/report'
        file_name = 'BC_PhanQuyen_Group.xlsx'
        file_path = '%s/%s' % (folder_name, file_name)

        if not os.path.exists(folder_name):
            os.mkdir(folder_name)

        if os.path.isfile(file_path):
            os.remove(file_path)

        workbook = Workbook(file_path, {'constant_memory': False})
        add_worksheet_new('BC_PhanQuyen_Group', workbook);
        #add_worksheet('BÁO CÁO PHÂN QUYỀN', workbook, selected_group_id)
        workbook.close()

        with open(file_path, 'r') as f:
            response = HttpResponse(f.read(), content_type="application/vnd.ms-excel")
            response['Content-Disposition'] = "attachment; filename=%s" % file_name
            return response
    if 'btn_REPORT_DETAIL' in request.POST:
        selected_group_id = int(request.POST['group'])

        folder_name = 'templates/report'
        file_name = 'BC_PhanQuyen_User.xlsx'
        file_path = '%s/%s' % (folder_name, file_name)

        if not os.path.exists(folder_name):
            os.mkdir(folder_name)

        if os.path.isfile(file_path):
            os.remove(file_path)

        workbook = Workbook(file_path, {'constant_memory': False})
        add_worksheet_user_new('BC_PhanQuyen_User', workbook);
        #add_worksheet('BÁO CÁO PHÂN QUYỀN', workbook, selected_group_id)
        workbook.close()

        with open(file_path, 'r') as f:
            response = HttpResponse(f.read(), content_type="application/vnd.ms-excel")
            response['Content-Disposition'] = "attachment; filename=%s" % file_name
            return response
    group_data = list()
    groups = Group.objects.all()
    group_data.append({"value": -2, "name": '-Tất cả-'})
    # group_data.append({"value": -1, "name": 'Nhóm tự do'})
    for group in groups:
        group_data.append({"value": group.id, "name": group.name})
    userid = request.user.id
    username = getUserName(userid)
    util = Utilities()
    qr = util.QuerySecond('getrootbyuser', userid)
    if qr and len(qr) > 0:
        isroot = True
    else:
        isroot = False
    return render(request, 'admin/rp_permission_config_fee.html', {'group_data': group_data,'username':username,"isroot":isroot})
##
@login_required(redirect_field_name='', login_url='/admin/')
def render_report_permission(request):
    userid = request.user.id
    if notallowreport(userid):
        return render(request, 'admin/nopermission.html')
    def add_worksheet_new(sheet_name, workbook):
        util=Utilities()
        pmsr=util.Query('getpermittionroot')
        gpmt=util.Query('getGroupPermission')
        groups = ()
        if selected_group_id == -1:
            groups = Group.objects.all()
        else:
            groups = Group.objects.filter(id=selected_group_id)
        LOGO_EXCEL_REPORT_PATH = 'parking/static/image/' + get_setting('logo_report',
                                                                       u'Mẫu logo dùng trong báo cáo',
                                                                       'logo_report.png')
        LOGO_EXCEL_REPORT_PATH_RIGHT = 'parking/static/image/' + get_setting('logo_repor',
                                                                             u'Mẫu logo dùng trong báo cáo',
                                                                             'logo_report_right.png')

        bold = workbook.add_format({'bold': True})
        border = workbook.add_format()
        border.set_border()
        bold_border = workbook.add_format({'bold': True, 'border': 1})#'align': 'center'
        bold_border_center = workbook.add_format({'bold': True, 'border': 1,'align': 'center'})  # 'align': 'center'
        sheet = workbook.add_worksheet(sheet_name)
        sheet.insert_image(0, 0, LOGO_EXCEL_REPORT_PATH)
        sheet.insert_image(0, 1+len(groups), LOGO_EXCEL_REPORT_PATH_RIGHT)
        sheet.set_column(1, 1, 30)
        sheet.set_column(2, 2, 20)
        sheet.set_column(3, 2+len(groups), 12)
        sheet.write(5, 0, 'BÁO CÁO PHÂN QUYỀN', bold)
        sheet.merge_range(6, 0, 7, 0, u'No.', bold_border_center)
        sheet.merge_range(6, 1, 7, 2, u'Functions', bold_border_center)
        if len(groups)>1:
            sheet.merge_range(6, 3, 6, len(groups)+2, u'Roles', bold_border_center)
        else:
            sheet.write(6, 3, u'Roles', bold_border_center)
        j=3
        for g in groups:
            sheet.write(7, j, g.name, bold_border)
            j=j+1
        i=8
        index=8
        id=pmsr[0][1]
        name=pmsr[0][2]
        l=len(pmsr)
        stt=1
        for r in pmsr:
            if r[1]!=id:
                if i-1>index:
                    sheet.merge_range(index, 0,i-1,0, stt, bold_border)
                    sheet.merge_range(index, 1, i-1, 1, name, bold_border)
                else:
                    sheet.write(index, 0, stt, bold_border)
                    sheet.write(index, 1, name, bold_border)
                stt=stt+1
                id=r[1]
                name=r[2]
                index=i
            elif l==i-7:
                if i > index:
                    sheet.merge_range(index, 0, i, 0, stt, bold_border)
                    sheet.merge_range(index, 1, i, 1, r[2], bold_border)
                else:
                    sheet.write(index, 0, stt, bold_border)
                    sheet.write(index, 1, r[2], bold_border)
                stt = stt + 1
            sheet.write(i, 2, r[3], bold_border)
            j=3
            for g in groups:
                val=[x for x in gpmt if x[1]==g.id and x[2]==r[0]]
                if val and len(val)>0:
                    sheet.write(i, j, u'X', bold_border)
                else:
                    sheet.write(i, j, u'', bold_border)
                j=j+1
            i=i+1
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
    def add_worksheet(sheet_name, workbook, group_id):
        LOGO_EXCEL_REPORT_PATH = 'parking/static/image/' + get_setting('logo_report',
                                                                       u'Mẫu logo dùng trong báo cáo',
                                                                       'logo_report.png')
        LOGO_EXCEL_REPORT_PATH_RIGHT = 'parking/static/image/' + get_setting('logo_repor',
                                                                             u'Mẫu logo dùng trong báo cáo',
                                                                             'logo_report_right.png')

        bold = workbook.add_format({'bold': True})
        border = workbook.add_format()
        border.set_border()
        bold_border = workbook.add_format({'bold': True, 'border': 1})

        sheet = workbook.add_worksheet(sheet_name)
        sheet.insert_image(0, 0, LOGO_EXCEL_REPORT_PATH)
        sheet.insert_image(0, 6, LOGO_EXCEL_REPORT_PATH_RIGHT)
        sheet.set_column(1,1, 50)

        sheet.write(5, 0, 'BÁO CÁO PHÂN QUYỀN', bold)

        i = 7
        # Header
        sheet.write(i, 0, 'Nhóm', bold_border)
        sheet.write(i, 1, 'Quyền', bold_border)
        sheet.write(i, 2, 'Tài khoản', bold_border)

        groups = ()
        if selected_group_id == -1:
            groups = Group.objects.all()
        else:
            groups = Group.objects.filter(id=group_id)

        permission_row = 0
        user_row = 0
        for group in groups:
            i += 1

            if permission_row > user_row:
                i+=permission_row
            else:
                i+=user_row

            permission_row = user_row = 0
            sheet.merge_range(i, 0, i, 1, group.name, bold_border)

            # Get permissions of group
            permissions = group.permissions.values('name')
            for permission in permissions:
                permission_row+=1
                sheet.write(i + permission_row, 1, permission['name'],border)

            # Get user of group
            users = group.user_set.values("username")
            for user in users:
                user_row += 1
                sheet.write(i + user_row, 2, user['username'], border)
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
    if 'btn_REPORT' in request.POST:
        selected_group_id = int(request.POST['group'])

        folder_name = 'templates/report'
        file_name = 'GPMS_BC_Permission.xlsx'
        file_path = '%s/%s' % (folder_name, file_name)

        if not os.path.exists(folder_name):
            os.mkdir(folder_name)

        if os.path.isfile(file_path):
            os.remove(file_path)

        workbook = Workbook(file_path, {'constant_memory': False})
        add_worksheet_new('BÁO CÁO PHÂN QUYỀN', workbook);
        #add_worksheet('BÁO CÁO PHÂN QUYỀN', workbook, selected_group_id)
        workbook.close()

        with open(file_path, 'r') as f:
            response = HttpResponse(f.read(), content_type="application/vnd.ms-excel")
            response['Content-Disposition'] = "attachment; filename=%s" % file_name
            return response

    group_data = list()
    groups = Group.objects.all()
    group_data.append({"value": -1, "name": 'Tất cả'})
    for group in groups:
        group_data.append({"value": group.id, "name": group.name})

    return render(request, 'admin/rp_permission.html', {'group_data': group_data})


def get_vehicle_types_metadata():
    vehicle_type_data = {}
    for type in VehicleType.objects.all():
        vehicle_type_data[type.id] = type.name
    return vehicle_type_data
VEHICLE_TYPE_NAMES_BY_FULL_ID = get_vehicle_types_metadata()
VEHICLE_TYPE_FULL_ID_BY_DECODED_ID_DICT = load_nghia_vehicle_type()
VEHICLE_TYPE_NAME_BY_DECODED_ID = load_vehicle_type_name()

def getsampleandgroupredempt(vtype):
    try:
        util = Utilities()
        qr = util.QuerySecond('geredemtiontactivelist',vtype)
        if qr and len(qr)>0:
            return qr[0][2],qr[0][3]
        else:
            return '',''
    except Exception as e:
        return '',''
@login_required(redirect_field_name='', login_url='/admin/')
def render_report_parking_redemption(request):
    userid = request.user.id
    if notallowreport(userid):
        return render(request, 'admin/nopermission.html')
    def convert_to_currency(i):
        return "{0}".format("{:,}".format(i))

    if 'btn_REPORT' in request.POST:
        from_time = request.POST['from_time']
        to_time = request.POST['to_time']
        try:
            _from_time = datetime.strptime(from_time, "%d/%m/%Y %H:%M")  # Datetime
            _to_time = datetime.strptime(to_time, "%d/%m/%Y %H:%M")
        except Exception as e:
            messages.error(request, u'Khoảng thời gian không phù hợp!', fail_silently=True)
            return HttpResponseRedirect('')

        # Prepare report data old
        # user_claim = ''
        # claim_promotions = ClaimPromotionV2.objects.filter(used=True, server_time__gte=_from_time,
        #                                                    server_time__lte=_to_time).order_by('server_time').values(
        #     'id',
        #     'amount_a',
        #     'amount_b',
        #     'amount_c',
        #     'amount_d',
        #     'server_time',
        #     'parking_session__check_in_time',
        #     'parking_session__vehicle_number',
        #     'parking_session__vehicle_type',
        #     'parking_session__card__card_label',
        #     'user__userprofile__fullname')
        #
        # claim_promotion_ids = [e['id'] for e in claim_promotions]
        #
        # # Filter bills link with these ClaimPromotions
        # claim_promotion_bills = ClaimPromotionBillV2.objects.filter(claim_promotion__id__in=claim_promotion_ids) \
        #     .order_by('claim_promotion') \
        #     .values('claim_promotion_id', 'company_info', 'bill_number', 'bill_amount')
        #
        # claim_promotion_bills_by_claim_promotion_id = {}
        # for item in claim_promotion_bills:
        #     claim_promotion_id = item['claim_promotion_id']
        #
        #     if claim_promotion_id in claim_promotion_bills_by_claim_promotion_id:
        #         claim_promotion_bills_by_claim_promotion_id[claim_promotion_id].append(item)
        #     else:
        #         claim_promotion_bills_by_claim_promotion_id[claim_promotion_id] = [item]
        ##end old

        ##begin new data 2018-01-25
        util = Utilities()
        dts = util.Query("getredempts", _from_time, _to_time)
        ##end new data 2018-01-25
        # Create a new Excel file
        folder_name = 'templates/report'
        file_name = 'GPMS_Parking_Redemption.xlsx'
        file_path = '%s/%s' % (folder_name, file_name)

        if not os.path.exists(folder_name):
            os.mkdir(folder_name)

        if os.path.isfile(file_path):
            os.remove(file_path)

        workbook = Workbook(file_path, {'constant_memory': True})

        bold = workbook.add_format({'bold': True})
        border = workbook.add_format({'border': 1})
        bold_border = workbook.add_format({'bold': True, 'border': 1})
        bold_center = workbook.add_format({'bold': True, 'align': 'center'})
        number_border = workbook.add_format({'num_format': '#,###0', 'border': 1, 'align': 'right'})
        bold_number_border = workbook.add_format({'num_format': '#,###0', 'border': 1, 'bold': True, 'align': 'right'})

        sheet = workbook.add_worksheet('Parking Redemption Record')
        LOGO_EXCEL_REPORT_PATH = 'parking/static/image/' + get_setting('logo_report',
                                                                       u'Mẫu logo dùng trong báo cáo',
                                                                       'logo_report.png')
        LOGO_EXCEL_REPORT_PATH_RIGHT = 'parking/static/image/' + get_setting('logo_repor',
                                                                             u'Mẫu logo dùng trong báo cáo',
                                                                             'logo_report_right.png')
        # Write headers
        sheet.insert_image(0, 0, LOGO_EXCEL_REPORT_PATH)
        sheet.insert_image(0, 18, LOGO_EXCEL_REPORT_PATH_RIGHT)
        # sheet.merge_range(0, 0, 2, 16, "Saigon Centre Phase II \nProperty Management".format(), bold_center)
        sheet.merge_range(3, 0, 4, 16,
                          "BÁO CÁO REDEMTION ",bold_center)
        sheet.merge_range(5, 0, 5, 16,
                          "Từ: {0} đến: {1}".format(from_time, to_time), bold_center)
        HEADER_STARTING_ROW = 8
        HEADER_ENDING_ROW = 7

        HEADERS = ['Audit No.', 'STT', 'Biển số xe', 'Loại xe', 'Số thẻ','Nhân viên claimed',
                   'Giờ vào', 'Giờ claimed', 'Thời gian lưu bãi','Tên công thức', 'Nhóm đối tác',
                   'Tên cửa hàng', 'Số hóa đơn', 'Giá trị hóa đơn', 'Tổng cộng hóa đơn',
                   'Phí giữ xe', 'Miễn giảm', 'Còn phải thu', 'Ghi chú']

        sheet.write_row(HEADER_ENDING_ROW, 0, HEADERS, bold_border)
        sheet.set_column(0, 3, 15)
        sheet.set_column(4, 7, 20)
        sheet.set_column(9, 10, 30)
        sheet.set_column(11, 17, 20)

        from django.utils.timezone import localtime
        # Write report data
        DATA_STARTING_ROW = HEADER_ENDING_ROW + 1
        totalbill=0
        total_parking_fee = 0
        total_redemption_fee = 0
        total_remaifee = 0
        i = 0
        ##old row data
        # audit_number_tracker = {}
        #
        # for item in claim_promotions:
        #     current_claim_promotion_id = item['id']
        #     audit_number = '0'
        #     current_claimed_date = localtime(item['server_time']).strftime("%d%m%Y")
        #
        #     if current_claimed_date not in audit_number_tracker:
        #         audit_number_tracker[current_claimed_date] = 1
        #     else:
        #         audit_number_tracker[current_claimed_date] += 1
        #
        #     audit_number = u"{0}.{1:04}".format(current_claimed_date, audit_number_tracker[current_claimed_date])
        #     tmp=item['parking_session__vehicle_type']
        #     current_row = [audit_number,
        #                    item['id'], item['parking_session__vehicle_number'],
        #                    VEHICLE_TYPE_NAMES_BY_FULL_ID[
        #                        VEHICLE_TYPE_FULL_ID_BY_DECODED_ID_DICT[tmp]],
        #                    item['parking_session__card__card_label'],
        #                    item['user__userprofile__fullname'],
        #                    localtime(item['parking_session__check_in_time']).strftime("%d/%m/%Y %H:%M:%S"),
        #                    localtime(item['server_time']).strftime("%d/%m/%Y %H:%M:%S"),
        #                    unicode(item['server_time'] - item['parking_session__check_in_time'])
        #                    ]
        #
        #     if current_claim_promotion_id in claim_promotion_bills_by_claim_promotion_id:
        #         bills_data = {'tenants': [], 'bill_numbers': [], 'bill_amounts': [], 'total_bill_amount': 0}
        #         for bill in claim_promotion_bills_by_claim_promotion_id[current_claim_promotion_id]:
        #             if bill['company_info']: bills_data['tenants'].append(bill['company_info'])
        #             bills_data['bill_numbers'].append(unicode(bill['bill_number']))
        #
        #             bill_amount = abs(bill['bill_amount'])
        #             bills_data['bill_amounts'].append(unicode(convert_to_currency(bill_amount)))
        #             bills_data['total_bill_amount'] += bill_amount
        #         current_row += [
        #             "; ".join(bills_data['tenants']),
        #             "; ".join(bills_data['bill_numbers']),
        #             "; ".join(bills_data['bill_amounts']),
        #             unicode(convert_to_currency(bills_data['total_bill_amount']))
        #         ]
        #     else:
        #         current_row += ['', '', '', '']
        #     total_parking_fee += item['amount_a']
        #
        #     redemption = abs(item['amount_b']) + abs(item['amount_c']) + abs(item['amount_d'])
        #     finalfee = item['amount_a'] - redemption
        #     if finalfee < 0:
        #         finalfee = 0
        #
        #     total_redemption_fee += redemption
        #     total_remaifee +=finalfee
        #
        #     money_data = [item['amount_a'], redemption,
        #                   finalfee
        #                   ]
        #
        #     sheet.write_row(DATA_STARTING_ROW + i, 0, current_row, border)
        #     sheet.write_row(DATA_STARTING_ROW + i, len(current_row), money_data, number_border)
        #     i += 1
        ##begin new row data 2018-01-25
        for r in dts:
            ds=getsampleandgroupredempt(r[16])
            claimtime = datetime.strptime(r[6], "%Y-%m-%d %H:%M:%S")
            claimtimestr=claimtime.strftime('%d/%m/%Y %H:%M:%S')
            checkinstr = datetime.strptime(r[5], "%Y-%m-%d %H:%M:%S").strftime('%d/%m/%Y %H:%M:%S')
            audit_number = u"{0}.{1:04}".format(claimtime.strftime("%d%m%Y"), i+1)
            totalbill+=float(r[11])
            total_parking_fee+=float(r[12])
            total_redemption_fee+=float(r[13])
            total_remaifee+=float(r[14])
            current_row = [audit_number, str(r[0]), r[1], r[2], r[3], r[4], checkinstr, claimtimestr, r[7],ds[0],ds[1], r[8], r[9],
                           r[10], r[11], r[12],r[13], r[14],r[15]]
            sheet.write_row(DATA_STARTING_ROW + i, 0, current_row, number_border)
            i+=1
        ##end new row data 2018-01-25
        last_total_row=[u'TOTAL','','','','','','','','','','','','','',totalbill,total_parking_fee,total_redemption_fee,total_remaifee,'']
        sheet.write_row(DATA_STARTING_ROW + i, 0, last_total_row, bold_number_border)

        ###2017-12-20
        sheet.write(DATA_STARTING_ROW + i + 2, 1, 'Handed Over by ', bold)
        sheet.write(DATA_STARTING_ROW + i + 2, 4, 'Taken Over by ', bold)
        sheet.write(DATA_STARTING_ROW + i + 2, 7, 'Date Handed Over ', bold)
        sheet.write(DATA_STARTING_ROW + i + 2, 11, 'Checked by ', bold)
        ###

        # Save Excel file and return for downloading
        workbook.close()
        with open(file_path, 'r') as f:
            response = HttpResponse(f.read(), content_type="application/vnd.ms-excel")
            response['Content-Disposition'] = "attachment; filename=%s" % file_name
            return response

    now = datetime.now()
    from_time = datetime(now.year, now.month, now.day).replace(day=1)  # TG bat dau: Dau thang hien tai
    to_time = from_time.replace(hour=23, minute=59, second=59)
    return render(request, 'admin/rp-claimpromotion.html', {'from_time': from_time.strftime("%d/%m/%Y %H:%M"),
                                                            'to_time': to_time.strftime("%d/%m/%Y %H:%M")})


@login_required(redirect_field_name='', login_url='/admin/')
def render_report_parking_redemption1(request):
    userid = request.user.id
    if notallowreport(userid):
        return render(request, 'admin/nopermission.html')
    def convert_to_currency(i):
        return "{0}".format("{:,}".format(i))
    if 'btn_REPORT' in request.POST:
        from_time = request.POST['from_time']
        to_time = request.POST['to_time']
        try:
            _from_time = datetime.strptime(from_time, "%d/%m/%Y %H:%M:%S")  # Datetime
            _to_time = datetime.strptime(to_time, "%d/%m/%Y %H:%M:%S")
        except Exception as e:
            messages.error(request, u'Khoảng thời gian không phù hợp!', fail_silently=True)
            return HttpResponseRedirect('')

        # Prepare report data old
        # user_claim = ''
        # claim_promotions = ClaimPromotionV2.objects.filter(used=True, server_time__gte=_from_time,
        #                                                    server_time__lte=_to_time).order_by('server_time').values(
        #     'id',
        #     'amount_a',
        #     'amount_b',
        #     'amount_c',
        #     'amount_d',
        #     'server_time',
        #     'parking_session__check_in_time',
        #     'parking_session__vehicle_number',
        #     'parking_session__vehicle_type',
        #     'parking_session__card__card_label',
        #     'user__userprofile__fullname')
        #
        # claim_promotion_ids = [e['id'] for e in claim_promotions]
        #
        # # Filter bills link with these ClaimPromotions
        # claim_promotion_bills = ClaimPromotionBillV2.objects.filter(claim_promotion__id__in=claim_promotion_ids) \
        #     .order_by('claim_promotion') \
        #     .values('claim_promotion_id', 'company_info', 'bill_number', 'bill_amount')
        #
        # claim_promotion_bills_by_claim_promotion_id = {}
        # for item in claim_promotion_bills:
        #     claim_promotion_id = item['claim_promotion_id']
        #
        #     if claim_promotion_id in claim_promotion_bills_by_claim_promotion_id:
        #         claim_promotion_bills_by_claim_promotion_id[claim_promotion_id].append(item)
        #     else:
        #         claim_promotion_bills_by_claim_promotion_id[claim_promotion_id] = [item]
        ##end old

        ##begin new data 2018-01-25
        util = Utilities()
        dts = util.Query("getredempts", _from_time, _to_time)
        ##end new data 2018-01-25
        # Create a new Excel file
        folder_name = 'templates/report'
        file_name = 'BC_Redemption.xlsx'
        file_path = '%s/%s' % (folder_name, file_name)

        if not os.path.exists(folder_name):
            os.mkdir(folder_name)

        if os.path.isfile(file_path):
            os.remove(file_path)

        workbook = Workbook(file_path, {'constant_memory': True})

        bold = workbook.add_format({'bold': True})
        border = workbook.add_format({'border': 1})
        bold_border = workbook.add_format({'bold': True, 'border': 1})
        bold_center = workbook.add_format({'bold': True, 'align': 'center'})
        number_border = workbook.add_format({'num_format': '#,###0', 'border': 1, 'align': 'right'})
        bold_number_border = workbook.add_format({'num_format': '#,###0', 'border': 1, 'bold': True, 'align': 'right'})

        sheet = workbook.add_worksheet('BC_Redemption')
        LOGO_EXCEL_REPORT_PATH = 'parking/static/image/' + get_setting('logo_report',
                                                                       u'Mẫu logo dùng trong báo cáo',
                                                                       'logo_report.png')
        LOGO_EXCEL_REPORT_PATH_RIGHT = 'parking/static/image/' + get_setting('logo_repor',
                                                                             u'Mẫu logo dùng trong báo cáo',
                                                                             'logo_report_right.png')
        # Write headers
        sheet.insert_image(0, 0, LOGO_EXCEL_REPORT_PATH)
        sheet.insert_image(0, 18, LOGO_EXCEL_REPORT_PATH_RIGHT)
        # sheet.merge_range(0, 0, 2, 16, "Saigon Centre Phase II \nProperty Management".format(), bold_center)
        sheet.merge_range(3, 0, 4, 16,
                          "BÁO CÁO REDEMPTION ",bold_center)
        sheet.merge_range(5, 0, 5, 16,
                          "Từ: {0} đến: {1}".format(from_time, to_time), bold_center)
        HEADER_STARTING_ROW = 8
        HEADER_ENDING_ROW = 7

        HEADERS = ['Audit No.', 'STT', 'Biển số xe', 'Loại xe', 'Số thẻ','Nhân viên claimed',
                   'Giờ Check-in', 'Giờ claimed', 'Thời gian lưu bãi','Tên công thức', 'Nhóm đối tác',
                   'Tên cửa hàng', 'Số hóa đơn', 'Giá trị hóa đơn', 'Tổng cộng hóa đơn',
                   'Phí giữ xe', 'Miễn giảm', 'Còn phải thu', 'Ghi chú']

        sheet.write_row(HEADER_ENDING_ROW, 0, HEADERS, bold_border)
        sheet.set_column(0, 3, 15)
        sheet.set_column(4, 7, 20)
        sheet.set_column(9, 10, 30)
        sheet.set_column(11, 17, 20)

        from django.utils.timezone import localtime
        # Write report data
        DATA_STARTING_ROW = HEADER_ENDING_ROW + 1
        totalbill=0
        total_parking_fee = 0
        total_redemption_fee = 0
        total_remaifee = 0
        i = 0
        ##old row data
        # audit_number_tracker = {}
        #
        # for item in claim_promotions:
        #     current_claim_promotion_id = item['id']
        #     audit_number = '0'
        #     current_claimed_date = localtime(item['server_time']).strftime("%d%m%Y")
        #
        #     if current_claimed_date not in audit_number_tracker:
        #         audit_number_tracker[current_claimed_date] = 1
        #     else:
        #         audit_number_tracker[current_claimed_date] += 1
        #
        #     audit_number = u"{0}.{1:04}".format(current_claimed_date, audit_number_tracker[current_claimed_date])
        #     tmp=item['parking_session__vehicle_type']
        #     current_row = [audit_number,
        #                    item['id'], item['parking_session__vehicle_number'],
        #                    VEHICLE_TYPE_NAMES_BY_FULL_ID[
        #                        VEHICLE_TYPE_FULL_ID_BY_DECODED_ID_DICT[tmp]],
        #                    item['parking_session__card__card_label'],
        #                    item['user__userprofile__fullname'],
        #                    localtime(item['parking_session__check_in_time']).strftime("%d/%m/%Y %H:%M:%S"),
        #                    localtime(item['server_time']).strftime("%d/%m/%Y %H:%M:%S"),
        #                    unicode(item['server_time'] - item['parking_session__check_in_time'])
        #                    ]
        #
        #     if current_claim_promotion_id in claim_promotion_bills_by_claim_promotion_id:
        #         bills_data = {'tenants': [], 'bill_numbers': [], 'bill_amounts': [], 'total_bill_amount': 0}
        #         for bill in claim_promotion_bills_by_claim_promotion_id[current_claim_promotion_id]:
        #             if bill['company_info']: bills_data['tenants'].append(bill['company_info'])
        #             bills_data['bill_numbers'].append(unicode(bill['bill_number']))
        #
        #             bill_amount = abs(bill['bill_amount'])
        #             bills_data['bill_amounts'].append(unicode(convert_to_currency(bill_amount)))
        #             bills_data['total_bill_amount'] += bill_amount
        #         current_row += [
        #             "; ".join(bills_data['tenants']),
        #             "; ".join(bills_data['bill_numbers']),
        #             "; ".join(bills_data['bill_amounts']),
        #             unicode(convert_to_currency(bills_data['total_bill_amount']))
        #         ]
        #     else:
        #         current_row += ['', '', '', '']
        #     total_parking_fee += item['amount_a']
        #
        #     redemption = abs(item['amount_b']) + abs(item['amount_c']) + abs(item['amount_d'])
        #     finalfee = item['amount_a'] - redemption
        #     if finalfee < 0:
        #         finalfee = 0
        #
        #     total_redemption_fee += redemption
        #     total_remaifee +=finalfee
        #
        #     money_data = [item['amount_a'], redemption,
        #                   finalfee
        #                   ]
        #
        #     sheet.write_row(DATA_STARTING_ROW + i, 0, current_row, border)
        #     sheet.write_row(DATA_STARTING_ROW + i, len(current_row), money_data, number_border)
        #     i += 1
        ##begin new row data 2018-01-25
        for r in dts:
            ds=getsampleandgroupredempt(r[16])
            claimtime = datetime.strptime(r[6], "%Y-%m-%d %H:%M:%S")
            claimtimestr=claimtime.strftime('%d/%m/%Y %H:%M:%S')
            checkinstr = datetime.strptime(r[5], "%Y-%m-%d %H:%M:%S").strftime('%d/%m/%Y %H:%M:%S')
            audit_number = u"{0}.{1:04}".format(claimtime.strftime("%d%m%Y"), i+1)
            totalbill+=float(r[11])
            total_parking_fee+=float(r[12])
            total_redemption_fee+=float(r[13])
            total_remaifee+=float(r[14])
            current_row = [audit_number, str(r[0]), r[1], r[2], r[3], r[4], checkinstr, claimtimestr, r[7],ds[0],ds[1], r[8], r[9],
                           r[10], r[11], r[12],r[13], r[14],r[15]]
            sheet.write_row(DATA_STARTING_ROW + i, 0, current_row, number_border)
            i+=1
        ##end new row data 2018-01-25
        last_total_row=[u'TOTAL','','','','','','','','','','','','','',totalbill,total_parking_fee,total_redemption_fee,total_remaifee,'']
        sheet.write_row(DATA_STARTING_ROW + i, 0, last_total_row, bold_number_border)

        ###2017-12-20
        # sheet.write(DATA_STARTING_ROW + i + 2, 1, 'Handed Over by ', bold)
        # sheet.write(DATA_STARTING_ROW + i + 2, 4, 'Taken Over by ', bold)
        # sheet.write(DATA_STARTING_ROW + i + 2, 7, 'Date Handed Over ', bold)
        # sheet.write(DATA_STARTING_ROW + i + 2, 11, 'Checked by ', bold)
        ###

        # Save Excel file and return for downloading
        workbook.close()
        with open(file_path, 'r') as f:
            response = HttpResponse(f.read(), content_type="application/vnd.ms-excel")
            response['Content-Disposition'] = "attachment; filename=%s" % file_name
            return response
    now = datetime.now()
    from_time = datetime(now.year, now.month, now.day).replace(day=1)  # TG bat dau: Dau thang hien tai
    to_time = from_time.replace(hour=23, minute=59, second=59)
    userid = request.user.id
    username = getUserName(userid)
    util = Utilities()
    qr = util.QuerySecond('getrootbyuser', userid)
    if qr and len(qr) > 0:
        isroot = True
    else:
        isroot = False

    return render(request, 'admin/rp_redemption.html', {'from_time': from_time.strftime("%d/%m/%Y %H:%M:%S"),
                                                            'to_time': to_time.strftime("%d/%m/%Y %H:%M:%S"),'username':username,"isroot":isroot})


@login_required(redirect_field_name='', login_url='/admin/')
def render_report_parking_hourly(request):
    userid = request.user.id
    if notallowreport(userid):
        return render(request, 'admin/nopermission.html')
    def convert_to_currency(i):
        return "{0}".format("{:,}".format(i))

    if 'btn_REPORT' in request.POST:
        from_time = request.POST['from_time']
        to_time = request.POST['to_time']
        try:
            _from_time = datetime.strptime(from_time, "%d/%m/%Y %H:%M:%S").strftime("%Y-%m-%d %H:%M:%S")  # Datetime
            _to_time = datetime.strptime(to_time, "%d/%m/%Y %H:%M:%S").strftime("%Y-%m-%d %H:%M:%S")
        except Exception as e:
            messages.error(request, u'Khoảng thời gian không phù hợp!', fail_silently=True)
            return HttpResponseRedirect('')
        #old
        # rs1 = ParkingFeeSession.objects \
        #     .filter(payment_date__gte=_from_time, payment_date__lte=_to_time, session_type='OUT') \
        #     .values('vehicle_number',
        #             'parking_fee',
        #             'vehicle_type__name',
        #             'parking_session__card__card_label',
        #             'parking_session__card__card_type',
        #             'parking_session__check_in_time',
        #             'parking_session__check_out_time',
        #             'parking_fee',
        #             'payment_date',
        #             'parking_session_id',
        #             'parking_session__check_in_operator__userprofile__fullname',
        #             'parking_session__check_out_operator__userprofile__fullname',
        #             'parking_session__check_in_lane__terminal__name',
        #             'parking_session__check_out_lane__terminal__name')
        #
        # rs2 = ParkingSession.objects \
        #     .filter(check_out_time__gte=_from_time, check_out_time__lte=_to_time, check_out_exception_id__isnull=False) \
        #     .select_related('check_out_exception').values('vehicle_type', 'check_out_exception__parking_fee',
        #                                                   'vehicle_number',
        #                                                   'check_in_time',
        #                                                   'check_out_time',
        #                                                   'card__card_label',
        #                                                   'card__card_type',
        #                                                   'id',
        #                                                   'check_in_operator__userprofile__fullname',
        #                                                   'check_out_operator__userprofile__fullname',
        #                                                   'check_in_lane__terminal__name',
        #                                                   'check_out_lane__terminal__name')
        #2018-03-28
        util=Utilities()
        datarp=util.Query('getparkinghourly',_from_time,_to_time)
        # 2018-03-28
        # Create a new Excel file
        folder_name = 'templates/report'
        file_name = 'Pakring_Hourly.xlsx'
        file_path = '%s/%s' % (folder_name, file_name)

        if not os.path.exists(folder_name):
            os.mkdir(folder_name)

        if os.path.isfile(file_path):
            os.remove(file_path)

        workbook = Workbook(file_path, {'constant_memory': True})

        bold = workbook.add_format({'bold': True})
        border = workbook.add_format({'border': 1})
        bold_border = workbook.add_format({'bold': True, 'border': 1})
        bold_center = workbook.add_format({'bold': True, 'align': 'center'})
        number_border = workbook.add_format({'num_format': '#,###0', 'border': 1, 'align': 'right'})
        bold_number_border = workbook.add_format({'num_format': '#,###0', 'border': 1, 'bold': True, 'align': 'right'})
        number_border_format = workbook.add_format({'num_format': '#,###0', 'border': 1})

        sheet = workbook.add_worksheet('Parking Hour')

        # Write headers
        sheet.merge_range(0, 0, 2, 14, "Saigon Centre Phase II\n Property Management", bold)
        sheet.merge_range(3, 0, 5, 14,
                          "PARKING HOURLY DETAIL RECORD\nFrom: {0} To: {1}".format(from_time, to_time), bold_center)
        HEADER_STARTING_ROW = 8
        HEADER_ENDING_ROW = 7

        HEADERS = ['Transaction No.', 'Vehicle Number', 'Type', 'Card code', 'Card type',
                   'Check-in Time', 'Claimed', 'Check-out time', 'Parking Duration',
                   'Parking Fee', 'Redemption', 'Remaining Fee ',
                   'User check-in', 'User claimed', 'User check-out', 'Workstation check-in', 'Workstation check-out',
                   'Remark']

        sheet.write_row(HEADER_ENDING_ROW, 0, HEADERS, bold_border)
        sheet.set_column(0, 4, 15)
        sheet.set_column(5, 18, 20)

        # Write report data
        DATA_STARTING_ROW = HEADER_ENDING_ROW + 1
        total_Remaining_fee = 0
        total_redemp_fee = 0
        total_parking_fee = 0

        i = 0
        #2018-03-28
        for r in datarp:
            no="%s_%s"%(r[0],i+1)
            parkingfee=float(r[9]) if r[9] else 0
            redemption=float(r[10]) if r[10] else 0
            remainingfee=parkingfee-redemption if parkingfee>redemption else 0
            current_row=[no,r[1],r[2],r[3],r[4],r[5],r[6],r[7] if r[7] else '',r[8],parkingfee,redemption,remainingfee,r[11],r[12] if r[12] else u'',r[13],r[14],r[15],u'']
            sheet.write_row(DATA_STARTING_ROW + i, 0, current_row, number_border_format)
            total_parking_fee+=parkingfee
            total_redemp_fee+=redemption
            total_Remaining_fee+=remainingfee
            i=i+1
        #2018-03-28
        #old
        #bool(rs1)
        # card_types = list(CardType.objects.all())
        # for item in rs1:
        #     # Get claimpromotion
        #     claim_time = ''
        #     redemption = 0L
        #     user_claim = ''
        #     claim_promotion = ClaimPromotionV2.objects.filter(parking_session_id=item['parking_session_id'],
        #                                                     used=True).values('id',
        #                                                                       'amount_a',
        #                                                                       'amount_b',
        #                                                                       'amount_c',
        #                                                                       'amount_d',
        #                                                                       'server_time',
        #                                                                       'user__userprofile__fullname')
        #     if claim_promotion and len(claim_promotion) > 0:
        #         claim_promotion = claim_promotion[0]
        #         claim_time = localtime(claim_promotion['server_time']).strftime("%d/%m/%Y %H:%M:%S") if claim_promotion[
        #             'server_time'] else ''
        #         redemption = abs(claim_promotion['amount_b']) + abs(claim_promotion['amount_c']) + abs(
        #             claim_promotion['amount_d'])
        #         user_claim = claim_promotion["user__userprofile__fullname"]
        #
        #     card_type = CardType.objects.filter(id=item['parking_session__card__card_type'])
        #     card_type_name = card_type[0].name if len(card_type) > 0 else ''
        #
        #     #  Build data report
        #     remain_parking_fee = item['parking_fee'] - redemption if item['parking_fee'] - redemption > 0 else 0
        #     parking_fee = item['parking_fee']
        #
        #     total_Remaining_fee += remain_parking_fee
        #     total_redemp_fee += redemption
        #     total_parking_fee += parking_fee
        #
        #     current_row = [item['payment_date'].strftime("%d%m%Y") + "-{0:000}".format(i + 1),
        #                    item['vehicle_number'],
        #                    item['vehicle_type__name'],
        #                    item['parking_session__card__card_label'],
        #                    card_type_name,
        #                    localtime(item['parking_session__check_in_time']).strftime("%d/%m/%Y %H:%M:%S") if item[
        #                        'parking_session__check_in_time'] else '',
        #                    claim_time,
        #                    localtime(item['parking_session__check_out_time']).strftime("%d/%m/%Y %H:%M:%S") \
        #                         if item['parking_session__check_out_time'] else '',
        #                    unicode(item['parking_session__check_out_time'] - item['parking_session__check_in_time']),
        #                    item['parking_fee'],
        #                    redemption,
        #                    remain_parking_fee,
        #                    item["parking_session__check_in_operator__userprofile__fullname"],
        #                    user_claim,
        #                    item["parking_session__check_out_operator__userprofile__fullname"],
        #                    item["parking_session__check_in_lane__terminal__name"],
        #                    item["parking_session__check_out_lane__terminal__name"],
        #                    ''
        #                    ]
        #
        #     sheet.write_row(DATA_STARTING_ROW + i, 0, current_row, number_border_format)
        #     i += 1
        #
        # # Exception checkout
        # for item in rs2:
        #     # Get claimpromotion
        #     claim_time = ''
        #     redemption = 0L
        #     user_claim = ''
        #     claim_promotion = ClaimPromotionV2.objects.filter(parking_session_id=item['id'],
        #                                                       used=True).values('id',
        #                                                                         'amount_a',
        #                                                                         'amount_b',
        #                                                                         'amount_c',
        #                                                                         'amount_d',
        #                                                                         'server_time',
        #                                                                         'user__userprofile__fullname')
        #     if claim_promotion and len(claim_promotion) > 0:
        #         claim_promotion = claim_promotion[0]
        #         claim_time = localtime(claim_promotion['server_time']).strftime("%d/%m/%Y %H:%M:%S") if claim_promotion[
        #             'server_time'] else ''
        #         redemption = abs(claim_promotion['amount_b']) + abs(claim_promotion['amount_c']) + abs(
        #             claim_promotion['amount_d'])
        #         user_claim = claim_promotion["user__userprofile__fullname"]
        #
        #     card_type = CardType.objects.filter(id=item['card__card_type'])
        #     card_type_name = card_type[0].name if len(card_type) > 0 else ''
        #
        #     #  Build data report
        #     remain_parking_fee = item['check_out_exception__parking_fee'] - redemption if item[
        #                                                                                       'check_out_exception__parking_fee'] - redemption > 0 else 0
        #
        #     parking_fee = item['check_out_exception__parking_fee']
        #
        #     total_Remaining_fee += remain_parking_fee
        #     total_redemp_fee +=redemption
        #     total_parking_fee +=parking_fee
        #
        #     current_row = [item['check_out_time'].strftime("%d%m%Y") + "-{0:000}".format(i + 1),
        #                    item['vehicle_number'],
        #                    # item['vehicle_type'],
        #                    VEHICLE_TYPE_NAME_BY_DECODED_ID[item['vehicle_type']],
        #                    item['card__card_label'],
        #                    card_type_name,
        #                    localtime(item['check_in_time']).strftime("%d/%m/%Y %H:%M:%S") if item[
        #                        'check_in_time'] else '',
        #                    claim_time,
        #                    localtime(item['check_out_time']).strftime("%d/%m/%Y %H:%M:%S") \
        #                        if item['check_out_time'] else '',
        #                    unicode(item['check_out_time'] - item['check_in_time']),
        #                    item['check_out_exception__parking_fee'],
        #                    redemption,
        #                    remain_parking_fee,
        #                    item["check_in_operator__userprofile__fullname"],
        #                    user_claim,
        #                    item["check_out_operator__userprofile__fullname"],
        #                    item["check_in_lane__terminal__name"],
        #                    item["check_out_lane__terminal__name"],
        #                    ''
        #                    ]
        #
        #     sheet.write_row(DATA_STARTING_ROW + i, 0, current_row, number_border_format)
        #     i += 1
        sheet.merge_range(DATA_STARTING_ROW + i, 0, DATA_STARTING_ROW + i, 8, 'TOTAL', number_border_format)
        last_total_row = ['TOTAL'] * 12
        last_total_row[11] = convert_to_currency(total_Remaining_fee)
        last_total_row[10] = convert_to_currency(total_redemp_fee)
        last_total_row[9] = convert_to_currency(total_parking_fee)

        sheet.write_row(DATA_STARTING_ROW + i, 0, last_total_row, number_border_format)

        # Save Excel file and return for downloading
        workbook.close()
        with open(file_path, 'r') as f:
            response = HttpResponse(f.read(), content_type="application/vnd.ms-excel")
            response['Content-Disposition'] = "attachment; filename=%s" % file_name
            return response

    now = datetime.now()
    from_time = datetime(now.year, now.month, now.day).replace(day=1)  # TG bat dau: Dau thang hien tai
    to_time = from_time.replace(hour=23, minute=59, second=59)
    return render(request, 'admin/rp-parkinghourly.html', {'from_time': from_time.strftime("%d/%m/%Y %H:%M:%S"),
                                                           'to_time': to_time.strftime("%d/%m/%Y %H:%M:%S")})


@login_required(redirect_field_name='', login_url='/admin/')
def render_report_parking_hourly_new(request):
    userid = request.user.id
    if notallowreport(userid):
        return render(request, 'admin/nopermission.html')
    def convert_to_currency(i):
        return "{0}".format("{:,}".format(i))

    if 'btn_REPORT' in request.POST:
        from_time = request.POST['from_time']#
        to_time = request.POST['to_time']
        try:
            _from = datetime.strptime(from_time, "%d/%m/%Y %H:%M:%S")  # Date
            _to = datetime.strptime(to_time, "%d/%m/%Y %H:%M:%S")
        except Exception as e:
            messages.error(request, u'Khoảng thời gian không phù hợp!', fail_silently=True)
            return HttpResponseRedirect('')

        # Create a new Excel file
        folder_name = 'templates/report'
        file_name = 'Pakring_Hourly_New.xlsx'
        file_path = '%s/%s' % (folder_name, file_name)

        if not os.path.exists(folder_name):
            os.mkdir(folder_name)

        if os.path.isfile(file_path):
            os.remove(file_path)

        workbook = Workbook(file_path, {'constant_memory': False})

        bold = workbook.add_format({'bold': True})
        border = workbook.add_format({'border': 1})
        bold_border = workbook.add_format({'bold': True, 'border': 1})
        bold_center = workbook.add_format({'bold': True, 'align': 'center','border': 1})
        right_center = workbook.add_format({'bold': True, 'align': 'right', 'border': 1})
        number_border = workbook.add_format({'num_format': '#,###0', 'border': 1, 'align': 'right'})
        bold_number_border = workbook.add_format({'num_format': '#,###0', 'border': 1, 'bold': True, 'align': 'right'})
        number_border_format = workbook.add_format({'num_format': '#,###0', 'border': 1})
        date_border_format = workbook.add_format({'num_format': 'dd/mm/yyyy', 'border': 1})
        bold_two_decimal_place_format = workbook.add_format({'num_format': '#,###0.00', 'border': 1, 'bold': True, 'align': 'right'})
        two_decimal_place_format = workbook.add_format({'num_format': '#,###0.00', 'border': 1, 'align': 'right'})

        sheet = workbook.add_worksheet('Parking Hour')
        card_types = CardType.objects.all()

        # Write headers
        sheet.merge_range(0, 0, 2, 12, "Saigon Centre Phase II \r\n Property Management", bold)
        sheet.merge_range(3, 0, 5, 12, "PARKING HOURLY DETAIL RECORD \r\n From: {0} To: {1}".format(from_time, to_time), bold_center)
        start_col = 0
        end_col = len(card_types)
        sheet.merge_range(7, start_col, 7, end_col, "Card Issued", bold_center)
        sheet.write(8, 0, 'Total', border)
        i = 0
        for card_type in card_types:
            i += 1
            sheet.write(8, i, card_type.name)

        # start_col = end_col + 1
        # end_col += 2
        # sheet.merge_range(7, start_col, 7, end_col, "Period", bold_center)
        # sheet.write(8, start_col, 'From', border)
        # sheet.write(8, start_col + 1, 'To', border)
        start_col = end_col + 1
        end_col += 1
        sheet.merge_range(7, start_col, 7, end_col, "Period", bold_center)
        sheet.write(8, start_col, 'Date', border)

        start_col = end_col + 1
        end_col += 1
        sheet.merge_range(7, start_col, 8, end_col, "Revenue \r\n In VND", bold_center)

        start_col = end_col + 1
        end_col += 1
        sheet.merge_range(7, start_col, 8, end_col, "Adjust \r\n In VND", bold_center)

        start_col = end_col + 1
        end_col += 1
        sheet.merge_range(7, start_col, 8, end_col, "Adjust Revenue \r\n In VND", bold_center)

        start_col = end_col + 1
        end_col += 1
        sheet.merge_range(7, start_col, 8, end_col, "Exclude \r\n VAT", bold_center)

        start_col = end_col + 1
        end_col += 3
        sheet.merge_range(7, start_col, 7, end_col, "Certificate", bold_center)
        sheet.write(8, start_col, 'Invoice', border)
        sheet.write(8, start_col + 1, 'Receipt', border)
        sheet.write(8, start_col + 2, 'Date', border)

        HEADER_ENDING_ROW = 8
        sheet.set_column(0, 4, 15)
        sheet.set_column(5, 18, 20)

        # Write report data
        DATA_STARTING_ROW = HEADER_ENDING_ROW + 1
        total_parking_fee = 0
        i = 0
        _from_time = _from
        total_row = []
        card_type_counts = {}
        for card_type in card_types:
            card_type_counts[card_type.id] = 0

        total_count = 0
        total_revenue= 0
        total_adjustment = 0
        total_adjust_revenue= 0
        total_exclude_vat = 0
        total_day = (_to - _from).days + 1
        exclude_vat_col = 0
        while _from_time <= _to:
            _to_time = _from_time + timedelta(days=1)
            fee_session_by_day = ParkingFeeSession.objects.filter(payment_date__gte=_from_time, payment_date__lt=_to_time,
                                                session_type='OUT').values('parking_fee',
                                                                           'parking_session__card__card_type')

            exception_check_out_by_day = ParkingSession.objects.filter(check_out_time__gte=_from_time, check_out_time__lt=_to_time,
                             check_out_exception_id__isnull=False).values('check_out_exception__parking_fee',
                                                                          'card__card_type')  # Xe ra ngoai le

            adjustment_by_day = FeeAdjustment.objects.filter(time__gte=_from_time, time__lt=_to_time).values('fee')  # phi dieu chinh

            day_count = 0
            day_revenue = 0
            day_adjustment = 0
            day_adjusted_revenue = 0
            day_exclude_vat = 0

            day_card_type_counts = {}
            for card_type in card_types:
                day_card_type_counts[card_type.id] = 0

            for item in fee_session_by_day:
                day_count += 1
                day_revenue += item['parking_fee']
                card_type_id = item['parking_session__card__card_type']
                card_type_counts[card_type_id] += 1
                day_card_type_counts[card_type_id] += 1

            for item in exception_check_out_by_day:
                day_count += 1
                day_revenue += item['check_out_exception__parking_fee']
                card_type_id = item['card__card_type']
                card_type_counts[card_type_id] += 1
                day_card_type_counts[card_type_id] += 1

            for item in adjustment_by_day:
                day_adjustment += item['fee']
            ##old before 2018-01-16
            #day_adjusted_revenue = day_revenue - day_adjustment
            ##old before 2018-01-16
            ##new 2018-01-16
            day_adjusted_revenue = day_revenue + day_adjustment
            ##new 2018-01-16
            day_exclude_vat = day_adjusted_revenue / 1.1

            total_count += day_count
            total_revenue += day_revenue
            total_adjustment += day_adjustment
            total_adjust_revenue += day_adjusted_revenue
            total_exclude_vat += day_exclude_vat

            current_row = [day_count]
            current_row.extend(day_card_type_counts.values())
            current_row.append(_from_time.strftime("%d/%m/%Y"))
            # current_row.append(_to_time.strftime("%d/%m/%Y"))

            # Revenue
            # total = (fee_session_by_day.aggregate(Sum('parking_fee'))['parking_fee__sum'] or 0) + \
            #         (exception_check_out_by_day.aggregate(Sum('check_out_exception__parking_fee'))['check_out_exception__parking_fee__sum'] or 0)
            current_row.append(day_revenue)
            # total_revenue += total

            # Adjustment
            # adjust_fee = adjustment_by_day.aggregate(Sum('fee'))['fee__sum'] or 0
            current_row.append(day_adjustment)
            # total_adjustment += adjust_fee

            # Adjusted revenue
            # adjusted_revenue = total - adjust_fee
            current_row.append(day_adjusted_revenue)
            # total_adjust_revenue += adjusted_revenue

            # Exclude VAT
            # exclude_vat = adjusted_revenue / 1.1
            current_row.append(day_exclude_vat)
            # total_exclude_vat += exclude_vat

            current_row.extend(['','',''])
            sheet.write_row(DATA_STARTING_ROW + i, 0, current_row, number_border_format)

            i += 1
            _from_time = _to_time

        # sheet.merge_range(DATA_STARTING_ROW + i, 0, DATA_STARTING_ROW + i, 8, 'TOTAL', number_border_format)

        grand_total_col = 1
        grand_total_row = DATA_STARTING_ROW + i + 1
        # last_data_row_idx = grand_total_row - 1
        # sheet.write_formula(get_cell_name(grand_total_col, last_data_row_idx + 1), '=SUM(' + get_cell_name(grand_total_col, 10) + ':' + get_cell_name(grand_total_col, last_data_row_idx)+ ')', bold_number_border)
        # for card_type in card_types:
        #     grand_total_col += 1
        #     sheet.write_formula(get_cell_name(grand_total_col, last_data_row_idx + 1),
        #                         '=SUM(' + get_cell_name(grand_total_col, 10) + ':' + get_cell_name(grand_total_col,
        #                                                                                            last_data_row_idx) + ')', bold_number_border)
        #
        # grand_total_col += 3
        # for col in range(grand_total_col, grand_total_col+4):
        #     sheet.write_formula(get_cell_name(col, last_data_row_idx + 1),
        #                         '=SUM(' + get_cell_name(col, 10) + ':' + get_cell_name(col,last_data_row_idx) + ')', bold_number_border)

        total_row.append(total_count)
        total_row.extend(card_type_counts.values())
        total_row.extend([''])
        total_row.append(total_revenue)
        total_row.append(total_adjustment)
        total_row.append(total_adjust_revenue)
        total_row.append(total_exclude_vat)

        # sheet.set_column(exclude_vat_col, exclude_vat_col, 20, two_decimal_place_format)
        total_row.extend(['', '', ''])
        sheet.write_row(DATA_STARTING_ROW + i, 0, total_row, bold_number_border)

        grand_total_row +=2
        sheet.merge_range('A'+str(grand_total_row) +':C' + str(grand_total_row), 'Ave\' hourly card issued = ',right_center)
        sheet.write(grand_total_row - 1, 3, float(total_row[1]) / total_day, two_decimal_place_format)
        sheet.merge_range('E' + str(grand_total_row) + ':F' + str(grand_total_row), 'hourly card/day', right_center)
        grand_total_row += 1
        sheet.merge_range('A' + str(grand_total_row)+ ':C' + str(grand_total_row), 'Ave\' hourly income = ', right_center)
        sheet.write(grand_total_row - 1, 3, float(total_adjust_revenue) / total_row[1] if total_row[1] > 0 else 0, two_decimal_place_format)
        sheet.merge_range('E' + str(grand_total_row) + ':F' + str(grand_total_row), 'VND/card', right_center)
        grand_total_row += 1
        sheet.merge_range('A' + str(grand_total_row)+ ':C' + str(grand_total_row), 'Ave\' daily hourly income = ', right_center)
        sheet.write(grand_total_row - 1, 3, float(total_adjust_revenue) / total_day, two_decimal_place_format)
        sheet.merge_range('E' + str(grand_total_row) + ':F' + str(grand_total_row), 'VND/Day', right_center)

        grand_total_row += 2
        sheet.merge_range('A'+str(grand_total_row)+ ':F' + str(grand_total_row), 'SUMMARY', bold_center)
        grand_total_row += 1
        sheet.merge_range('A'+str(grand_total_row)+ ':D' + str(grand_total_row), 'Hourly parking (excl. VAT) = ', bold_center)
        sheet.merge_range('E' + str(grand_total_row) + ':F' + str(grand_total_row), total_exclude_vat, bold_two_decimal_place_format)
        grand_total_row += 1
        sheet.merge_range('A'+str(grand_total_row)+ ':D' + str(grand_total_row), 'TOTAL CARPARK INCOME = ', bold_center)
        sheet.merge_range('E' + str(grand_total_row) + ':F' + str(grand_total_row), total_exclude_vat, bold_two_decimal_place_format)

        grand_total_row += 2
        sheet.write('A'+str(grand_total_row), 'Prepared by:')
        sheet.write('F'+str(grand_total_row), 'Checked by:')
        sheet.write('I'+str(grand_total_row), 'Approved by:')

        # grand_total_row += 4
        # sheet.write('A'+str(grand_total_row), 'Truong Thuy Lan Ngoc')
        # sheet.write('F'+str(grand_total_row), 'Le Ngoc Dieu Thao')
        # sheet.write('I'+str(grand_total_row), 'Vuong Cam Sinh')

        # Save Excel file and return for downloading
        workbook.close()
        with open(file_path, 'r') as f:
            response = HttpResponse(f.read(), content_type="application/vnd.ms-excel")
            response['Content-Disposition'] = "attachment; filename=%s" % file_name
            return response

    now = datetime.now()
    from_time = datetime(now.year, now.month, now.day).replace(day=1)  # TG bat dau: Dau thang hien tai
    to_time = from_time.replace(hour=23, minute=59, second=59)
    return render(request, 'admin/rp-parkinghourly_new.html', {'from_time': from_time.strftime("%d/%m/%Y %H:%M:%S"),
                                                               'to_time': to_time.strftime("%d/%m/%Y %H:%M:%S")})


@login_required(redirect_field_name='', login_url='/admin/')
def render_report_parking_redemption_new(request):
    userid = request.user.id
    if notallowreport(userid):
        return render(request, 'admin/nopermission.html')
    def convert_to_currency(i):
        return "{0}".format("{:,}".format(i))

    if 'btn_REPORT' in request.POST:
        from_time = request.POST['from_time']
        to_time = request.POST['to_time']
        try:
            _from = datetime.strptime(from_time, "%d/%m/%Y %H:%M:%S")  # Date
            _to = datetime.strptime(to_time, "%d/%m/%Y %H:%M:%S")
        except Exception as e:
            messages.error(request, u'Khoảng thời gian không phù hợp!', fail_silently=True)
            return HttpResponseRedirect('')

        # Create a new Excel file
        folder_name = 'templates/report'
        file_name = 'Pakring_Redemption_New.xlsx'
        file_path = '%s/%s' % (folder_name, file_name)

        if not os.path.exists(folder_name):
            os.mkdir(folder_name)

        if os.path.isfile(file_path):
            os.remove(file_path)

        workbook = Workbook(file_path, {'constant_memory': False})

        bold = workbook.add_format({'bold': True})
        border = workbook.add_format({'border': 1})
        bold_border = workbook.add_format({'bold': True, 'border': 1})
        bold_center = workbook.add_format({'bold': True, 'align': 'center','border': 1})
        right_center = workbook.add_format({'bold': True, 'align': 'right', 'border': 1})
        number_border = workbook.add_format({'num_format': '#,###0', 'border': 1, 'align': 'right'})
        bold_number_border = workbook.add_format({'num_format': '#,###0', 'border': 1, 'bold': True, 'align': 'right'})
        number_border_format = workbook.add_format({'num_format': '#,###0', 'border': 1})
        date_border_format = workbook.add_format({'num_format': 'dd/mm/yyyy', 'border': 1})
        bold_two_decimal_place_format = workbook.add_format({'num_format': '#,###0.00', 'border': 1, 'bold': True, 'align': 'right'})
        two_decimal_place_format = workbook.add_format({'num_format': '#,###0.00', 'border': 1, 'align': 'right'})

        sheet = workbook.add_worksheet('Parking Redemption')
        card_types = CardType.objects.all()

        # Write headers
        sheet.merge_range(0, 0, 2, 12, 'Saigon Centre Phase II \n Property Management', bold)
        sheet.merge_range(3, 0, 5, 12, "PARKING REDEMPTION DETAIL RECORD \n From: {0} To: {1}".format(from_time, to_time), bold_center)
        start_col = 0
        end_col = 0
        sheet.write(7, start_col, "Period", bold_center)
        sheet.write(8, start_col, 'Date', border)

        start_col = end_col + 1
        end_col += 1
        sheet.merge_range(7, start_col, 8, end_col, "Transaction No.", bold_center)

        start_col = end_col + 1
        end_col += 1
        sheet.merge_range(7, start_col, 8, end_col, "Parking Fee", bold_center)

        start_col = end_col + 1
        end_col += 1
        sheet.merge_range(7, start_col, 8, end_col, "Redemption Fee", bold_center)

        start_col = end_col + 1
        end_col += 1
        sheet.merge_range(7, start_col, 8, end_col, 'Collected Fee \r\n In VND', bold_center)

        start_col = end_col + 1
        end_col += 1
        sheet.merge_range(7, start_col, 8, end_col, "Adjusted \r\n In VND", bold_center)

        start_col = end_col + 1
        end_col += 1
        sheet.merge_range(7, start_col, 8, end_col, "Adjusted Fee \r\n In VND", bold_center)

        start_col = end_col + 1
        end_col += 1
        sheet.merge_range(7, start_col, 8, end_col, "Exclude \r\n VAT", bold_center)

        start_col = end_col + 1
        end_col += 3
        sheet.merge_range(7, start_col, 7, end_col, "Certificate", bold_center)
        sheet.write(8, start_col, 'Invoice', border)
        sheet.write(8, start_col + 1, 'Receipt', border)
        sheet.write(8, start_col + 2, 'Date', border)

        HEADER_ENDING_ROW = 8
        sheet.set_column(0, 4, 15)
        sheet.set_column(5, 18, 20)

        # Write report data
        DATA_STARTING_ROW = HEADER_ENDING_ROW + 1
        i = 0
        _from_time = _from
        total_row = []
        card_type_counts = {}
        for card_type in card_types:
            card_type_counts[card_type.id] = 0

        total_count = 0
        total_parking_fee = 0
        total_redemption = 0
        total_remain_fee = 0
        total_adjust = 0
        total_adjust_revenue= 0
        total_exclude_vat = 0
        total_day = (_to - _from).days + 1
        exclude_vat_col = 0
        while _from_time <= _to:
            _to_time = _from_time + timedelta(days=1)
            redemption_fee_by_day = ClaimPromotionV2.objects.filter(used=True, server_time__gte=_from_time,
                                                         server_time__lte=_to_time).order_by('server_time').values('id',
                                                                                                                   'amount_a',
                                                                                                                   'amount_b',
                                                                                                                   'amount_c',
                                                                                                                   'amount_d',
                                                                                                                   'amount_e')

            day_count = 0
            day_parking_fee = 0
            day_redemption = 0
            day_remain_fee = 0
            day_adjust = 0
            day_exclude_vat = 0

            day_card_type_counts = {}
            for card_type in card_types:
                day_card_type_counts[card_type.id] = 0

            for item in redemption_fee_by_day:
                day_count += 1
                parking_fee = item['amount_a']
                redemption = item['amount_b'] + item['amount_c'] + item['amount_d']
                day_parking_fee += parking_fee
                day_redemption += -redemption
                remain = parking_fee + redemption
                day_remain_fee += remain if remain > 0 else 0

            day_adjusted_revenue = day_remain_fee
            day_exclude_vat = day_adjusted_revenue / 1.1

            total_count += day_count
            total_parking_fee += day_parking_fee
            total_redemption += day_redemption
            total_remain_fee += day_remain_fee
            total_adjust += day_adjust
            total_adjust_revenue += day_adjusted_revenue
            total_exclude_vat += day_exclude_vat

            current_row = [_from_time.strftime("%d/%m/%Y")]
            current_row.append(day_count)
            current_row.append(day_parking_fee)
            current_row.append(day_redemption)
            current_row.append(day_remain_fee)
            current_row.append(day_adjust)
            current_row.append(day_adjusted_revenue)
            current_row.append(day_exclude_vat)
            # total_exclude_vat += exclude_vat

            current_row.extend(['','',''])
            sheet.write_row(DATA_STARTING_ROW + i, 0, current_row, number_border_format)

            i += 1
            _from_time = _to_time

        # sheet.merge_range(DATA_STARTING_ROW + i, 0, DATA_STARTING_ROW + i, 8, 'TOTAL', number_border_format)

        grand_total_col = 1
        grand_total_row = DATA_STARTING_ROW + i + 1
        # last_data_row_idx = grand_total_row - 1
        # sheet.write_formula(get_cell_name(grand_total_col, last_data_row_idx + 1), '=SUM(' + get_cell_name(grand_total_col, 10) + ':' + get_cell_name(grand_total_col, last_data_row_idx)+ ')', bold_number_border)
        # for card_type in card_types:
        #     grand_total_col += 1
        #     sheet.write_formula(get_cell_name(grand_total_col, last_data_row_idx + 1),
        #                         '=SUM(' + get_cell_name(grand_total_col, 10) + ':' + get_cell_name(grand_total_col,
        #                                                                                            last_data_row_idx) + ')', bold_number_border)
        #
        # grand_total_col += 3
        # for col in range(grand_total_col, grand_total_col+4):
        #     sheet.write_formula(get_cell_name(col, last_data_row_idx + 1),
        #                         '=SUM(' + get_cell_name(col, 10) + ':' + get_cell_name(col,last_data_row_idx) + ')', bold_number_border)

        total_row.extend([''])
        total_row.append(total_count)
        total_row.append(total_parking_fee)
        total_row.append(total_redemption)
        total_row.append(total_remain_fee)
        total_row.append(total_adjust)
        total_row.append(total_adjust_revenue)
        total_row.append(total_exclude_vat)

        # sheet.set_column(exclude_vat_col, exclude_vat_col, 20, two_decimal_place_format)
        total_row.extend(['', '', ''])
        sheet.write_row(DATA_STARTING_ROW + i, 0, total_row, bold_number_border)

        # grand_total_row +=2
        # sheet.merge_range('A'+str(grand_total_row) +':C' + str(grand_total_row), 'Ave\' hourly card issued = ',right_center)
        # sheet.write(grand_total_row - 1, 3, float(total_row[1]) / total_day, two_decimal_place_format)
        # sheet.merge_range('E' + str(grand_total_row) + ':F' + str(grand_total_row), 'hourly card/day', right_center)
        # grand_total_row += 1
        # sheet.merge_range('A' + str(grand_total_row)+ ':C' + str(grand_total_row), 'Ave\' hourly income = ', right_center)
        # sheet.write(grand_total_row - 1, 3, float(total_adjust_revenue) / total_row[1] if total_row[1] > 0 else 0, two_decimal_place_format)
        # sheet.merge_range('E' + str(grand_total_row) + ':F' + str(grand_total_row), 'VND/card', right_center)
        # grand_total_row += 1
        # sheet.merge_range('A' + str(grand_total_row)+ ':C' + str(grand_total_row), 'Ave\' daily hourly income = ', right_center)
        # sheet.write(grand_total_row - 1, 3, float(total_adjust_revenue) / total_day, two_decimal_place_format)
        # sheet.merge_range('E' + str(grand_total_row) + ':F' + str(grand_total_row), 'VND/Day', right_center)

        grand_total_row += 2
        sheet.merge_range('A'+str(grand_total_row)+ ':F' + str(grand_total_row), 'SUMMARY', bold_center)
        grand_total_row += 1
        sheet.merge_range('A'+str(grand_total_row)+ ':D' + str(grand_total_row), 'Hourly parking (excl. VAT) = ', bold_center)
        sheet.merge_range('E' + str(grand_total_row) + ':F' + str(grand_total_row), total_exclude_vat, bold_two_decimal_place_format)
        grand_total_row += 1
        sheet.merge_range('A'+str(grand_total_row)+ ':D' + str(grand_total_row), 'TOTAL CARPARK INCOME = ', bold_center)
        sheet.merge_range('E' + str(grand_total_row) + ':F' + str(grand_total_row), total_exclude_vat, bold_two_decimal_place_format)

        grand_total_row += 2
        sheet.write('A'+str(grand_total_row), 'Prepared by:')
        sheet.write('F'+str(grand_total_row), 'Checked by:')
        sheet.write('I'+str(grand_total_row), 'Approved by:')

        # grand_total_row += 4
        # sheet.write('A'+str(grand_total_row), 'Truong Thuy Lan Ngoc')
        # sheet.write('F'+str(grand_total_row), 'Le Ngoc Dieu Thao')
        # sheet.write('I'+str(grand_total_row), 'Vuong Cam Sinh')

        # Save Excel file and return for downloading
        workbook.close()
        with open(file_path, 'r') as f:
            response = HttpResponse(f.read(), content_type="application/vnd.ms-excel")
            response['Content-Disposition'] = "attachment; filename=%s" % file_name
            return response

    now = datetime.now()
    from_time = datetime(now.year, now.month, now.day).replace(day=1)  # TG bat dau: Dau thang hien tai
    to_time = from_time.replace(hour=23, minute=59, second=59)
    return render(request, 'admin/rp-parking-redemption-new.html', {'from_time': from_time.strftime("%d/%m/%Y %H:%M:%S"),
                                                               'to_time': to_time.strftime("%d/%m/%Y %H:%M:%S")})

### bắt đầu thay thế: 2018-01-14
@login_required(redirect_field_name='', login_url='/admin/')
def render_report_parking_session(request):  # Bao cao phi ve thang
    userid = request.user.id
    if notallowreport(userid):
        return render(request, 'admin/nopermission.html')
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

    def calculate_fee_session(cancel_date, from_date, to_date, level_fee):
        fee = 0
        cancel_fee = 0
        # Check report time in range cancellation time
        if cancel_date and cancel_date < to_date:
            num_month = 0
            # If cancel_date earlier than from time, calculate month between from_time and to_time
            # else calculate time between cancel_date and to_time
            if cancel_date < from_date:
                num_month = count_months_between_dates(from_date.month, from_date.year, to_date.month, to_date.year) + 1
            else:
                num_month = count_months_between_dates(cancel_date.month, cancel_date.year, to_date.month, to_date.year)

                # If cancellation time earlier than day 15 in month, return full fee else return half
                if from_date.month == cancel_date.month and cancel_date.day <= 15:
                    cancel_fee = level_fee / 2

            # Calculate cancellation fee
            cancel_fee += num_month * level_fee

        while from_date <= to_date:
            last_date_current_month = make_last_date_of_month(from_date, 1)
            if last_date_current_month >= to_date:
                last_date_current_month = to_date

            num_current_month_payable_days = last_date_current_month.day - from_date.day + 1

            if num_current_month_payable_days == get_num_days_of_month(last_date_current_month.year,
                                                                       last_date_current_month.month):  # Tron 1 thang
                fee += int(level_fee)
            else:
                price_per_day = float(level_fee) / get_num_days_of_month(from_date.year, from_date.month)

                fee += ceil(num_current_month_payable_days * price_per_day / 1000) * 1000

            from_date = last_date_current_month + timedelta(days=1)
        fee = int(fee)
        return [fee, cancel_fee]

    vehicle_type_data = list()

    vehicle_types = VehicleType.objects.all()
    for type in vehicle_types:
        vehicle_type_data.append({"value": type.id, "name": type.name})

    now = datetime.now()
    from_time = datetime(now.year, now.month, now.day).replace(day=1)  # TG bat dau: Dau thang hien tai

    month = now.month
    year = now.year + month / 12

    try:
        month = (month + 1) % 12
        if month == 0: month = 12
    except:
        pass
    to_time = from_time.replace(year=year, month=month, day=1, hour=23) + timedelta(
        days=-1)  # TG ket thuc: Cuoi thang hien tai

    customer_type_data = list()

    customer_types = CustomerType.objects.all()
    customer_type_data.append({"value": "", "name": "Tất cả"})
    for customer_type in customer_types:
        customer_type_data.append({"value": customer_type.name, "name": customer_type.name})

    payment_method_data = [{"value": "", "name": "Tất cả"},
                           {"value": "TM", "name": "Tiền mặt"},
                           {"value": "CK", "name": "Chuyển khoản"}]

    def cal_feesession(canceleddate, fromdate, todate, feelevel):
        if feelevel == 0 or fromdate is None or todate is None:
            return [0, 0]
        bill = getfeeByMonthSGCT(fromdate, todate, feelevel)
        canceled_fee=None
        if canceleddate is None:
            canceled_fee = 0
        else:
            if canceleddate < fromdate:
                if canceleddate.month == fromdate.month and canceleddate.year == fromdate.year and canceleddate.day <= 15:
                    canceled_fee = feelevel / 2
                else:
                    canceled_fee = bill
            else:
                if canceleddate.day <= 15:
                    canceled_fee = feelevel / 2
                else:
                    canceled_fee = 0
        return [bill, canceled_fee]
    if 'btn_REPORT_DETAIL' in request.POST:
        def add_worksheet_stat(sheet_name, workbook, TEMPLATE, rs, rs2, custom_param, sheet_protect=True):
            LOGO_EXCEL_REPORT_PATH = 'parking/static/image/' + get_setting('logo_report',
                                                                           u'Mẫu logo dùng trong báo cáo',
                                                                           'logo_report.png')
            LOGO_EXCEL_REPORT_PATH_RIGHT = 'parking/static/image/' + get_setting('logo_repor',
                                                                                 u'Mẫu logo dùng trong báo cáo',
                                                                                 'logo_report_right.png')
            TITLE = TEMPLATE['TITLE']
            TOP = TEMPLATE['TOP']
            VEHICLE_TYPE = TEMPLATE['VEHICLE_TYPE']
            HEADER = TEMPLATE['HEADER']
            DETAIL = TEMPLATE['DETAIL']

            sheet = workbook.add_worksheet(sheet_name)
            sheet.insert_image(0, 0, LOGO_EXCEL_REPORT_PATH)
            sheet.insert_image(0, len(HEADER), LOGO_EXCEL_REPORT_PATH_RIGHT)
            bold = workbook.add_format({'bold': True})
            wrap = workbook.add_format()
            wrap.set_text_wrap()
            border = workbook.add_format()
            border.set_border()
            bold_border = workbook.add_format({'bold': True, 'border': 1})
            bold_center_border = workbook.add_format({'align': 'center', 'bold': True, 'border': 1})
            bold_center_border.set_pattern(1)  # This is optional when using a solid fill.
            bold_center_border.set_bg_color('#ffffff')
            number_border_format = workbook.add_format({'num_format': '#,###0', 'border': 1})
            bold_number_border_format = workbook.add_format({'num_format': '#,###0', 'border': 1, 'bold': True})

            cancelled_cell = workbook.add_format({'bg_color': 'red', 'border': 1})

            for r in TITLE:  # Viet tieu de: (dong r[0], cot r[1])
                sheet.write(r[0], r[1], r[2], bold)
                if len(r) == 4:
                    sheet.write(r[0], r[1] + 1, r[3])

            sheet.write_row(5, 0, [''] * 13, bold_center_border)

            for i in xrange(len(HEADER)):
                sheet.write(7, i, HEADER[i][0], bold_center_border)
                sheet.set_column(i, i, 20)  # Default width

            sheet.write_row(7, len(HEADER), DETAIL, bold_center_border)

            i = 0
            total_sum = 0

            list_val = [w[1] for w in TEMPLATE['HEADER'] if
                        len(w) == 2]  # Co field query hoac custom function

            list_val2 = [w[1] for w in TEMPLATE['HEADER2'] if
                         len(w) == 2]  # Co field query hoac custom function

            rs_value_list = enumerate(rs.values(*list_val))  # Query data
            rs2_value_list = enumerate(rs2.values(*list_val2))
            rs = list(rs)
            rs2 = list(rs2)
            all_receipt = Receipt.objects.all()
            _from = _from_time.date()
            _to = _to_time.date()

            for k, r in rs_value_list:
                temp_data = list()
                count_newline = 0

                cancelled_receipts = None
                for j in range(0, len(TEMPLATE_SHEET_PARKING_FEE_DETAIL['HEADER'])):
                    column = TEMPLATE_SHEET_PARKING_FEE_DETAIL['HEADER'][j]
                    if column[0] == u'NO.':  # Cell so thu tu tang dan
                        i += 1
                        temp_data.append(i)
                    elif len(column) == 2 and column[1] in r:
                        # Nhung cot co dinh nghia query field
                        temp_value = r[column[1]]
                        if isinstance(temp_value, date):  # Du lieu cell kieu datetime.date
                            temp_value = temp_value.strftime("%d/%m/%Y")

                        if column[1] == 'ticket_payment_id':
                            cancelled_receipts = all_receipt.filter(type=0, ref_id=int(temp_value),
                                                                    cancel=True).values_list('receipt_number',
                                                                                             'action_date', 'notes')
                            if cancelled_receipts:
                                temp_value = " ,".join(str(w[0]) for w in cancelled_receipts)
                            else:
                                temp_value = ""

                        if temp_value and column[0] == u'Số':
                            temp_value = int(temp_value)
                        temp_data.append(temp_value)

                        count_newline = max(count_newline, temp_value.count('\n')) if (
                            isinstance(temp_value, str) or isinstance(temp_value, unicode)) else count_newline

                    else:  # Cot rong
                        temp_data.append('')  # Cell trong

                # Get card type name
                card_type = CardType.objects.filter(id=temp_data[10])
                temp_data[10] = card_type[0].name if len(card_type) > 0 else ''

                row = rs[k]
                month_duration = row.duration / 30
                day_duration = row.day_duration
                payment_detail_fee = row.payment_detail_fee or 0

                # Calculate time duration from report
                if _from < row.effective_date:
                    tmp_from_time = row.effective_date
                else:
                    tmp_from_time = _from

                if _to > row.expired_date:
                    tmp_to_time = row.expired_date
                else:
                    tmp_to_time = _to

                payment_result = calculate_fee_session(row.cancel_date, tmp_from_time, tmp_to_time, row.level_fee)

                temp_data.append(u"%s tháng %s ngày" % (month_duration, day_duration))
                temp_data.append(payment_result[0])
                temp_data.append(payment_result[1])
                total_sum += payment_result[0]

                sheet.write_row(8 + i, 0, temp_data,
                                number_border_format)  # Ghi toan dong du lieu (xuat phat tu dong 6)

                if cancelled_receipts:
                    for cancelled_receipt in cancelled_receipts:
                        i += 1
                        temp_data = [i, cancelled_receipt[0],
                                     cancelled_receipt[1].strftime("%d/%m/%Y") if cancelled_receipt[1] else "",
                                     u"HUỶ%s" % (": " + cancelled_receipt[2] if cancelled_receipt[2] else "")]
                        temp_data += [''] * (10 + 3 * len(vehicle_type_list))
                        sheet.write_row(8 + i, 0, temp_data, cancelled_cell)

            for k, r in rs2_value_list:  # Coc
                temp_data = list()
                count_newline = 0
                cancelled_receipts = None
                for j in range(0, len(TEMPLATE_SHEET_PARKING_FEE_DETAIL['HEADER2'])):
                    column = TEMPLATE_SHEET_PARKING_FEE_DETAIL['HEADER2'][j]
                    if column[0] == u'NO.':  # Cell so thu tu tang dan
                        i += 1
                        temp_data.append(i)
                    elif len(column) == 2 and column[1] in r:
                        # Nhung cot co dinh nghia query field
                        temp_value = r[column[1]]
                        if isinstance(temp_value, date):  # Du lieu cell kieu datetime.date
                            temp_value = temp_value.strftime("%d/%m/%Y")

                        if column[1] == 'deposit_payment_id':
                            cancelled_receipts = all_receipt.filter(type=1, ref_id=int(temp_value),
                                                                    cancel=True).values_list('receipt_number',
                                                                                             'action_date', 'notes')
                            if cancelled_receipts:
                                temp_value = " ,".join(str(w[0]) for w in cancelled_receipts)
                            else:
                                temp_value = ""

                        temp_data.append(temp_value)

                        count_newline = max(count_newline, temp_value.count('\n')) if (
                            isinstance(temp_value, str) or isinstance(temp_value, unicode)) else count_newline
                    else:  # Cot rong
                        temp_data.append('')  # Cell trong

                # Get card type name
                card_type = CardType.objects.filter(id=temp_data[10])
                temp_data[10] = card_type[0].name if len(card_type) > 0 else ''

                row = rs2[k]
                deposit_action_fee = row.deposit_action_fee.fee if row.deposit_action_fee else 0
                temp_data.append('')
                temp_data.append(deposit_action_fee)
                total_sum += deposit_action_fee
                sheet.write_row(8 + i, 0, temp_data,
                                number_border_format)  # Ghi toan dong du lieu (xuat phat tu dong 6)

                if cancelled_receipts:
                    for cancelled_receipt in cancelled_receipts:
                        i += 1
                        temp_data = [i, cancelled_receipt[0],
                                     cancelled_receipt[1].strftime("%d/%m/%Y") if cancelled_receipt[1] else "",
                                     u"HUỶ%s" % (": " + cancelled_receipt[2] if cancelled_receipt[2] else "")]
                        temp_data += [''] * (10 + 3 * len(vehicle_type_list))
                        sheet.write_row(8 + i, 0, temp_data,
                                        cancelled_cell)  # Ghi toan dong du lieu (xuat phat tu dong 6)

            i += 1
            temp_sum_data = [''] * len(TEMPLATE_SHEET_PARKING_FEE_DETAIL['HEADER'])
            temp_sum_data += ['']
            temp_sum_data.append(total_sum)
            sheet.write_row(8 + i, 0, temp_sum_data,
                            bold_number_border_format)  # Ghi toan dong du lieu (xuat phat tu dong 6)

            sheet.write(8 + i + 1, TOP[1][1]['col'], u"Ngày lập báo cáo", bold)
            sheet.write(8 + i + 1, TOP[1][1]['col'] + 1, now.strftime("%d/%m/%Y"))

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
        ##2018-01-12 begin
        def add_sheet(sheetname,workbook,fdate,tdate,cus_type,payment_type,sheet_protect=True):
            util = Utilities()
            dts = util.Query("parkingsessionreportdetail", fdate, tdate, cus_type, payment_type)
            LOGO_EXCEL_REPORT_PATH = 'parking/static/image/' + get_setting('logo_report',
                                                                           u'Mẫu logo dùng trong báo cáo',
                                                                           'logo_report.png')
            LOGO_EXCEL_REPORT_PATH_RIGHT = 'parking/static/image/' + get_setting('logo_repor',
                                                                                 u'Mẫu logo dùng trong báo cáo',
                                                                                 'logo_report_right.png')
            sheet = workbook.add_worksheet(sheetname)
            sheet.insert_image(0, 0, LOGO_EXCEL_REPORT_PATH)
            sheet.insert_image(0,21, LOGO_EXCEL_REPORT_PATH_RIGHT)
            bold = workbook.add_format({'bold': True})
            wrap = workbook.add_format()
            wrap.set_text_wrap()
            border = workbook.add_format()
            border.set_border()
            bold_border = workbook.add_format({'bold': True, 'border': 1})
            bold_center_border = workbook.add_format({'align': 'center', 'bold': True, 'border': 1})
            bold_center_border.set_pattern(1)  # This is optional when using a solid fill.
            bold_center_border.set_bg_color('#ffffff')
            number_border_format = workbook.add_format({'num_format': '#,###0', 'border': 1})
            bold_number_border_format = workbook.add_format({'num_format': '#,###0', 'border': 1, 'bold': True})

            cancelled_cell = workbook.add_format({'bg_color': 'red', 'border': 1})
            title = [u'BÁO CÁO TỔNG PHÍ GIỮ XE THÁNG']
            sheet.write_row(2, 2, title, bold)
            titleContent = [u'Từ', fdate.strftime("%d/%m/%Y"), u'Đến', tdate.strftime("%d/%m/%Y")]
            sheet.write_row(3, 2, titleContent)
            header_row = [u'NO.', u'Customer type', u'Customer name', u'Company', u'Building', u'Driver name',
                          u'old license plate', u'Vehicle number', u'Vehicle type', u'Card code', u'Card type',
                          u'Commencement Date', u'Expiry Date', u'Fee type', u'Payment no.', u'Payment date',
                          u'Cancel date', u'Canceled Recipe', u'Method', u'Remark', u'Bill', u'Cancel fee']
            DATA_STARTING_ROW=6
            sheet.write_row(DATA_STARTING_ROW, 0, header_row, bold_center_border)
            sheet.set_column(1, 1, 20)
            sheet.set_column(2, 2, 30)
            sheet.set_column(3, 21, 15)
            sheet.set_column(5, 6, 30)
            sheet.set_column(6, 7, 35)
            DATA_STARTING_ROW = 7
            tt_bill=0
            tt_cancel_fee=0
            i=0
            for r in dts:
                _from_date =  datetime.strptime(str(r[10]),"%Y-%m-%d").date()
                _to_date = datetime.strptime(str(r[11]),"%Y-%m-%d").date()
                _cancel_date=None
                if r[16]:
                    _cancel_date = datetime.strptime(str(r[16]),"%Y-%m-%d").date()
                feemonth=0
                try:
                    feemonth=float(r[13])
                except:
                    feemonth = 0
                fee_sesion=cal_feesession(_cancel_date,_from_date,_to_date,feemonth)
                bill=fee_sesion[0]
                cancelfee=fee_sesion[1]
                tt_bill=tt_bill+bill
                tt_cancel_fee=tt_cancel_fee+cancelfee
                r2=u''
                if r[2]:
                    r2=r[2]
                r10=u''
                if r[10]:
                    r10=datetime.strptime(str(r[10]),"%Y-%m-%d").date().strftime('%d/%m/%Y')
                r11=u''
                if r[11]:

                    r11 = datetime.strptime(str(r[11]), "%Y-%m-%d").date().strftime('%d/%m/%Y')
                r15=u''
                if r[15]:
                    try:
                        r15 = datetime.strptime(str(r[15]), "%Y-%m-%d T%H:%M:%SZ").date().strftime('%d/%m/%Y %H:%M:%S')
                    except:
                        r15 = str(r[15])
                r16=u''
                if r[16]:
                    r16 = str(r[16])
                    r16=datetime.strptime(str(r[16]), "%Y-%m-%d").date().strftime('%d/%m/%Y')
                    #T%H:%M:%SZ
                r17=u''
                if r[17]:
                    r17=r[17]
                current_row = [i + 1, r[0], r[1], r2, r[3], r[4], r[5], r[6], r[7], r[8], r[9], r10, r11, r[12],
                               r[14], r15, r16, r17, r[18], r[19], bill, cancelfee]
                sheet.write_row(DATA_STARTING_ROW + i, 0, current_row, border)
                i = i + 1
            total_row = [u'', u'', u'', u'', u'', u'', u'', u'', u'', u'', u'', u'', u'', u'',
                           u'', u'', u'', u'', u'', u'', tt_bill, tt_cancel_fee]
            sheet.write_row(DATA_STARTING_ROW + i, 0, total_row, bold_center_border)
            i = i + 1
            title = [u'Ngày lập báo cáo',now.strftime("%d/%m/%Y")]
            sheet.write_row(DATA_STARTING_ROW + i, 5, title, bold)
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
        ##2018-01-12 end
        # TEMPLATE_SHEET_PARKING_FEE_DETAIL = {
        #     'TOP': [
        #         (u'Phiếu thu', {'col': 1, 'h_merge_range': 3}),
        #         [u'Cọc', {'col': 13, 'h_merge_range': 0}],
        #         [u'Phí gửi xe tháng', ],
        #     ],
        #
        #     # 'HEADER': [
        #     #     (u'STT',),
        #     #     (u'Số', 'ticket_payment__receipt_number',),
        #     #     (u'Ngày nộp', 'ticket_payment__payment_date'),
        #     #     (u'Phiếu thu bị huỷ', 'ticket_payment_id'),
        #     #     (u'Tòa nhà', 'ticket_payment__customer__building__name'),
        #     #     (u'Mã căn hộ', 'ticket_payment__customer__apartment__address'),
        #     #     (u'Tên khách hàng', 'ticket_payment__customer__customer_name'),
        #     #     (u'Công ty', 'ticket_payment__customer__company__name'),
        #     #     (u'Loại khách hàng', 'ticket_payment__customer__customer_type__name'),
        #     #     (u'Phương thức', 'ticket_payment__payment_method'),
        #     #     (u'Số xe', 'vehicle_number'),
        #     #     (u'Ngày hiệu lực', 'effective_date'),
        #     #     (u'Hạn hiện tại', 'expired_date'),
        #     # ],
        #     'HEADER': [
        #         (u'NO.',),
        #         (u'Customer type', 'ticket_payment__customer__customer_type__name'),
        #         (u'Customer name', 'ticket_payment__customer__customer_name'),
        #         (u'Company', 'ticket_payment__customer__company__name'),
        #         (u'Building', 'ticket_payment__customer__building__name'),
        #         # (u'Mã căn hộ', 'ticket_payment__customer__apartment__address'),
        #         (u'Driver name', 'vehicle_registration__vehicle_driver_name'),
        #         (u'old license plate', 'vehicle_registration__vehicle_driver_id'),
        #         (u'Vehicle number', 'vehicle_registration__vehicle_number'),
        #         (u'Vehicle type', 'vehicle_registration__vehicle_type__name'),
        #         (u'Card code', 'vehicle_registration__card__card_label'),
        #         (u'Card type', 'vehicle_registration__card__card_type'),
        #         (u'Commencement Date', 'effective_date'),
        #         (u'Expiry Date', 'expired_date'),
        #         (u'Fee type', 'vehicle_registration__level_fee__name'),
        #         (u'Payment no.', 'ticket_payment__receipt_number',),
        #         (u'Payment date', 'ticket_payment__payment_date'),
        #         (u'Cancel date', 'cancel_date'),
        #         (u'Canceled Recipe', 'ticket_payment_id'),
        #         (u'Method', 'ticket_payment__payment_method'),
        #         (u'Remark',),
        #         ],
        #
        #     # 'HEADER2': [
        #     #     (u'STT',),
        #     #     (u'Số', 'deposit_payment__receipt_number',),
        #     #     (u'Ngày nộp', 'deposit_payment__payment_date'),
        #     #     (u'Phiếu thu bị huỷ', 'deposit_payment_id'),
        #     #     (u'Tòa nhà', 'deposit_payment__customer__building__name'),
        #     #     (u'Mã căn hộ', 'deposit_payment__customer__apartment__address'),
        #     #     (u'Tên khách hàng', 'deposit_payment__customer__customer_name'),
        #     #     (u'Công ty', 'deposit_payment__customer__company__name'),
        #     #     (u'Loại khách hàng', 'deposit_payment__customer__customer_type__name'),
        #     #     (u'Phương thức', 'deposit_payment__payment_method'),
        #     #     (u'Số xe', 'vehicle_number'),
        #     #     (u'Ngày hiệu lực', 'deposit_payment__payment_date'),
        #     #     (u'Hạn hiện tại',),
        #     # ],
        #     'HEADER2': [
        #         (u'NO.',),
        #         (u'Customer type', 'deposit_payment__customer__customer_type__name'),
        #         (u'Customer name', 'deposit_payment__customer__customer_name'),
        #         (u'Company', 'deposit_payment__customer__company__name'),
        #         (u'Building', 'deposit_payment__customer__building__name'),
        #         # (u'Mã căn hộ', 'deposit_payment__customer__apartment__address'),
        #         (u'Driver name', 'vehicle_registration__vehicle_driver_name'),
        #         (u'old license plate', 'vehicle_registration__vehicle_driver_id'),
        #         (u'Vehicle number', 'vehicle_registration__vehicle_number'),
        #         (u'Vehicle type', 'vehicle_registration__vehicle_type__name'),
        #         (u'Card code', 'vehicle_registration__card__card_label'),
        #         (u'Card type', 'vehicle_registration__card__card_type'),
        #         (u'Commencement Date', 'deposit_payment__payment_date'),
        #         (u'Expiry Date',),
        #         (u'Fee type', 'vehicle_registration__level_fee__name'),
        #         (u'Payment no.', 'deposit_payment__receipt_number',),
        #         (u'Payment date', 'deposit_payment__payment_date'),
        #         (u'Canceled Recipe', 'deposit_payment_id'),
        #         (u'Method', 'deposit_payment__payment_method'),
        #         (u'Remark',),
        #
        #     ],
        #
        #     'VEHICLE_TYPE': [],
        #
        #     'DETAIL': [
        #         (u'Active Duration'),
        #         (u'Bill'),
        #         (u'Cancel fee'),
        #     ],
        #
        #     'TITLE': [
        #         (2, 5, u'BÁO CÁO TỔNG PHÍ GIỮ XE THÁNG'),
        #         [3, 5, u'Từ'],
        #         [3, 8, u'Đến'],
        #     ],
        #
        #     'STAT': [],  # [(4, 6 + i*2, type[1], type[0]) for i, type in enumerate(VEHICLE_TYPE)]
        # }

        is_protected_report = False if 'unprotected_report' in request.POST else True

        current_user = request.user
        if not is_protected_report and not current_user.has_perm('parking.export_unprotected_excel'):
            messages.error(request, u'Tài khoản hiện tại không có quyền này!', fail_silently=True)
            return redirect(reverse('render_report_parking_session'))

        from_time = request.POST['from_time']
        to_time = request.POST['to_time']

        # payment_method = request.POST['payment_method'].strip()
        # TEMPLATE_SHEET_PARKING_FEE_DETAIL['TITLE'][1].append(from_time)  # Tu
        # TEMPLATE_SHEET_PARKING_FEE_DETAIL['TITLE'][2].append(to_time)  # Den
        #
        # customer_type = request.POST['customer_type'].strip()
        # payment_method = request.POST['payment_method'].strip()
        #
        # vehicle_type_list = list()
        # vehicle_types = VehicleType.objects.all()
        # for type in vehicle_types:
        #     TEMPLATE_SHEET_PARKING_FEE_DETAIL['VEHICLE_TYPE'].append(type.name)
        #     vehicle_type_list.append(type.id)
        # len_vehicle_type_list = len(vehicle_type_list)
        #
        # TEMPLATE_SHEET_PARKING_FEE_DETAIL['TOP'][1][1]['h_merge_range'] = len_vehicle_type_list
        # TEMPLATE_SHEET_PARKING_FEE_DETAIL['TOP'][2].append(
        #     {'col': TEMPLATE_SHEET_PARKING_FEE_DETAIL['TOP'][1][1]['col'] + len_vehicle_type_list,
        #      'h_merge_range': 2 * len_vehicle_type_list})
        # data = list()

        # rs = TicketPaymentDetail.objects.filter(effective_date__isnull=False, expired_date__isnull=False).exclude(duration=0).select_related(
        #     'level_fee',
        #     'ticket_payment__payment_date',
        #     'ticket_payment__customer__customer_type__name',
        #     'ticket_payment__payment_method',
        #     'ticket_payment__receipt_number',
        #     'ticket_payment__customer__building__name',
        #     'ticket_payment__customer__apartment__address',
        #     'ticket_payment__customer__customer_name',
        #     'ticket_payment__customer__customer_type__name',
        #     'ticket_payment__customer__company__name',
        #     'vehicle_registration__level_fee__fee',
        #     'vehicle_registration__level_fee__name',
        #     'vehicle_registration__vehicle_type',
        #     'vehicle_registration__vehicle_driver_name',
        #     'vehicle_registration__vehicle_number',
        #     'vehicle_registration__card__card_label',
        #     'vehicle_registration__card__card_type',
        #     'vehicle_registration__vehicle_driver_id'
        # )  # Chi tiet thanh toan ve thang
        # rs2 = DepositPaymentDetail.objects.all().select_related('deposit_payment__payment_date'
        #                                                         'deposit_payment__customer__customer_type__name',
        #                                                         'deposit_payment__payment_method',
        #                                                         'deposit_payment__receipt_number',
        #                                                         'deposit_payment__customer__building__name',
        #                                                         'deposit_payment__customer__apartment__address',
        #                                                         'deposit_payment__customer__customer_name',
        #                                                         'deposit_payment__customer__company__name',
        #                                                         'deposit_action_fee__fee',
        #                                                         'vehicle_registration__level_fee__fee',
        #                                                         'vehicle_registration__vehicle_type',
        #                                                         'vehicle_registration__vehicle_driver_name',
        #                                                         'vehicle_registration__vehicle_number',
        #                                                         'vehicle_registration__card__card_label',
        #                                                         'vehicle_registration__card__card_type',
        #                                                         'vehicle_registration__vehicle_driver_id'
        #                                                         )

        try:
            _from_time = datetime.strptime(from_time, "%d/%m/%Y %H:%M:%S").replace(tzinfo=utc).date()
            _to_time = datetime.strptime(to_time, "%d/%m/%Y %H:%M:%S").replace(tzinfo=utc).date()
            customer_type = request.POST['customer_type'].strip()
            payment_method = request.POST['payment_method'].strip()

            # rs = rs.exclude(expired_date__lt=_from_time).exclude(effective_date__gt=_to_time)  # Xe vao
            # rs2 = rs2.filter(deposit_payment__payment_date__lte=_to_time,
            #                  deposit_payment__payment_date__gte=_from_time)  # Xe vao
        except:
            messages.error(request, u'Khoảng thời gian không phù hợp!', fail_silently=True)
            return HttpResponseRedirect('')
        folder_name = 'templates/report'
        file_name = 'Parking_Season.xlsx'
        file_path = '%s/%s' % (folder_name, file_name)

        if not os.path.exists(folder_name):
            os.mkdir(folder_name)
        if os.path.isfile(file_path):
            os.remove(file_path)

        workbook = Workbook(file_path, {'constant_memory': True})

        add_sheet('Tong phi ve thang', workbook, _from_time, _to_time, customer_type, payment_method,
                  is_protected_report)
        workbook.close()

        with open(file_path, 'r') as f:
            response = HttpResponse(f.read(), content_type="application/vnd.ms-excel")
            response['Content-Disposition'] = "attachment; filename=%s" % file_name
            return response
        # if len(customer_type) > 0:
        #     rs = rs.filter(ticket_payment__customer__customer_type__name=customer_type)
        #     rs2 = rs2.filter(deposit_payment__customer__customer_type__name=customer_type)
        #
        # if len(payment_method) > 0:
        #     rs = rs.filter(ticket_payment__payment_method=payment_method)
        #     rs2 = rs2.filter(deposit_payment__payment_method=payment_method)

        # folder_name = 'templates/report'
        # file_name = 'Parking_Season.xlsx'
        # file_path = '%s/%s' % (folder_name, file_name)
        #
        # if not os.path.exists(folder_name):
        #     os.mkdir(folder_name)
        # if os.path.isfile(file_path):
        #     os.remove(file_path)
        #
        # workbook = Workbook(file_path, {'constant_memory': True})
        # # add_worksheet_stat('Tong phi ve thang', workbook, TEMPLATE_SHEET_PARKING_FEE_DETAIL, rs,
        # #                    rs2, {}, is_protected_report)  # D
        # add_sheet('Tong phi ve thang', workbook,_from_time,_to_time,0,'all')
        # workbook.close()
        #
        # with open(file_path, 'r') as f:
        #     response = HttpResponse(f.read(), content_type="application/vnd.ms-excel")
        #     response['Content-Disposition'] = "attachment; filename=%s" % file_name
        #     return response

    return render(request, 'admin/rp-parkingsession.html', {'vehicle_type_data': vehicle_type_data,
                                                            'customer_type_data': customer_type_data,
                                                            'payment_method_data': payment_method_data,
                                                            'from_time': from_time.strftime("%d/%m/%Y %H:%M:%S"),
                                                            'to_time': to_time.strftime("%d/%m/%Y %H:%M:%S"),
                                                            })


@login_required(redirect_field_name='', login_url='/admin/')
def render_report_parking_session_cancellation(request):  # Bao cao phi ve thang
    userid = request.user.id
    if notallowreport(userid):
        return render(request, 'admin/nopermission.html')
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

    def calculate_fee_session(cancel_date, from_date, to_date, level_fee):
        fee = 0
        cancel_fee = 0
        # Check report time in range cancellation time
        if cancel_date and cancel_date < to_date:
            num_month = 0
            # If cancel_date earlier than from time, calculate month between from_time and to_time
            # else calculate time between cancel_date and to_time
            if cancel_date < from_date:
                num_month = count_months_between_dates(from_date.month, from_date.year, to_date.month, to_date.year) + 1
            else:
                num_month = count_months_between_dates(cancel_date.month, cancel_date.year, to_date.month, to_date.year)

                # If cancellation time earlier than day 15 in month, return half else no cancel fee
                if from_date.month == cancel_date.month and cancel_date.day <= 15:
                    cancel_fee = level_fee / 2

            # Calculate cancellation fee
            cancel_fee += num_month * level_fee



        while from_date <= to_date:
            last_date_current_month = make_last_date_of_month(from_date, 1)
            if last_date_current_month >= to_date:
                last_date_current_month = to_date

            num_current_month_payable_days = last_date_current_month.day - from_date.day + 1

            if num_current_month_payable_days == get_num_days_of_month(last_date_current_month.year,
                                                                       last_date_current_month.month):  # Tron 1 thang
                fee += int(level_fee)
            else:
                price_per_day = float(level_fee) / get_num_days_of_month(from_date.year, from_date.month)

                fee += ceil(num_current_month_payable_days * price_per_day / 1000) * 1000

            from_date = last_date_current_month + timedelta(days=1)
        fee = int(fee)
        return [fee, cancel_fee, fee - cancel_fee]

    vehicle_types = VehicleType.objects.all()
    len_vehicle_type_list = len(vehicle_types)
    vehicle_type_data = list()
    for type in vehicle_types:
        vehicle_type_data.append({"value": type.id, "name": type.name})

    now = datetime.now()
    from_time = datetime(now.year, now.month, now.day).replace(day=1)  # TG bat dau: Dau thang hien tai

    month = now.month
    year = now.year + month / 12

    try:
        month = (month + 1) % 12
        if month == 0: month = 12
    except:
        pass
    to_time = from_time.replace(year=year, month=month, day=1, hour=23) + timedelta(
        days=-1)  # TG ket thuc: Cuoi thang hien tai

    customer_type_data = list()

    customer_types = CustomerType.objects.all()
    customer_type_data.append({"value": "", "name": "Tất cả"})
    for customer_type in customer_types:
        customer_type_data.append({"value": customer_type.name, "name": customer_type.name})

    def add_worksheet_stat(sheet_name, workbook, TEMPLATE, rs, custom_param, sheet_protect=True):
        LOGO_EXCEL_REPORT_PATH = 'parking/static/image/' + get_setting('logo_report',
                                                                       u'Mẫu logo dùng trong báo cáo',
                                                                       'logo_report.png')
        LOGO_EXCEL_REPORT_PATH_RIGHT = 'parking/static/image/' + get_setting('logo_repor',
                                                                             u'Mẫu logo dùng trong báo cáo',
                                                                             'logo_report_right.png')

        TITLE = TEMPLATE['TITLE']
        TOP = TEMPLATE['TOP']
        HEADER = TEMPLATE['HEADER']
        DETAIL = TEMPLATE['DETAIL']
        VEHICLE_TYPE = TEMPLATE['VEHICLE_TYPE']

        TOP[2][1]['h_merge_range'] = len_vehicle_type_list * 2 + 1
        tmp_col = TOP[2][1]['col'] + len_vehicle_type_list * 2 + 1
        TOP[3][1]['col'] = tmp_col
        TOP[4][1]['col'] = tmp_col + 1
        TOP[5][1]['col'] = tmp_col + 2

        sheet = workbook.add_worksheet(sheet_name)
        sheet.insert_image(0, 0, LOGO_EXCEL_REPORT_PATH)
        sheet.insert_image(0, 8, LOGO_EXCEL_REPORT_PATH_RIGHT)

        bold = workbook.add_format({'bold': True})
        wrap = workbook.add_format()
        wrap.set_text_wrap()
        border = workbook.add_format()
        border.set_border()
        bold_border = workbook.add_format({'bold': True, 'border': 1})
        bold_center_border = workbook.add_format({'align': 'center', 'bold': True, 'border': 1})
        bold_center_border.set_pattern(1)  # This is optional when using a solid fill.
        bold_center_border.set_bg_color('#ffffff')
        number_border_format = workbook.add_format({'num_format': '#,###0', 'border': 1})
        bold_number_border_format = workbook.add_format({'num_format': '#,###0', 'border': 1, 'bold': True})

        cancelled_cell = workbook.add_format({'bg_color': 'red', 'border': 1})

        for r in TITLE:  # Viet tieu de: (dong r[0], cot r[1])
            sheet.write(r[0], r[1], r[2], bold)
            if len(r) == 4:
                sheet.write(r[0], r[1] + 1, r[3])

        # sheet.write_row(5, 0, [''] * 13, bold_center_border)

        # for i in xrange(len(HEADER)):
        #     sheet.write(5, i, HEADER[i][0], bold_center_border)
        #     sheet.set_column(i, i, 20)  # Default width

        for r in TOP:
            cur_col = r[1]['col']
            end_col = cur_col + r[1]['h_merge_range'] - 1
            end_row = 5 + r[1]['v_merge_range'] - 1
            sheet.merge_range(5, cur_col, end_row, end_col, r[0], bold_center_border)
            sheet.set_column(cur_col, end_col, r[1]['width'])

        col = 2
        for type in vehicle_types:
            sheet.merge_range(6, col, 6, col + 1, type.name, bold_center_border)
            col += 2

        sheet.merge_range(6, col, 7, col, "Total", bold_center_border)

        col = 2
        for type in vehicle_types:
            sheet.write_row(7, col, DETAIL, bold_center_border)
            col += 2

        i = 0
        list_val = [w[1] for w in TEMPLATE['HEADER'] if
                    len(w) == 2]  # Co field query hoac custom function

        rs_value_list = enumerate(rs.values(*list_val))  # Query data
        rs = list(rs)
        _from = _from_time.date()
        _to = _to_time.date()


        cur_customer = None
        fee_list = [0] * (len_vehicle_type_list * 2 + 3)
        grand_total = ['Grand Total', 'Grand Total']
        grand_total.extend(fee_list)

        temp_data = list()
        customer_data = []

        # Build data
        for k, r in rs_value_list:
            # New customer
            if cur_customer is None or cur_customer != r["ticket_payment__customer__customer_name"]:
                cur_customer = r["ticket_payment__customer__customer_name"]

                # Tao moi du lieu customer
                temp_data = list()
                customer_data.append(temp_data)
                for j in range(0, len(TEMPLATE_SHEET_PARKING_FEE_DETAIL['HEADER'])):
                    column = TEMPLATE_SHEET_PARKING_FEE_DETAIL['HEADER'][j]
                    if len(column) == 2 and column[1] in r:
                        # Nhung cot co dinh nghia query field
                        temp_value = r[column[1]]
                        if isinstance(temp_value, date):  # Du lieu cell kieu datetime.date
                            temp_value = temp_value.strftime("%d/%m/%Y")
                        temp_data.append(temp_value)
                    else:  # Cot rong
                        temp_data.append('')  # Cell trong
                temp_data.extend(fee_list)

            row = rs[k]

            index = 2
            for type in vehicle_types:
                if row.vehicle_registration and row.vehicle_registration.vehicle_type \
                        and type.name == row.vehicle_registration.vehicle_type.name:

                    # Calculate time duration from report
                    if _from < row.effective_date:
                        tmp_from_time = row.effective_date
                    else:
                        tmp_from_time = _from

                    if _to > row.expired_date:
                        tmp_to_time = row.expired_date
                    else:
                        tmp_to_time = _to

                    payment_result = calculate_fee_session(row.cancel_date, tmp_from_time, tmp_to_time, row.level_fee)

                    # Quantity
                    temp_data[index] += 1
                    grand_total[index] += 1

                    # Fee
                    temp_data[index + 1] += payment_result[0]
                    grand_total[index + 1] += payment_result[0]

                    # Total
                    temp_data[len_vehicle_type_list * 2 + 2] += payment_result[0]
                    grand_total[len_vehicle_type_list * 2 + 2] += payment_result[0]

                    # Cancel fee
                    temp_data[len_vehicle_type_list * 2 + 3]+=payment_result[1]
                    grand_total[len_vehicle_type_list * 2 + 3]+=payment_result[1]

                    # Total after reduction
                    temp_data[len_vehicle_type_list * 2 + 4]+=payment_result[2]
                    grand_total[len_vehicle_type_list * 2 + 4]+=payment_result[2]

                    # final_ticket_payment_sum += sum
                else:
                    index += 2

        # Write data
        for item in customer_data:
            sheet.write_row(8 + i, 0, item, number_border_format)
            i += 1

        sheet.write_row(8 + i, 0, grand_total, bold_number_border_format)
        i+=2

        sheet.write(8 + i + 1, 0, u"Reported by", bold)
        sheet.write(8 + i + 1, 3, u"Checked by", bold)
        sheet.write(8 + i + 1, 5, u"Approved by", bold)
        # sheet.write(8 + i + 1, TOP[3][1]['col'], u"Approved by", bold)
        # i += 6

        # sheet.write(8 + i + 1, 0, u"Voong Bao Tran", bold)
        # sheet.write(8 + i + 1, 1, u"Voong Bao Tran", bold)
        # sheet.write(8 + i + 1, TOP[3][1]['col'], u"Vuong Cam Sinh", bold)
        # i+=2

        # sheet.write(8 + i + 1, 0, u"Prepared", bold)
        # sheet.write(8 + i + 1, 1, u"Prepared", bold)
        # sheet.write(8 + i + 1, 10, u"Checked", bold)
        # sheet.write(8 + i + 1, TOP[3][1]['col'] + 3, u"Approved by", bold)
        # i+=6

        # sheet.write(8 + i + 1, 0, u"Tran Thi Mai Chi", bold)
        # sheet.write(8 + i + 1, 1, u"Tran Thi Mai Chi", bold)
        # sheet.write(8 + i + 1, 10, u"Nguyen Thi Hanh", bold)
        # sheet.write(8 + i + 1, TOP[3][1]['col'], u"Tran Thi Kim Chau", bold)

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

    ##2018-01-12 begin

    def cal_feesession(canceleddate, fromdate, todate, feelevel):
        if feelevel == 0 or fromdate is None or todate is None:
            return [0, 0]
        bill = getfeeByMonthSGCT(fromdate, todate, feelevel)
        canceled_fee=None
        if canceleddate is None:
            canceled_fee = 0
        else:
            if canceleddate < fromdate:
                if canceleddate.month == fromdate.month and canceleddate.year == fromdate.year and canceleddate.day <= 15:
                    canceled_fee = feelevel / 2
                else:
                    canceled_fee = bill
            else:
                if canceleddate.day <= 15:
                    canceled_fee = feelevel / 2
                else:
                    canceled_fee = 0
        return [bill, canceled_fee]

    def add_sheet(sheetname, workbook, fdate, tdate, cus_type, payment_type, sheet_protect=True):
        util = Utilities()
        dts = util.Query("cusreport")
        dtvt = util.Query("vehicletypereport")
        lenvhc=len(dtvt)
        dtdetail = util.Query("parkingsessionreportdetail", fdate, tdate, cus_type, payment_type)
        LOGO_EXCEL_REPORT_PATH = 'parking/static/image/' + get_setting('logo_report',
                                                                       u'Mẫu logo dùng trong báo cáo',
                                                                       'logo_report.png')
        LOGO_EXCEL_REPORT_PATH_RIGHT = 'parking/static/image/' + get_setting('logo_repor',
                                                                             u'Mẫu logo dùng trong báo cáo',
                                                                             'logo_report_right.png')
        sheet = workbook.add_worksheet(sheetname)
        sheet.insert_image(0, 0, LOGO_EXCEL_REPORT_PATH)
        sheet.insert_image(0, 8, LOGO_EXCEL_REPORT_PATH_RIGHT)
        bold = workbook.add_format({'bold': True})
        wrap = workbook.add_format()
        wrap.set_text_wrap()
        border = workbook.add_format()
        border.set_border()
        bold_border = workbook.add_format({'bold': True, 'border': 1})
        bold_center_border = workbook.add_format({'align': 'center', 'bold': True, 'border': 1})
        bold_center_border.set_pattern(1)  # This is optional when using a solid fill.
        bold_center_border.set_bg_color('#ffffff')
        number_border_format = workbook.add_format({'num_format': '#,###0', 'border': 1})
        bold_number_border_format = workbook.add_format({'num_format': '#,###0', 'border': 1, 'bold': True})

        cancelled_cell = workbook.add_format({'bg_color': 'red', 'border': 1})
        title = [u'SAIGON CENTRE PHASE II PROPERTY MANAGERMENT SEASON PARKING']
        sheet.write_row(2, 2, title, bold)
        titleContent = [u'Từ', fdate.strftime("%d/%m/%Y"), u'Đến', tdate.strftime("%d/%m/%Y")]
        sheet.write_row(3, 2, titleContent)
        sheet.merge_range(5, 0, 7, 0, u'Loại khách hàng', bold_center_border)
        sheet.set_column(0, 0, 20)
        sheet.merge_range(5, 1, 7, 1, u'Tên khách hàng', bold_center_border)
        sheet.set_column(1, 1, 30)
        sheet.merge_range(5, 2, 5, 2+lenvhc*2, u'System export(PM)', bold_center_border)
        sheet.set_column(2, lenvhc*2+5, 15)
        index=2
        dt_row = []
        for v in dtvt:
            dt_row.append(0)
            dt_row.append(0)
            sheet.merge_range(6, index, 6, index+1, v[0], bold_center_border)
            header_row = [u'Quantity',u'Amount(VND)']
            sheet.write_row(7, index, header_row, bold_center_border)
            index=index+2
        sheet.merge_range(6, index, 7, index, u'Total', bold_center_border)
        sheet.merge_range(5, index+1, 7, index+1, u'Reduction', bold_center_border)
        sheet.merge_range(5, index + 2, 7, index + 2, u'Total after reduc', bold_center_border)
        DATA_STARTING_ROW = 8
        tt_bill = 0
        tt_cancel_fee = 0
        tt_bill_after = 0
        i = 0
        e_row=[]
        e_row.append(u'Grand Total')
        e_row.append(u'Grand Total')
        for r in dts:
            if cus_type!=u'' and r[0]!=cus_type:
                continue
            current_row=[]
            current_row.append(r[0])
            current_row.append(r[1])

            dtl=[x for x in dtdetail if x[1] == r[1] and x[0]==r[0]]

            tt_bill_r=0
            tt_cancel_fee_r=0
            tt_bill_after_r=0
            index=0
            for vt in dtvt:
                tt=0
                tt_c=0

                dtlst=[x for x in dtl if x[7] == vt[0]]

                am=len(dtlst)
                dt_row[index]=dt_row[index]+am
                jj=0
                for dr in dtlst:
                    _from_date = datetime.strptime(str(dr[10]), "%Y-%m-%d").date()
                    _to_date = datetime.strptime(str(dr[11]), "%Y-%m-%d").date()
                    _cancel_date = None
                    if dr[16]:
                        _cancel_date = datetime.strptime(str(dr[16]), "%Y-%m-%d").date()
                    feemonth=0
                    try:
                        feemonth=float(dr[13])
                    except:
                        feemonth = 0
                    fee_sesion = cal_feesession(_cancel_date, _from_date, _to_date, feemonth)
                    tt =tt + fee_sesion[0]
                    tt_c = tt_c+ fee_sesion[1]
                    jj=jj+1
                current_row.append(am)
                current_row.append(tt)
                dt_row[index+1] = dt_row[index+1] + tt
                tt_bill_r=tt_bill_r+tt
                tt_cancel_fee_r=tt_cancel_fee_r+tt_c
                index=index+2
            current_row.append(tt_bill_r)
            current_row.append(tt_cancel_fee_r)
            tt_bill_after_r=tt_bill_r-tt_cancel_fee_r
            current_row.append(tt_bill_after_r)
            tt_bill = tt_bill + tt_bill_r
            tt_cancel_fee = tt_cancel_fee + tt_cancel_fee_r
            tt_bill_after=tt_bill_after+tt_bill_after_r
            sheet.write_row(DATA_STARTING_ROW + i, 0, current_row, border)
            i = i + 1
        for ddd in dt_row:
            e_row.append(ddd)
        e_row.append(tt_bill)
        e_row.append(tt_cancel_fee)
        e_row.append(tt_bill_after)
        sheet.write_row(DATA_STARTING_ROW + i, 0, e_row, bold_center_border)
        i = i + 3
        title = [u'Reported by', u'',u'',u'Checked by',u'','Approved by']
        sheet.write_row(DATA_STARTING_ROW + i, 0, title, bold)
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
    ##2018-01-12 end
    if 'btn_REPORT_DETAIL' in request.POST:

        # TEMPLATE_SHEET_PARKING_FEE_DETAIL = {
        #     'TOP': [
        #         (u'Loại khách hàng', {'col': 0, 'h_merge_range': 1, 'v_merge_range': 3, 'width': 20}),
        #         (u'Tên khách hàng', {'col': 1, 'h_merge_range': 1, 'v_merge_range': 3, 'width': 30}),
        #         (u'System export(PM)', {'col': 2, 'h_merge_range': 3, 'v_merge_range': 1, 'width': 15}),
        #         [u'Reduction',{'col': 2, 'h_merge_range': 1, 'v_merge_range': 3, 'width': 15} ],
        #         [u'Total after reduc', {'col': 2, 'h_merge_range': 1, 'v_merge_range': 3, 'width': 15}],
        #         [u'Note', {'col': 2, 'h_merge_range': 1, 'v_merge_range': 1, 'width': 15}],
        #     ],
        #
        #     'VEHICLE_TYPE': [],
        #
        #     'HEADER': [
        #         (u'Loại khách hàng', 'ticket_payment__customer__customer_type__name'),
        #         (u'Tên khách hàng', 'ticket_payment__customer__customer_name')
        #     ],
        #
        #     'DETAIL': [
        #         (u'Quantity'),
        #         (u'Amount(VND)')
        #     ],
        #
        #     'TITLE': [
        #         (2, 5, u'SAIGON CENTRE PHASE I\n PROPERTY MANAGEMENT\n SEASON PARKING'),
        #         [3, 5, u'Từ'],
        #         [3, 8, u'Đến'],
        #     ],
        #
        #
        #     'STAT': [],  # [(4, 6 + i*2, type[1], type[0]) for i, type in enumerate(VEHICLE_TYPE)]
        # }

        is_protected_report = False if 'unprotected_report' in request.POST else True

        current_user = request.user
        if not is_protected_report and not current_user.has_perm('parking.export_unprotected_excel'):
            messages.error(request, u'Tài khoản hiện tại không có quyền này!', fail_silently=True)
            return redirect(reverse('render_report_parking_session_cancellation'))

        from_time = request.POST['from_time']
        to_time = request.POST['to_time']
        # TEMPLATE_SHEET_PARKING_FEE_DETAIL['TITLE'][1].append(from_time)  # Tu
        # TEMPLATE_SHEET_PARKING_FEE_DETAIL['TITLE'][2].append(to_time)  # Den

        customer_type = request.POST['customer_type'].strip()

        # data = list()
        #
        # rs = TicketPaymentDetail.objects.filter(effective_date__isnull=False,
        #                                         expired_date__isnull=False).select_related(
        #     'level_fee',
        #     'ticket_payment__customer__customer_type__name',
        #     'ticket_payment__customer__customer_name',
        #     'vehicle_registration__vehicle_type__name',
        #     'vehicle_registration__vehicle_driver_id',
        # )  # Chi tiet thanh toan ve thang

        try:
            _from_time = datetime.strptime(from_time, "%d/%m/%Y %H:%M:%S").replace(tzinfo=utc).date()
            _to_time = datetime.strptime(to_time, "%d/%m/%Y %H:%M:%S").replace(tzinfo=utc).date()

        except:
            messages.error(request, u'Khoảng thời gian không phù hợp!', fail_silently=True)
            return HttpResponseRedirect('')
        folder_name = 'templates/report'
        file_name = 'Parking_Season_Cancellation.xlsx'
        file_path = '%s/%s' % (folder_name, file_name)

        if not os.path.exists(folder_name):
            os.mkdir(folder_name)
        if os.path.isfile(file_path):
            os.remove(file_path)
        workbook = Workbook(file_path, {'constant_memory': False})
        add_sheet('Tong phi ve thang', workbook, _from_time, _to_time, customer_type, u'', is_protected_report)
        workbook.close()
        with open(file_path, 'r') as f:
            response = HttpResponse(f.read(), content_type="application/vnd.ms-excel")
            response['Content-Disposition'] = "attachment; filename=%s" % file_name
            return response
        # if len(customer_type) > 0:
        #     rs = rs.filter(ticket_payment__customer__customer_type__name=customer_type)
        #
        # folder_name = 'templates/report'
        # file_name = 'Parking_Season_Cancellation.xlsx'
        # file_path = '%s/%s' % (folder_name, file_name)
        #
        # if not os.path.exists(folder_name):
        #     os.mkdir(folder_name)
        # if os.path.isfile(file_path):
        #     os.remove(file_path)
        #
        # workbook = Workbook(file_path, {'constant_memory': False})
        # add_worksheet_stat('Tong phi ve thang', workbook, TEMPLATE_SHEET_PARKING_FEE_DETAIL,
        #                    rs.order_by("ticket_payment__customer__customer_type__name", "ticket_payment__customer__customer_name"),
        #                    {}, is_protected_report)  # D
        # workbook.close()
        #
        # with open(file_path, 'r') as f:
        #     response = HttpResponse(f.read(), content_type="application/vnd.ms-excel")
        #     response['Content-Disposition'] = "attachment; filename=%s" % file_name
        #     return response

    return render(request, 'admin/rp-parkingsessioncancellation.html', {'vehicle_type_data': vehicle_type_data,
                                                                        'customer_type_data': customer_type_data,
                                                                        'from_time': from_time.strftime("%d/%m/%Y %H:%M:%S"),
                                                                        'to_time': to_time.strftime("%d/%m/%Y %H:%M:%S"),
                                                                        })
### kết thúc thay thế: 2018-01-14
### bắt đầu thêm mới: 2018-01-14
###2018-01-12 support report function
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
##2018-01-12##########################################################
### kết thúc thêm mới: 2018-01-14
