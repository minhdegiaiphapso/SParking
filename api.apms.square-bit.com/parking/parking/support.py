# -*- coding: utf-8 -*-
import hashlib
import json
from django.utils.safestring import mark_safe
from pytz import timezone
from django.utils.timezone import utc
from django.http import HttpResponse
from django.db.models import Q
from fdfgen import forge_fdf
import os
from models import *
from site_settings.settings import TIME_ZONE

__author__ = 'nghiaht'

patterns = {
    'a': u'á|à|ả|ã|ạ|ă|ắ|ằ|ẳ|ẵ|ặ|â|ấ|ầ|ẩ|ẫ|ậ',
    'd': u'đ',
    'e': u'é|è|ẻ|ẽ|ẹ|ê|ế|ề|ể|ễ|ệ',
    'i': u'í|ì|ỉ|ĩ|ị',
    'o': u'ó|ò|ỏ|õ|ọ|ô|ố|ồ|ổ|ỗ|ộ|ơ|ớ|ờ|ở|ỡ|ợ',
    'u': u'ú|ù|ủ|ũ|ụ|ư|ứ|ừ|ử|ữ|ự',
    'y': u'ý|ỳ|ỷ|ỹ|ỵ'
}
convert_dict = dict()
for c, p in patterns.iteritems():
    keys = p.split('|')
    c_up = c.upper()
    for k in keys:
        convert_dict[k] = c
        convert_dict[k.upper()] = c_up


def remove_sign_vietnamese(t):
    rs = ''
    for i in t:
        if i in convert_dict:
            rs += convert_dict[i]
        else:
            rs += i
    return rs.strip()


def get_now_utc():
    return datetime.datetime.utcnow().replace(microsecond=0, tzinfo=utc)


def int_format(value, decimal_points=3, seperator=u'.'):
    value = str(value)
    if len(value) <= decimal_points:
        return value
    # say here we have value = '12345' and the default params above
    parts = []
    while value:
        parts.append(value[-decimal_points:])
        value = value[:-decimal_points]
    # now we should have parts = ['345', '12']
    parts.reverse()
    # and the return value should be u'12.345'
    return seperator.join(parts)


def number_to_text(num):
    if num == 0:
        return u'Không'
    level = [u'', u'ngàn', u'triệu', u'tỉ', u'ngàn tỉ']
    num_str = [u'không', u'một', u'hai', u'ba', u'bốn', u'năm', u'sáu', u'bảy', u'tám', u'chín']
    level_idx = 0
    rs = u''
    value = int(num)
    while value > 0:
        sub_rs = u''
        v = value % 1000
        value /= 1000
        if v > 0:
            t = v / 100
            c = (v % 100) / 10
            u = v % 10
            if value > 0 or (value == 0 and t > 0):
                sub_rs += u'%s trăm' % num_str[t]
                if c == 0 and u > 0:
                    sub_rs += u' lẻ'
            if c == 1:
                sub_rs += u' mười'
            elif c > 1:
                sub_rs += u' %s mươi' % num_str[c]
            if u > 1:
                sub_rs += u' %s' % num_str[u]
            elif u == 1:
                if c > 1:
                    sub_rs += u' mốt'
                else:
                    sub_rs += u' một'
            sub_rs += u' %s' % level[level_idx]
            rs = sub_rs.strip() + u' ' + rs
        level_idx += 1
    return rs.strip().capitalize()


# Autocomplete ten the
def get_card_labels(request):
    if request.is_ajax():
        q = request.GET.get('term', '')
        cards = Card.objects.filter(card_label__icontains=q)[:20]
        results = []
        if cards:
            for card in cards:
                card_json = dict()
                card_json['id'] = card.card_id
                card_json['label'] = card.card_label
                card_json['value'] = card.card_label
                results.append(card_json)
        data = json.dumps(results)
    else:
        data = 'fail'
    return HttpResponse(data, content_type='application/json')


def get_vehicle_name(vehicle_type):
    vehicle = VehicleType.objects.get(id=vehicle_type)
    if vehicle: return "%s %s" % (vehicle_type, vehicle.name)
    return u'Không rõ'


# Autocomplete ten nhan vien
def get_users(request):
    if request.is_ajax():
        q = request.GET.get('term', '')
        users = UserProfile.objects.filter(Q(fullname__icontains=q) | Q(staff_id__icontains=q))[:20]
        results = []
        for user in users:
            # user_json = {}
            # user_json['id'] = user.user_id
            # user_json['label'] = user.fullname
            # user_json['value'] = user.user_id
            results.append('%s | %s' % (user.staff_id, user.fullname))
        data = json.dumps(results)
    else:
        data = 'fail'
    return HttpResponse(data, content_type='application/json')


