# -*- coding: utf-8 -*-
from threading import Thread
import time
import sys
import traceback
import xlsxwriter
import os
from models import *
from django.db.models import Q
from support import to_local_time, get_now_utc, int_format

__author__ = 'ndhoang'


VEHICLE_LOCATIONS = [
    {'name': u'Tất cả', 'code': None},
    {'name': u'Cao Bằng', 'code': ['11']},
    {'name': u'Lạng Sơn', 'code': ['12']},
    {'name': u'Quảng Ninh', 'code': ['14']},
    {'name': u'Hải Phòng', 'code': ['15', '16']},
    {'name': u'Thái Bình', 'code': ['17']},
    {'name': u'Nam Định', 'code': ['18']},
    {'name': u'Phú Thọ', 'code': ['19']},
    {'name': u'Thái Nguyên', 'code': ['20']},
    {'name': u'Yên Bái', 'code': ['21']},
    {'name': u'Tuyên Quang', 'code': ['22']},
    {'name': u'Hà Giang', 'code': ['23']},
    {'name': u'Lào Cai', 'code': ['24']},
    {'name': u'Lai Châu', 'code': ['25']},
    {'name': u'Sơn La', 'code': ['26']},
    {'name': u'Điện Biên', 'code': ['27']},
    {'name': u'Hòa Bình', 'code': ['28']},
    {'name': u'Hà Nội', 'code': ['29', '30', '31', '32', '33', '40']},
    {'name': u'Hải Dương', 'code': ['34']},
    {'name': u'Ninh Bình', 'code': ['35']},
    {'name': u'Thanh Hóa', 'code': ['36']},
    {'name': u'Nghệ An', 'code': ['37']},
    {'name': u'Hà Tĩnh', 'code': ['38']},
    {'name': u'Đồng Nai', 'code': ['60']},
    {'name': u'Biên Hòa', 'code': ['39']},
    {'name': u'Đà Nẵng', 'code': ['43']},
    {'name': u'Đắk Lắk', 'code': ['47']},
    {'name': u'Đắk Nông', 'code': ['48']},
    {'name': u'Lâm Đồng', 'code': ['49']},
    {'name': u'TP Hồ Chí Minh', 'code': ['41', '50', '51', '52', '53', '54', '55', '56', '57', '58', '59']},
    {'name': u'Bình Dương', 'code': ['61']},
    {'name': u'Long An', 'code': ['62']},
    {'name': u'Tiền Giang', 'code': ['63']},
    {'name': u'Vĩnh Long', 'code': ['64']},
    {'name': u'Cần Thơ', 'code': ['65']},
    {'name': u'Đồng Tháp', 'code': ['66']},
    {'name': u'An Giang', 'code': ['67']},
    {'name': u'Kiên Giang', 'code': ['68']},
    {'name': u'Cà Mau', 'code': ['69']},
    {'name': u'Tây Ninh', 'code': ['70']},
    {'name': u'Bến Tre', 'code': ['71']},
    {'name': u'Bà Rịa–Vũng Tàu', 'code': ['72']},
    {'name': u'Quảng Bình', 'code': ['73']},
    {'name': u'Quảng Trị', 'code': ['74']},
    {'name': u'Thừa Thiên–Huế', 'code': ['75']},
    {'name': u'Quảng Ngãi', 'code': ['76']},
    {'name': u'Bình Định', 'code': ['77']},
    {'name': u'Phú Yên', 'code': ['78']},
    {'name': u'Khánh Hòa', 'code': ['79']},
    {'name': u'Gia Lai', 'code': ['81']},
    {'name': u'Kon Tum', 'code': ['82']},
    {'name': u'Sóc Trăng', 'code': ['83']},
    {'name': u'Trà Vinh', 'code': ['84']},
    {'name': u'Ninh Thuận', 'code': ['85']},
    {'name': u'Bình Thuận', 'code': ['86']},
    {'name': u'Vĩnh Phúc', 'code': ['88']},
    {'name': u'Hưng Yên', 'code': ['89']},
    {'name': u'Hà Nam', 'code': ['90']},
    {'name': u'Quảng Nam', 'code': ['92']},
    {'name': u'Bình Phước', 'code': ['93']},
    {'name': u'Bạc Liêu', 'code': ['94']},
    {'name': u'Hậu Giang', 'code': ['95']},
    {'name': u'Bắc Kạn', 'code': ['97']},
    {'name': u'Bắc Giang', 'code': ['13', '98']},
    {'name': u'Bắc Ninh', 'code': ['99']},
]


