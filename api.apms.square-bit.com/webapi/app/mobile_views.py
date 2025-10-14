# -*- coding: utf-8 -*-
import csv
import os
from django.core.files.uploadedfile import TemporaryUploadedFile, InMemoryUploadedFile

from django.http import Http404, HttpResponse
from django.contrib.auth.models import User
from django.db.models import Q
import io
import xlsxwriter
import requests
from datetime import datetime, time, timedelta
import dateutil.parser

from rest_framework.decorators import api_view
from rest_framework.response import Response
from rest_framework import status
from rest_framework import generics
from rest_framework.views import APIView

from mobile_serializers import *
from webapi.settings import MEDIA_ROOT
from  parking.parking.invoiceService import DoRetailInvoice
from parking.parking.models import Card, CardStatus, Terminal, Lane, Camera, ParkingSession, CheckInImage, UserCard, \
    UserShift, ParkingSetting, CheckOutException, UserProfile, Attendance, CardType, VehicleType, \
    CheckOutExceptionInfo, TerminalGroup, ParkingFeeSession, VehicleRegistration, \
    ClaimPromotion, ClaimPromotionBill, ClaimPromotionCoupon, ClaimPromotionV2, ClaimPromotionBillV2, ClaimPromotionCouponV2, \
    ClaimPromotionTenant, ClaimPromotionVoucher, Customer,\
    VEHICLE_TYPE_CATEGORY, load_nghia_vehicle_type, get_storaged_vehicle_type, decode_vehicle_type, get_setting
from parking.parking.common import CARD_STATUS
from parking.parking.services import search_parking_session, get_statistics, search_parking_session_new, search_claim_promotion
from utils import *
from image_replication import ImageReplicationController
from django.db import connections
from .views import CalculatorParkingFee
import redis
import json
import datetime
import math


def getFeebyParkingSession(p):
    try:
        if p.check_out_time is None:
            return  0
        elif p.check_out_exception is not None:
            p.check_out_exception.parking_fee
        pf = ParkingFeeSession.objects.filter(parking_session = p, session_type = 'OUT').order_by('-parking_fee')
        if pf:
            return pf[0].parking_fee
        else:
            return 0
    except:
        return 0

def getTotalFees(pks):
    pksFees = [getFeebyParkingSession(p) for p in pks]
    return  sum(pksFees) if pksFees else 0

def getMonthlyBySession(p):
    try:
        card = p.card
        vh = VehicleRegistration.objects.filter(card = card)
        if vh:
            return vh[0].vehicle_number
        return None
    except:
        return None

def GetVehicleName(pt, vts):
    try:
        chk = [v for v in vts if v[0]/10000 == pt]
        if chk:
            return chk[0][1]
        return "Khác"
    except:
        return "Khác"

def getUser(userId):
    try:
        return  User.objects.get(id = userId)
    except:
        return None

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

import os

def save_image(file_obj, filename):
    if not file_obj:
        raise ValueError("No file to save")

    if not isinstance(file_obj, TemporaryUploadedFile) :
        if not isinstance(file_obj, InMemoryUploadedFile):
            raise TypeError("Expected File, got %s"%(type(file_obj)))

    if filename[0] == '/':
        save_path = MEDIA_ROOT + filename
    else:
        save_path = MEDIA_ROOT + '/' + filename
    dir_path = os.path.dirname(save_path)
    if not os.path.exists(dir_path):
        os.makedirs(dir_path)
    with open(save_path, 'wb+') as f:
        for chunk in file_obj.chunks():
            f.write(chunk)

def ProcessOutException(parking_session, lane, operator, reson):
    try:
        to_time = get_now_utc()
        parking_fee = CalculatorParkingFee(parking_session)
        images = create_save_paths(parking_session.card.card_label, False)
        exo = CheckOutExceptionInfo()
        exo.notes = reson

        exo.parking_fee = parking_fee
        exo.save()
        parking_session.check_out_time = to_time
        parking_session.check_out_images = images
        parking_session.check_out_lane = lane
        parking_session.check_out_operator = operator
        parking_session.check_out_alpr_vehicle_number = '0000-00000'
        parking_session.duration = (parking_session.check_out_time - parking_session.check_in_time).total_seconds()
        parking_session.check_out_exception = exo
        parking_session.save()
        return True
    except Exception as ex:
        return False

