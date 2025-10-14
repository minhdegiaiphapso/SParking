# -*- coding: utf-8 -*-
import csv
import os
from django.http import Http404, HttpResponse
from django.contrib.auth.models import User
from django.db.models import Q

import requests
from datetime import datetime, time, timedelta
import dateutil.parser

from rest_framework.decorators import api_view
from rest_framework.response import Response
from rest_framework import status
from rest_framework import generics
from rest_framework.views import APIView
# from parking.views import calculate_parking_fee
from parking.views import callredemtion1, get_parking_fee_info, get_parking_fee_or_customer_info, calculate_parking_fee, is_vehicle_registration_available
from serializers import *
from webapi.settings import MEDIA_ROOT, IS_TEST, MEM_CACHE_ENABLE
from parking.models import Card, CardStatus, Terminal, Lane, Camera, ParkingSession, CheckInImage, UserCard, \
    UserShift, ParkingSetting, CheckOutException, UserProfile, Attendance, CardType, VehicleType, \
    CheckOutExceptionInfo, TerminalGroup, ParkingFeeSession, VehicleRegistration, \
    ClaimPromotion, ClaimPromotionBill, ClaimPromotionCoupon, ClaimPromotionV2, ClaimPromotionBillV2, ClaimPromotionCouponV2, \
    ClaimPromotionTenant, ClaimPromotionVoucher, Customer,\
    VEHICLE_TYPE_CATEGORY, load_nghia_vehicle_type, get_storaged_vehicle_type, decode_vehicle_type, get_setting
from parking.common import CARD_STATUS
from parking.services import search_parking_session, get_statistics, search_parking_session_new, search_claim_promotion
from utils import *
from image_replication import ImageReplicationController
from django.db import connections
import redis
import json
import datetime

MAGIC_BYPASS_NUMBER = 999999

VEHICLE_TYPE_DICT = load_nghia_vehicle_type()


# PARKING_NAME = get_setting('parking_name', 'Parking Name', 'Green Parking')
# LIMIT_SLOTS = dict()
# LIMIT_SLOTS[1] = int(get_setting('car_limit_slot', 'Car Limit Slots', 500))
# LIMIT_SLOTS[2] = int(get_setting('bike_limit_slot', 'Bike Limit Slots', 5000))

def get_limit_slots(vehicle_type):
    if vehicle_type == VEHICLE_TYPE_CATEGORY[2][0]:
        # return LIMIT_SLOTS[1]
        return int(get_setting('car_limit_slot', u'Số chỗ ô tô', 500))
    else:
        # return LIMIT_SLOTS[2]
        return int(get_setting('bike_limit_slot', u'Số chỗ xe máy', 5000))

##2018Dec13
def getparkingslots(vehicle_type):
    try:
        util=Utilities()
        qr=util.Query('getslotbyvehicle',vehicle_type)
        if qr and len(qr)>0:
            slottotal=qr[0][1] if qr[0][1] else 0
            curentslot=qr[0][2] if qr[0][2] else 0
            blankslot=slottotal-curentslot
            return  slottotal,curentslot,blankslot
        else:
            return 0, 0, 0
    except Exception as e:
        return  0,0,0
##2018Dec13
redis_client = None
if IS_TEST:
    if MEM_CACHE_ENABLE:
        redis_client = FakeStrictRedis()
    image_replication = None
else:
    if MEM_CACHE_ENABLE:
        redis_client = redis.StrictRedis(host='localhost', port=6379, db=0)
    image_replication = ImageReplicationController()


def get_cached_checkin_info(card_id):
    if not redis_client:
        return None
    rs = redis_client.get('checkin_' + card_id)
    if not rs:
        return None
    data = json.loads(rs)
    return {
        'card_status': dict_to_card_status(data['card_status']),
        'parking_session': dict_to_parking_session(data['parking_session']),
        'card': dict_to_card(data['card']),
        'terminal_id': data['terminal_id']
    }


def set_cached_checkin_info(card_id, card_status, parking_session, card, terminal_id):
    if not redis_client:
        return
    data = {
        'card_status': card_status_to_dict(card_status),
        'parking_session': parking_session_to_dict(parking_session),
        'card': card_to_dict(card),
        'terminal_id': terminal_id
    }
    redis_client.set('checkin_' + card_id, json.dumps(data), None)


def del_cached_checkin_info(card_id):
    if not redis_client:
        return
    redis_client.delete('checkin_' + card_id)


def lp_exists(vehicle_number):
    if not redis_client:
        return CardStatus.objects.filter(status=1, parking_session__vehicle_number=vehicle_number).count() > 1
    if redis_client.get('lp_' + vehicle_number) > 1:
        return True
    return False


def lp_add(vehicle_number):
    if not redis_client:
        return lp_exists(vehicle_number)
    if redis_client.get('lp_' + vehicle_number):
        redis_client.incr('lp_' + vehicle_number)
        return True
    else:
        redis_client.set('lp_' + vehicle_number, 1, None)
        return False

def lp_remove(vehicle_number):
    if redis_client and redis_client.decr('lp_' + vehicle_number) == 0:
        redis_client.delete('lp_' + vehicle_number)


def get_vehicle_count():
    # if not redis_client:
    #     return CardStatus.objects.filter(status=1).count()
    # return int(redis_client.get('vehicle_count'))
    ##
    return ParkingSession.objects.filter(check_out_time=None).count()
    ##
def inc_vehicle_count():
    if not redis_client:
        return get_vehicle_count()
    return redis_client.incr('vehicle_count')


def dec_vehicle_count():
    if redis_client:
        redis_client.decr('vehicle_count')


def init_cache_data():
    if not redis_client:
        return
    records = CardStatus.objects.filter(status=1)
    redis_client.set('vehicle_count', records.count(), None)
    for card_status in records:
        parking_session = card_status.parking_session
        set_cached_checkin_info(card_status.card_id, card_status, parking_session, card_status.card,
                                parking_session.check_in_lane.terminal_id)
        lp_add(card_status.parking_session.vehicle_number)


init_cache_data()


def get_ip(request):
    x_forwarded_for = request.META.get('HTTP_X_FORWARDED_FOR')
    if x_forwarded_for:
        ip = x_forwarded_for.split(',')[0]
    else:
        ip = request.META.get('REMOTE_ADDR')
    return ip


def save_image(image, filename):
    if filename[0] == '/':
        save_path = MEDIA_ROOT + filename
    else:
        save_path = MEDIA_ROOT + '/' + filename
    dir_path = os.path.dirname(save_path)
    if not os.path.exists(dir_path):
        os.makedirs(dir_path)
    with open(save_path, 'wb') as f:
        f.write(image.read())


def delete_image(filename):
    if filename[0] == '/':
        save_path = MEDIA_ROOT + filename
    else:
        save_path = MEDIA_ROOT + '/' + filename
    if os.path.exists(save_path):
        os.remove(save_path)


def create_save_paths(card_label, is_check_in):
    if is_check_in:
        direction = 'in'
    else:
        direction = 'out'
    if len(card_label) < 2:
        subfix = '0' + card_label
    else:
        subfix = card_label[-2:]
    check_time = datetime.datetime.now()
    date_str = '%.4d%.2d%.2d' % (check_time.year, check_time.month, check_time.day)
    time_str = '%.2d%.2d' % (check_time.hour, check_time.minute)
    file_name_base = '%s/%s/%s/%s/%s_%s' % (date_str, direction, subfix, card_label, time_str, card_label)
    return {
        'front': file_name_base + '_f.jpg',
        'back': file_name_base + '_b.jpg',
        'extra1': file_name_base + '_e1.jpg',
        'extra2': file_name_base + '_e2.jpg'
    }


class TerminalGroupListView(generics.ListAPIView):
    """
    Terminal Group Collection
    """
    serializer_class = TerminalGroupListSerializer
    queryset = TerminalGroup.objects.all()


class CardListView(generics.ListCreateAPIView):
    """
    Parking Card Collection
    """
    serializer_class = CardListSerializer
    queryset = Card.objects.all()

    def create(self, request, *args, **kwargs):
        if 'bulk' in request.DATA:
            create_count = 0
            error_cards = list()
            try:
                bulk_data = json.loads(request.DATA['bulk'])
            except ValueError:
                return Response({'detail': 'Bulk data error'}, status.HTTP_400_BAD_REQUEST)
            for data in bulk_data:
                serializer = self.get_serializer(data=data)
                if serializer.is_valid():
                    serializer.save()
                    create_count += 1
                else:
                    error_cards.append(data['card_id'])
            return Response({'created': create_count, 'error_cards': error_cards}, status=status.HTTP_201_CREATED)
        else:
            return super(CardListView, self).create(request, args, kwargs)

    def list(self, request, *args, **kwargs):
        response = HttpResponse(content_type='text/csv; charset=utf-8')
        response['Content-Disposition'] = 'attachment; filename="statistics.csv"'

        card_type_dict = dict()
        for card_type in CardType.objects.all():
            card_type_dict[card_type.id] = card_type.name
        vehicle_type_dict = dict()
        for vehicle_type in VehicleType.objects.all():
            vehicle_type_dict[vehicle_type.id] = vehicle_type.name
        card_status_dict = dict(CARD_STATUS)
        writer = UnicodeWriter(response)
        writer.writerow([u'ID', u'Mã thẻ',
                         u'Loại thẻ',
                         u'Loại xe',
                         u'Trạng thái'])
        for card in Card.objects.all():
            writer.writerow(
                [str(card.id), card.card_label, card_type_dict[card.card_type], vehicle_type_dict[card.vehicle_type],
                 card_status_dict[card.status]])

        # card_type_dict = dict()
        # for card_type in CardType.objects.all():
        #     card_type_dict[card_type.id] = unicode(card_type.name).encode("utf-8")
        # vehicle_type_dict = dict()
        # for vehicle_type in VehicleType.objects.all():
        #     vehicle_type_dict[vehicle_type.id] = unicode(vehicle_type.name).encode("utf-8")
        # card_status_dict = dict(CARD_STATUS)
        # writer = csv.writer(response)
        # writer.writerow(['ID', unicode(u'Mã thẻ').encode("utf-8"),
        #                  unicode(u'Loại thẻ').encode("utf-8"),
        #                  unicode(u'Loại xe').encode("utf-8"),
        #                  unicode(u'Trạng thái').encode("utf-8")])
        # for card in Card.objects.all():
        #     writer.writerow([card.id, card.card_label, card_type_dict[card.card_type], vehicle_type_dict[card.vehicle_type], card_status_dict[card.status]])


        # card_type_dict = dict()
        # for card_type in CardType.objects.all():
        #     card_type_dict[card_type.id] = card_type.name
        # vehicle_type_dict = dict()
        # for vehicle_type in VehicleType.objects.all():
        #     vehicle_type_dict[vehicle_type.id] = vehicle_type.name
        # card_status_dict = dict(CARD_STATUS)
        # rs = u'ID,Mã thẻ,Loại thẻ,Loại xe,Trạng thái\n'
        # for card in Card.objects.all():
        #     rs += u'%s,%s,%s,%s,%s\n' % (card.id, card.card_label, card_type_dict[card.card_type], vehicle_type_dict[card.vehicle_type], card_status_dict[card.status])

        # response.write(unicode(rs).encode('utf-8'))

        return response


class CardDetailView(generics.RetrieveUpdateAPIView):
    """
    Parking Card Detail
    """
    lookup_field = 'card_id'
    serializer_class = CardDetailSerializer
    queryset = Card.objects.all()


def get_card(card_id):
    try:
        return Card.objects.get(card_id=card_id)
    except Card.DoesNotExist:
        #raise Http404("Card is Not Found")
        return  None


def get_lane(lane_id):
    try:
        return Lane.objects.get(id=lane_id)
    except Lane.DoesNotExist:
        return None


def get_card_status(card_id):
    try:
        return CardStatus.objects.get(card_id=card_id)
    except CardStatus.DoesNotExist:
        return None


class ParkingSessionUpdateView(generics.UpdateAPIView):
    """
    Update Parking session info
    """
    serializer_class = UpdateVehicleNumberSerializer

    def update(self, request, *args, **kwargs):
        parking_session_id = kwargs['id']
        try:
            parking_session = ParkingSession.objects.get(id=parking_session_id)
        except ParkingSession.DoesNotExist:
            return Response(status=status.HTTP_404_NOT_FOUND)
        should_save = False
        if 'vehicle_number' in request.DATA and request.DATA['vehicle_number']:
            parking_session.vehicle_number = request.DATA['vehicle_number']
            should_save = True
        if 'check_in_alpr_vehicle_number' in request.DATA and request.DATA['check_in_alpr_vehicle_number']:
            parking_session.check_in_alpr_vehicle_number = request.DATA['check_in_alpr_vehicle_number']
            should_save = True
        if 'check_out_alpr_vehicle_number' in request.DATA and request.DATA['check_out_alpr_vehicle_number']:
            parking_session.check_out_alpr_vehicle_number = request.DATA['check_out_alpr_vehicle_number']
            should_save = True
        if parking_session and should_save:
            parking_session.save()
        return Response(status=status.HTTP_200_OK)


class StatisticsView(generics.ListAPIView):
    """
    Statistics
    time_from -- From timestamp (REQUIRED)
    time_to -- To timestamp (REQUIRED)
    terminal_id -- Terminal ID
    """
    serializer_class = StatisticsSerializer

    def list(self, request, *args, **kwargs):
        if 'time_from' in request.QUERY_PARAMS and request.QUERY_PARAMS['time_from']:
            time_from = timestamp2datetime(int(request.QUERY_PARAMS['time_from']))
        else:
            return Response({'detail': 'time_from is required'}, status=status.HTTP_400_BAD_REQUEST)
        if 'time_to' in request.QUERY_PARAMS and request.QUERY_PARAMS['time_to']:
            time_to = timestamp2datetime(int(request.QUERY_PARAMS['time_to']))
        else:
            return Response({'detail': 'time_to is required'}, status=status.HTTP_400_BAD_REQUEST)
        if 'terminal_id' in request.QUERY_PARAMS and request.QUERY_PARAMS['terminal_id']:
            terminal_id = request.QUERY_PARAMS['terminal_id']
        else:
            terminal_id = None
        return Response(data=get_statistics(time_from, time_to, None, terminal_id), status=status.HTTP_200_OK)


class CalculateParkingFeeView(generics.CreateAPIView):
    """
    Statistics
    """
    serializer_class = ParkingFeeSerializer

    def create(self, request, *args, **kwargs):
        serialize = self.get_serializer(data=request.DATA)

        if serialize.is_valid():
            obj = serialize.object
            fromTime = obj['from_time']
            toTime = obj['to_time']
            vehicleType = get_storaged_vehicle_type(obj['vehicleType'])
            return Response(data=calculate_parking_fee("",None, vehicleType, fromTime, toTime), status=status.HTTP_200_OK)
        else:
            return Response(serialize.errors, status=status.HTTP_400_BAD_REQUEST)