def search_parking_session(mode=0, limit=20, card_id=None, card_label=None, vehicle_number=None, vehicle_type=None, from_time=None, to_time=None):
    if mode == 0:
        rs = ParkingSession.objects.filter(check_out_time__isnull=True).select_related('card__card_label')
    elif mode == 1:
        rs = ParkingSession.objects.filter(check_out_time__isnull=False).select_related('card__card_label')
    else:
        rs = ParkingSession.objects.all().select_related('card__card_label')
    if mode == 1:
        if from_time:
            rs = rs.filter(check_out_time__gte=from_time)
        if to_time:
            rs = rs.filter(check_out_time__lte=to_time)
    else:
        if from_time:
            rs = rs.filter(check_in_time__gte=from_time)
        if to_time:
            rs = rs.filter(check_in_time__lte=to_time)
    if card_id:
        return rs.filter(card__card_id=card_id).prefetch_related('card')
    if vehicle_type:
        vehicle_type_range = decode_vehicle_type(int(vehicle_type))
        rs = rs.filter(vehicle_type__gte=vehicle_type_range[0],
                       vehicle_type__lte=vehicle_type_range[1])
    if card_label:
        search_query = card_label.strip()
        if search_query.startswith('*') and search_query.endswith('*'):
            rs = rs.filter(card__card_label__icontains=search_query[1:-1])
        elif search_query.startswith('*'):
            rs = rs.filter(card__card_label__iendswith=search_query[1:])
        elif search_query.endswith('*'):
            rs = rs.filter(card__card_label__istartswith=search_query[:-1])
        else:
            rs = rs.filter(card__card_label__iexact=search_query)

    if vehicle_number:
        search_query = vehicle_number.strip()
        if search_query.startswith('*') and search_query.endswith('*'):
            rs = rs.filter(vehicle_number__icontains=search_query[1:-1])
        elif search_query.startswith('*'):
            rs = rs.filter(vehicle_number__iendswith=search_query[1:])
        elif search_query.endswith('*'):
            rs = rs.filter(vehicle_number__istartswith=search_query[:-1])
        else:
            rs = rs.filter(vehicle_number__iexact=search_query)

    if mode == 1:
        rs = rs.order_by('-check_out_time').select_related('card', 'check_out_exception')
    else:
        rs = rs.order_by('-check_in_time').select_related('card', 'check_out_exception')
    if limit > 0:
        return rs[:limit]
    return rs