def ProcesInOut(card, lane, operator, vehicleType, vehicleNumber, ANPRVehicleNumber, frontThumb, backThumb, asthesameCard = False):
    try:
        parking_session = None
        to_time = get_now_utc()
        pkss = ParkingSession.objects.filter(check_out_time=None, card=card).order_by('-check_in_time')
        if pkss and len(pkss) > 0:
            parking_session = pkss[0]
            for pk in pkss:
                if pk != parking_session:
                    pf = ParkingFeeSession.objects.filter(parking_session_id=pk.id)
                    for f in pf:
                        f.delete()
                    pk.delete()
        if parking_session is not None: # Process Out
            images = create_save_paths(card.card_label, False)
            if frontThumb is not None:
                save_image(frontThumb, images['front'])
            if backThumb is not None:
                save_image(backThumb, images['back'])
            parking_fee = CalculatorParkingFee(parking_session)
            parking_session.check_out_time = to_time
            parking_session.check_out_images = images
            parking_session.check_out_lane = lane
            parking_session.check_out_operator = operator
            parking_session.check_out_alpr_vehicle_number = ANPRVehicleNumber if ANPRVehicleNumber is not None else ''
            parking_session.duration = (parking_session.check_out_time - parking_session.check_in_time).total_seconds()
            parking_session.save()
            parking_fee_session = ParkingFeeSession(parking_session_id=int(parking_session.id), card_id=str(card.card_id),
                                                    vehicle_number=parking_session.vehicle_number or '',
                                                    parking_fee=parking_fee, parking_fee_detail="Mobile Method",
                                                    session_type='OUT',
                                                    vehicle_type_id=vehicleType)
            parking_fee_session.save()
            claim_promotion = ClaimPromotionV2.objects.filter(parking_session_id=parking_session.id, used=False)
            if claim_promotion and len(claim_promotion) > 0:
                claim_promotion = claim_promotion[0]
                claim_promotion.used = True
                claim_promotion.parking_fee_session_id = parking_fee_session.id
                claim_promotion.save()
            vehicle_registration = VehicleRegistration.objects.filter(card=card)
            is_vehicle_registration = True if vehicle_registration else False
            if is_vehicle_registration:
                parking_fee_session.is_vehicle_registration = is_vehicle_registration
                parking_fee_session.save()
            return 2, parking_session, parking_fee
        else:
            parking_session = ParkingSession()
            images = create_save_paths(card.card_label, True)
            if frontThumb is not None:
                save_image(frontThumb, images['front'])
            if backThumb is not None:
                save_image(backThumb, images['back'])
            parking_session.card = card
            parking_session.check_in_time = to_time
            parking_session.check_in_images = images
            parking_session.check_in_lane = lane
            parking_session.check_in_operator = operator
            parking_session.vehicle_type = vehicleType/10000
            parking_session.vehicle_number = vehicleNumber if vehicleNumber is not None else ''
            parking_session.check_in_alpr_vehicle_number = ANPRVehicleNumber if ANPRVehicleNumber else ''
            parking_session.save()
            parking_fee_session = ParkingFeeSession(parking_session_id=int(parking_session.id), card_id=str(card.card_id),
                                                    vehicle_number=parking_session.vehicle_number or '',
                                                    parking_fee=0, parking_fee_detail="",
                                                    session_type='IN',
                                                    vehicle_type_id=vehicleType)
            vehicle_registration = VehicleRegistration.objects.filter(card=card)
            is_vehicle_registration = True if vehicle_registration else False
            if is_vehicle_registration:
                parking_fee_session.is_vehicle_registration = is_vehicle_registration
                parking_fee_session.save()
            return 1, parking_session, 0
    except Exception as ex:
        return 0, None

def ProcesUpdateIn(card, vehicleType, vehicleNumber, ANPRVehicleNumber, frontThumb, backThumb, asthesameCard = False):
    try:
        parking_session = None
        pkss = ParkingSession.objects.filter(check_out_time=None, card=card).order_by('-check_in_time')
        if pkss and len(pkss) > 0:
            parking_session = pkss[0]
            for pk in pkss:
                if pk != parking_session:
                    pf = ParkingFeeSession.objects.filter(parking_session_id=pk.id)
                    for f in pf:
                        f.delete()
                    pk.delete()
        if parking_session is not None: # Process Update In
            images = parking_session.check_in_images
            if frontThumb is not None:
                save_image(frontThumb, images['front'])
            if backThumb is not None:
                save_image(backThumb, images['back'])
            parking_session.vehicle_number = vehicleNumber if vehicleNumber is not None else ''
            parking_session.check_in_alpr_vehicle_number = ANPRVehicleNumber if ANPRVehicleNumber is not None else ''
            parking_session.vehicle_type = vehicleType/10000
            parking_session.save()
            pkf = ParkingFeeSession.objects.filter(parking_session_id = parking_session.id, session_type = 'IN')
            for pf in pkf:
                pf.vehicle_type_id =vehicleType
                pf.save()
            return 3, parking_session,0
        else:
            return 0, None, 0
    except Exception as ex:
        return 0, None, 0

def ProcesUpdateOut(card, ANPRVehicleNumber, frontThumb, backThumb, asthesameCard = False):
    try:
        parking_session = None
        pkss = ParkingSession.objects.filter(check_out_time__isnull=False, card=card).order_by('-check_out_time')
        if pkss and len(pkss) > 0:
            parking_session = pkss[0]
        if parking_session is not None: # Process Update In
            images = parking_session.check_out_images
            if frontThumb is not None:
                save_image(frontThumb, images['front'])
            if backThumb is not None:
                save_image(backThumb, images['back'])

            parking_session.check_out_alpr_vehicle_number = ANPRVehicleNumber if ANPRVehicleNumber is not None else ''
            parking_session.save()
            return 4, parking_session,0
        else:
            return 0, None, 0
    except Exception as ex:
        return 0, None, 0