##2018Dec13
class ParkingSessionSlotView(generics.ListAPIView):
    """
    Slots parking session information
    """
    serializer_class = ParkingSessionSlotSerializer
    def get_queryset(self):
        util = Utilities()
        res = util.Query('getslots')
        rs = []
        for it in res:
            dt = {}
            dt["slot_id"] = int(it[0])
            dt["name"] = it[1]
            dt["prefix"] = it[2]
            dt["suffixes"] = it[3]
            dt["numlength"] = int(it[4]) if it[4] else 0
            dt["hascheckkey"] = int(it[5]) if it[5] else 0
            dt["slottotal"] = int(it[6]) if it[6] else 0
            dt["currentslot"] = int(it[7]) if it[7] else 0
            dt["plankslot"] = dt["slottotal"]-dt["currentslot"]
            dt["vehicletypesid"] = it[8]
            dt["vehicletypesidname"] = it[9]
            rs.append(dt);
            # datas = self.get_serializer(rs, many=True).data
        return rs
##2018Dec13
class CardCheckInSearchView(generics.ListAPIView):
    """
    Search check in information
    mode -- 0: In parking (Default)<br/>1: Only out<br/>2: All
    card_id -- Card id
    card_label -- Card label
    vehicle_number -- Vehicle number
    from_time -- From time
    to_time -- To time
    vehicle_type -- Vehicle type
    limit -- Maximum number of return items. Default 20
    """
    serializer_class = ParkingSessionSerializer

    def list(self, request, *args, **kwargs):
        try:
            limit = int(get_param(request, 'limit', 20))
            mode = int(get_param(request, 'mode', 0))
            card_id = get_param(request, 'card_id')
            card_label = get_param(request, 'card_label')
            vehicle_number = get_param(request, 'vehicle_number')
            vehicle_type = get_param(request, 'vehicle_type')
            from_timestamp = get_param(request, 'from_time')
            to_timestamp = get_param(request, 'to_time')
            from_time = None
            to_time = None
            if from_timestamp:
                from_time = timestamp2datetime(int(from_timestamp))
            if to_timestamp:
                to_time = timestamp2datetime(int(to_timestamp))
            rs = search_parking_session(mode, limit, card_id, card_label, vehicle_number, vehicle_type, from_time,
                                        to_time)
            serializer = self.get_serializer(rs, many=True)
            return Response(serializer.data)
        except ValueError:
            return Response({'detail': 'Invalid query parameter'}, status=status.HTTP_400_BAD_REQUEST)
class ParkingSessionSearchView(generics.ListAPIView):
    """
    Search parking session information
    mode -- 0: In parking (Default)<br/>1: Only out<br/>2: All
    card_id -- Card id
    card_label -- Card label
    vehicle_number -- Vehicle number
    from_time -- From time
    to_time -- To time
    vehicle_type -- Vehicle type
    terminal_group -- Terminal Group id to search
    page -- page index
    page_size -- number items per page (default = 20)
    """
    serializer_class = ParkingSessionSearchSerializer_new
    #paginate_by = 20
    #paginate_by_param = 'page_size'F
    # queryset = ParkingSession.objects.all().prefetch_related('card')
    def get_queryset(self):
        request = self.request
        mode = int(get_param(request, 'mode', 0))
        card_id = get_param(request, 'card_id') if get_param(request, 'card_id') else 0
        card_label = get_param(request, 'card_label') if get_param(request, 'card_label') else ''
        vehicle_number = get_param(request, 'vehicle_number') if get_param(request, 'vehicle_number') else ''
        vehicle_type = int(get_param(request, 'vehicle_type')) if get_param(request, 'vehicle_type') and get_param(
            request, 'vehicle_type') != '100000000'  else 0
        if vehicle_type>0:
            vehicle_type = vehicle_type / 10000
        from_timestamp = get_param(request, 'from_time')
        to_timestamp = get_param(request, 'to_time')
        terminal_group = get_param(request, 'terminal_group') if get_param(request, 'terminal_group') else 0
        operator_id = get_param(request, "operator_id") if get_param(request, "operator_id") else 0
        from_time = None
        to_time = None
        if from_timestamp:
            from_time = timestamp2datetime(int(from_timestamp)).strftime("%Y-%m-%d %H:%M:%S")
        if to_timestamp:
            to_time = timestamp2datetime(int(to_timestamp)).strftime("%Y-%m-%d %H:%M:%S")
        page = int(get_param(request, 'page', 1))
        page_size = int(get_param(request, 'page_size', 20))
        util = Utilities()
        if from_timestamp and to_timestamp and int(to_timestamp)-int(from_timestamp)<3600*24*32:
            res = util.Query('get_parkingsessionsearch', from_time, to_time, card_id, card_label, vehicle_number,
                             vehicle_type, mode, terminal_group, operator_id, page, page_size)
        else:
            res = util.Query('get_parkingsessionsearch_multi', from_time, to_time, card_id, card_label, vehicle_number,
                             vehicle_type, mode, terminal_group, operator_id, page, page_size)
        rs = []
        for it in res:
            dt = {}
            dt["id"] = int(it[1])
            dt["card_id"] = it[2]
            dt["card_label"] = it[3]
            dt["card_type"] = it[4]
            dt["vehicle_type"] = int(it[5]) if it[5] else 0
            dt["vehicle_number"] = it[6]
            dt["check_in_alpr_vehicle_number"] = it[7]
            dt["check_out_alpr_vehicle_number"] = it[8]
            dt["check_in_images"] = json.loads(it[9]) if it[9] else None
            dt["check_out_images"] = json.loads(it[10]) if it[10] else None
            dt["check_in_time"] = it[11]
            dt["check_out_time"] = it[12]
            dt["check_in_lane_id"] = it[13]
            dt["check_out_lane_id"] = it[14]
            #dt["fee"]=it[15]
            dt["total"] = it[15]
            rs.append(dt);
       # datas = self.get_serializer(rs, many=True).data
        return  rs

    # def list(self, request, *args, **kwargs):
    #     try:
    #         mode = int(get_param(request, 'mode', 0))
    #         card_id = get_param(request, 'card_id') if get_param(request, 'card_id') else 0
    #         card_label = get_param(request, 'card_label') if get_param(request, 'card_label') else ''
    #         vehicle_number = get_param(request, 'vehicle_number') if get_param(request, 'vehicle_number') else ''
    #         vehicle_type = get_param(request, 'vehicle_type') if get_param(request, 'vehicle_type') and get_param(
    #             request, 'vehicle_type') != '100000000'  else 0
    #         from_timestamp = get_param(request, 'from_time')
    #         to_timestamp = get_param(request, 'to_time')
    #         terminal_group = get_param(request, 'terminal_group') if get_param(request, 'terminal_group') else 0
    #         operator_id = get_param(request, "operator_id") if get_param(request, "operator_id") else 0
    #         from_time = None
    #         to_time = None
    #         if from_timestamp:
    #             from_time = timestamp2datetime(int(from_timestamp)).strftime("%Y-%m-%d %H:%M:%S")
    #         if to_timestamp:
    #             to_time = timestamp2datetime(int(to_timestamp)).strftime("%Y-%m-%d %H:%M:%S")
    #         page = int(get_param(request, 'page', 1))
    #         page_size = int(get_param(request, 'page_size', 20))
    #         util = Utilities()
    #         res = util.Query('get_parkingsessionsearch', from_time, to_time, card_id, card_label, vehicle_number,
    #                          vehicle_type, mode, terminal_group, operator_id, page, page_size)
    #         rs = []
    #         for it in res:
    #             dt = {}
    #             dt["id"] = int(it[1])
    #             dt["card_id"] = it[2]
    #             dt["card_label"] = it[3]
    #             dt["card_type"] = it[4]
    #             dt["vehicle_type"] = int(it[5]) if it[5] else 0
    #             dt["vehicle_number"] = it[6]
    #             dt["check_in_alpr_vehicle_number"] = it[7]
    #             dt["check_out_alpr_vehicle_number"] = it[8]
    #             dt["check_in_images"] = json.loads(it[9]) if it[9] else None
    #             dt["check_out_images"] = json.loads(it[10]) if it[10] else None
    #             dt["check_in_time"] = it[11]
    #             dt["check_out_time"] = it[12]
    #             dt["check_in_lane_id"] = it[13]
    #             dt["check_out_lane_id"] = it[14]
    #             # dt["fee"]=it[15]
    #             dt["total"] = it[15]
    #             rs.append(dt);
    #         datas = self.get_serializer(rs, many=True).data
    #         return Response(datas)
    #         # return Response(serializer.data)
    #     except Exception as e:
    #         return Response({'detail': 'Invalid query parameter'}, status=status.HTTP_400_BAD_REQUEST)

def get_time(datetime1):
    return time(datetime1.hour, datetime1.minute, datetime1.second, datetime1.microsecond)
def is_overnight(in_time, out_time):
    _out_time_temp = in_time.replace(hour=0, minute=0, second=0)
    _out_time_temp = _out_time_temp + timedelta(days=1)

    if out_time >= _out_time_temp:
        # print "Co qua dem"
        return True
    return False
##2024Jan10
def raisCardId(card_id):
    try:
        rg = VehicleRegistration.objects.filter(numberplate=card_id, status=1).order_by('-vehicle_type')
        if rg:
            r= rg[0]
            return r.card.card_id, r.vehicle_number, card_id
        return  card_id, None, None
    except:
        return  card_id, None, None