def search_parking_session_new(queryset, mode=0, card_id=None, card_label=None, vehicle_number=None, vehicle_type=None, from_time=None, to_time=None, terminal_group=None, operator_id=None):
    if mode == 0:
        rs = queryset.filter(check_out_time__isnull=True)
    elif mode == 1:
        rs = queryset.filter(check_out_time__isnull=False)
    else:
        rs = queryset.all()
    if mode == 1:
        if from_time:
            rs = rs.filter(check_out_time__gte=from_time)
        if to_time:
            rs = rs.filter(check_out_time__lte=to_time)
    else:
        if from_time:
            rs = rs.filter(check_in_time__gte=from_time)
        if to_time:
            rs = rs.filter(check_in_time__lte=to_time)
    if card_id:
        return rs.filter(card__card_id=card_id).prefetch_related('card')
    if vehicle_type:
        vehicle_type_range = decode_vehicle_type(int(vehicle_type))
        rs = rs.filter(vehicle_type__gte=vehicle_type_range[0],
                       vehicle_type__lte=vehicle_type_range[1])
    if card_label:
        search_query = card_label.strip()
        if search_query.startswith('*') and search_query.endswith('*'):
            rs = rs.filter(card__card_label__icontains=search_query[1:-1])
        elif search_query.startswith('*'):
            rs = rs.filter(card__card_label__iendswith=search_query[1:])
        elif search_query.endswith('*'):
            rs = rs.filter(card__card_label__istartswith=search_query[:-1])
        else:
            rs = rs.filter(card__card_label__iexact=search_query)

    if vehicle_number:
        search_query = vehicle_number.strip()
        if search_query.startswith('*') and search_query.endswith('*'):
            rs = rs.filter(vehicle_number__icontains=search_query[1:-1])
        elif search_query.startswith('*'):
            rs = rs.filter(vehicle_number__iendswith=search_query[1:])
        elif search_query.endswith('*'):
            rs = rs.filter(vehicle_number__istartswith=search_query[:-1])
        else:
            rs = rs.filter(vehicle_number__iexact=search_query)

    if terminal_group:
        lanes = Lane.objects.filter(terminal__terminal_group_id=terminal_group)
        if mode == 1:
            rs = rs.filter(check_out_lane_id__in=lanes)
        else:
            rs = rs.filter(check_in_lane_id__in=lanes)

    if operator_id:
        if mode == 1:
            rs = rs.filter(check_out_operator_id=operator_id)
        else:
            rs = rs.filter(check_in_operator_id=operator_id)
    if mode == 1:
        rs = rs.order_by('-check_out_time')
    else:
        rs = rs.order_by('-check_in_time')

    return rs


def search_claim_promotion(queryset, card_id=None, vehicle_number=None, vehicle_type=None, from_time=None, to_time=None):

    rs = queryset.all().select_related('parking_session')

    if card_id:
        return rs.filter(parking_session_card__card_id=card_id)

    if from_time:
        rs = rs.filter(server_time__gte=from_time)

    if to_time:
        rs = rs.filter(server_time__lte=to_time)

    if vehicle_type:
        vehicle_type_range = decode_vehicle_type(int(vehicle_type))
        rs = rs.filter(parking_session__vehicle_type__gte=vehicle_type_range[0],
                       parking_session__vehicle_type__lte=vehicle_type_range[1])

    if vehicle_number:
        search_query = vehicle_number.strip()
        if search_query.startswith('*') and search_query.endswith('*'):
            rs = rs.filter(parking_session__vehicle_number__icontains=search_query[1:-1])
        elif search_query.startswith('*'):
            rs = rs.filter(parking_session__vehicle_number__iendswith=search_query[1:])
        elif search_query.endswith('*'):
            rs = rs.filter(parking_session__vehicle_number__istartswith=search_query[:-1])
        else:
            rs = rs.filter(parking_session__vehicle_number__iexact=search_query)

    rs = rs.order_by('-server_time')

    return rs