def to_local_time(time):
    return time.astimezone(timezone(TIME_ZONE))


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


def strip_sign_fields(fields):
    rs = []
    for f in fields:
        rs.append((f[0], remove_sign_vietnamese(f[1])))
    return rs


def form_fill(template_filename, fields, strip_sign=False):
    m = hashlib.sha1()
    m.update(template_filename + str(fields))
    temp_filename_base = '/tmp/' + m.hexdigest()
    pdf_filename = temp_filename_base + '.pdf'

    if os.path.exists(pdf_filename):
        fp = open(pdf_filename)
        rs_data = fp.read()
        fp.close()
        return rs_data
    fdf_filename = temp_filename_base + '.fdf'

    # form_filename = 'templates/pdf-form/' + template_filename
    form_filename = template_filename

    if strip_sign:
        fdf_fields = strip_sign_fields(fields)
        cmd_addition = ''
    else:
        fdf_fields = fields
        cmd_addition = ' need_appearances'
    fdf = forge_fdf("", fdf_fields, [], [], [])
    fdf_file = open(fdf_filename, "w")
    fdf_file.write(fdf)
    fdf_file.close()

    cmd = 'pdftk "{0}" fill_form "{1}" output "{2}"{3}'.format(form_filename, fdf_filename, pdf_filename, cmd_addition)
    exit_code = os.system(cmd)

    os.remove(fdf_filename)
    if not os.path.exists(pdf_filename):
        if not strip_sign:
            return form_fill(template_filename, fields, True)
        else:
            return None
    fp = open(pdf_filename)
    rs_data = fp.read()
    fp.close()
    return rs_data


# import fpdf
#
#
# def form_fill_new(template_filename, fields):
#     m = hashlib.sha1()
#     m.update(template_filename + str(fields))
#     temp_filename_base = '/tmp/' + m.hexdigest()
#     pdf_filename = temp_filename_base + '.pdf'
#     form_filename = 'templates/pdf-form/' + template_filename
#     pdf = fpdf.FPDF(form_filename)
#     fdf_filename = temp_filename_base + '.fdf'
#
#     fdf = forge_fdf("", fields, [], [], [])
#     fdf_file = open(fdf_filename, "w")
#     fdf_file.write(fdf)
#     fdf_file.close()
#     cmd = 'pdftk "{0}" fill_form "{1}" output "{2}" dont_ask'.format(form_filename, fdf_filename, pdf_filename)
#     os.system(cmd)
#     os.remove(fdf_filename)
#     fp = open(pdf_filename)
#     rs_data = fp.read()
#     fp.close()
#     return rs_data

from common import VEHICLE_STATUS_COLOR_VALUE_DICT


def get_status(status, safe=True):
    COLOR_VALUE_DICT = {
        0: {"color": "gray", "value": u"Huỷ"},
        1: {"color": "green", "value": u"Đang dùng"},
        2: {"color": "blue", "value": u"Tạm ngừng"},
        3: {"color": "red", "value": u"Chưa đăng ký"},
    }

    status = int(status)

    if safe:
        return mark_safe(u'<p style="color: %s">%s</p>' % (VEHICLE_STATUS_COLOR_VALUE_DICT[status]["color"], VEHICLE_STATUS_COLOR_VALUE_DICT[status]["value"]))
    else:
        return u'<p style="color: {0}">{1}</p>'.format(VEHICLE_STATUS_COLOR_VALUE_DICT[status]["color"],
                                                       VEHICLE_STATUS_COLOR_VALUE_DICT[status]["value"])
##2019Jul10
from datetime import date
def get_status_new(obj, safe=True):
    COLOR_VALUE_DICT = {
        0: {"color": "gray", "value": u"Hủy"},
        1: {"color": "blue", "value": u"Đang dùng"},
        2: {"color": "brown", "value": u"Tạm ngừng"},
        3: {"color": "green", "value": u"Chưa đăng ký"},
        4: {"color": "red", "value": u"Hết hạn"},
    }

    status = -1
    if obj.cancel_date:
        status=0
    elif obj.pause_date:
        status = 2
    elif obj.first_renewal_effective_date==obj.expired_date:
        status=3
    else:
        todate=date.today()
        if obj.expired_date<todate:
            status= 4
        else:
            status=1
    if safe:
        return mark_safe(u'<p style="color: %s">%s</p>' % (
            COLOR_VALUE_DICT[status]["color"], COLOR_VALUE_DICT[status]["value"]))
    else:
        return u'<p style="color: {0}">{1}</p>'.format(COLOR_VALUE_DICT[status]["color"],
                                                       COLOR_VALUE_DICT[status]["value"])
##2019Jul10