def makeResult(card, vehicle_registration, parking_session, parking_fee, mode, base_url):
    regis = None
    to_day = get_now_utc().date()

    if vehicle_registration:
        regis = {
            "Id":vehicle_registration.id,
            "VehicleNumber":vehicle_registration.vehicle_number,
            "VehicleType":vehicle_registration.vehicle_type.name if vehicle_registration.vehicle_type is not None else '',
            "CustomerName":vehicle_registration.customer.customer_name if vehicle_registration.customer is not None else '',
            "DriverName":vehicle_registration.vehicle_driver_name if vehicle_registration.vehicle_driver_name is not None else '',
            "ActivatedDate":vehicle_registration.start_date if vehicle_registration.start_date is not None else to_day,
            "ExpiredDate": vehicle_registration.expired_date if vehicle_registration.expired_date is not None else to_day,
            "Status": vehicle_registration.status,
            "FeePerMonth": vehicle_registration.level_fee.fee if vehicle_registration.level_fee is not None else 0
        }
    cardType = getCardType(card)
    vehicleType = getVehicleType(card)
    CardInfo = {
        "Id": card.id,
        "CardId": card.card_id,
        "CardLabel": card.card_label,
        "CardStatus": card.status,
        "CardType": {"id":cardType.id, "name":cardType.name} if cardType is not None else None,
        "VehicleType":{"id":vehicleType.id, "name":vehicleType.name} if vehicleType is not None else None,
        "Ticket": regis
    }
    result = {
        "ParkingSessionId": parking_session.id,
        "Card": CardInfo,
        "ParkingFee": parking_fee,
        "VehicleType": parking_session.vehicle_type,
        "VehicleNumberIn": parking_session.check_in_alpr_vehicle_number,
        "InTime": parking_session.check_in_time,
        "OutTime": parking_session.check_out_time,
        "Urls": getImgesUrls(parking_session, base_url),
        "Mode": mode
    }
    return  result

def RevenueInfo(fr, to):
    header = [u"Xe ra", u"Số lượt", u"Tổng phí"]
    vts = VehicleType.objects.all()
    pks = ParkingSession.objects.filter(check_out_time__gte = fr, check_out_time__lte = to)
    dataByVehicle = []
    for v in vts:
        dataByVehicle.append(("Khác" if v.id == 100000000 else v.name, pks.filter(vehicle_type = v.id/10000)))
    data = []
    for d in dataByVehicle:
        data.append([d[0], d[1].count(), getTotalFees(d[1])])
    total = ["Tổng", pks.count(), getTotalFees(pks)]

    return header, data, total

def RevenueDetailInfo(fr, to):
    header = [u"Nhãn thẻ", u"Biển số", u"Giờ vào", u"Giờ ra", u"Phí lượt"]
    pks = ParkingSession.objects.filter(check_out_time__gte = fr, check_out_time__lte = to).order_by('check_out_time', 'vehicle_type')
    data = []
    for p in pks:
        data.append([p.card.card_label if p.card is not None else '',
                     p.vehicle_number if p.vehicle_number is not None else '',
                     p.check_in_time.strftime("%d/%m/%y %H:%M") if p.check_in_time is not None else '',
                     p.check_out_time.strftime("%d/%m/%y %H:%M") if p.check_out_time is not None else '',
                     getFeebyParkingSession(p)])
    return header, data

class RetailInvoiceView(generics.CreateAPIView):
    """
        Thực hiện gửi hóa đơn khi khách yêu cầu
        """
    serializer_class = RetailInvoiceSerializer
    def create(self, request, *args, **kwargs):
        try:
            serializer = self.get_serializer(data=request.DATA)
            if serializer.is_valid():
                obj = serializer.object
                parking_id = int(obj["parking_id"])
                fee = int(obj["fee"])
                completed = bool(obj["completed"]) if "completed" in obj else False
                has_buyer = bool(obj["has_buyer"]) if "has_buyer" in obj else False
                buyer_code = obj["buyer_code"] if "buyer_code" in obj else None
                buyer_name = obj["buyer_name"] if "buyer_name" in obj else None
                legal_name = obj["legal_name"] if "legal_name" in obj else None
                taxcode = obj["taxcode"] if "taxcode" in obj else None
                receiver_name = obj["receiver_name"] if "receiver_name" in obj else None
                receiver_emails = obj["receiver_emails"] if "receiver_emails" in obj else None
                phone = obj["phone"] if "phone" in obj else None
                email = obj["email"] if "email" in obj else None
                address = obj["address"] if "address" in obj else None
                res = DoRetailInvoice(parking_id, fee, completed, has_buyer, buyer_code, legal_name,
                                      buyer_name, taxcode, address, phone, email, receiver_name, receiver_emails)
                if res is True:
                    return Response({"Success": res}, status=status.HTTP_201_CREATED)
                else:
                    return Response({"detail": "Fail"}, status=status.HTTP_400_BAD_REQUEST)
            else:
                return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)
        except Exception as ex:
            return Response({'detail': ex}, status=status.HTTP_400_BAD_REQUEST)