def get_statistics_by_location(time_from, time_to, card_type_id=None):
    rs = dict()
    rs['vehicle_types'] = list()
    for vehicle_type in VehicleType.objects.all():
        rs['vehicle_types'].append({'id': vehicle_type.id, 'name': vehicle_type.name, 'range': decode_vehicle_type(vehicle_type.id)})
    rs['vehicle_types'].reverse()
    check_filter = ParkingSession.objects.filter(Q(check_in_time__range=[time_from, time_to]) | Q(check_out_time__range=[time_from, time_to]))
    if card_type_id:
        check_filter = check_filter.filter(card__card_type=card_type_id)
    location_stats = list()
    location_map = dict()
    for loc in VEHICLE_LOCATIONS:
        stats = {'name': loc['name'], 'stats': {}}
        for vehicle_type in rs['vehicle_types']:
            stats['stats'][vehicle_type['id']] = {'check_in': 0, 'check_out': 0, 'remain': 0}
        location_stats.append(stats)
        if loc['code']:
            for code in loc['code']:
                location_map[code] = stats
        else:
            location_map['all'] = stats

    all_stats = location_map['all']
    for p in check_filter.values('check_in_alpr_vehicle_number', 'vehicle_type', 'check_in_time', 'check_out_time').iterator():
        code = p['check_in_alpr_vehicle_number'][:2]
        stats = location_map[code] if code in location_map else None
        vh = p['vehicle_type']
        check_in_time = p['check_in_time']
        check_out_time = p['check_out_time']
        for vehicle_type in rs['vehicle_types']:
            vehicle_type_id = vehicle_type['id']
            vehicle_type_range = vehicle_type['range']
            if vehicle_type_range[0] <= vh <= vehicle_type_range[1]:
                if time_from <= check_in_time <= time_to:
                    all_stats['stats'][vehicle_type_id]['check_in'] += 1
                    if stats:
                        stats['stats'][vehicle_type_id]['check_in'] += 1
                    if not check_out_time or check_out_time > time_to:
                        all_stats['stats'][vehicle_type_id]['remain'] += 1
                        if stats:
                            stats['stats'][vehicle_type_id]['remain'] += 1
                if check_out_time and time_from <= check_out_time <= time_to:
                    all_stats['stats'][vehicle_type_id]['check_out'] += 1
                    if stats:
                        stats['stats'][vehicle_type_id]['check_out'] += 1

    rs['data'] = list()
    raw_data = list()
    for loc in location_stats:
        row = [loc['name']]
        max_val = 0
        for vehicle_type in rs['vehicle_types']:
            check_in_count = loc['stats'][vehicle_type['id']]['check_in']
            check_out_count = loc['stats'][vehicle_type['id']]['check_out']
            remain_count = loc['stats'][vehicle_type['id']]['remain']
            max_val = max(check_in_count, max_val)
            max_val = max(check_out_count, max_val)
            max_val = max(remain_count, max_val)
            row.append(int_format(check_in_count))
            row.append(int_format(check_out_count))
            row.append(int_format(remain_count))
        if max_val > 0:
            raw_data.append({'max': max_val, 'row': row})
    for item in sorted(raw_data, key=lambda data: data['max'], reverse=True):
        rs['data'].append(item['row'])
    return rs


def get_statistics_by_location_only_checkin(time_from, time_to, card_type_id=None):
    rs = dict()
    rs['vehicle_types'] = list()
    for vehicle_type in VehicleType.objects.all():
        rs['vehicle_types'].append({'id': vehicle_type.id, 'name': vehicle_type.name, 'range': decode_vehicle_type(vehicle_type.id)})
    rs['vehicle_types'].reverse()
    check_filter = ParkingSession.objects.filter(check_in_time__range=[time_from, time_to])
    if card_type_id:
        check_filter = check_filter.filter(card__card_type=card_type_id)
    location_stats = list()
    location_map = dict()
    for loc in VEHICLE_LOCATIONS:
        stats = {'name': loc['name'], 'stats': {}}
        for vehicle_type in rs['vehicle_types']:
            stats['stats'][vehicle_type['id']] = 0
        location_stats.append(stats)
        if loc['code']:
            for code in loc['code']:
                location_map[code] = stats
        else:
            location_map['all'] = stats

    all_stats = location_map['all']
    for p in check_filter.values('check_in_alpr_vehicle_number', 'vehicle_type').iterator():
        code = p['check_in_alpr_vehicle_number'][:2]
        stats = location_map[code] if code in location_map else None
        vh = p['vehicle_type']
        for vehicle_type in rs['vehicle_types']:
            vehicle_type_id = vehicle_type['id']
            vehicle_type_range = vehicle_type['range']
            if vehicle_type_range[0] <= vh <= vehicle_type_range[1]:
                all_stats['stats'][vehicle_type_id] += 1
                if stats:
                    stats['stats'][vehicle_type_id] += 1

    rs['data'] = list()
    raw_data = list()
    for loc in location_stats:
        row = [loc['name']]
        max_val = 0
        for vehicle_type in rs['vehicle_types']:
            check_in_count = loc['stats'][vehicle_type['id']]
            max_val = max(check_in_count, max_val)
            row.append(int_format(check_in_count))
        if max_val > 0:
            raw_data.append({'max': max_val, 'row': row})
    for item in sorted(raw_data, key=lambda data: data['max'], reverse=True):
        rs['data'].append(item['row'])
    return rs