##2018Dec14
class CardCheckInView_New(generics.CreateAPIView, generics.RetrieveUpdateAPIView):
    """
    Check in
    """
    serializer_class = CardCheckInSerializer
    def create(self, request, *args, **kwargs):
        card_id = kwargs['card_id']
        #2024Jan10
        card_id, alpr_vehicle_number, vehicle_number, numberplate = raisCardId(card_id)
        # if get_cached_checkin_info(card_id):
        #     return Response({'detail': 'Card is in use'}, status=status.HTTP_400_BAD_REQUEST)
        card = get_card(card_id)
        if not card:
            return Response({'detail': 'Card not found'}, status=status.HTTP_404_NOT_FOUND)
        can_check_in_out_when_in_effective_range = is_vehicle_registration_available(card_id)
        if not can_check_in_out_when_in_effective_range[0]:
            return_obj = dict()
            return_obj['detail'] = 'Card is not in effective range'
            return_obj['customer_info'] = can_check_in_out_when_in_effective_range[1]
            return Response(return_obj, status=status.HTTP_406_NOT_ACCEPTABLE)
        if card.status == 1:
            pkss = ParkingSession.objects.filter(check_out_time=None, card=card).order_by('-check_in_time')
            if pkss and len(pkss)>0:
                l=len(pkss)
                for pk in pkss:
                    if pk != pkss[0]:
                        pf = ParkingFeeSession.objects.filter(parking_session_id=pk.id)
                        for f in pf:
                            f.delete()
                        pk.delete()
                pcm=ParkingSession.objects.filter(card=card).order_by('-check_out_time')
                if pcm and len(pcm)>0 and pcm[0].check_out_time>=pkss[0].check_in_time:
                    pf = ParkingFeeSession.objects.filter(parking_session_id=pkss[0].id)
                    for f in pf:
                        f.delete()
                    pkss[0].delete()
                else:
                    return Response({'detail': 'Card is in use'}, status=status.HTTP_400_BAD_REQUEST)
            serializer = self.get_serializer(data=request.DATA, files=request.FILES)
            if serializer.is_valid():
                obj = serializer.object
                images = create_save_paths(card.card_label, True)
                save_image(obj['front_thumb'], images['front'])
                save_image(obj['back_thumb'], images['back'])
                if 'extra1_thumb' in obj:
                    save_image(obj['extra1_thumb'], images['extra1'])
                if 'extra2_thumb' in obj:
                    save_image(obj['extra2_thumb'], images['extra2'])
                vtype=obj['vehicle_type']
                vehicle_type = get_storaged_vehicle_type(int(obj['vehicle_type']))

                #  Check number of entry of user
                start_day_time = get_now_utc().replace(hour=0, minute=0, second=0)
                entryCount = 0
                entries = []
                
                if obj['entry_check']:
                    entries = ParkingSession.objects.exclude(check_in_alpr_vehicle_number='????') \
                        .filter(check_in_alpr_vehicle_number__contains=obj['prefix_vehicle_number'],
                                vehicle_number=obj['vehicle_number'],
                                check_in_time__gte=start_day_time,
                                check_out_time__isnull=False).values('check_in_time', 'check_out_time')
                    entryCount = entries.count()
                cursor = connections['default'].cursor()
                cursor.execute("begin")
                try:
                    ci_time=get_now_utc()
                    img=json.dumps(images, ensure_ascii=False).encode('utf8')
                    qr = "insert into parking_parkingsession(`card_id`,`vehicle_type`,`vehicle_number`,`check_in_alpr_vehicle_number`,`check_in_operator_id`,`check_in_time`,`check_in_lane_id`,`check_in_images`) values('%s','%s','%s','%s','%s','%s','%s','%s');" % (
                        card.id, vehicle_type, obj['vehicle_number'] if vehicle_number is None else vehicle_number, obj['alpr_vehicle_number'] if alpr_vehicle_number is None else alpr_vehicle_number, obj['operator_id'],
                        ci_time.strftime("%Y-%m-%d %H:%M:%S"), obj['lane_id'], img)
                    cursor.execute(qr)
                    id = cursor.lastrowid
                    qr = "insert into parking_checkinimage(`terminal_id`,`parking_session_id`) values('%s','%s');" % (
                    obj['terminal_id'], id)
                    cursor.execute(qr)

                    parking_session=ParkingSession.objects.filter(id=id)
                    if parking_session:
                        parking_session=parking_session[0]
                    else:
                        parking_session=None
                    pk_slot=getparkingslots(vtype)
                    res_obj = dict()
                    res_obj['parking_session_id'] = int(id)
                    res_obj['card_label'] = card.card_label
                    res_obj['card_type'] = card.card_type
                    res_obj['front_image_path'] = images['front']
                    res_obj['back_image_path'] = images['back']
                    res_obj['check_in_time'] = datetime2timestamp(ci_time)
                    res_obj['vehicle_number_exist'] = lp_add(obj['vehicle_number'])
                    res_obj['limit_num_slots'] =pk_slot[0]# get_limit_slots(card.vehicle_type)
                    res_obj['current_num_slots'] =pk_slot[1]# inc_vehicle_count()
                    res_obj['entryCount'] = entryCount
                    res_obj['entries'] = list(entries)

                    # Create card checkin
                    result = get_parking_fee_info(card_id, vehicle_type,
                                                     card.card_type,
                                                  ci_time, get_now_utc())
                    ## before 208May10
                    #result=get_parking_fee_or_customer_info(card_id, parking_session.id, parking_session.vehicle_type,
                    #                                             parking_session.check_in_time, get_now_utc())
                    res_obj['customer_info'] = result

                    is_vehicle_registration = True if 'vehicle_registration_info' in result else False

                    parking_fee_session = ParkingFeeSession(parking_session_id=int(id),
                                                            card_id=str(card_id),
                                                            vehicle_number=obj['vehicle_number'] or '',
                                                            parking_fee=result['parking_fee'],
                                                            parking_fee_detail=result['parking_fee_detail'],
                                                            session_type='IN',
                                                            is_vehicle_registration=is_vehicle_registration,
                                                            vehicle_type_id=VEHICLE_TYPE_DICT[vehicle_type])
                    parking_fee_session.save()

                    # Check begin staff attendance
                    staffs = UserProfile.objects.filter(card=card)
                    if staffs.count() > 0:
                        staff = staffs[0]
                        Attendance.objects.create(user=staff.user, time_in=get_now(), parking_session=parking_session)
                    cursor.execute("commit")
                except Exception as e:
                    cursor.execute("rollback")
                    return Response({'detail': 'Save Fail'}, status=status.HTTP_400_BAD_REQUEST)
                return Response(res_obj, status=status.HTTP_201_CREATED)
            else:
                return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)
        elif card.status == 2:
            return Response({'detail': 'Card is locked'}, status=status.HTTP_400_BAD_REQUEST)
        else:
            return Response({'detail': 'Card is not enabled'}, status=status.HTTP_400_BAD_REQUEST)
    ##Before 2018Jun21
    def update(self, request, *args, **kwargs):
        card_id = kwargs['card_id']
        #2024Jan10
        card_id, alpr_vehicle_number, vehicle_number = raisCardId(card_id)
        serializer = self.get_serializer(data=request.DATA, files=request.FILES)
        if serializer.is_valid():
            card = get_card(card_id)
            if not card:
                return Response({'detail': 'Card not found'}, status=status.HTTP_404_NOT_FOUND)
            ##
            if card.status == 2:
                return Response({'detail': 'Card is locked'}, status=status.HTTP_400_BAD_REQUEST)
            elif card.status != 1 :
                return Response({'detail': 'Card is not enabled'}, status=status.HTTP_400_BAD_REQUEST)
            pkss = ParkingSession.objects.filter(check_out_time=None, card=card)
            if not pkss or len(pkss) <= 0:
                return Response({'detail': 'Card is not in use'}, status=status.HTTP_400_BAD_REQUEST)
            parking_session = pkss[0]
            card = parking_session.card
            terminal_id = parking_session.check_in_lane.terminal_id
            obj = serializer.object
            lp_changed = (has_param('vehicle_number', obj) and obj['vehicle_number'] != parking_session.vehicle_number)
            if lp_changed:
                lp_remove(parking_session.vehicle_number)
            should_save = False
            should_save = self.try_update_vehicle_number(obj, parking_session) or should_save
            should_save = self.try_update_alpr_vehicle_number(obj, parking_session) or should_save
            should_save = self.try_update_vehicle_type(obj, parking_session) or should_save
            should_save = self.try_update_images(obj, parking_session) or should_save
            if should_save:
                parking_session.save()
                # set_cached_checkin_info(card_id, card_status, parking_session, card, terminal_id)
            images = parking_session.check_in_images

            #  Check number of entry of user
            start_day_time = get_now_utc().replace(hour=0, minute=0, second=0)
            entryCount = 0
            entries = []

            if obj['entry_check']:
                entries = ParkingSession.objects.exclude(check_in_alpr_vehicle_number='????') \
                    .filter(check_in_alpr_vehicle_number__contains=obj['prefix_vehicle_number'],
                            vehicle_number=obj['vehicle_number'],
                            check_in_time__gte=start_day_time,
                            check_out_time__isnull=False).values('check_in_time', 'check_out_time')
                entryCount = entries.count()
            vtype = obj['vehicle_type']
            ps_slots=getparkingslots(vtype)
            res_obj = dict()
            res_obj['parking_session_id'] =parking_session.id
            res_obj['card_label'] = card.card_label
            res_obj['card_type'] = card.card_type
            res_obj['front_image_path'] = images['front']
            res_obj['back_image_path'] = images['back']
            res_obj['check_in_time'] = datetime2timestamp(parking_session.check_in_time)
            if lp_changed:
                res_obj['vehicle_number_exist'] = lp_add(parking_session.vehicle_number)
            else:
                res_obj['vehicle_number_exist'] = lp_exists(parking_session.vehicle_number)
            res_obj['limit_num_slots'] =ps_slots[0]# get_limit_slots(card.vehicle_type)
            res_obj['current_num_slots'] =ps_slots[1]# get_vehicle_count()
            res_obj['entryCount'] = entryCount
            res_obj['entries'] = list(entries)
            res_obj['customer_info'] = get_parking_fee_info(card_id, parking_session.vehicle_type,
                                                 card.card_type,
                                                 parking_session.check_in_time, get_now_utc())
            ## before 208May10
            # res_obj['customer_info']=get_parking_fee_or_customer_info(card_id, parking_session.id, parking_session.vehicle_type,
            #                                             parking_session.check_in_time, get_now_utc())
            return Response(res_obj, status=status.HTTP_200_OK)
    def retrieve(self, request, *args, **kwargs):
        card_id = kwargs['card_id']
        #2024Jan10
        card_id, alpr_vehicle_number, vehicle_number = raisCardId(card_id)
        card = get_card(card_id)
        if not card:
            return Response({'detail': 'Card not found'}, status=status.HTTP_404_NOT_FOUND)
        pkss = ParkingSession.objects.filter(check_out_time=None, card=card)
        if not pkss or len(pkss)<=0:
            return Response({'detail': 'Card is not in use'}, status=status.HTTP_400_BAD_REQUEST)
        parking_session=pkss[0]
        terminal_id = parking_session.check_in_lane.terminal_id
        serializer = BasicCheckInSessionSerializer(parking_session, many=False)
        rs = serializer.data
        rs['image_hosts'] = self.get_image_hosts(parking_session)
        rs['card_label'] = card.card_label
        rs['card_type'] = card.card_type
        rs['terminal_id'] = terminal_id
        rs['parking_session_id'] = parking_session.id

        to_time = get_now_utc()
        ut = Utilities()
        qr = ut.QuerySecond('isactivetoolfee')
        if qr and len(qr) == 1 and qr[0][0] == 'new':
            ##From 2018May10
            # Check if claim promotion exist
            claim_promotion_session = ClaimPromotionV2.objects.filter(parking_session_id=parking_session.id, used=False)
            if claim_promotion_session and claim_promotion_session.count > 0:  # If claim promotion exist
                claim_promotion_session = claim_promotion_session[0]
                # Final parking fee would be: claim_promotion_session 's amount_e + parking_fee calculated from claim_promotion's creation time + 15 minutes till now
                duration = (to_time - claim_promotion_session.server_time).total_seconds()
                time_in = get_time(parking_session.check_in_time)
                time_out = get_time(get_now_utc())
                #old#if duration <= 1800 or (not is_overnight(parking_session.check_in_time, get_now_utc()) and time_in >= time(17, 0, 0, 0)):
                #2018-02-27
                if duration <= 1800:
                    customer_info = get_parking_fee_info(card_id, parking_session.vehicle_type,
                                                         card.card_type,
                                                         parking_session.check_in_time, to_time)
                    customer_info["parking_fee"] = 0
                else:
                    customer_info = get_parking_fee_info(card_id, parking_session.vehicle_type,
                                                         card.card_type,
                                                         claim_promotion_session.server_time, to_time)
                rs['claim_promotion_id'] = claim_promotion_session.id
                rs['claim_promotion_create_time'] = claim_promotion_session.server_time
                rs['claim_promotion_hold_time'] = claim_promotion_session.server_time + datetime.timedelta(minutes=30)
            else:
                customer_info = get_parking_fee_info(card_id, parking_session.vehicle_type,
                                                     card.card_type,
                                                     parking_session.check_in_time, to_time)
            rs['customer_info'] = customer_info
            rs['parking_fee'] = customer_info["parking_fee"]
            rs['parking_fee_details'] = customer_info["parking_fee_detail"]
            rs['check_in_time_server'] = parking_session.check_in_time
            rs['check_out_time_server'] = to_time
            return Response(rs, status=status.HTTP_200_OK)
        ##
        ## Before 2018May10
        else:
            customer_info = get_parking_fee_or_customer_info(card_id, parking_session.id, parking_session.vehicle_type,
                                                             parking_session.check_in_time, to_time)

            parking_fee_result = 0, ''

            # Check if claim promotion exist
            claim_promotion_session = ClaimPromotionV2.objects.filter(parking_session_id=parking_session.id, used=False)
            # claim_promotion_session =[];
            if claim_promotion_session and claim_promotion_session.count > 0:  # If claim promotion exist
                claim_promotion_session = claim_promotion_session[0]
                # Final parking fee would be: claim_promotion_session 's amount_e + parking_fee calculated from claim_promotion's creation time + 15 minutes till now
                duration = (get_now_utc() - claim_promotion_session.server_time).total_seconds()
                time_in = get_time(parking_session.check_in_time)
                time_out = get_time(get_now_utc())
                #old#if duration <= 1800 or (not is_overnight(parking_session.check_in_time, get_now_utc()) and time_in >= time(17, 0, 0, 0)):
                #2018-02-27
                if duration <= 1800:
                    customer_info["parking_fee"] = 0
                else:
                    ## 2017-12-14--vehicle_registration_info; cancel_date; expired_date
                    cit = claim_promotion_session.server_time
                    time_out = get_now_utc()
                    cc = None
                    if len(customer_info) > 2:
                        cif = customer_info["vehicle_registration_info"]
                        ccd = cif['cancel_date']
                        efd = cif['expired_date']
                        cit = get_activecheckin(claim_promotion_session.server_time, ccd, efd)
                        cc = canchanges(time_out, ccd, efd)
                    parking_fee_result = calculate_parking_fee(parking_session.id,card_id,
                                                               parking_session.vehicle_type,
                                                               cit, time_out)
                    ##
                    if customer_info["parking_fee"]>0 or cc:
                        customer_info["parking_fee"] = parking_fee_result[0]

                # Return more claim_promotion_session metadata

                s = [{"type": "BILL", "company_info": "Cong ty A2", "bill_number": "ASDC0012", "bill_amount": 213000,
                      "notes": "Day la Bill 1"},
                     {"type": "BILL", "company_info": "Cong ty B2", "bill_number": "ASDC0023", "bill_amount": 178000,
                      "notes": "Day la Bill 2"},
                     {"type": "COUPON", "company_info": "Cong ty NIKE2", "coupon_code": "COUP2320", "coupon_amount": 8000,
                      "notes": "Ma giam gia mua san pham NIKE"}]

                rs['claim_promotion_id'] = claim_promotion_session.id
                rs['claim_promotion_create_time'] = claim_promotion_session.server_time
                rs['claim_promotion_hold_time'] = claim_promotion_session.server_time + datetime.timedelta(minutes=15)
            else:
                ## 2017-12-14--vehicle_registration_info; cancel_date; expired_date
                cit = parking_session.check_in_time
                time_out = get_now_utc()
                cc = None
                if len(customer_info) > 2:
                    cif = customer_info["vehicle_registration_info"]
                    ccd = cif['cancel_date']
                    efd = cif['expired_date']
                    cit = get_activecheckin(parking_session.check_in_time, ccd, efd)
                    cc = canchanges(time_out, ccd, efd)
                parking_fee_result = calculate_parking_fee(parking_session.id,card_id, parking_session.vehicle_type,
                                                           cit, time_out)
                if customer_info["parking_fee"] > 0 or cc:
                    customer_info["parking_fee"] = parking_fee_result[0]

            rs['customer_info'] = customer_info
            rs['parking_fee'] = parking_fee_result[0]
            rs['parking_fee_details'] = parking_fee_result[1]
            rs['check_in_time_server'] = parking_session.check_in_time
            rs['check_out_time_server'] = to_time

            return Response(rs, status=status.HTTP_200_OK)
        ##Before 2018May10
    @staticmethod
    def try_update_vehicle_type(request_obj, parking_session):
        name = 'vehicle_type'
        if has_param(name, request_obj):
            vehicle_type_id = parking_session.card.vehicle_type
            use_vehicle_type_from_card = request_obj['use_vehicle_type_from_card']
            if not use_vehicle_type_from_card:
                vehicle_type_id = int(request_obj['vehicle_type']) 

            vehicle_type = get_storaged_vehicle_type(vehicle_type_id)
           
	    if vehicle_type != parking_session.vehicle_type:
                parking_session.vehicle_type = vehicle_type

                parking_fee_session = ParkingFeeSession.objects.filter(parking_session_id=parking_session.id, session_type='IN')
                if parking_fee_session.count() > 0:
                    # print "@@@@@@@CAP NHAT LOAI XE NE"
                    parking_fee_session = parking_fee_session[0]
                    parking_fee_session.vehicle_type_id = vehicle_type_id
                    parking_fee_session.save()
                return True
        return False

    @staticmethod
    def try_update_vehicle_number(request_obj, parking_session):
        name = 'vehicle_number'
        if has_param(name, request_obj) and request_obj[name] != parking_session.vehicle_number:
            parking_session.vehicle_number = request_obj[name]
            return True
        return False

    @staticmethod
    def try_update_alpr_vehicle_number(request_obj, parking_session):
        name = 'alpr_vehicle_number'
        if has_param(name, request_obj) and request_obj[name] != parking_session.check_in_alpr_vehicle_number:
            parking_session.check_in_alpr_vehicle_number = request_obj[name]
            return True
        return False

    @staticmethod
    def try_update_images(request_obj, parking_session):
        if has_param('front_thumb', request_obj):
            images = parking_session.check_in_images
            save_image(request_obj['front_thumb'], images['front'])
            save_image(request_obj['back_thumb'], images['back'])
        if has_param('extra1_thumb', request_obj):
            images = parking_session.check_in_images
            save_image(request_obj['extra1_thumb'], images['extra1'])
            save_image(request_obj['extra2_thumb'], images['extra2'])
        return False
    @staticmethod
    def get_image_hosts(parking_session):
        image_hosts = list()
        timeout_point = get_now_utc() - datetime.timedelta(minutes=5)
        for terminal in Terminal.objects.filter(status=1, last_check_health__gte=timeout_point,
                                                checkinimage__parking_session=parking_session):
            image_hosts.append({'id': terminal.id, 'ip': terminal.ip})
        return image_hosts