class MobileSearchView(generics.CreateAPIView):
    """
          Mobile Thao tác tìm kiếm
          """
    serializer_class = SearchSerializer

    def create(self, request, *args, **kwargs):
        try:
            serializer = self.get_serializer(data=request.DATA)
            if serializer.is_valid():
                obj = serializer.object
                operator = getUser(obj["userId"])
                if operator is None:
                    return Response({'detail': 'Operator does not exits'}, status=status.HTTP_400_BAD_REQUEST)
                searchType = int(obj["searchType"])
                cardLabel = str(obj["cardLabel"]) if 'cardLabel' in obj else None
                if cardLabel is not None and len(cardLabel)<1:
                    cardLabel = None
                pks = GetParkingSearch(searchType, cardLabel)
                if pks is None:
                    return Response({'detail': 'Invalid parameters'}, status=status.HTTP_400_BAD_REQUEST)
                pks = pks.order_by('-check_out_time', '-check_in_time')
                total = pks.count()
                vts = [(v.id,v.name) for v in VehicleType.objects.all()]
                result = [ {
                    "ParkingSessionId": parking_session.id,
                    "CardLabel": parking_session.card.card_label,
                    "ParkingFee": getFeebyParkingSession(parking_session),
                    "VehicleType": GetVehicleName(parking_session.vehicle_type, vts),
                    "VehicleNumberIn": parking_session.check_in_alpr_vehicle_number,
                    "VehicleNumberOut": parking_session.check_out_alpr_vehicle_number,
                    "InTime": parking_session.check_in_time,
                    "OutTime": parking_session.check_out_time,
                    "MonthlyVehicleNumber": getMonthlyBySession(parking_session)
                } for parking_session in pks]
                fees = [f["ParkingFee"] for f in result]
                totalFee = sum(fees) if fees else 0
                return Response({"Total": total, "TotalFee": totalFee, "Data":result}, status=status.HTTP_200_OK)
            else:
                return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)
        except Exception as ex:
            return Response({'detail': ex}, status=status.HTTP_400_BAD_REQUEST)

class CheckCusView(generics.CreateAPIView):
    """
       Mobile Thao tác vào ra
       """
    serializer_class = CheckCusSerializer

    def create(self, request, *args, **kwargs):
        try:
            serializer = self.get_serializer(data=request.DATA)
            if serializer.is_valid():
                obj = serializer.object
                cardId = obj["cardId"]
                asthesameCard = True if "asthesameCard" in obj else False
                vehicleTypeCheck = int(obj["vehicleType"]) if "vehicleType" in obj else 0
                vehicleNumber = str(obj["vehicleNumber"]) if 'vehicleNumber' in obj else ''
                vehicleNumberCheck = vehicleNumber.replace('-', '')

                if asthesameCard is True and vehicleTypeCheck > 0 and vehicleNumberCheck == cardId and len(vehicleNumberCheck) >=7 and len(vehicleNumberCheck) <=9:
                    card, vehicle_registration = makeTheSmaeCard(cardId, vehicleTypeCheck, vehicleNumber)
                else:
                    card, vehicle_registration = getCard(obj["cardId"])
                if card is None:
                    return Response(u"Thẻ chưa đăng ký", status=status.HTTP_400_BAD_REQUEST)
                lane = getLane(obj["appId"])
                if lane is None:
                    return Response(u"Mã ứng dụng không đúng", status=status.HTTP_400_BAD_REQUEST)
                operator = getUser(obj["userId"])
                if operator is None:
                    return Response("Tài khoản đăng nhập không hợp lệ", status=status.HTTP_400_BAD_REQUEST)
                base_url = request.build_absolute_uri('/')
                checkMode = int(obj["checkMode"])
                tmpVehicleType = int(obj["vehicleType"]) if "vehicleType" in obj else card.vehicle_type
                vehicleType = vehicle_registration.vehicle_type.id if vehicle_registration and vehicle_registration.vehicle_type else tmpVehicleType
                anprVehicleNumber = str(obj["anprVehicleNumber"]) if 'anprVehicleNumber' in obj else ''
                frontThumb = request.FILES.get('frontThumb', None)
                backThumb = request.FILES.get('backThumb', None)
                # print("front_thumb: %s"%(type(frontThumb)))
                # print("back_thumb: %s" % (type(backThumb)))
                if checkMode == 0:
                   mode, pk, parking_fee = ProcesInOut(card, lane, operator, vehicleType, vehicleNumber, anprVehicleNumber, frontThumb, backThumb)
                elif checkMode == 1:
                   mode, pk, parking_fee = ProcesUpdateIn(card, vehicleType, vehicleNumber,anprVehicleNumber, frontThumb,backThumb)
                elif checkMode == 2:
                    mode, pk, parking_fee = ProcesUpdateOut(card, anprVehicleNumber, frontThumb, backThumb)
                else:
                    return Response("Yêu cầu không hợp lệ", status=status.HTTP_400_BAD_REQUEST)
                if mode == 0:
                    return Response("Lỗi thao tác xử lý", status=status.HTTP_400_BAD_REQUEST)
                res = makeResult(card, vehicle_registration, pk, parking_fee, mode, base_url)
                return Response(res, status=status.HTTP_200_OK)
            else:
                return Response("Thiếu thông tin để thực hiện", status=status.HTTP_400_BAD_REQUEST)
        except Exception as ex:
            return Response("Xẩy ra ngoại lệ", status=status.HTTP_400_BAD_REQUEST)

class EditSessionView(generics.CreateAPIView):
    """
       Mobile Thao tác biển số
       """
    serializer_class = EditParkingSerializer

    def create(self, request, *args, **kwargs):
        try:
            serializer = self.get_serializer(data=request.DATA)
            if serializer.is_valid():
                obj = serializer.object
                parkingId = obj["parkingId"]
                parking_session = ParkingSession.objects.get(id = parkingId)

                card, vehicle_registration = getCard(parking_session.card.card_id)
                if card is None:
                    return Response({'detail': 'Card does not exist'}, status=status.HTTP_400_BAD_REQUEST)

                tmpVehicleType = int(obj["vehicleType"]) if "vehicleType" in obj else card.vehicle_type
                vehicleType = vehicle_registration.vehicle_type.id if vehicle_registration and vehicle_registration.vehicle_type else tmpVehicleType
                vehicleNumber = str(obj["vehicleNumber"]) if 'vehicleNumber' in obj else ''
                parking_session.vehicle_type = vehicleType / 10000
                parking_session.vehicle_number = vehicleNumber if vehicleNumber is not None else ''
                parking_session.check_in_alpr_vehicle_number = vehicleNumber if vehicleNumber is not None else ''
                parking_session.save()
                return Response({"detail":"OK"}, status=status.HTTP_200_OK)
            else:
                return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)
        except Exception as ex:
            return Response({'detail': ex}, status=status.HTTP_400_BAD_REQUEST)

