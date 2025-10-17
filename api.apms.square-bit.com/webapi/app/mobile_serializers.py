# -*- coding: utf-8 -*-
from rest_framework import serializers
from django.db.models import Q
import  datetime
from utils import get_now_utc, datetime2timestamp
from parking.models import Customer, Card, Terminal, Lane, Camera, ParkingSession, ParkingFeeSession, CheckInImage, UserProfile , UserShift, CardType,\
    VehicleType, TerminalGroup, ClaimPromotionTenant, ClaimPromotionVoucher, ClaimPromotion, ClaimPromotionBill, ClaimPromotionCoupon, ClaimPromotionV2, \
    ClaimPromotionBillV2, ClaimPromotionCouponV2, MobileShift, VehicleRegistration

def getImgesUrls(p, baseUrl):
    imagesIn = p.check_in_images
    imagesOut = p.check_out_images
    res = {
        "FrontIn": '%simages/%s'%(baseUrl,imagesIn["front"]) if imagesIn is not None else None,
        "BackIn": '%simages/%s' % (baseUrl, imagesIn["back"]) if imagesIn is not None else None,
        "FrontOut": '%simages/%s' % (baseUrl, imagesOut["front"]) if imagesOut is not None else None,
        "BackOut": '%simages/%s' % (baseUrl, imagesOut["back"]) if imagesOut is not None else None,
    }
    return res

def getCard(cardId):
    try:
        card = Card.objects.get(card_id = cardId)
        vehicle_registration = VehicleRegistration.objects.filter(card=card)
        if vehicle_registration:
            vehicle_registration = vehicle_registration[0]
            return card, vehicle_registration
        else:
            return card, None
    except:
        return None, None

def makeTheSmaeCard(cardId, vehicleType, vehiclenumber):
    card, regis = getCard(cardId)
    if card is None:
        try:
            regis = VehicleRegistration.objects.filter(vehicle_number = vehiclenumber, card__isnull = False)
            if regis.count() == 1:
                return regis[0].card, regis[0]
            else:
                session = ParkingSession.objects.filter(vehicle_number = vehiclenumber, check_out_time__isnull = True, card__isnull = False)
                if session.count() == 1:
                    card = session[0].card
                    vehicle_registration = VehicleRegistration.objects.filter(card=card)
                    if vehicle_registration:
                        vehicle_registration = vehicle_registration[0]
                        return card, vehicle_registration
                    else:
                        return  card, None
                else:
                    card = Card()
                    card.card_id = cardId
                    card.card_label = cardId
                    card.card_type = 0
                    card.vehicle_type = vehicleType
                    card.status = 1
                    card.save()
                    return card, None
        except Exception as ex:
            return  None, None
    else:
        return card, regis


def getCardType(card):
    try:
        return CardType.objects.get(id = card.card_type)
    except:
        return None

def getVehicleType(card):
    try:
        return VehicleType.objects.get(id = card.card_type)
    except:
        return None

def getMonthlyCard():
    try:
        rgs = VehicleRegistration.objects.filter(card__isnull = False).values_list('card__id', flat = True).distinct()
        return rgs
    except:
        return [0]

def getCurrentMobileShift(appId, username):
    try:
        ms = MobileShift.objects.filter(app_id = appId, staff = username, to_time__isnull = True)
        if ms:
            return ms[0]
    except:
        return None

def getStaffByUser(user_id):
    try:
        return UserProfile.objects.get(user__id = user_id)
    except:
        return None

def getStaffByCard(card_id):
    try:
        card = Card.objects.get(card_id= card_id)
        return UserProfile.objects.get(card = card)
    except:
        return None

def getPosition(appId):
    try:
        return Terminal.objects.get(terminal_id = appId)
    except:
        return None

def getPosition1(appId):
    try:
        terminal = Terminal.objects.get(terminal_id = appId)
        return {"Id": terminal.id, "GateName":terminal.name}
    except:
        return None

def getLane(appId):
    terminal = getPosition(appId)
    if terminal is None:
        return None
    lanes = Lane.objects.filter(terminal = terminal)
    if lanes:
        return lanes[0]
    return None

def getLane1(appId):
    terminal = getPosition(appId)
    if terminal is None:
        return None
    lanes = Lane.objects.filter(terminal = terminal)
    if lanes:
        l = lanes[0]
        return {
            "Id": l.id,
            "LaneName": l.name,
            "GatePosition":{"Id": l.terminal.id, "GateName":l.terminal.name}
        }
    return None

MobileSearchType = (
    (1, "Tồn bãi"),
    (2, "Trong ngày"),
    (3, "Vào trong ngày"),
    (4, "Ra trong ngày"),
    (5, "Trong vòng 3 ngày"),
    (6, "Vào trong vòng 3 ngày"),
    (7, "Ra trong vòng 3 ngày"),
    (8, "Trong vòng 7 ngày"),
    (9, "Vào trong vòng 7 ngày"),
    (10, "Ra trong vòng 7 ngày"),
)


def getSeachTime(searchType):
    if searchType ==1 or searchType>10:
        return None, None
    now = get_now_utc()
    first = now.replace(hour = 0, minute =0, second =0) - datetime.timedelta(hours =7)
    last = first + datetime.timedelta(hours =23, minutes = 59, seconds = 59)
    if searchType >=2 and searchType <=4:
        return first, last
    elif searchType >=5 and searchType <=7:
        return first - datetime.timedelta(days = 3), last
    elif searchType >=8 and searchType <=10:
        return first - datetime.timedelta(days = 7), last
    else:
        return None, None