##2018Dec14
class CardCheckInView(generics.CreateAPIView, generics.RetrieveUpdateAPIView):
    """
    Check in
    """
    serializer_class = CardCheckInSerializer

    ##Before 2018Jun21
    def create(self, request, *args, **kwargs):
        card_id = kwargs['card_id']
        #2024Jan10
        card_id, alpr_vehicle_number, vehicle_number = raisCardId(card_id)
        # if get_cached_checkin_info(card_id):
        #     return Response({'detail': 'Card is in use'}, status=status.HTTP_400_BAD_REQUEST)
        card = get_card(card_id)
        if not card:
            return Response({'detail': 'Card not found'}, status=status.HTTP_404_NOT_FOUND)
        can_check_in_out_when_in_effective_range = is_vehicle_registration_available(card_id)
        if not can_check_in_out_when_in_effective_range[0]:
            return_obj = dict()
            return_obj['detail'] = 'Card is not in effective range'
            return_obj['customer_info'] = can_check_in_out_when_in_effective_range[1]
            return Response(return_obj, status=status.HTTP_406_NOT_ACCEPTABLE)
        if card.status == 1:
            pkss = ParkingSession.objects.filter(check_out_time=None, card=card).order_by('-check_in_time')
            if pkss and len(pkss)>0:
                l=len(pkss)
                for pk in pkss:
                    if pk != pkss[0]:
                        pf = ParkingFeeSession.objects.filter(parking_session_id=pk.id)
                        for f in pf:
                            f.delete()
                        pk.delete()
                pcm=ParkingSession.objects.filter(card=card).order_by('-check_out_time')
                if pcm and len(pcm)>0 and pcm[0].check_out_time>=pkss[0].check_in_time:
                    pf = ParkingFeeSession.objects.filter(parking_session_id=pkss[0].id)
                    for f in pf:
                        f.delete()
                    pkss[0].delete()
                else:
                    return Response({'detail': 'Card is in use'}, status=status.HTTP_400_BAD_REQUEST)
            serializer = self.get_serializer(data=request.DATA, files=request.FILES)
            if serializer.is_valid():
                obj = serializer.object
                images = create_save_paths(card.card_label, True)
                save_image(obj['front_thumb'], images['front'])
                save_image(obj['back_thumb'], images['back'])
                if 'extra1_thumb' in obj:
                    save_image(obj['extra1_thumb'], images['extra1'])
                if 'extra2_thumb' in obj:
                    save_image(obj['extra2_thumb'], images['extra2'])
                vtype=obj['vehicle_type']
		use_vehicle_type_from_card = obj['use_vehicle_type_from_card']
                vehicle_type = get_storaged_vehicle_type(int(obj['vehicle_type']))
                if use_vehicle_type_from_card:
                    vehicle_type = get_storaged_vehicle_type(card.vehicle_type)
               
                #  Check number of entry of user
                start_day_time = get_now_utc().replace(hour=0, minute=0, second=0)
                entryCount = 0
                entries = []
                if obj['entry_check']:
                    entries = ParkingSession.objects.exclude(check_in_alpr_vehicle_number='????') \
                        .filter(check_in_alpr_vehicle_number__contains=obj['prefix_vehicle_number'],
                                vehicle_number=obj['vehicle_number'],
                                check_in_time__gte=start_day_time,
                                check_out_time__isnull=False).values('check_in_time', 'check_out_time')
                    entryCount = entries.count()
                cursor = connections['default'].cursor()
                cursor.execute("begin")
                try:
                    ci_time=get_now_utc()
                    img=json.dumps(images, ensure_ascii=False).encode('utf8')
                    qr = "insert into parking_parkingsession(`card_id`,`vehicle_type`,`vehicle_number`,`check_in_alpr_vehicle_number`,`check_in_operator_id`,`check_in_time`,`check_in_lane_id`,`check_in_images`) values('%s','%s','%s','%s','%s','%s','%s','%s');" % (
                        card.id, vehicle_type, obj['vehicle_number'], obj['alpr_vehicle_number'], obj['operator_id'],
                        ci_time.strftime("%Y-%m-%d %H:%M:%S"), obj['lane_id'], img)
                    cursor.execute(qr)
                    id = cursor.lastrowid
                    qr = "insert into parking_checkinimage(`terminal_id`,`parking_session_id`) values('%s','%s');" % (
                    obj['terminal_id'], id)
                    cursor.execute(qr)

                    parking_session=ParkingSession.objects.filter(id=id)
                    if parking_session:
                        parking_session=parking_session[0]
                    else:
                        parking_session=None
                    pk_slot=getparkingslots(vtype)
                    res_obj = dict()
                    res_obj['parking_session_id'] = int(id)
                    res_obj['card_label'] = card.card_label
                    res_obj['card_type'] = card.card_type
                    res_obj['front_image_path'] = images['front']
                    res_obj['back_image_path'] = images['back']
                    res_obj['check_in_time'] = datetime2timestamp(ci_time)
                    res_obj['vehicle_number_exist'] = lp_add(obj['vehicle_number'])
                    res_obj['limit_num_slots'] =pk_slot[0]# get_limit_slots(card.vehicle_type)
                    res_obj['current_num_slots'] =pk_slot[1]# inc_vehicle_count()
                    res_obj['entryCount'] = entryCount
                    res_obj['entries'] = list(entries)

                    # Create card checkin
                    result = get_parking_fee_info(card_id, vehicle_type,
                                                     card.card_type,
                                                  ci_time, get_now_utc())
                    ## before 208May10
                    #result=get_parking_fee_or_customer_info(card_id, parking_session.id, parking_session.vehicle_type,
                    #                                             parking_session.check_in_time, get_now_utc())
                    res_obj['customer_info'] = result

                    is_vehicle_registration = True if 'vehicle_registration_info' in result else False

                    parking_fee_session = ParkingFeeSession(parking_session_id=int(id),
                                                            card_id=str(card_id),
                                                            vehicle_number=obj['vehicle_number'] or '',
                                                            parking_fee=result['parking_fee'],
                                                            parking_fee_detail=result['parking_fee_detail'],
                                                            session_type='IN',
                                                            is_vehicle_registration=is_vehicle_registration,
                                                            vehicle_type_id=VEHICLE_TYPE_DICT[vehicle_type])
                    parking_fee_session.save()

                    # Check begin staff attendance
                    staffs = UserProfile.objects.filter(card=card)
                    if staffs.count() > 0:
                        staff = staffs[0]
                        Attendance.objects.create(user=staff.user, time_in=get_now(), parking_session=parking_session)
                    cursor.execute("commit")
                except Exception as e:
                    cursor.execute("rollback")
                    return Response({'detail': 'Save Fail'}, status=status.HTTP_400_BAD_REQUEST)
                return Response(res_obj, status=status.HTTP_201_CREATED)
            else:
                return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)
        elif card.status == 2:
            return Response({'detail': 'Card is locked'}, status=status.HTTP_400_BAD_REQUEST)
        else:
            return Response({'detail': 'Card is not enabled'}, status=status.HTTP_400_BAD_REQUEST)
    ##Before 2018Jun21
    def update(self, request, *args, **kwargs):
        card_id = kwargs['card_id']
        #2024Jan10
        card_id, alpr_vehicle_number, vehicle_number = raisCardId(card_id)
        serializer = self.get_serializer(data=request.DATA, files=request.FILES)
        if serializer.is_valid():
            card = get_card(card_id)
            if not card:
                return Response({'detail': 'Card not found'}, status=status.HTTP_404_NOT_FOUND)
            ##
            if card.status == 2:
                return Response({'detail': 'Card is locked'}, status=status.HTTP_400_BAD_REQUEST)
            elif card.status != 1 :
                return Response({'detail': 'Card is not enabled'}, status=status.HTTP_400_BAD_REQUEST)
            pkss = ParkingSession.objects.filter(check_out_time=None, card=card)
            if not pkss or len(pkss) <= 0:
                return Response({'detail': 'Card is not in use'}, status=status.HTTP_400_BAD_REQUEST)
            parking_session = pkss[0]
            card = parking_session.card
            terminal_id = parking_session.check_in_lane.terminal_id
            obj = serializer.object
            lp_changed = (has_param('vehicle_number', obj) and obj['vehicle_number'] != parking_session.vehicle_number)
            if lp_changed:
                lp_remove(parking_session.vehicle_number)
            should_save = False
            should_save = self.try_update_vehicle_number(obj, parking_session) or should_save
            should_save = self.try_update_alpr_vehicle_number(obj, parking_session) or should_save
            should_save = self.try_update_vehicle_type(obj, parking_session, card.vehicle_type) or should_save
            should_save = self.try_update_images(obj, parking_session) or should_save
            if should_save:
                parking_session.save()
                # set_cached_checkin_info(card_id, card_status, parking_session, card, terminal_id)
            images = parking_session.check_in_images

            #  Check number of entry of user
            start_day_time = get_now_utc().replace(hour=0, minute=0, second=0)
            entryCount = 0
            entries = []

            if obj['entry_check']:
                entries = ParkingSession.objects.exclude(check_in_alpr_vehicle_number='????') \
                    .filter(check_in_alpr_vehicle_number__contains=obj['prefix_vehicle_number'],
                            vehicle_number=obj['vehicle_number'],
                            check_in_time__gte=start_day_time,
                            check_out_time__isnull=False).values('check_in_time', 'check_out_time')
                entryCount = entries.count()
            vtype = obj['vehicle_type']
            ps_slots=getparkingslots(vtype)
            res_obj = dict()
            res_obj['parking_session_id'] =parking_session.id
            res_obj['card_label'] = card.card_label
            res_obj['card_type'] = card.card_type
            res_obj['front_image_path'] = images['front']
            res_obj['back_image_path'] = images['back']
            res_obj['check_in_time'] = datetime2timestamp(parking_session.check_in_time)
            if lp_changed:
                res_obj['vehicle_number_exist'] = lp_add(parking_session.vehicle_number)
            else:
                res_obj['vehicle_number_exist'] = lp_exists(parking_session.vehicle_number)
            res_obj['limit_num_slots'] =ps_slots[0]# get_limit_slots(card.vehicle_type)
            res_obj['current_num_slots'] =ps_slots[1]# get_vehicle_count()
            res_obj['entryCount'] = entryCount
            res_obj['entries'] = list(entries)
            res_obj['customer_info'] = get_parking_fee_info(card_id, parking_session.vehicle_type,
                                                 card.card_type,
                                                 parking_session.check_in_time, get_now_utc())
            ## before 208May10
            # res_obj['customer_info']=get_parking_fee_or_customer_info(card_id, parking_session.id, parking_session.vehicle_type,
            #                                             parking_session.check_in_time, get_now_utc())
            return Response(res_obj, status=status.HTTP_200_OK)

    def retrieve(self, request, *args, **kwargs):
        card_id = kwargs['card_id']
        #2024Jan10
        card_id, alpr_vehicle_number, vehicle_number = raisCardId(card_id)
        card = get_card(card_id)
        if not card:
            return Response({'detail': 'Card not found'}, status=status.HTTP_404_NOT_FOUND)
        pkss = ParkingSession.objects.filter(check_out_time=None, card=card)
        if not pkss or len(pkss)<=0:
            return Response({'detail': 'Card is not in use'}, status=status.HTTP_400_BAD_REQUEST)
        parking_session=pkss[0]
        terminal_id = parking_session.check_in_lane.terminal_id
        serializer = BasicCheckInSessionSerializer(parking_session, many=False)
        rs = serializer.data
        rs['image_hosts'] = self.get_image_hosts(parking_session)
        rs['card_label'] = card.card_label
        rs['card_type'] = card.card_type
        rs['terminal_id'] = terminal_id
        rs['parking_session_id'] = parking_session.id

        to_time = get_now_utc()
        ut = Utilities()
        qr = ut.QuerySecond('isactivetoolfee')
        if qr and len(qr) == 1 and qr[0][0] == 'new':
            ##From 2018May10
            # Check if claim promotion exist
            claim_promotion_session = ClaimPromotionV2.objects.filter(parking_session_id=parking_session.id, used=False)
            if claim_promotion_session and claim_promotion_session.count > 0:  # If claim promotion exist
                claim_promotion_session = claim_promotion_session[0]
                # Final parking fee would be: claim_promotion_session 's amount_e + parking_fee calculated from claim_promotion's creation time + 15 minutes till now
                duration = (to_time - claim_promotion_session.server_time).total_seconds()
                time_in = get_time(parking_session.check_in_time)
                time_out = get_time(get_now_utc())
                #old#if duration <= 1800 or (not is_overnight(parking_session.check_in_time, get_now_utc()) and time_in >= time(17, 0, 0, 0)):
                #2018-02-27
                if duration <= 1800:
                    customer_info = get_parking_fee_info(card_id, parking_session.vehicle_type,
                                                         card.card_type,
                                                         parking_session.check_in_time, to_time)
                    customer_info["parking_fee"] = 0
                else:
                    customer_info = get_parking_fee_info(card_id, parking_session.vehicle_type,
                                                         card.card_type,
                                                         claim_promotion_session.server_time, to_time)
                rs['claim_promotion_id'] = claim_promotion_session.id
                rs['claim_promotion_create_time'] = claim_promotion_session.server_time
                rs['claim_promotion_hold_time'] = claim_promotion_session.server_time + datetime.timedelta(minutes=30)
            else:
                customer_info = get_parking_fee_info(card_id, parking_session.vehicle_type,
                                                     card.card_type,
                                                     parking_session.check_in_time, to_time)
            rs['customer_info'] = customer_info
            rs['parking_fee'] = customer_info["parking_fee"]
            rs['parking_fee_details'] = customer_info["parking_fee_detail"]
            rs['check_in_time_server'] = parking_session.check_in_time
            rs['check_out_time_server'] = to_time
            return Response(rs, status=status.HTTP_200_OK)
        ##
        ## Before 2018May10
        else:
            customer_info = get_parking_fee_or_customer_info(card_id, parking_session.id, parking_session.vehicle_type,
                                                             parking_session.check_in_time, to_time)

            parking_fee_result = 0, ''

            # Check if claim promotion exist
            claim_promotion_session = ClaimPromotionV2.objects.filter(parking_session_id=parking_session.id, used=False)
            # claim_promotion_session =[];
            if claim_promotion_session and claim_promotion_session.count > 0:  # If claim promotion exist
                claim_promotion_session = claim_promotion_session[0]
                # Final parking fee would be: claim_promotion_session 's amount_e + parking_fee calculated from claim_promotion's creation time + 15 minutes till now
                duration = (get_now_utc() - claim_promotion_session.server_time).total_seconds()
                time_in = get_time(parking_session.check_in_time)
                time_out = get_time(get_now_utc())
                #old#if duration <= 1800 or (not is_overnight(parking_session.check_in_time, get_now_utc()) and time_in >= time(17, 0, 0, 0)):
                #2018-02-27
                if duration <= 1800:
                    customer_info["parking_fee"] = 0
                else:
                    ## 2017-12-14--vehicle_registration_info; cancel_date; expired_date
                    cit = claim_promotion_session.server_time
                    time_out = get_now_utc()
                    cc = None
                    if len(customer_info) > 2:
                        cif = customer_info["vehicle_registration_info"]
                        ccd = cif['cancel_date']
                        efd = cif['expired_date']
                        cit = get_activecheckin(claim_promotion_session.server_time, ccd, efd)
                        cc = canchanges(time_out, ccd, efd)
                    parking_fee_result = calculate_parking_fee(parking_session.id,card_id,
                                                               parking_session.vehicle_type,
                                                               cit, time_out)
                    ##
                    if customer_info["parking_fee"]>0 or cc:
                        customer_info["parking_fee"] = parking_fee_result[0]

                # Return more claim_promotion_session metadata

                s = [{"type": "BILL", "company_info": "Cong ty A2", "bill_number": "ASDC0012", "bill_amount": 213000,
                      "notes": "Day la Bill 1"},
                     {"type": "BILL", "company_info": "Cong ty B2", "bill_number": "ASDC0023", "bill_amount": 178000,
                      "notes": "Day la Bill 2"},
                     {"type": "COUPON", "company_info": "Cong ty NIKE2", "coupon_code": "COUP2320", "coupon_amount": 8000,
                      "notes": "Ma giam gia mua san pham NIKE"}]

                rs['claim_promotion_id'] = claim_promotion_session.id
                rs['claim_promotion_create_time'] = claim_promotion_session.server_time
                rs['claim_promotion_hold_time'] = claim_promotion_session.server_time + datetime.timedelta(minutes=15)
            else:
                ## 2017-12-14--vehicle_registration_info; cancel_date; expired_date
                cit = parking_session.check_in_time
                time_out = get_now_utc()
                cc = None
                if len(customer_info) > 2:
                    cif = customer_info["vehicle_registration_info"]
                    ccd = cif['cancel_date']
                    efd = cif['expired_date']
                    cit = get_activecheckin(parking_session.check_in_time, ccd, efd)
                    cc = canchanges(time_out, ccd, efd)
                parking_fee_result = calculate_parking_fee(parking_session.id,card_id, parking_session.vehicle_type,
                                                           cit, time_out)
                if customer_info["parking_fee"] > 0 or cc:
                    customer_info["parking_fee"] = parking_fee_result[0]

            rs['customer_info'] = customer_info
            rs['parking_fee'] = parking_fee_result[0]
            rs['parking_fee_details'] = parking_fee_result[1]
            rs['check_in_time_server'] = parking_session.check_in_time
            rs['check_out_time_server'] = to_time

            return Response(rs, status=status.HTTP_200_OK)
        ##Before 2018May10
    @staticmethod
    def try_update_vehicle_type(request_obj, parking_session, vehicle_type_from_card):
        name = 'vehicle_type'
        if has_param(name, request_obj):
   	    vehicle_type_id = parking_session.card.vehicle_type
            use_vehicle_type_from_card = request_obj['use_vehicle_type_from_card']
            if not use_vehicle_type_from_card:
                vehicle_type_id = int(request_obj['vehicle_type'])
    
	    vehicle_type = get_storaged_vehicle_type(vehicle_type_id)

            if vehicle_type != parking_session.vehicle_type:
                parking_session.vehicle_type = vehicle_type

                parking_fee_session = ParkingFeeSession.objects.filter(parking_session_id=parking_session.id, session_type='IN')
                if parking_fee_session.count() > 0:
                    # print "@@@@@@@CAP NHAT LOAI XE NE"
                    parking_fee_session = parking_fee_session[0]
                    parking_fee_session.vehicle_type_id = int(request_obj[name])
                    parking_fee_session.save()
                return True
        return False

    @staticmethod
    def try_update_vehicle_number(request_obj, parking_session):
        name = 'vehicle_number'
        if has_param(name, request_obj) and request_obj[name] != parking_session.vehicle_number:
            parking_session.vehicle_number = request_obj[name]
            return True
        return False

    @staticmethod
    def try_update_alpr_vehicle_number(request_obj, parking_session):
        name = 'alpr_vehicle_number'
        if has_param(name, request_obj) and request_obj[name] != parking_session.check_in_alpr_vehicle_number:
            parking_session.check_in_alpr_vehicle_number = request_obj[name]
            return True
        return False

    @staticmethod
    def try_update_images(request_obj, parking_session):
        if has_param('front_thumb', request_obj):
            images = parking_session.check_in_images
            save_image(request_obj['front_thumb'], images['front'])
            save_image(request_obj['back_thumb'], images['back'])
        if has_param('extra1_thumb', request_obj):
            images = parking_session.check_in_images
            save_image(request_obj['extra1_thumb'], images['extra1'])
            save_image(request_obj['extra2_thumb'], images['extra2'])
        return False
    @staticmethod
    def get_image_hosts(parking_session):
        image_hosts = list()
        timeout_point = get_now_utc() - datetime.timedelta(minutes=5)
        for terminal in Terminal.objects.filter(status=1, last_check_health__gte=timeout_point,
                                                checkinimage__parking_session=parking_session):
            image_hosts.append({'id': terminal.id, 'ip': terminal.ip})
        return image_hosts