class OutSessionExceptionView(generics.CreateAPIView):
    """
       Mobile Cho ra ngoại lệ
       """
    serializer_class = OutSessionExceptionSerializer

    def create(self, request, *args, **kwargs):
        try:
            serializer = self.get_serializer(data=request.DATA)
            if serializer.is_valid():
                obj = serializer.object
                lane = getLane(obj["appId"])
                if lane is None:
                    return Response({'detail': 'Mobile App ID is invalid'}, status=status.HTTP_400_BAD_REQUEST)
                operator = getUser(obj["userId"])
                if operator is None:
                    return Response({'detail': 'Operator does not exits'}, status=status.HTTP_400_BAD_REQUEST)
                parkingId = obj["parkingId"]
                parking_session = ParkingSession.objects.get(id = parkingId)
                if parking_session.check_out_time is not None:
                    return Response({'detail': 'Sesson has completed'}, status=status.HTTP_400_BAD_REQUEST)
                reson = obj["reson"]
                res = ProcessOutException(parking_session,lane,operator, reson)
                if res is False:
                    return Response({'detail': 'Exception Out Process error'}, status=status.HTTP_400_BAD_REQUEST)
                return Response({"detail":"OK"}, status=status.HTTP_200_OK)
            else:
                return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)
        except Exception as ex:
            return Response({'detail': ex}, status=status.HTTP_400_BAD_REQUEST)

class CancelSessionView(generics.CreateAPIView):
    """
       Mobile Thao tác hủy phiên
       """
    serializer_class = EditParkingSerializer

    def create(self, request, *args, **kwargs):
        try:
            serializer = self.get_serializer(data=request.DATA)
            if serializer.is_valid():
                obj = serializer.object
                parkingId = obj["parkingId"]
                parking_session = ParkingSession.objects.get(id = parkingId)
                check_out_time = parking_session.check_out_time
                if check_out_time is None:
                    pf = ParkingFeeSession.objects.filter(parking_session_id=parking_session.id)
                    for f in pf:
                        f.delete()
                    parking_session.delete()
                else:
                    parking_session.check_out_images = None
                    parking_session.check_out_time = None
                    parking_session.check_out_images = None
                    parking_session.check_out_lane = None
                    parking_session.check_out_operator = None
                    parking_session.check_out_alpr_vehicle_number = None
                    parking_session.duration = None
                    parking_session.save()
                    pf = ParkingFeeSession.objects.filter(parking_session_id=parking_session.id, session_type = "OUT")
                    for f in pf:
                        f.delete()
                return Response({"detail":"OK"}, status=status.HTTP_200_OK)
            else:
                return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)
        except Exception as ex:
            return Response({'detail': ex}, status=status.HTTP_400_BAD_REQUEST)

class ShiftAssignmentView(generics.CreateAPIView):
    """
       Mobile Giao ca
       """
    serializer_class = MobileShiftSerializer

    def create(self, request, *args, **kwargs):
        serializer = self.get_serializer(data=request.DATA)
        if serializer.is_valid():
            car_filter = [200]
            bicycle_filter = [100]
            monthlyCardIds = getMonthlyCard()
            obj = serializer.object
            try:
                user = User.objects.get(username=obj['username'], is_active=True)
                isApply = obj["isApply"]
                ms = getCurrentMobileShift(obj["appId"], user.username)
                if ms is None:
                    ms = MobileShift()
                    ms.staff = user.username
                    ms.app_id = obj["appId"]
                    ms.from_time = get_now_utc()
                    ms.save()
                from_time = ms.from_time
                to_time = get_now_utc()
                pk_in = ParkingSession.objects.filter(check_in_time__gte= from_time, check_in_time__lte = to_time)
                pk_car_in = pk_in.filter(vehicle_type__in = car_filter)
                pk_bicycle_in = pk_in.filter(vehicle_type__in=bicycle_filter)
                pk_monthly_in = pk_in.filter(card__id__in = monthlyCardIds)
                pk_visitor_in = pk_in.exclude(card__id__in = monthlyCardIds)
                pk_out = ParkingSession.objects.filter(check_out_time__gte=from_time, check_out_time__lte=to_time)
                pk_car_out = pk_out.filter(vehicle_type__in=car_filter)
                pk_bicycle_out = pk_out.filter(vehicle_type__in=bicycle_filter)
                pk_monthly_out = pk_out.filter(card__id__in=monthlyCardIds)
                pk_visitor_out = pk_out.exclude(card__id__in=monthlyCardIds)
                feeList = [getFeebyParkingSession(p) for p in pk_out]
                totalFee = sum(feeList) if feeList else 0
                totalActualFee = obj['actualFee'] if isApply and 'actualFee' in obj else totalFee
                ajustmentFee = obj['ajustmentFee'] if isApply and 'ajustmentFee' in obj else 0
                note = obj["note"] if "note" in obj else None
                if isApply:
                    ms.to_time = to_time
                    ms.total_in = pk_in.count()
                    ms.total_out = pk_out.count()
                    ms.total_car_in = pk_car_in.count()
                    ms.total_car_out = pk_car_out.count()
                    ms.total_bicycle_in = pk_bicycle_in.count()
                    ms.total_bicycle_out = pk_bicycle_out.count()
                    ms.total_monthly_in = pk_monthly_in.count()
                    ms.total_monthly_out = pk_monthly_out.count()
                    ms.total_visitor_in = pk_visitor_in.count()
                    ms.total_visitor_out = pk_visitor_out.count()
                    ms.total_fee = totalFee
                    ms.total_actual_fee = totalActualFee
                    ms.ajustment_fee = ajustmentFee
                    ms.note = note
                    ms.save()
                res = {
                    "AppId": ms.app_id,
                    "Username": user.username,
                    "FromTime": ms.from_time,
                    "ToTime": to_time,
                    "TotalIn": pk_in.count(),
                    "TotalOut": pk_out.count(),
                    "CarIn": pk_car_in.count(),
                    "CarOut": pk_car_out.count(),
                    "BicycleIn": pk_bicycle_in.count(),
                    "BicycleOut": pk_bicycle_out.count(),
                    "MonthlyIn": pk_monthly_in.count(),
                    "MonthlyOut": pk_monthly_out.count(),
                    "VisitorIn": pk_visitor_in.count(),
                    "VisitorOut": pk_visitor_out.count(),
                    "TotalFee": totalFee,
                    "ActualFee": totalActualFee,
                    "AjustmentFee": ajustmentFee,
                    "Note": note
                }
                return Response(res, status=status.HTTP_200_OK)
            except Exception as ex:
                return Response({'detail': 'User does not exist'}, status=status.HTTP_400_BAD_REQUEST)
        else:
            return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)