def get_statistics(time_from, time_to, user_id=None, terminal_id=None):
    rs = dict()
    rs['card_types'] = list()
    rs['vehicle_types'] = list()
    rs['data'] = dict()
    rs['card_types'].append({'id': -1, 'name': u'Tất cả'})
    for item in CardType.objects.all():
        rs['card_types'].append({'id': item.id, 'name': item.name})
    check_in_filter = ParkingSession.objects.filter(check_in_time__range=[time_from, time_to])
    check_out_filter = ParkingSession.objects.filter(check_out_time__range=[time_from, time_to])
    if user_id:
        check_in_filter = check_in_filter.filter(check_in_operator_id=user_id)
        check_out_filter = check_out_filter.filter(check_out_operator_id=user_id)
    if terminal_id:
        check_in_filter = check_in_filter.filter(check_in_lane__terminal_id=terminal_id)
        check_out_filter = check_out_filter.filter(check_out_lane__terminal_id=terminal_id)
    for vehicle_type in VehicleType.objects.all():
        vehicle_type_range = decode_vehicle_type(vehicle_type.id)
        rs['vehicle_types'].append({'id': vehicle_type.id, 'name': vehicle_type.name})
        rs['data'][vehicle_type.id] = dict()
        for card_type in rs['card_types']:
            if card_type['id'] == -1:
                check_in = check_in_filter.filter(vehicle_type__gte=vehicle_type_range[0],
                                                  vehicle_type__lte=vehicle_type_range[1])
                check_out = check_out_filter.filter(vehicle_type__gte=vehicle_type_range[0],
                                                    vehicle_type__lte=vehicle_type_range[1])
            else:
                check_in = check_in_filter.filter(vehicle_type__gte=vehicle_type_range[0],
                                                  vehicle_type__lte=vehicle_type_range[1],
                                                  card__card_type=card_type['id'])
                check_out = check_out_filter.filter(vehicle_type__gte=vehicle_type_range[0],
                                                    vehicle_type__lte=vehicle_type_range[1],
                                                    card__card_type=card_type['id'])
            remain = check_in.filter(Q(check_out_operator__isnull=True) | Q(check_out_time__gt=time_to))
            rs['data'][vehicle_type.id][card_type['id']] = {
                'check_in': check_in.count(),
                'check_out': check_out.count(),
                'remain': remain.count()
            }
    return rs


def get_parking_session_duration(obj):
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