class CardCheckOutView(generics.CreateAPIView,generics.UpdateAPIView):
    """
    Check out
    """
    serializer_class = CardCheckOutSerializer

    def create(self, request, *args, **kwargs):
        card_id = kwargs['card_id']
        #2024Jan10
        card_id, alpr_vehicle_number, vehicle_number = raisCardId(card_id)
        card = get_card(card_id)
        if not card:
            return Response({'detail': 'Card not found'}, status=status.HTTP_404_NOT_FOUND)
        elif card.status==0:
            return Response({'detail': 'Card is not enabled'}, status=status.HTTP_404_NOT_FOUND)
        elif card.status == 2:
            return Response({'detail': 'This card is locked'}, status=status.HTTP_400_BAD_REQUEST)
        pkss = ParkingSession.objects.filter(check_out_time=None, card=card).order_by('-check_in_time')
        if pkss and len(pkss) > 0:
            l = len(pkss)
            for pk in pkss:
                if pk != pkss[0]:
                    pf=ParkingFeeSession.objects.filter(parking_session_id=pk.id)
                    for f in pf:
                        f.delete()
                    pk.delete()
        if not pkss or len(pkss) <= 0:
            return Response({'detail': 'This card is not in use'}, status=status.HTTP_400_BAD_REQUEST)
        serializer = self.get_serializer(data=request.DATA, files=request.FILES)
        if serializer.is_valid():
            obj = serializer.object
            images = create_save_paths(card.card_label, False)
            save_image(obj['front_thumb'], images['front'])
            save_image(obj['back_thumb'], images['back'])
            if 'extra1_thumb' in obj:
                save_image(obj['extra1_thumb'], images['extra1'])
            if 'extra2_thumb' in obj:
                save_image(obj['extra2_thumb'], images['extra2'])
            parking_session=pkss[0]
            parking_session.check_out_alpr_vehicle_number = obj['alpr_vehicle_number'] if alpr_vehicle_number is None else alpr_vehicle_number
            parking_session.check_out_operator_id = obj['operator_id']
            # parking_session.check_out_time = get_now_utc()
            parking_session.check_out_time = obj['check_out_time']
            parking_session.check_out_images = images
            parking_session.check_out_lane_id = obj['lane_id']
            parking_session.duration = (parking_session.check_out_time - parking_session.check_in_time).total_seconds()
            parking_session.save()

            CheckInImage.objects.filter(parking_session=parking_session).delete()
            del_cached_checkin_info(card_id)
            lp_remove(parking_session.vehicle_number)
            dec_vehicle_count()

            parking_fee = obj['parking_fee'] if 'parking_fee' in obj else 0
            parking_fee_details = obj['parking_fee_details'] if 'parking_fee_details' in obj else ''

            parking_session.save()

            parking_fee_session = ParkingFeeSession(parking_session_id=int(parking_session.id), card_id=str(card_id),
                                                    vehicle_number=parking_session.vehicle_number or '',
                                                    parking_fee=parking_fee, parking_fee_detail=parking_fee_details,
                                                    session_type='OUT',
                                                    vehicle_type_id=VEHICLE_TYPE_DICT[parking_session.vehicle_type])
            parking_fee_session.save()

            # If claim promotion is applied, set it to be "used"
            claim_promotion = ClaimPromotionV2.objects.filter(parking_session_id=parking_session.id, used=False)
            if claim_promotion and len(claim_promotion) > 0:  # If claim promotion exist
                claim_promotion = claim_promotion[0]
                claim_promotion.used = True
                claim_promotion.parking_fee_session_id = parking_fee_session.id
                claim_promotion.save()
            vehicle_registration = VehicleRegistration.objects.filter(card__card_id=card_id)
            is_vehicle_registration = True if vehicle_registration else False
            if is_vehicle_registration:
                parking_fee_session.is_vehicle_registration = is_vehicle_registration
                parking_fee_session.save()

            if image_replication:
                check_in_images = parking_session.check_in_images
                image_replication.replicate_check_out(images['front'], images['back'])
                image_replication.delete(check_in_images['front'], check_in_images['back'])
            # Check end staff attendance
            try:
                attendance = Attendance.objects.get(parking_session=parking_session)
                attendance.time_out = get_now()
                attendance.total_time_of_date = (attendance.time_out.replace(tzinfo=utc) - attendance.time_in.replace(
                    tzinfo=utc)).total_seconds() / 3600
                attendance.save()
            except Attendance.DoesNotExist:
                pass
            res_obj = dict()
            res_obj['front_image_path'] = images['front']
            res_obj['back_image_path'] = images['back']
            res_obj['parking_fee'] = parking_fee_session.parking_fee
            res_obj['check_in_time'] = parking_session.check_in_time
            res_obj['check_out_time'] = parking_session.check_out_time

            return Response(res_obj, status=status.HTTP_201_CREATED)
        else:
            return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)

    def update(self, request, *args, **kwargs):
        card_id = kwargs['card_id']
        #2024Jan10
        card_id, alpr_vehicle_number, vehicle_number = raisCardId(card_id)
        card = get_card(card_id)
        if card:
            pkss = ParkingSession.objects.filter(check_out_time=None, card=card)
            if pkss and len(pkss) > 0:
                for pk in pkss:
                    pf = ParkingFeeSession.objects.filter(parking_session_id=pk.id)
                    for f in pf:
                        f.delete()
                    pk.delete()
        return Response("{'detail': 'This Check_in has canceled.'}", status=status.HTTP_200_OK)
class CardExceptionCheckOutView(generics.CreateAPIView):
    """
    Exception check out
    """
    serializer_class = CardExceptionCheckOutSerializer
    def create(self, request, *args, **kwargs):
        card_id = kwargs['card_id']
        card = get_card(card_id)
        if not card:
            return Response({'detail': 'Card not found'}, status=status.HTTP_404_NOT_FOUND)
        elif card.status == 0:
            return Response({'detail': 'Card is not enabled'}, status=status.HTTP_404_NOT_FOUND)
        elif card.status == 2:
            return Response({'detail': 'This card is locked'}, status=status.HTTP_400_BAD_REQUEST)
        pkss = ParkingSession.objects.filter(check_out_time=None, card=card).order_by('-check_in_time')
        if pkss and len(pkss) > 0:
            l = len(pkss)
            for pk in pkss:
                if pk != pkss[0]:
                    pf = ParkingFeeSession.objects.filter(parking_session_id=pk.id)
                    for f in pf:
                        f.delete()
                    pk.delete()
        if not pkss or len(pkss) <= 0:
            return Response({'detail': 'This card is not in use'}, status=status.HTTP_400_BAD_REQUEST)
        serializer = self.get_serializer(data=request.DATA)
        if serializer.is_valid():
            obj = serializer.object
            parking_session = pkss[0]
            parking_session.check_out_operator_id = obj['operator_id']
            parking_session.check_out_time = get_now_utc()
            parking_session.check_out_lane_id = obj['lane_id']
            parking_session.duration = (parking_session.check_out_time - parking_session.check_in_time).total_seconds()
            parking_session.check_out_exception = CheckOutExceptionInfo.objects.create(notes=obj['notes'],
                                                                                       parking_fee=obj['parkingfee'])
            parking_session.save()
            # If claim promotion is applied, set it to be "used"
            claim_promotion = ClaimPromotionV2.objects.filter(parking_session_id=parking_session.id, used=False)
            if claim_promotion and len(claim_promotion) > 0:  # If claim promotion exist
                claim_promotion = claim_promotion[0]
                claim_promotion.used = True
                #claim_promotion.parking_fee_session_id = parking_fee_session.id
                claim_promotion.save()
            CheckInImage.objects.filter(parking_session=parking_session).delete()

            if obj['lock_card'] != 0:
                card.status = 2
                card.save()
            del_cached_checkin_info(card_id)
            lp_remove(parking_session.vehicle_number)
            dec_vehicle_count()
            if image_replication:
                check_in_images = parking_session.check_in_images
                image_replication.delete(check_in_images['front'], check_in_images['back'])
            try:
                attendance = Attendance.objects.get(parking_session=parking_session)
                attendance.time_out = get_now()
                attendance.total_time_of_date = (attendance.time_out.replace(tzinfo=utc) - attendance.time_in.replace(
                    tzinfo=utc)).total_seconds() / 3600
                attendance.save()
            except Attendance.DoesNotExist:
                pass
            return Response(status=status.HTTP_201_CREATED)
        else:
            return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)
class CardCheckInImageView(generics.ListCreateAPIView):
    """
    Check in image host registration
    """
    serializer_class = CheckInImageHostSerializer

    def create(self, request, *args, **kwargs):
        card_id = kwargs['card_id']
        cached_data = get_cached_checkin_info(card_id)
        if cached_data:
            card_status = cached_data['card_status']
        else:
            card_status = get_card_status(card_id)
        if not card_status or card_status.status == 0:
            return Response({'detail': 'This card is not in use'}, status=status.HTTP_400_BAD_REQUEST)
        if 'id' not in request.DATA:
            return Response({'detail': 'Field id is required'}, status=status.HTTP_400_BAD_REQUEST)
        terminal_id = request.DATA['id']
        try:
            Terminal.objects.get(id=terminal_id)
            CheckInImage.objects.create(terminal_id=terminal_id, parking_session=card_status.parking_session)
            return Response({'detail': 'OK'}, status=status.HTTP_201_CREATED)
        except Terminal.DoesNotExist:
            return Response({'detail': 'Terminal does not exist'}, status=status.HTTP_400_BAD_REQUEST)

    def list(self, request, *args, **kwargs):
        card_id = kwargs['card_id']
        cached_data = get_cached_checkin_info(card_id)
        if cached_data:
            card_status = cached_data['card_status']
        else:
            card_status = get_card_status(card_id)
        if not card_status or card_status.status == 0:
            return Response({'detail': 'This card is not in use'}, status=status.HTTP_400_BAD_REQUEST)
        serializer = self.get_serializer(
            Terminal.objects.filter(checkinimage__parking_session=card_status.parking_session), many=True)
        return Response(serializer.data, status=status.HTTP_200_OK)