class CerificationView(generics.CreateAPIView):
    """
    Certification Mobile App
    """
    serializer_class = CertificationSerializer
    def create(self, request, *args, **kwargs):
        serializer = self.get_serializer(data=request.DATA)
        if serializer.is_valid():
            obj = serializer.object
            try:
                user = User.objects.get(username=obj['username'], is_active=True)
                if not user.check_password(obj['password']):
                    return Response({'detail': 'Password is not right.'}, status=status.HTTP_400_BAD_REQUEST)
                if user.is_superuser is None or user.is_superuser is False:
                    return Response({'detail': 'User not permission'}, status=status.HTTP_400_BAD_REQUEST)
                CardId = None
                staff = getStaffByUser(user.id)
                if staff is not None and staff.card is not None:
                    CardId = staff.card.card_id
                UniqueId = obj['appId']
                terminals = Terminal.objects.filter(terminal_id = UniqueId)
                if not  terminals:
                    terminal = Terminal()
                else:
                    terminal = terminals[0]
                terminal.terminal_id = UniqueId
                terminal.name = obj["appName"]
                terminal.ip = '%s,%s'%(obj["urlBase1"],obj["urlBase2"])
                terminal.status = 1
                terminal.last_check_health = datetime.datetime.now()
                terminal.save()
                lane = getLane(terminal.terminal_id)
                if lane is None:
                    lane = Lane()
                    lane.terminal = terminal
                    lane.name = "Làn di động"
                    lane.direction = 0
                    lane.enabled = True
                    lane.vehicle_type  = 100000000
                    lane.save()
                GatePosition = {"Id":terminal.id, "GateName":terminal.name}
                Signator = {
                    "Id": user.id,
                    "Username":user.username,
                    "Password":None,
                    "Email":user.email,
                    "FirstName":user.first_name,
                    "LastName":user.last_name,
                    "CardId":CardId
                }
                res = {"GatePosition": GatePosition, "Signator": Signator, "UniqueId": UniqueId,
                       "UrlBase1": obj["urlBase1"], "UrlBase2": obj["urlBase2"]}
                return Response(res, status=status.HTTP_200_OK)
            except Exception as ex:
                return Response({'detail': 'User does not exist'}, status=status.HTTP_400_BAD_REQUEST)
        else:
            return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)

class UserLoginView(generics.CreateAPIView):
    """
    User Login Mobile
    """
    serializer_class = UserLoginSerializer
    def create(self, request, *args, **kwargs):
        serializer = self.get_serializer(data=request.DATA)
        if serializer.is_valid():
            obj = serializer.object
            try:
                user = User.objects.get(username=obj['username'], is_active=True)
                if not user.check_password(obj['password']):
                    return Response({'detail': 'Password is not right.'}, status=status.HTTP_400_BAD_REQUEST)
                isAdmin = obj["isAdmin"]
                if isAdmin:
                    if user.is_superuser is None or user.is_superuser is False:
                        return Response({'detail': 'User is not Admin role'}, status=status.HTTP_400_BAD_REQUEST)
                else:
                    if user.is_staff is None or user.is_staff is False:
                        return Response({'detail': 'User is not Staff role'}, status=status.HTTP_400_BAD_REQUEST)
                CardId = None
                staff = getStaffByUser(user.id)
                if staff is not None and staff.card is not None:
                    CardId = staff.card.card_id
                res = {
                    "Id": user.id,
                    "Username":user.username,
                    "Password":None,
                    "Email":user.email,
                    "FirstName":user.first_name,
                    "LastName":user.last_name,
                    "CardId":CardId
                }
                if isAdmin is None or isAdmin is False:
                    ms = getCurrentMobileShift(obj["appId"], user.username)
                    if ms is None:
                        ms = MobileShift()
                        ms.staff = user.username
                        ms.app_id = obj["appId"]
                        ms.from_time = get_now_utc()
                        ms.save()
                return Response(res, status=status.HTTP_200_OK)
            except User.DoesNotExist:
                return Response({'detail': 'User does not exist'}, status=status.HTTP_400_BAD_REQUEST)
        else:
            return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)