# def export_parking_sessions(time_from, time_to):
#     rs = list()
#     rs.append([u'Tên thẻ', u'Loại thẻ', u'Loại xe', u'Biển số',
#                u'Nhận dạng biển số vào', u'Nhân viên cho vào', u'Cổng vào', u'Làn vào', u'Thời gian vào', u'Ảnh vào',
#                u'Nhận dạng biển số ra', u'Nhân viên cho ra', u'Cổng ra', u'Làn ra', u'Thời gian ra', u'Ảnh ra',
#                u'Thời gian trong bãi', u'Thông tin ngoại lệ'])
#     vehicle_type_dict = dict()
#     for t in VehicleType.objects.all():
#         vehicle_type_dict[get_storaged_vehicle_type(t.id)] = t.name
#     card_type_dict = dict()
#     for t in CardType.objects.all():
#         card_type_dict[int(t.id)] = t.name
#     user_dict = dict()
#     for u in UserProfile.objects.all():
#         user_dict[u.user_id] = u.staff_id + ' - ' + u.fullname
#     lane_dict = dict()
#     for l in Lane.objects.all().select_related('terminal'):
#         lane_dict[l.id] = {'name': l.name, 'terminal': l.terminal.name}
#     for p in ParkingSession.objects.filter(check_in_time__range=[time_from, time_to]).select_related('card', 'check_out_exception'):
#         card_type = card_type_dict[p.card.card_type] if p.card.card_type is not None and p.card.card_type in card_type_dict else ''
#         in_operator = user_dict[p.check_in_operator_id] if p.check_in_operator_id and p.check_in_operator_id in user_dict else ''
#         out_operator = user_dict[p.check_out_operator_id] if p.check_out_operator_id and p.check_out_operator_id in user_dict else ''
#         in_lane = lane_dict[p.check_in_lane_id] if p.check_in_lane_id and p.check_in_lane_id in lane_dict else {'name': '', 'terminal': ''}
#         out_lane = lane_dict[p.check_out_lane_id] if p.check_out_lane_id and p.check_out_lane_id in lane_dict else {'name': '', 'terminal': ''}
#         in_time = to_local_time(p.check_in_time).strftime('%d/%m/%Y %H:%M:%S') if p.check_in_time else ''
#         out_time = to_local_time(p.check_in_time).strftime('%d/%m/%Y %H:%M:%S') if p.check_out_time else ''
#         in_images = '%s, %s' % (p.check_in_images['front'], p.check_in_images['back']) if p.check_in_images else ''
#         out_images = '%s, %s' % (p.check_out_images['front'], p.check_out_images['back']) if p.check_out_images else ''
#         out_exception = p.check_out_exception.notes if p.check_out_exception else ''
#         duration = 1
#         if p.duration / 60 > 0:
#             duration = p.duration / 60
#         rs.append([p.card.card_label, card_type, vehicle_type_dict[p.vehicle_type], p.vehicle_number,
#                    p.check_in_alpr_vehicle_number, in_operator, in_lane['terminal'], in_lane['name'], in_time, in_images,
#                    p.check_out_alpr_vehicle_number, out_operator, out_lane['terminal'], out_lane['name'], out_time, out_images,
#                    duration, out_exception])
#     return rs