class TerminalListView(generics.ListCreateAPIView):
    """
    Terminal Collection
    """
    serializer_class = TerminalListSerializer
    queryset = Terminal.objects.all()

    def create(self, request, *args, **kwargs):
        serializer = self.get_serializer(data=request.DATA)
        if serializer.is_valid():
            obj = serializer.object
            try:
                rs = Terminal.objects.get(terminal_id=obj.terminal_id)
                rs.status = obj.status
                rs.name = obj.name
            except Terminal.DoesNotExist:
                rs = obj
            rs.ip = get_ip(self.request)
            rs.last_check_health = get_now_utc()
            rs.save()
            serializer = self.get_serializer(rs, many=False)
            return Response(serializer.data, status=status.HTTP_201_CREATED)
        return Response(serializer.errors, status.HTTP_400_BAD_REQUEST)
class TerminalDetailView(generics.RetrieveUpdateAPIView):
    """
    Parking Card Detail
    """
    lookup_field = 'id'
    serializer_class = TerminalDetailSerializer
    queryset = Terminal.objects.all()

    def pre_save(self, obj):
        if not obj.last_check_health:
            raise Http404
        obj.ip = get_ip(self.request)
        obj.last_check_health = get_now_utc()
        super(TerminalDetailView, self).pre_save(obj)
class TerminalHealthView(APIView):
    """
    Terminal check health
    """

    def put(self, request, *args, **kwargs):
        item_id = kwargs['id']
        try:
            rs = Terminal.objects.get(id=item_id)
            ip = get_ip(request)
            if rs.ip != ip:
                rs.ip = ip
            rs.last_check_health = get_now_utc()
            rs.save()
            serializer = TerminalDetailSerializer(rs, many=False)
            return Response(serializer.data, status=status.HTTP_200_OK)
        except Terminal.DoesNotExist:
            raise Http404
class TerminalLaneView(generics.ListAPIView):
    """
    Lanes of specified Terminal
    """
    serializer_class = LaneSerializer
    queryset = Lane.objects.all()

    def list(self, request, *args, **kwargs):
        item_id = int(kwargs['id'])
        serializer = self.get_serializer(self.get_queryset().filter(terminal_id=item_id), many=True)
        return Response(serializer.data, status=status.HTTP_200_OK)
class TerminalTimeOutView(APIView):
    """
    Force timeout a terminal
    """

    def put(self, request, *args, **kwargs):
        item_id = kwargs['id']
        try:
            rs = Terminal.objects.get(id=item_id)
            rs.last_check_health = get_now_utc() - datetime.timedelta(minutes=10)
            rs.save()
            serializer = TerminalDetailSerializer(rs, many=False)
            return Response(serializer.data, status=status.HTTP_200_OK)
        except Terminal.DoesNotExist:
            raise Http404
class LaneListView(generics.ListCreateAPIView):
    """
    Lane Collection
    """
    serializer_class = LaneSerializer
    queryset = Lane.objects.all()

    def create(self, request, *args, **kwargs):
        if 'bulk' in request.DATA:
            try:
                bulk_data = json.loads(request.DATA['bulk'])
            except ValueError:
                return Response({'detail': 'Bulk data error'}, status.HTTP_400_BAD_REQUEST)
            lst_rs = list()
            for data in bulk_data:
                serializer = self.get_serializer(data=data)
                if serializer.is_valid():
                    obj = serializer.object
                    try:
                        rs = Lane.objects.get(terminal_id=obj.terminal_id, name=obj.name)
                        rs.direction = obj.direction
                        rs.enabled = obj.enabled
                        rs.vehicle_type = obj.vehicle_type
                        rs.save()
                    except Lane.DoesNotExist:
                        rs = obj
                        rs.save()
                    lst_rs.append(rs)
                else:
                    return Response(serializer.errors, status.HTTP_400_BAD_REQUEST)
            serializer = self.get_serializer(lst_rs, many=True)
            return Response(serializer.data, status=status.HTTP_201_CREATED)
        else:
            serializer = self.get_serializer(data=request.DATA)
            if serializer.is_valid():
                obj = serializer.object
                try:
                    rs = Lane.objects.get(terminal_id=obj.terminal_id, name=obj.name)
                    rs.direction = obj.direction
                    rs.enabled = obj.enabled
                    rs.vehicle_type = obj.vehicle_type
                    rs.save()
                except Lane.DoesNotExist:
                    rs = obj
                    rs.save()
                serializer = self.get_serializer(rs, many=False)
                return Response(serializer.data, status=status.HTTP_201_CREATED)
            return Response(serializer.errors, status.HTTP_400_BAD_REQUEST)
class LaneDetailView(generics.RetrieveUpdateAPIView):
    """
    Lane Detail
    """
    lookup_field = 'id'
    serializer_class = LaneSerializer
    queryset = Lane.objects.all()
class CameraListView(generics.ListCreateAPIView):
    """
    Lane Collection
    """
    serializer_class = CameraSerializer
    queryset = Camera.objects.all()
class CameraDetailView(generics.RetrieveUpdateAPIView):
    """
    Lane Detail
    """
    lookup_field = 'id'
    serializer_class = CameraSerializer
    queryset = Camera.objects.all()
class UserLoginView(generics.CreateAPIView):
    """
    User Login
    """
    serializer_class = UserLoginSerializer

    def create(self, request, *args, **kwargs):
        serializer = self.get_serializer(data=request.DATA)
        if serializer.is_valid():
            obj = serializer.object
            try:
                user = User.objects.get(username=obj['username'], is_active=True)
                if not user.check_password(obj['password']):
                    return Response({'detail': 'Login fail'}, status=status.HTTP_400_BAD_REQUEST)
                obj['id'] = user.id
                obj['is_staff'] = user.is_staff
                obj['is_admin'] = user.is_superuser
                obj['display_name'] = user.userprofile.fullname or ''
                obj['staff_id'] = user.userprofile.staff_id or ''
                lane_id = obj.get('lane_id', 0)
                if lane_id != MAGIC_BYPASS_NUMBER:
                    shift = UserShift.objects.create(user=user, lane_id=obj['lane_id'], begin=get_now_utc())
                    obj['shift_id'] = shift.id
                rs = serializer.data
                del rs['password']
                return Response(rs, status=status.HTTP_200_OK)
            except User.DoesNotExist:
                return Response({'detail': 'Login fail'}, status=status.HTTP_400_BAD_REQUEST)
        else:
            return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)


class UserCardLoginView(generics.CreateAPIView):
    """
    User Login
    """
    serializer_class = UserCardLoginSerializer

    def create(self, request, *args, **kwargs):
        serializer = self.get_serializer(data=request.DATA)
        if serializer.is_valid():
            obj = serializer.object
            users = User.objects.filter(userprofile__card__card_id=obj['card_id'])
            if users.count() > 0:
                user = users[0]
                obj['id'] = user.id
                obj['is_staff'] = user.is_staff
                obj['is_admin'] = user.is_superuser
                obj['username'] = user.username
                obj['display_name'] = user.userprofile.fullname
                obj['staff_id'] = user.userprofile.staff_id

                lane_id = obj.get('lane_id', 0)
                if lane_id != MAGIC_BYPASS_NUMBER:
                    shift = UserShift.objects.create(user=user, lane_id=obj['lane_id'], begin=get_now_utc())
                    obj['shift_id'] = shift.id
                rs = serializer.data
                del rs['card_id']
                return Response(rs, status=status.HTTP_200_OK)
            return Response({'detail': 'Login fail'}, status=status.HTTP_400_BAD_REQUEST)
        else:
            return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)


class UserLogoutView(generics.CreateAPIView):
    """
    User Logout
    """
    serializer_class = UserLogoutSerializer

    def create(self, request, *args, **kwargs):
        serializer = self.get_serializer(data=request.DATA)
        if serializer.is_valid():
            obj = serializer.object
            try:
                shift = UserShift.objects.get(id=obj['shift_id'])
                if shift.user_id != obj['user_id'] or shift.lane_id != obj['lane_id']:
                    return Response({'detail': 'Logout fail. Shift info is fail to verify.'},
                                    status=status.HTTP_400_BAD_REQUEST)
                if not shift.end:
                    shift.end = get_now_utc()
                    info = dict()
                    info['check_in'] = ParkingSession.objects.filter(
                        check_in_operator=shift.user, check_in_lane=shift.lane,
                        check_in_time__gt=shift.begin, check_in_time__lt=shift.end).count()
                    info['check_out'] = ParkingSession.objects.filter(
                        check_out_operator=shift.user, check_out_lane=shift.lane,
                        check_out_time__gt=shift.begin, check_out_time__lt=shift.end).count()
                    shift.info = info
                shift.info['revenue'] = obj['revenue']
                shift.save()
                obj['begin_timestamp'] = datetime2timestamp(shift.begin)
                obj['end_timestamp'] = datetime2timestamp(shift.end)
                obj['num_check_in'] = shift.info['check_in']
                obj['num_check_out'] = shift.info['check_out']
                return Response(serializer.data, status=status.HTTP_200_OK)
            except UserShift.DoesNotExist:
                return Response({'detail': 'Logout fail. Unknown shift info.'}, status=status.HTTP_400_BAD_REQUEST)
        else:
            return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)


class ImageReplicationView(generics.ListCreateAPIView):
    """
    Images Replication
    """
    serializer_class = ImageReplicationSerializer

    def create(self, request, *args, **kwargs):
        serializer = self.get_serializer(data=request.DATA)
        if serializer.is_valid():
            obj = serializer.object
            ip = get_ip(request)
            if image_replication:
                image_replication.replicate_check_in(ip, obj['front_image'], obj['back_image'],obj['extra1_image'], obj['extra2_image'], obj['card_id'])
            return Response(image_replication.get_replicate_terminals(ip), status=status.HTTP_200_OK)
        else:
            return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)

    def list(self, request, *args, **kwargs):
        ip = get_ip(request)
        if image_replication:
            return Response(image_replication.get_replicate_terminals(ip), status=status.HTTP_200_OK)
        else:
            return Response([], status=status.HTTP_200_OK)


class CardTypeListView(generics.ListAPIView):
    """
    Card Types
    """
    serializer_class = CardTypeSerializer
    queryset = CardType.objects.all()


class VehicleTypeListView(generics.ListAPIView):
    """
    Vehicle Types
    """
    serializer_class = VehicleTypeSerializer
    queryset = VehicleType.objects.all()


from uuid import uuid4 as uuid_uuid4


##
# CLAIM PROMOTION
##

class ClaimPromotionSearchView(generics.ListAPIView):
    """
    Search parking session information
    mode -- 0: In parking (Default)<br/>1: Only out<br/>2: All
    card_id -- Card id
    card_label -- Card label
    vehicle_number -- Vehicle number
    from_time -- From time
    to_time -- To time
    vehicle_type -- Vehicle type
    terminal_group -- Terminal Group id to search
    page -- page index
    page_size -- number items per page (default = 20)
    """
    serializer_class = ClaimPromotionSearchSerializer_new
    # paginate_by = 20
    # paginate_by_param = 'page_size'
    # queryset = ClaimPromotionV2.objects.all()

    # def get_queryset(self):
    #     request = self.request
    #     card_id = get_param(request, 'card_id') if get_param(request, 'card_id') else 0
    #     vehicle_number = get_param(request, 'vehicle_number') if get_param(request, 'vehicle_number') else ''
    #     vehicle_type = int(get_param(request, 'vehicle_type')) if get_param(request, 'vehicle_type') and get_param(
    #         request, 'vehicle_type') != '100000000'  else 0
    #     if vehicle_type > 0:
    #         vehicle_type = vehicle_type / 10000
    #     from_timestamp = get_param(request, 'from_time')
    #     to_timestamp = get_param(request, 'to_time')
    #     from_time = None
    #     to_time = None
    #     if from_timestamp:
    #         from_time = timestamp2datetime(int(from_timestamp)).strftime("%Y-%m-%d %H:%M:%S")
    #     if to_timestamp:
    #         to_time = timestamp2datetime(int(to_timestamp)).strftime("%Y-%m-%d %H:%M:%S")
    #     page = int(get_param(request, 'page', 1))
    #     page_size = int(get_param(request, 'page_size', 20))
    #     util = Utilities()
    #     res = util.Query('get_redemptionsearch', from_time, to_time, card_id, vehicle_number,
    #                      vehicle_type, page, page_size)
    #     rs = []
    #     for it in res:
    #         dt = {}
    #         dt["id"] = int(it[1])
    #         dt["parking_session_id"] = it[3]
    #         dt["card_label"] = it[3]
    #         dt["user_id"] = it[4]
    #         dt["amount_a"] = int(it[5]) if it[5] else 0
    #         dt["amount_b"] = int(it[6]) if it[6] else 0
    #         dt["amount_c"] = int(it[7]) if it[7] else 0
    #         dt["amount_d"] = int(it[8]) if it[8] else 0
    #         dt["amount_e"] = int(it[9]) if it[9] else 0
    #         dt["client_time"] = it[10] if it[10] else None
    #         dt["server_time"] = it[11] if it[11] else None
    #         dt["used"] = True if it[12] and it[12]>0 else False
    #         dt["notes"] = it[13]
    #         dt["total"] = it[14]
    #         rs.append(dt);
    #     return rs
    #     # return search_claim_promotion(self.queryset, card_id, vehicle_number, vehicle_type,
    #     #                                   from_time, to_time)
    def list(self, request, *args, **kwargs):
        try:
            request = self.request
            card_id = get_param(request, 'card_id') if get_param(request, 'card_id') else 0
            vehicle_number = get_param(request, 'vehicle_number') if get_param(request, 'vehicle_number') else ''
            vehicle_type = int(get_param(request, 'vehicle_type')) if get_param(request, 'vehicle_type') and get_param(
                request, 'vehicle_type') != '100000000'  else 0
            if vehicle_type > 0:
                vehicle_type = vehicle_type / 10000
            from_timestamp = get_param(request, 'from_time')
            to_timestamp = get_param(request, 'to_time')
            from_time = None
            to_time = None
            if from_timestamp:
                from_time = timestamp2datetime(int(from_timestamp)).strftime("%Y-%m-%d %H:%M:%S")
            if to_timestamp:
                to_time = timestamp2datetime(int(to_timestamp)).strftime("%Y-%m-%d %H:%M:%S")
            page = int(get_param(request, 'page', 1))
            page_size = int(get_param(request, 'page_size', 20))
            util = Utilities()
            res = util.Query('get_redemptionsearch', from_time, to_time, card_id, vehicle_number,
                             vehicle_type, page, page_size)
            rs = []
            for it in res:
                dt = {}
                dt["id"] = int(it[1])
                dt["parking_session_id"] = it[3]
                dt["card_label"] = it[3]
                dt["user_id"] = it[4]
                dt["amount_a"] = int(it[5]) if it[5] else 0
                dt["amount_b"] = int(it[6]) if it[6] else 0
                dt["amount_c"] = int(it[7]) if it[7] else 0
                dt["amount_d"] = int(it[8]) if it[8] else 0
                dt["amount_e"] = int(it[9]) if it[9] else 0
                dt["client_time"] = it[10] if it[10] else None
                dt["server_time"] = it[11] if it[11] else None
                dt["used"] = True if it[12] and it[12] > 0 else False
                dt["notes"] = it[13]
                dt["total"] = it[14]
                rs.append(dt);
            datas = self.get_serializer(rs, many=True).data
            return Response(datas)
        except Exception as e:
            return Response({'detail': 'Invalid query parameter'}, status=status.HTTP_400_BAD_REQUEST)