def GetParkingSearch(searchType, cardLabel):
    fr, to = getSeachTime(searchType)
    pks = ParkingSession.objects.all()
    if cardLabel is not None:
        pks = pks.filter(card__card_label = cardLabel)
    if searchType == 1:
        pks = pks.filter(check_out_time = None)
    elif searchType == 2 or searchType == 5 or searchType == 8:
        pks = pks.filter(
            Q(check_in_time__gte=fr, check_in_time__lte=to) |
            Q(check_out_time__gte=fr, check_out_time__lte=to))
    elif searchType == 3 or searchType == 6 or searchType == 9:
        pks = pks.filter(check_in_time__gte=fr, check_in_time__lte = to)
    elif searchType == 4 or searchType == 7 or searchType == 10:
        pks = pks.filter(check_out_time__gte=fr, check_out_time__lte = to)
    else:
        pks = None
    return pks

class SearchSerializer(serializers.Serializer):
    userId = serializers.IntegerField(help_text='Operator')
    searchType = serializers.ChoiceField(help_text="Request type", choices=MobileSearchType, default=1)
    cardLabel = serializers.CharField(help_text='Card Label', required=False)

class CheckCusSerializer(serializers.Serializer):
    appId = serializers.CharField(help_text='Unique Mobile App ID')
    userId = serializers.IntegerField(help_text='Operator')
    cardId = serializers.CharField(help_text="Card ID")
    frontThumb = serializers.FileField(help_text='Front thumbnail', required=False)
    backThumb = serializers.FileField(help_text='Back thumbnail', required=False)
    vehicleType = serializers.IntegerField(help_text='Vehicle Type', required = False )
    vehicleNumber = serializers.CharField(help_text='Vehicle number', required=False)
    anprVehicleNumber = serializers.CharField(help_text='ANPR vehicle number', required=False)
    checkMode = serializers.IntegerField(help_text='Check mode')
    asthesameCard = serializers.IntegerField(help_text = 'Vehicle Number as the same card', required=False)

class EditParkingSerializer(serializers.Serializer):
    parkingId = serializers.IntegerField(help_text='Parking Session ID')
    vehicleType = serializers.IntegerField(help_text='Vehicle Type', required = False )
    vehicleNumber = serializers.CharField(help_text='Vehicle number' , required = False)

class OutSessionExceptionSerializer(serializers.Serializer):
    appId = serializers.CharField(help_text='Unique Mobile App ID')
    userId = serializers.IntegerField(help_text='Operator')
    parkingId = serializers.IntegerField(help_text='Parking Session ID')
    reson = serializers.CharField(help_text='Exception Reson')

class MobileShiftSerializer(serializers.Serializer):
    appId = serializers.CharField(help_text='Unique Mobile App ID')
    username = serializers.CharField(help_text='Mobile App Name')
    actualFee = serializers.IntegerField(help_text='Actual Fee', required=False)
    ajustmentFee = serializers.IntegerField(help_text='Ajustment Fee', required=False)
    note = serializers.CharField(help_text='Note', required=False)
    isApply = serializers.BooleanField(help_text='Is apply')

class CertificationSerializer(serializers.Serializer):
    appId = serializers.CharField(help_text='Unique Mobile App ID')
    appName = serializers.CharField(help_text='Mobile App Name')
    urlBase1 = serializers.CharField(help_text='Url Base 1')
    urlBase2 = serializers.CharField(help_text='Url Base 2')
    username = serializers.CharField(help_text='Username')
    password = serializers.CharField(help_text='Password')

class UserLoginSerializer(serializers.Serializer):
    appId = serializers.CharField(help_text='Unique Mobile App ID')
    username = serializers.CharField(help_text='Username')
    password = serializers.CharField(help_text='Password')
    isAdmin = serializers.BooleanField(help_text='Is Admin')

class UserCardLoginSerializer(serializers.Serializer):
    appId = serializers.CharField(help_text='Unique Mobile App ID')
    cardId = serializers.CharField(help_text='Card ID')
    isAdmin = serializers.BooleanField(help_text='Is Admin')

class RetailInvoiceSerializer(serializers.Serializer):
    parking_id = serializers.IntegerField(help_text=u'Mã phiên gửi xe')
    fee = serializers.IntegerField(help_text=u'Số tiền gửi xe')
    completed = serializers.BooleanField(help_text=u'Đã hoàn thành phiên', required=False)
    has_buyer = serializers.BooleanField(help_text=u'Có thông tin khách mua', required=False)
    buyer_code = serializers.CharField(help_text=u'Mã khách hàng', required=False)
    buyer_name = serializers.CharField(help_text=u'Tên khách hàng', required=False)
    legal_name = serializers.CharField(help_text=u'Tên đơn vị', required=False)
    taxcode = serializers.CharField(help_text=u'Mã số thuế', required=False)
    phone = serializers.CharField(help_text=u'SĐT', required=False)
    email = serializers.CharField(help_text=u'Email', required=False)
    address = serializers.CharField(help_text=u'Địa chỉ', required=False)
    receiver_name = serializers.CharField(help_text=u'Tên người nhận', required=False)
    receiver_emails = serializers.CharField(help_text=u'Địa chỉ các email nhận (cách nhau dấu ;)', required=False)