def export_parking_sessions(workbook, time_from, time_to):
    worksheet = workbook.add_worksheet()
    header_format = workbook.add_format({'bold': True, 'font_color': 'white', 'bg_color': '#002060'})
    headers = [u'Tên thẻ', u'Loại thẻ', u'Loại xe', u'Biển số',
               u'Nhận dạng biển số vào', u'Nhân viên cho vào', u'Cổng vào', u'Làn vào', u'Thời gian vào', u'Ảnh vào',
               u'Nhận dạng biển số ra', u'Nhân viên cho ra', u'Cổng ra', u'Làn ra', u'Thời gian ra', u'Ảnh ra',
               u'Thời gian trong bãi (phút)', u'Thông tin ngoại lệ']
    for ci, val in enumerate(headers):
        worksheet.write(0, ci, val, header_format)
    vehicle_type_dict = dict()
    for t in VehicleType.objects.all():
        vehicle_type_dict[get_storaged_vehicle_type(t.id)] = t.name
    card_type_dict = dict()
    for t in CardType.objects.all():
        card_type_dict[int(t.id)] = t.name
    user_dict = dict()
    for u in UserProfile.objects.all():
        user_dict[u.user_id] = u.staff_id + ' - ' + u.fullname
    lane_dict = dict()
    for l in Lane.objects.all().select_related('terminal'):
        lane_dict[l.id] = {'name': l.name, 'terminal': l.terminal.name}
    records = ParkingSession.objects.filter(check_in_time__range=[time_from, time_to]).select_related('card', 'check_out_exception')
    ri = 1
    total = records.count()
    for i in range(0, total, 10000):
        for p in records[i: i + 10000].iterator():
            card_type = card_type_dict[p.card.card_type] if p.card.card_type is not None and p.card.card_type in card_type_dict else ''
            in_operator = user_dict[p.check_in_operator_id] if p.check_in_operator_id and p.check_in_operator_id in user_dict else ''
            out_operator = user_dict[p.check_out_operator_id] if p.check_out_operator_id and p.check_out_operator_id in user_dict else ''
            in_lane = lane_dict[p.check_in_lane_id] if p.check_in_lane_id and p.check_in_lane_id in lane_dict else {'name': '', 'terminal': ''}
            out_lane = lane_dict[p.check_out_lane_id] if p.check_out_lane_id and p.check_out_lane_id in lane_dict else {'name': '', 'terminal': ''}
            in_time = to_local_time(p.check_in_time).strftime('%d/%m/%Y %H:%M:%S') if p.check_in_time else ''
            out_time = to_local_time(p.check_out_time).strftime('%d/%m/%Y %H:%M:%S') if p.check_out_time else ''
            in_images = '%s, %s' % (p.check_in_images['front'], p.check_in_images['back']) if p.check_in_images else ''
            out_images = '%s, %s' % (p.check_out_images['front'], p.check_out_images['back']) if p.check_out_images else ''
            out_exception = p.check_out_exception.notes if p.check_out_exception else ''
            if p.duration:
                seconds = p.duration
            else:
                seconds = int((get_now_utc() - p.check_in_time).total_seconds())
            duration = 1
            if seconds / 60 > 0:
                duration = seconds / 60
            row = [p.card.card_label, card_type, vehicle_type_dict[p.vehicle_type], p.vehicle_number,
                   p.check_in_alpr_vehicle_number, in_operator, in_lane['terminal'], in_lane['name'], in_time, in_images,
                   p.check_out_alpr_vehicle_number, out_operator, out_lane['terminal'], out_lane['name'], out_time, out_images,
                   duration, out_exception]
            for ci, val in enumerate(row):
                worksheet.write(ri, ci, val)
            ri += 1
    worksheet.freeze_panes(1, 0)
    worksheet.protect('ndhoang', options={
        'format_cells':          True,
        'format_columns':        True,
        'format_rows':           True,
        'select_locked_cells':   True,
        'sort':                  True,
        'autofilter':            True,
        'pivot_tables':          True,
        'select_unlocked_cells': True,
    })
    workbook.close()


def do_export_parking_sessions(savepath, time_from, time_to):
    try:
        temppath = os.path.splitext(savepath)[0] + '.tmp'
        workbook = xlsxwriter.Workbook(temppath, {'constant_memory': True})
        export_parking_sessions(workbook, time_from, time_to)
        os.rename(temppath, savepath)
    except:
        exc_type, exc_value, exc_traceback = sys.exc_info()
        lines = traceback.format_exception(exc_type, exc_value, exc_traceback)
        detail = ''.join('!! ' + line for line in lines)  # Log it or whatever here
        with open(os.path.splitext(savepath)[0] + '.err', 'w') as f:
            f.write(detail)


def export_parking_sessions_to_file(filepath, time_from, time_to):
    savepath = '/data/export/admin.apms.square-bit.com/' + filepath
    if os.path.exists(savepath):
        return
    savedir = os.path.dirname(savepath)
    if not os.path.exists(savedir):
        os.makedirs(savedir)
    now = time.time()
    for f in os.listdir(savedir):
        fp = os.path.join(savedir, f)
        if os.path.isfile(fp) and os.stat(fp).st_ctime < now - 7 * 86400:
            os.remove(fp)
    t = Thread(target=do_export_parking_sessions, args=(savepath, time_from, time_to))
    t.daemon = True
    t.start()