class UserCardLoginView(generics.CreateAPIView):
    """
    Card Login Mobile
    """
    serializer_class = UserCardLoginSerializer

    def create(self, request, *args, **kwargs):
        serializer = self.get_serializer(data=request.DATA)
        if serializer.is_valid():
            obj = serializer.object
            try:
                user = User.objects.get(userprofile__card__card_id=obj['card_id'])
                isAdmin = obj["isAdmin"]
                if isAdmin:
                    if user.is_superuser is None or user.is_superuser is False:
                        return Response({'detail': 'User is not Admin role'}, status=status.HTTP_400_BAD_REQUEST)
                else:
                    if user.is_staff is None or user.is_staff is False:
                        return Response({'detail': 'User is not Staff role'}, status=status.HTTP_400_BAD_REQUEST)
                CardId = None
                res = {
                    "Id": user.id,
                    "Username": user.username,
                    "Password": None,
                    "Email": user.email,
                    "FirstName": user.first_name,
                    "LastName": user.last_name,
                    "CardId": CardId
                }
                if isAdmin is None or isAdmin is False:
                    ms = getCurrentMobileShift(obj["appId"], user.username)
                    if ms is None:
                        ms = MobileShift()
                        ms.staff = user.username
                        ms.app_id = obj["appId"]
                        ms.from_time = get_now_utc()
                        ms.save()
                return Response(res, status=status.HTTP_200_OK)
            except:
                return Response({'detail': 'Card is invalid'}, status=status.HTTP_400_BAD_REQUEST)
        else:
            return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)

import base64
def MakeBase64Images(filename):
    if filename[0] == '/':
        save_path = MEDIA_ROOT + filename
    else:
        save_path = MEDIA_ROOT + '/' + filename
    if not os.path.exists(save_path):
        return None
    with open(save_path, "rb") as image_file:
        encoded_string = base64.b64encode(image_file.read()).decode('utf-8')
        return encoded_string

@api_view(('GET',))
def get_parking_images(request,parking_id):
    """
            Get Parking Session Images
    """
    try:
        p = ParkingSession.objects.get(id = parking_id)
        check_in_images = p.check_in_images
        check_out_images = p.check_out_images
        if check_out_images:
            res = {
                "FrontIn": None,
                "BackIn": MakeBase64Images(check_in_images["back"]),
                "FrontOut": None,
                "BackOut": MakeBase64Images(check_out_images["back"]) if check_out_images else None,
            }
        else:
            res = {
                "FrontIn": None,
                "BackIn": MakeBase64Images(check_in_images["back"]),
                "FrontOut": None,
                "BackOut": None,
            }
    except:
        res = {
            "FrontIn": None,
            "BackIn": None,
            "FrontOut": None,
            "BackOut": None,
        }
    return Response(res, status.HTTP_200_OK)

@api_view(('GET',))
def revenue_report(request):
    try:
        from_date = request.GET.get("from")
        to_date = request.GET.get("to")
        type = int(request.GET.get("type" , 0))

        output = io.BytesIO()

        workbook = xlsxwriter.Workbook(output, {'in_memory': True})
        worksheet = workbook.add_worksheet("Báo Cáo Tổng") if type == 0 else workbook.add_worksheet("Báo Cáo Chi Tiết")
        bold = workbook.add_format({'bold': True})
        wrap = workbook.add_format()
        wrap.set_text_wrap()
        border = workbook.add_format()
        border.set_border()
        bold_border = workbook.add_format({'bold': True, 'border': 1})
        number_border_format = workbook.add_format({'num_format': '#,###', 'border': 1})
        number_bold_border_format = workbook.add_format({'num_format': '#,###', 'border': 1, 'bold': True})

        fr = datetime.datetime.strptime(from_date, "%Y-%m-%d")
        to = datetime.datetime.strptime(to_date, "%Y-%m-%d") + datetime.timedelta(hours =23, minutes = 59, seconds = 59)

        fromTime = fr.strftime("%d/%m%Y")
        toTime = to.strftime("%d/%m/%Y")
        titleTime = "Từ ngày %s đến ngày %s" % (fromTime, toTime)

        now = datetime.datetime.now()
        footerSign = "Thời điểm xuất: %s"%now.strftime("%d/%m/%Y %H:%M")
        if type==0:
            worksheet.write(1, 0, u"BÁO CÁO TỔNG", bold)
            worksheet.write(2, 0, titleTime)
            index = 4
            header, data, total = RevenueInfo(fr, to)
            worksheet.write_row(index, 0, header, bold_border)
            index += 1
            for d in data:
                worksheet.write_row(index, 0, d, number_border_format)
                index += 1
            worksheet.write_row(index, 0, total, number_bold_border_format)
            index += 2
            worksheet.write(index, 0, footerSign, bold)
        else:
            worksheet.write(1, 0, u"BÁO CÁO CHÍ TIẾT", bold)
            worksheet.write(2, 0, titleTime)
            index = 4
            header, data = RevenueDetailInfo(fr, to)
            worksheet.write_row(index, 0, header, bold_border)
            index += 1
            for d in data:
                worksheet.write_row(index, 0, d, number_border_format)
                index += 1
            index += 1
            worksheet.write(index, 0, footerSign, bold)


        workbook.close()

        # Reset con trỏ về đầu buffer
        output.seek(0)

        # Trả về HttpResponse thay vì FileResponse
        filename = "bao_cao_%s.xlsx" % datetime.datetime.now().strftime("%Y%m%d_%H%M%S")
        response = HttpResponse(
            output.getvalue(),
            content_type='application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
        )
        response['Content-Disposition'] = 'attachment; filename=%s' % filename

        return response
    except Exception as ex:
        return Response({"Error":ex}, status.HTTP_200_OK)