class VehicleTypeListView(generics.ListAPIView):
    """
    Vehicle Types
    """
    serializer_class = VehicleTypeSerializer
    queryset = VehicleType.objects.all()

class ClaimPromotionCreateView(generics.CreateAPIView):
    """
    Claim Promotion

    * Requirements: card_id, parking_session_id, and user_id are invalid

    * Data sample:

    [
       {
           "type": "BILL",
           "company_info": "Cong ty A",
           "bill_number": "ASDC001",
           "bill_amount": 213000,
           "notes": "Day la Bill 1"
       },
       {
           "type": "BILL",
           "company_info": "Cong ty B",
           "bill_number": "ASDC002",
           "bill_amount": 178000,
           "notes": "Day la Bill 2"
       },
       {
           "type": "COUPON",
           "company_info": "Cong ty NIKE",
           "coupon_code": "COUP220",
           "coupon_amount": 1000,
           "notes": "Ma giam gia mua san pham NIKE"
       }
    ]

    * Return

      * 200 OK and status "Promotion is claimed"

      * 400 if has error
    """
    serializer_class = ClaimPromotionSerializer

    def create(self, request, *args, **kwargs):
        serializer = self.get_serializer(data=request.DATA)
        if serializer.is_valid():
            obj = serializer.object
            # print "obj", obj
            ##2018-04-03
            cursor = connections['default'].cursor()
            cursor.execute("begin")

            try:
                parking_session_id = obj.get('parking_session_id', 0)
                parking_session = None
                if parking_session_id != MAGIC_BYPASS_NUMBER:
                    existing_parking_sessions = ParkingSession.objects.filter(id=parking_session_id)
                    if existing_parking_sessions.count() <= 0:
                        return Response({
                            'status_code': 3,
                            "message": "Luot gui xe khong hop le",
                            'status_text': 'Luot gui xe khong hop le'},
                            status=status.HTTP_400_BAD_REQUEST)
                    parking_session = existing_parking_sessions[0]

                # Check user (staff) perform this task does exist
                existing_users = User.objects.filter(id=obj['user_id'])
                if existing_users.count() <= 0:
                    return Response({
                        'status_code': 3,
                        "message": "Nhan vien khong ton tai",
                        'status_text': 'Nhan vien khong ton tai'},
                        status=status.HTTP_400_BAD_REQUEST)

                # Check has existing unused ClaimPromotionSession
                existing_claim_promotions = ClaimPromotionV2.objects.filter(
                    parking_session_id=obj['parking_session_id'], used=False)
                if len(existing_claim_promotions) > 0:
                    return Response({
                        'status_code': 2,
                        "message": "Claim Promotion ton tai, va co the chua duoc su dung",
                        'status_text': 'Claim Promotion ton tai, va co the chua duoc su dung'
                    }, status=status.HTTP_400_BAD_REQUEST)

                # Create a ClaimPromotion instance
                #trace_code = str(uuid_uuid4())
                rt = "insert into `parking_claimpromotionv2`(`parking_session_id`, `user_id`,`amount_a`,`amount_b`,`amount_c`,`amount_d`,`amount_e`,`server_time`,`notes`,`used`) values('%s','%s','%s','%s','%s','%s','%s','%s','%s',0)" % (
                    obj['parking_session_id'] if 'parking_session_id' in obj else 0,
                    obj['user_id'] if 'user_id' in obj else 0,
                    obj['amount_a'] or 0,
                    obj['amount_b'] or 0,
                    obj['amount_c'] or 0,
                    obj['amount_d'] or 0,
                    obj['amount_e'] or 0,

                    datetime.datetime.utcnow().replace(microsecond=0, tzinfo=utc).strftime("%Y-%m-%d %H:%M:%S"),
                    obj['notes'] if 'notes' in obj else '')
                cursor.execute(rt)
                claim_id = cursor.lastrowid
                claim_promotion_data = json.loads(obj['data'])
                for row in claim_promotion_data:
                    # Check type
                    if row["type"] == "BILL":
                        row_bill_number = row["bill_number"] if "bill_number" in row else ""
                        row_company_info = row["company_info"] if "company_info" in row else ""
                        try:
                            row_date = dateutil.parser.parse(row["dateUtc"])
                        except:
                            row_date = get_now_utc()
                        row_date=row_date.strftime("%Y-%m-%d %H:%M:%S")
                        rtdt = "insert into `parking_claimpromotionbillv2`(`claim_promotion_id`,`company_info`,`date`,`bill_number`,`bill_amount`,`notes`) values('%s','%s','%s','%s','%s','%s');" % (
                            claim_id, row_company_info, row_date, row_bill_number,
                            row["bill_amount"] if "bill_amount" in row else 0, row["notes"] if "notes" in row else "")
                        cursor.execute(rtdt)
                    elif row["type"] == "COUPON":
                        rtdt="insert into parking_claimpromotioncouponv2(claim_promotion_id,company_info,coupon_code,coupon_amount,notes) values('%s','%s','%s','%s','%s')"%(
                            claim_id,
                            row["coupon_code"] if "coupon_code" in row else "",
                            row["company_info"] if "company_info" in row else "",
                            row["coupon_amount"] if "coupon_amount" in row else 0,
                            row["notes"] if "notes" in row else "")
                        cursor.execute(rtdt)
                cursor.execute("commit")
                cursor.close()
                return Response({
                    'claim_promotion_id': claim_id,
                    'status_code': 1,
                    'status_text': 'Promotion is claimed',
                    'message': 'Promotion is claimed',
                    'notes': 'Please check-out within 15 minutes. Otherwise, additional fee would be charged'
                })
            except Exception as e:
                cursor.execute("rollback")
                cursor1 = connections['default'].cursor()
                cursor1.execute("begin")
                try:
                    rt="insert into parking_claimpromotion_logerror(session_id,client_time,server_time,sendata,amount_a,amount_b,amount_c,amount_d,amount_e,user_id) values('%s','%s','%s','%s',%s,%s,%s,%s,%s,%s)"%(
                        obj['parking_session_id'] if 'parking_session_id' in obj else 0,
                        datetime.datetime.now().strftime("%Y-%m-%d %H:%M:%S"),
                        datetime.datetime.utcnow().replace(microsecond=0, tzinfo=utc).strftime("%Y-%m-%d %H:%M:%S"),
                        obj['data'] if 'data' in obj else "",
                        obj['amount_a'] or 0,
                        obj['amount_b'] or 0,
                        obj['amount_c'] or 0,
                        obj['amount_d'] or 0,
                        obj['amount_e'] or 0,
                        obj['user_id'] if 'user_id' in obj else 0,
                    )
                    cursor1.execute(rt)
                    cursor1.execute("commit")
                    cursor1.close()
                except:
                    cursor1.execute("rollback")
                return Response({
                    'status_code': 2,
                    'status_text': 'Co loi xay ra',
                    'message': 'Co loi xay ra',
                    'notes': e
                }, status=status.HTTP_500_INTERNAL_SERVER_ERROR)
            ##20180403
            ##old
            # try:
            #
            #     # Check parking session exist
            #     parking_session_id = obj.get('parking_session_id', 0)
            #     parking_session = None                if parking_session_id != MAGIC_BYPASS_NUMBER:
            #         existing_parking_sessions = ParkingSession.objects.filter(id=parking_session_id)
            #         if existing_parking_sessions.count() <= 0:
            #             return Response({
            #                 'status_code': 3,
            #                 "message": "Luot gui xe khong hop le",
            #                 'status_text': 'Luot gui xe khong hop le'},
            #                 status=status.HTTP_400_BAD_REQUEST)
            #         parking_session = existing_parking_sessions[0]
            #
            #     # Check user (staff) perform this task does exist
            #     existing_users = User.objects.filter(id=obj['user_id'])
            #     if existing_users.count() <= 0:
            #         return Response({
            #             'status_code': 3,
            #             "message": "Nhan vien khong ton tai",
            #             'status_text': 'Nhan vien khong ton tai'},
            #             status=status.HTTP_400_BAD_REQUEST)
            #
            #     # Check has existing unused ClaimPromotionSession
            #     existing_claim_promotions = ClaimPromotionV2.objects.filter(parking_session_id=obj['parking_session_id'], used=False)
            #
            #     if len(existing_claim_promotions) > 0:
            #         return Response({
            #             'status_code': 2,
            #             "message": "Claim Promotion ton tai, va co the chua duoc su dung",
            #             'status_text': 'Claim Promotion ton tai, va co the chua duoc su dung'
            #         }, status=status.HTTP_400_BAD_REQUEST)
            #
            #     # Create a ClaimPromotion instance
            #     trace_code = str(uuid_uuid4())
            #
            #     # print "obj parking_session_id", obj['parking_session_id']
            #
            #     new_claim_promotion = ClaimPromotionV2(
            #         # parking_session=parking_session,
            #         parking_session_id=obj['parking_session_id'] if 'parking_session_id' in obj else 0,
            #         user_id=obj['user_id'] if 'user_id' in obj else 0,
            #         amount_a=obj['amount_a'] or 0,
            #         amount_b=obj['amount_b'] or 0,
            #         amount_c=obj['amount_c'] or 0,
            #         amount_d=obj['amount_d'] or 0,
            #         amount_e=obj['amount_e'] or 0,
            #         client_time=obj['client_time'] if 'client_time' in obj else None,
            #         server_time=get_now_utc(),
            #         notes=obj['notes'] if 'notes' in obj else '',
            #         used=False
            #     )
            #     new_claim_promotion.save()
            #
            #
            #     # print "@@@@ TAO CLAIM PROMOTION THANH CONG ", new_claim_promotion
            #     # print "OBJ data ne", obj["data"]
            #     # Parse claim_promotion_data
            #     try:
            #         claim_promotion_data = json.loads(obj['data'])
            #         # print "DA load dc JSON", claim_promotion_data
            #
            #         for row in claim_promotion_data:
            #             # Check type
            #             if row["type"] == "BILL":
            #                 # Check bill da duoc su dung hay chua
            #                 row_bill_number = row["bill_number"] if "bill_number" in row else ""
            #                 row_company_info = row["company_info"] if "company_info" in row else ""
            #
            #                 try:
            #                     row_date = dateutil.parser.parse(row["dateUtc"])
            #                 except:
            #                     row_date = get_now_utc()
            #
            #                 # print "ROW DATE", row_date
            #                 ##2018-02-23
            #                 # if row_bill_number != "" and row_company_info != "":
            #                 #     if len(ClaimPromotionBillV2.objects.filter(
            #                 #             bill_number=row_bill_number,
            #                 #             company_info=row_company_info)) > 0:
            #                 #         return Response({"error": {
            #                 #             "message": u"Mã bill & thông tin công ty đã được sử dụng",
            #                 #             "metadata": row
            #                 #         }}, status=status.HTTP_400_BAD_REQUEST)
            #                 ##2018-02-23
            #                 # Tao ClaimPromotionBill
            #                 new_claim_promotion_bill = ClaimPromotionBillV2(
            #                     claim_promotion=new_claim_promotion,
            #                     company_info=row_company_info,
            #                     date=row_date,
            #                     bill_number=row_bill_number,
            #                     bill_amount=row["bill_amount"] if "bill_amount" in row else 0,
            #                     notes=row["notes"] if "notes" in row else "")
            #                 new_claim_promotion_bill.save()
            #             elif row["type"] == "COUPON":
            #                 # Check coupon da duoc su dung hay chua
            #                 row_coupon_code = row["coupon_code"] if "coupon_code" in row else ""
            #                 row_company_info = row["company_info"] if "company_info" in row else ""
            #
            #                 if row_coupon_code != "" and row_company_info != "":
            #                     if len(ClaimPromotionCouponV2.objects.filter(
            #                             coupon_code=row_coupon_code,
            #                             company_info=row_company_info)) > 0:
            #                         return Response({"error": {
            #                             "message": u"Mã coupon & thông tin công ty đã được sử dụng",
            #                             "metadata": row
            #                         }}, status=status.HTTP_400_BAD_REQUEST)
            #
            #                 # Tao ClaimPromotionCoupon
            #                 new_claim_promotion_coupon = ClaimPromotionCouponV2(
            #                     claim_promotion=new_claim_promotion,
            #                     company_info=row_company_info,
            #                     coupon_code=row_coupon_code,
            #                     coupon_amount=row["coupon_amount"] if "coupon_amount" in row else 0,
            #                     notes=row["notes"] if "notes" in row else ""
            #                 )
            #                 new_claim_promotion_coupon.save()
            #     except Exception as ex2:
            #         # print "Exception 2", ex2.args, ex2.message, ex2
            #
            #         new_claim_promotion.delete()
            #         return Response({
            #             'status_code': 2,
            #             'status_text': 'Du lieu khong hop le',
            #             'message': 'Du lieu khong hop le',
            #             'notes': ex2
            #         }, status=status.HTTP_400_BAD_REQUEST)
            #
            #     return Response({
            #         'claim_promotion_id': new_claim_promotion.id,
            #         'status_code': 1,
            #         'status_text': 'Promotion is claimed',
            #         'message': 'Promotion is claimed',
            #         'notes': 'Please check-out within 15 minutes. Otherwise, additional fee would be charged'
            #     })
            # except Exception as e:
            #     return Response({
            #         'status_code': 2,
            #         'status_text': 'Co loi xay ra',
            #         'message': 'Co loi xay ra',
            #         'notes': e
            #     }, status=status.HTTP_500_INTERNAL_SERVER_ERROR)
            ##end old
        else:
            cursor1 = connections['default'].cursor()
            cursor1.execute("begin")
            try:
                rt = "insert into parking_claimpromotion_logerror(session_id,client_time,server_time,sendata,amount_a,amount_b,amount_c,amount_d,amount_e,user_id) values('%s','%s','%s','%s',%s,%s,%s,%s,%s,%s)" % (
                    -1,
                    datetime.datetime.now().strftime("%Y-%m-%d %H:%M:%S"),
                    datetime.datetime.utcnow().replace(microsecond=0, tzinfo=utc).strftime("%Y-%m-%d %H:%M:%S"),
                    "Gởi thông tim sai",0,0,0,0,0,-1)
                cursor1.execute(rt)
                cursor1.execute("commit")
                cursor1.close()
            except:
                cursor1.execute("rollback")
            return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)
##2018May15 call redemtion
class ClaimPromotionCallReduction(generics.CreateAPIView):
    """
    Call reduction
    * Return

      * 200 OK and status "Promotion is called Reduction"
      * 400 if has error
    """
    serializer_class = ClaimPromotionCallBill

    def create(self, request, *args, **kwargs):
        serializer = self.get_serializer(data=request.DATA)
        if serializer.is_valid():
            obj = serializer.object
            # print "obj", obj
            ##2018-04-03
            try:
                vehicletype = obj['vehicletype']
                checkintime = obj['checkintime']
                claimtime = obj['claimtime']
                billdata=json.loads(obj['data'])
                reduction=callredemtion1(vehicletype,checkintime,claimtime,billdata)
                return Response(reduction,status=status.HTTP_200_OK)
            except Exception as e:
                cursor.execute("rollback")
                return Response(0, status=status.HTTP_500_INTERNAL_SERVER_ERROR)
        else:
            return Response(0, status=status.HTTP_400_BAD_REQUEST)


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
class CheckBillInfoView(generics.RetrieveAPIView):
    """
        Bill State Info
    """
    serializer_class = BillStateInfoSerializer
    def retrieve(self, request, *args, **kwargs):
        d= kwargs['billdate'].replace("___"," ")
        c = kwargs['company'].replace("___"," ")
        bc = kwargs['billcode'].replace("___"," ")
        util = Utilities()
        datares = util.Query("checkbill",d,c,bc)
        if len(datares)==1:
            returned_data = {
                'result': datares[0][0]
            }
        else:
            returned_data = {
                'result': 'can not connect to database'
            }
        rs = BillStateInfoSerializer(returned_data)
        return Response(rs.data)
###
class TimeInfoView(generics.RetrieveAPIView):
    """
    Time Info
    """
    serializer_class = TimeInfoSerializer

    def retrieve(self, request, *args, **kwargs):
        returned_data = {
            'utc_time': get_now_utc(),
            'local_time': get_now()
        }
        rs = TimeInfoSerializer(returned_data)

        return Response(rs.data)


class ClaimPromotionTenantListView(generics.ListCreateAPIView):
    """
    List of Tenants joining Claim Promotion programme
    """
    queryset = ClaimPromotionTenant.objects.all()
    serializer_class = ClaimPromotionTenantModelSerializer

    def list(self, request):
        # Note the use of `get_queryset()` instead of `self.queryset`
        queryset = self.get_queryset()
        serializer = ClaimPromotionTenantModelSerializer(queryset, many=True)
        return Response(serializer.data)


class ClaimPromotionVoucherListView(generics.ListCreateAPIView):
    """
        List of Vouchers for Claim Promotion
    """
    queryset = ClaimPromotionVoucher.objects.all()
    serializer_class = ClaimPromotionVoucherModelSerializer

    def list(self, request):
        # Note the use of `get_queryset()` instead of `self.queryset`
        queryset = self.get_queryset()
        serializer = ClaimPromotionVoucherModelSerializer(queryset, many=True)
        return Response(serializer.data)

#2018May21
class ForcedBarierView(generics.CreateAPIView):
    """
    Forced Barier
    """
    serializer_class = ForcedBarierSerializer
    def create(self, request, *args, **kwargs):
        serializer = self.get_serializer(data=request.DATA, files=request.FILES)
        if serializer.is_valid():
            obj = serializer.object
            images = create_save_forced_paths("ForcedBarier")
            save_image(obj['front_thumb'], images['front'])
            save_image(obj['back_thumb'], images['back'])
            res_obj = dict()
            res_obj['front_image_path'] = images['front']
            res_obj['back_image_path'] = images['back']
            res_obj['forced_time']=datetime2timestamp(get_now_utc())
            res_obj['forced_date'] = get_now().strftime("%Y-%m-%d %H:%M:%S")
            res_obj['pc_address'] = obj['terminal']
            res_obj['lane'] = obj['lane']
            res_obj['user'] = obj['user']
            util = Utilities()
            util.NonQuery('forcedbariersave',res_obj['user'], res_obj['lane'],res_obj['pc_address'],res_obj['forced_time'],res_obj['front_image_path'],res_obj['back_image_path'],res_obj['forced_date'])
            return Response(res_obj, status=status.HTTP_201_CREATED)
        else:
            return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)
#2018-03-16
#2018Oct17
class FindAndNotifyBlacklistView(generics.CreateAPIView):
    """
    Find And Notify Blacklist
    """
    serializer_class = BlackListSerializer
    def create(self, request, *args, **kwargs):
        serializer = self.get_serializer(data=request.DATA, files=request.FILES)
        if serializer.is_valid():
            obj = serializer.object
            util = Utilities()
            qr=util.Query('findbacklist',obj['vehiclenumber'])
            if qr and len(qr)>0:
                blacklist=qr[0][0]
                datecompare = (get_now() - datetime.timedelta(seconds=15)).strftime("%Y-%m-%d %H:%M:%S")
                qrr = util.Query('findcurentbacklist', blacklist,datecompare)
                if not qrr or len(qrr)==0:
                    notes = qr[0][3]
                    date = get_now().strftime("%Y-%m-%d %H:%M:%S")
                    util.NonQuery('addcurrentbacklist',obj['parking_session_id'], obj['image_path'],blacklist, obj['gate'],obj['user'],date,obj['stateparking'],0,notes)
                return Response("find new", status=status.HTTP_201_CREATED)
            else:
                return Response("nomal", status=status.HTTP_201_CREATED)
        else:
            return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)

class FindAndNotifyToService(generics.CreateAPIView):
    """
    Find And Notify Blacklist
    """
    serializer_class = NotifiedParmSerializer
    def create(self, request, *args, **kwargs):
        serializer = self.get_serializer(data=request.DATA, files=request.FILES)
        result = []
        if serializer.is_valid():
            obj = serializer.object
            datestr = get_now().replace(hour=0,minute=0,second=0).strftime("%Y-%m-%d %H:%M:%S")
            util = Utilities()
            qr = util.Query('getblacklistnotify', obj['gatename'],datestr, obj['duration'])
            #qr = util.Query('getblacklistnotify', obj['gatename'],obj['datestr'],obj['duration'])
            for item in qr:
                result.append({
                    "id":item[0],"parkingdate":item[1],"vehiclenumber":item[2],"vehicletype":item[3],"gatename":item[4],"user":item[5],
                    "stateparking":item[6],"note":item[7],"parkingsection":item[8],"imagepath":item[9]
                })
            if result and len(result)>0:
                return Response({"message":"had data","data":result}, status=status.HTTP_201_CREATED)
            else:
                return Response({"message": "no data", "data": result}, status=status.HTTP_201_CREATED)
        else:
            return Response({"message": "fail", "data": result}, status=status.HTTP_201_CREATED)
#2018Oct17
class CreateVoucher(generics.CreateAPIView):
    """
        Voucher
    """
    serializer_class = VoucherSerializer
    def create(self, request, *args, **kwargs):
        card_id = kwargs['card_id']
        serializer = self.get_serializer(data=request.DATA, files=request.FILES)
        if serializer.is_valid():
            obj = serializer.object
            check_in_time=obj["check_in_time"].strftime("%Y-%m-%d %H:%M:%S")
            pk_fee=obj['parking_fee']
            actual_fee = obj['actual_fee']
            voucher_amount=obj['voucher_amount']
            voucher_type=obj['voucher_type']
            util = Utilities()
            if util.NonQuery('savevoucher',card_id,check_in_time,voucher_type,voucher_amount,pk_fee,actual_fee):
                return Response(1, status=status.HTTP_201_CREATED)
            else:
                return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)
        else:
            return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)
class DeleteVoucher(generics.CreateAPIView):
    """
        Voucher
    """
    serializer_class = VoucherParamSerializer
    def create(self, request, *args, **kwargs):
        card_id = kwargs['card_id']
        serializer = self.get_serializer(data=request.DATA, files=request.FILES)
        if serializer.is_valid():
            obj = serializer.object
            check_in_time = obj["check_in_time"].strftime("%Y-%m-%d %H:%M:%S")
            util = Utilities()
            if util.NonQuery('cancelvoucher', card_id, check_in_time):
                return Response(1, status=status.HTTP_201_CREATED)
            else:
                return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)
        else:
            return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)
def create_save_forced_paths(label):
    check_time = datetime.datetime.now()
    date_str = '%.4d%.2d%.2d' % (check_time.year, check_time.month, check_time.day)
    time_str = '%.2d%.2d' % (check_time.hour, check_time.minute)
    file_name_base = '%s/%s/%s_%s' % (date_str, label, time_str, label)
    return {
        'front': file_name_base + '_f.jpg',
        'back': file_name_base + '_b.jpg'
    }
#2018-03-16
@api_view(('GET',))
def get_vehicle_type_category(request, format=None):
    """
    Get vehicle type category
    """
    rs = list()
    for item in VEHICLE_TYPE_CATEGORY:
        rs.append({'id': item[0], 'name': item[1]})
    return Response(rs, status.HTTP_200_OK)


@api_view(('GET',))
def get_health(request, format=None):
    """
    Get health of server
    """
    list(ParkingSetting.objects.all())
    return Response({'detail': 'OK'}, status.HTTP_200_OK)

@api_view(('GET',))
def clearcache(request, format=None):
    """
    Get health of server
    """
    if redis_client:
        redis_client.flushall()
        init_cache_data()
    return Response({'detail': 'OK'}, status.HTTP_200_OK)


@api_view(('GET',))
def get_parking_name(request, format=None):
    """
    Get parking name
    """
    return Response(
        {'request_ip': get_ip(request), 'parking_name': get_setting('parking_name', u'Tên bãi xe', 'Green Parking')},
        status.HTTP_200_OK)


@api_view(('GET',))
def get_global_config(request, format=None):
    """
    Get global config
    terminal_id -- Terminal ID
    version -- Version of client application
    """
    version = get_param(request, 'version')
    terminal_id = get_param(request, 'terminal_id')
    if version and terminal_id:
        try:
            rs = Terminal.objects.get(id=terminal_id)
            rs.version = version
            rs.save()
        except Terminal.DoesNotExist:
            pass
    rs = {
        'parking_name': get_setting('parking_name', u'Tên bãi xe', 'Green Parking'),
        'log_server': get_setting('log_server', u'Địa chỉ máy chủ Log', '192.168.1.41')
    }
    return Response(rs, status.HTTP_200_OK)
@api_view(('GET',))
def get_recallfee(request):
    """
    Get Recall Fee
    card_id -- Terminal ID
    voucher_hour -- Voucher hours
    """
    try:
        card_id = get_param(request, 'card_id')
        voucherhour = int(get_param(request, 'voucher_hour'))
        card = get_card(card_id)
        if not card:
            return Response(-1, status=status.HTTP_200_OK)
        pkss = ParkingSession.objects.filter(card=card).order_by('-check_in_time')[:1]#check_out_time=None,
        if not pkss or len(pkss) <= 0:
            return Response(-1, status=status.HTTP_200_OK)
        parking_session = pkss[0]
        terminal_id = parking_session.check_in_lane.terminal_id
        serializer = BasicCheckInSessionSerializer(parking_session, many=False)
        f_time = parking_session.check_in_time
        if voucherhour > 0:
            t_time = f_time + timedelta(hours=voucherhour)
        to_time = get_now_utc()
        ut = Utilities()
        qr = ut.QuerySecond('isactivetoolfee')
        if qr and len(qr) == 1 and qr[0][0] == 'new':

            customer_info1= get_parking_fee_info(card_id, parking_session.vehicle_type,
                                                 card.card_type,
                                                 f_time, t_time)
            customer_info = get_parking_fee_info(card_id, parking_session.vehicle_type,
                                                  card.card_type,
                                                  f_time, to_time)
            refee1 =  int(customer_info1["parking_fee"]) if customer_info1 else 0
            refee2 = int(customer_info["parking_fee"]) if customer_info else 0
            refee=refee2-refee1 if refee2>refee1 else 0
            return Response(refee, status=status.HTTP_200_OK)
        ##
        ## Before 2018May10
        else:
            cit = f_time
            time_out = get_now_utc()
            cc = None
            if len(customer_info) > 2:
                cif = customer_info["vehicle_registration_info"]
                ccd = cif['cancel_date']
                efd = cif['expired_date']
                cit = get_activecheckin(parking_session.check_in_time, ccd, efd)
                cc = canchanges(time_out, ccd, efd)
            parking_fee_result = calculate_parking_fee(parking_session.id, card_id, parking_session.vehicle_type,
                                                       cit, time_out)
            refee = int(parking_fee_result[0]) if parking_fee_result else 0
            return Response(refee, status=status.HTTP_200_OK)
    except Exception as e:
        return Response(-1, status=status.HTTP_200_OK)
# ##2018-02-09 test ActiveCheckInTime
# class activecheckintime(generics.CreateAPIView):
#     """
#     ActiveCheckInTime
#     """
#     serializer_class = ActiveCheckInTimeSerializer
#
#     def create(self, request, *args, **kwargs):
#         serializer = self.get_serializer(data=request.DATA, files=request.FILES)
#         if serializer.is_valid():
#             obj = serializer.object
#             check_in_time=obj['check_in_time']
#             try:
#                 cancel_time=obj['cancel_time']
#             except:
#                 cancel_time = None
#             try:
#                 expirate_time = obj['expirate_time']
#             except:
#                 expirate_time = None
#             active_check_in_time=get_activecheckin(check_in_time,cancel_time,expirate_time)
#
#             return Response(active_check_in_time, status=status.HTTP_201_CREATED)
#         else:
#             return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)
# ##2018-02-09