from django.contrib.auth import authenticate
from  parking.parking.models import ApiToken, RetailInvoice, ConsolidatedInvoice

@api_view(('POST',))
def login_api(request):
    if request.method == 'POST':
        try:
            data = json.loads(request.body)
            username = data.get('username')
            password = data.get('password')
        except:
            return HttpResponse(json.dumps({'error': 'Invalid data'}), content_type='application/json', status=400)

        user = authenticate(username=username, password=password)
        if user is not None:
            token, created = ApiToken.objects.get_or_create(user=user)
            return HttpResponse(json.dumps({'token': token.key}), content_type='application/json')
        else:
            return HttpResponse(json.dumps({'error': 'Invalid credentials'}), content_type='application/json', status=401)

    return HttpResponse(json.dumps({'error': 'POST required'}), content_type='application/json', status=405)

@api_view(('POST',))
def invoice_log_retail(request):
    if request.method == 'POST':
        try:
            auth_header = request.META.get('HTTP_AUTHORIZATION')
            if not auth_header or not auth_header.startswith('Token '):
                return HttpResponse(json.dumps({'error': 'Unauthorized'}), content_type='application/json', status=401)

            token_key = auth_header.split(' ')[1]
            try:
                token = ApiToken.objects.get(key=token_key)
            except ApiToken.DoesNotExist:
                return HttpResponse(json.dumps({'error': 'Invalid token'}), content_type='application/json', status=401)

            data = json.loads(request.body)
            from_str = '%s 00:00:00' % data.get('from')
            to_str = '%s 23:59:59' % data.get('to')
            try:
                from_date = datetime.datetime.strptime(from_str, '%Y-%m-%d %H:%M:%S')
                to_date = datetime.datetime.strptime(to_str, '%Y-%m-%d %H:%M:%S')
                qr =RetailInvoice.objects.filter(reqestedtime__lte = to_date, reqestedtime__gte = from_date, iscompleted= True)
                data = []
                for d in qr:
                    data.append({
                        "Request": json.dumps(d.contentrequest, ensure_ascii=False),
                        "Response": json.dumps(d.contentresponse, ensure_ascii=False),
                        'RequestedTime': d.eqestedtime.strftime('%Y-%m-%d %H:%M:%S')
                    })
                return HttpResponse(json.dumps({'Logs': data}), content_type='application/json')
            except:
                return HttpResponse(json.dumps({'error': 'Invalid date format, use YYYY-MM-DD'}),
                                    content_type='application/json', status=400)
        except:
            return HttpResponse(json.dumps({'error': 'Invalid data'}), content_type='application/json', status=400)



    return HttpResponse(json.dumps({'error': 'POST required'}), content_type='application/json', status=405)

@api_view(('POST',))
def invoice_log_consoliddate(request):
    if request.method == 'POST':
        try:
            auth_header = request.META.get('HTTP_AUTHORIZATION')
            if not auth_header or not auth_header.startswith('Token '):
                return HttpResponse(json.dumps({'error': 'Unauthorized'}), content_type='application/json', status=401)

            token_key = auth_header.split(' ')[1]
            try:
                token = ApiToken.objects.get(key=token_key)
            except ApiToken.DoesNotExist:
                return HttpResponse(json.dumps({'error': 'Invalid token'}), content_type='application/json', status=401)

            data = json.loads(request.body)
            from_str = '%s 00:00:00'%data.get('from')
            to_str = '%s 23:59:59'%data.get('to')
            try:
                from_date = datetime.datetime.strptime(from_str, '%Y-%m-%d %H:%M:%S')
                to_date = datetime.datetime.strptime(to_str, '%Y-%m-%d %H:%M:%S')
                qr =ConsolidatedInvoice.objects.filter(reqestedtime__lte = to_date, reqestedtime__gte = from_date, iscompleted= True)
                data = []
                for d in qr:
                    data.append({
                        "Request": json.dumps(d.contentrequest, ensure_ascii=False),
                        "Response": json.dumps(d.contentresponse, ensure_ascii=False),
                        'RequestedTime': d.eqestedtime.strftime('%Y-%m-%d %H:%M:%S')
                    })
                return HttpResponse(json.dumps({'Logs': data}), content_type='application/json')
            except:
                return HttpResponse(json.dumps({'error': 'Invalid date format, use YYYY-MM-DD'}),
                                    content_type='application/json', status=400)
        except:
            return HttpResponse(json.dumps({'error': 'Invalid data'}), content_type='application/json', status=400)



    return HttpResponse(json.dumps({'error': 'POST required'}), content_type='application/json', status=405)
