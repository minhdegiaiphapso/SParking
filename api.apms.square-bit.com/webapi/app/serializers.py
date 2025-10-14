# -*- coding: utf-8 -*-
from rest_framework import serializers
from utils import get_now_utc, datetime2timestamp
from parking.parking.models import Customer, Card, Terminal, Lane, Camera, ParkingSession, ParkingFeeSession, CheckInImage, UserProfile , UserShift, CardType,\
    VehicleType, TerminalGroup, ClaimPromotionTenant, ClaimPromotionVoucher, ClaimPromotion, ClaimPromotionBill, ClaimPromotionCoupon, ClaimPromotionV2, ClaimPromotionBillV2, ClaimPromotionCouponV2


time_out_threshold = 1000


class TerminalGroupListSerializer(serializers.ModelSerializer):
    id = serializers.IntegerField(source='id', help_text='ID')
    name = serializers.CharField(source='name', help_text='Name')

    class Meta:
        model = TerminalGroup
        fields = ('id', 'name')

class CardCollectedSerializer(serializers.Serializer):
    card_id = serializers.CharField(help_text='Card Id')
    note = serializers.CharField(help_text='note')
    
class CardListSerializer(serializers.ModelSerializer):
    card_id = serializers.CharField(source='card_id', help_text='ID characters of card')
    status = serializers.IntegerField(source='status', help_text='0: Disabled | 1: Enabled | 2: Lost')
    card_label = serializers.CharField(source='card_label', help_text='Printed code characters label of card')
    vehicle_type = serializers.IntegerField(source='vehicle_type', help_text='Vehicle type of card')
    card_type = serializers.IntegerField(source='card_type', help_text='0: Unknown | 1: Guest | 2: Staff | 3: Parking Staff | 4: Admin')

    class Meta:
        model = Card
        fields = ('card_id', 'status', 'card_label', 'vehicle_type', 'card_type')


class CardDetailSerializer(CardListSerializer):
    card_id = serializers.CharField(source='card_id', help_text='ID characters of card', required=False, read_only=True)


class TerminalListSerializer(serializers.ModelSerializer):
    terminal_id = serializers.CharField(source='terminal_id', help_text='ID characters of terminal')
    name = serializers.CharField(source='name', help_text='Name of terminal')
    id = serializers.IntegerField(source='id', help_text='System ID of terminal', required=False, read_only=True)
    status = serializers.IntegerField(source='status', help_text='0 - Disabled | 1 - Enabled')
    version = serializers.CharField(source='version', help_text='Version of client app on this terminal', required=False)
    ip = serializers.CharField(source='ip', help_text='IP of terminal', required=False)
    timeout = serializers.SerializerMethodField('is_time_out')
    last_check_health_time = serializers.SerializerMethodField('get_last_check_timestamp')

    def is_time_out(self, obj):
        delta = get_now_utc() - obj.last_check_health
        if delta.total_seconds() > 300:
            return True
        return False

    def get_last_check_timestamp(self, obj):
        return datetime2timestamp(obj.last_check_health)

    class Meta:
        model = Terminal
        fields = ('id', 'terminal_id', 'name', 'status', 'version', 'ip', 'timeout', 'last_check_health_time')


class TerminalDetailSerializer(TerminalListSerializer):
    terminal_id = serializers.CharField(source='terminal_id', help_text='ID characters of terminal', required=False, read_only=True)


class LaneSerializer(serializers.ModelSerializer):
    id = serializers.IntegerField(source='id', help_text='ID of lane', required=False, read_only=True)
    name = serializers.CharField(source='name', help_text='Name of lane')
    vehicle_type = serializers.IntegerField(source='vehicle_type', help_text='1 - Car | 2 - Bike')
    direction = serializers.IntegerField(source='direction', help_text='0 - In | 1 - Out')
    enabled = serializers.BooleanField(source='enabled', help_text='State of lane')
    terminal_id = serializers.PrimaryKeyRelatedField(source='terminal', help_text='Terminal ID of lane')

    class Meta:
        model = Lane
        fields = ('id', 'name', 'vehicle_type', 'direction', 'enabled', 'terminal_id')


class CameraSerializer(serializers.ModelSerializer):
    id = serializers.IntegerField(source='id', help_text='ID of camera', required=False, read_only=True)
    name = serializers.CharField(source='name', help_text='Name of camera')
    ip = serializers.CharField(source='ip', help_text='Network IP of camera')
    position = serializers.IntegerField(source='position', help_text='Position of camera')
    direction = serializers.IntegerField(source='direction', help_text='0 - In | 1 - Out')
    serial_number = serializers.CharField(source='serial_number', help_text='Serial Number of camera')
    lane_id = serializers.PrimaryKeyRelatedField(source='lane', help_text='Lane ID of camera')

    class Meta:
        model = Camera
        fields = ('id', 'name', 'ip', 'lane_id', 'position', 'direction', 'serial_number')


def has_param(name, obj):
        return name in obj and obj[name]


class CardCheckInSerializer(serializers.Serializer):
    card_label = serializers.CharField(help_text='Card code', required=False, read_only=True)
    card_type = serializers.IntegerField(help_text='Card type', required=False, read_only=True)
    terminal_id = serializers.IntegerField(help_text='Terminal ID of check in process', required=False)
    lane_id = serializers.IntegerField(help_text='Lane ID of check in process', required=False)
    operator_id = serializers.IntegerField(help_text='Operator ID makes check in process', required=False)
    vehicle_type = serializers.IntegerField(help_text='1 - Car | 2 - Bike', required=False)
    vehicle_number = serializers.CharField(help_text='Vehicle number', required=False)
    alpr_vehicle_number = serializers.CharField(help_text='ALPR vehicle number', required=False)
    front_image_path = serializers.CharField(help_text='Path of check in front image', required=False, read_only=True)
    back_image_path = serializers.CharField(help_text='Path of check in back image', required=False, read_only=True)
    front_thumb = serializers.FileField(help_text='Check in front thumbnail', required=False)
    back_thumb = serializers.FileField(help_text='Check in back thumbnail', required=False)
    image_hosts = serializers.CharField(help_text='List hosts stores images', required=False, read_only=True)
    entry_check = serializers.BooleanField(help_text='Check number of entry', required=False)
    prefix_vehicle_number = serializers.CharField(help_text='prefix_vehicle_number', required=False)
    extra1_image_path = serializers.CharField(help_text='Path of extra1 in image', required=False, read_only=True)
    extra2_image_path = serializers.CharField(help_text='Path of extra2 in image', required=False, read_only=True)
    extra1_thumb = serializers.FileField(help_text='extra1 in thumbnail', required=False)
    extra2_thumb = serializers.FileField(help_text='extra2 in thumbnail', required=False)
    use_vehicle_type_from_card = serializers.BooleanField(help_text='option get vehicletype from card', required=False)
    def validate(self, attrs):
        request = self.context['request']
        if request.method == 'POST':
            if not has_param('terminal_id', attrs):
                raise serializers.ValidationError({'terminal_id': ['Field is required']})
            if not has_param('lane_id', attrs):
                raise serializers.ValidationError({'lane_id': ['Field is required']})
            if not has_param('operator_id', attrs):
                raise serializers.ValidationError({'operator_id': ['Field is required']})
            if not has_param('vehicle_type', attrs):
                raise serializers.ValidationError({'vehicle_type': ['Field is required']})
            if not has_param('vehicle_number', attrs):
                raise serializers.ValidationError({'vehicle_number': ['Field is required']})
            if not has_param('alpr_vehicle_number', attrs):
                raise serializers.ValidationError({'alpr_vehicle_number': ['Field is required']})
            # if not has_param('front_image_path', attrs):
            #     raise serializers.ValidationError({'front_image_path': ['Field is required']})
            # if not has_param('back_image_path', attrs):
            #     raise serializers.ValidationError({'back_image_path': ['Field is required']})
            if not has_param('front_thumb', attrs):
                raise serializers.ValidationError({'front_thumb': ['Field is required']})
            if not has_param('back_thumb', attrs):
                raise serializers.ValidationError({'back_thumb': ['Field is required']})
        # elif request.method == 'PUT' or request.method == 'PATCH':
        #     if has_param('front_image_path', attrs) or has_param('back_image_path', attrs):
        #         if not has_param('front_image_path', attrs):
        #             raise serializers.ValidationError({'front_image_path': ['Field is required']})
        #         if not has_param('back_image_path', attrs):
        #             raise serializers.ValidationError({'back_image_path': ['Field is required']})
        #         if not has_param('front_thumb', attrs):
        #             raise serializers.ValidationError({'front_thumb': ['Field is required']})
        #         if not has_param('back_thumb', attrs):
        #             raise serializers.ValidationError({'back_thumb': ['Field is required']})
        return attrs


class BasicCheckInSessionSerializer(serializers.ModelSerializer):
    lane_id = serializers.IntegerField(source='check_in_lane_id')
    operator_id = serializers.IntegerField(source='check_in_operator_id')
    vehicle_type = serializers.IntegerField(source='vehicle_type', help_text='1 - Car | 2 - Bike')
    vehicle_number = serializers.CharField(source='vehicle_number', help_text='Vehicle register number')
    alpr_vehicle_number = serializers.CharField(source='check_in_alpr_vehicle_number', help_text='ALPR vehicle number')
    front_image_path = serializers.SerializerMethodField('get_front_image_path')
    back_image_path = serializers.SerializerMethodField('get_back_image_path')
    check_in_time = serializers.SerializerMethodField('get_check_in_time')

    def get_front_image_path(self, obj):
        return obj.check_in_images['front']

    def get_back_image_path(self, obj):
        return obj.check_in_images['back']

    def get_check_in_time(self, obj):
        return datetime2timestamp(obj.check_in_time)

    class Meta:
        model = ParkingSession
        fields = ('lane_id', 'operator_id', 'vehicle_type',
                  'vehicle_number', 'alpr_vehicle_number', 'front_image_path', 'back_image_path', 'check_in_time')


class CheckInSessionSerializer(BasicCheckInSessionSerializer):
    card_id = serializers.CharField(source='card.card_id', help_text='Card ID')
    card_label = serializers.CharField(source='card.card_label', help_text='Card code')
    card_type = serializers.IntegerField(source='card.card_type', help_text='Card type')
    terminal_id = serializers.SerializerMethodField('get_terminal_id')

    def get_terminal_id(self, obj):
        return obj.check_in_lane.terminal_id

    class Meta:
        model = ParkingSession
        fields = ('card_id', 'card_label', 'terminal_id', 'lane_id', 'operator_id', 'vehicle_type',
                  'vehicle_number', 'alpr_vehicle_number', 'front_image_path', 'back_image_path', 'check_in_time')


class ParkingSessionSerializer(serializers.ModelSerializer):
    id = serializers.IntegerField(source='id', help_text='ID of parking session')
    card_id = serializers.CharField(source='card.card_id', help_text='Card ID')
    card_label = serializers.CharField(source='card.card_label', help_text='Card code')
    card_type = serializers.IntegerField(source='card.card_type', help_text='Card type')
    vehicle_type = serializers.IntegerField(source='vehicle_type', help_text='Vehicle type')
    vehicle_number = serializers.CharField(source='vehicle_number', help_text='Vehicle register number')
    check_in_lane = serializers.CharField(source='check_in_lane.name')
    check_in_alpr_vehicle_number = serializers.CharField(source='check_in_alpr_vehicle_number', help_text='Check in ALPR vehicle number')
    check_in_front_image = serializers.SerializerMethodField('get_check_in_front_image')
    check_in_back_image = serializers.SerializerMethodField('get_check_in_back_image')
    check_in_time = serializers.SerializerMethodField('get_check_in_time')
    check_out_lane = serializers.CharField(source='check_out_lane.name')
    check_out_alpr_vehicle_number = serializers.CharField(source='check_out_alpr_vehicle_number', help_text='Check out ALPR vehicle number')
    check_out_front_image = serializers.SerializerMethodField('get_check_out_front_image')
    check_out_back_image = serializers.SerializerMethodField('get_check_out_back_image')
    check_out_time = serializers.SerializerMethodField('get_check_out_time')

    def get_check_in_front_image(self, obj):
        return obj.check_in_images['front']

    def get_check_in_back_image(self, obj):
        return obj.check_in_images['back']

    def get_check_out_front_image(self, obj):
        if obj.check_out_images:
            return obj.check_out_images['front']
        return None

    def get_check_out_back_image(self, obj):
        if obj.check_out_images:
            return obj.check_out_images['back']
        return None

    def get_check_in_time(self, obj):
        return datetime2timestamp(obj.check_in_time)

    def get_check_out_time(self, obj):
        if obj.check_out_time:
            return datetime2timestamp(obj.check_out_time)
        return -1

    class Meta:
        model = ParkingSession
        fields = ('id', 'card_id', 'card_label', 'card_type', 'vehicle_type', 'vehicle_number',
                  'check_in_lane', 'check_in_alpr_vehicle_number', 'check_in_front_image', 'check_in_back_image', 'check_in_time',
                  'check_out_lane', 'check_out_alpr_vehicle_number', 'check_out_front_image', 'check_out_back_image', 'check_out_time',)

#2018Jun06
from django.db import connections
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

        rows = cursor.fetchall()
        retVal = []
        for row in rows:
            retVal.append(row)
        return retVal
def getvoucher(sID):
    try:
        util = Utilities()
        datares = util.Query("getparking_voucher", sID)
        vc=[]
        for r in datares:
            vc.append({
                'vouchertype':str(r[2]),
                'amountvoucher':r[3],
                'actualfee': r[5],
                'fee':r[4]
            })
        if  len(vc)>0:
            return  vc[0]
        return  None
    except Exception as e:
        return None

class ParkingSessionSearchSerializer_new(serializers.ModelSerializer):
    id = serializers.IntegerField(help_text='ID of parking session')
    total = serializers.IntegerField(help_text='Total row')
    card_id = serializers.CharField(help_text='Card ID')
    card_label = serializers.CharField(help_text='Card code')
    card_type = serializers.IntegerField(help_text='Card type')
    vehicle_type = serializers.IntegerField(help_text='Vehicle type')
    vehicle_number = serializers.CharField(help_text='Vehicle register number')
    check_in_lane = serializers.SerializerMethodField('get_check_in_lane')
    check_in_alpr_vehicle_number = serializers.CharField(help_text='Check in ALPR vehicle number')
    check_in_front_image = serializers.SerializerMethodField('get_check_in_front_image')
    check_in_back_image = serializers.SerializerMethodField('get_check_in_back_image')
    check_in_time = serializers.SerializerMethodField('get_check_in_time')
    check_out_lane = serializers.SerializerMethodField('get_check_out_lane')
    check_out_alpr_vehicle_number = serializers.CharField(help_text='Check out ALPR vehicle number')
    check_out_front_image = serializers.SerializerMethodField('get_check_out_front_image')
    check_out_back_image = serializers.SerializerMethodField('get_check_out_back_image')
    check_out_time = serializers.SerializerMethodField('get_check_out_time')
    check_in_extra1_image = serializers.SerializerMethodField('get_check_in_extra1_image')
    check_in_extra2_image = serializers.SerializerMethodField('get_check_in_extra2_image')
    check_out_extra1_image = serializers.SerializerMethodField('get_check_out_extra1_image')
    check_out_extra2_image = serializers.SerializerMethodField('get_check_out_extra2_image')
    fee = serializers.SerializerMethodField('get_fee')
    voucher = serializers.SerializerMethodField('get_voucher')
    def get_voucher(self, obj):
        vc = getvoucher(obj["id"])
        if vc:
            return vc
    def get_fee(self, obj):
        parking_fee_session = ParkingFeeSession.objects.all().filter(parking_session_id=obj["id"],                                                               session_type='OUT')
        fee = parking_fee_session[0].parking_fee if parking_fee_session else 0
        return fee
    def get_check_in_extra1_image(self, obj):
        if obj["check_in_images"]:
            if 'extra1' in obj["check_in_images"]:
                return obj["check_in_images"]["extra1"]
            return  None
        return None

    def get_check_in_extra2_image(self, obj):
        if obj["check_in_images"]:
            if 'extra2' in obj["check_in_images"]:
                return obj["check_in_images"]["extra2"]
            return None
        return None
    def get_check_out_extra1_image(self, obj):
        if obj["check_out_images"]:
            if 'extra1' in obj["check_out_images"]:
                return obj["check_out_images"]["extra1"]
            return  None
        return None

    def get_check_out_extra2_image(self, obj):
        if obj["check_out_images"]:
            if 'extra2' in obj["check_out_images"]:
                return obj["check_out_images"]["extra2"]
            return None
        return None
    def get_check_in_front_image(self, obj):
        if obj["check_in_images"]:
            return obj["check_in_images"]["front"]
        return  None
    def get_check_in_back_image(self, obj):
        if obj["check_in_images"]:
            return obj["check_in_images"]["back"]
        return None
    def get_check_out_front_image(self, obj):
        if obj["check_out_images"]:
            return obj["check_out_images"]["front"]
        return None
    def get_check_out_back_image(self, obj):
        if obj["check_out_images"]:
            return obj["check_out_images"]["back"]
        return None
    def get_check_in_time(self, obj):
        return datetime2timestamp(obj["check_in_time"])
    def get_check_out_time(self, obj):
        if obj["check_out_time"]:
            return datetime2timestamp(obj["check_out_time"])
        return -1
    def get_check_in_lane(self, obj):
        if obj["check_in_lane_id"]:
            return str(obj["check_in_lane_id"])
        return None
    def get_check_out_lane(self, obj):
        if obj["check_out_lane_id"]:
            return str(obj["check_out_lane_id"])
        return None
    class Meta:
        model = ParkingSession
        fields = ('id', 'card_id', 'card_label', 'card_type', 'fee','voucher', 'vehicle_type', 'vehicle_number','total',
                  'check_in_lane', 'check_in_alpr_vehicle_number', 'check_in_front_image', 'check_in_back_image',
                  'check_in_time','check_in_extra1_image','check_in_extra2_image','check_out_extra1_image','check_out_extra2_image',
                  'check_out_lane', 'check_out_alpr_vehicle_number', 'check_out_front_image', 'check_out_back_image',
                  'check_out_time',)
#2018Jun06
class ParkingSessionSearchSerializer(serializers.ModelSerializer):
    id = serializers.IntegerField(source='id', help_text='ID of parking session')
    card_id = serializers.CharField(source='card.card_id', help_text='Card ID')
    card_label = serializers.CharField(source='card.card_label', help_text='Card code')
    card_type = serializers.IntegerField(source='card.card_type', help_text='Card type') 
    vehicle_type = serializers.IntegerField(source='vehicle_type', help_text='Vehicle type')
    vehicle_number = serializers.CharField(source='vehicle_number', help_text='Vehicle register number')
    check_in_lane = serializers.SerializerMethodField('get_check_in_lane')
    check_in_alpr_vehicle_number = serializers.CharField(source='check_in_alpr_vehicle_number', help_text='Check in ALPR vehicle number')
    check_in_front_image = serializers.SerializerMethodField('get_check_in_front_image')
    check_in_back_image = serializers.SerializerMethodField('get_check_in_back_image')
    check_in_time = serializers.SerializerMethodField('get_check_in_time')
    check_out_lane = serializers.SerializerMethodField('get_check_out_lane')
    check_out_alpr_vehicle_number = serializers.CharField(source='check_out_alpr_vehicle_number', help_text='Check out ALPR vehicle number')
    check_out_front_image = serializers.SerializerMethodField('get_check_out_front_image')
    check_out_back_image = serializers.SerializerMethodField('get_check_out_back_image')
    check_out_time = serializers.SerializerMethodField('get_check_out_time')
    fee = serializers.SerializerMethodField('get_fee')

    def get_fee(self, obj):
        parking_fee_session = ParkingFeeSession.objects.all().filter(parking_session_id=obj.id,
                                                                     session_type='OUT')
        fee = parking_fee_session[0].parking_fee if parking_fee_session else 0
        return fee


    def get_check_in_front_image(self, obj):
        return obj.check_in_images['front']

    def get_check_in_back_image(self, obj):
        return obj.check_in_images['back']

    def get_check_out_front_image(self, obj):
        if obj.check_out_images:
            return obj.check_out_images['front']
        return None

    def get_check_out_back_image(self, obj):
        if obj.check_out_images:
            return obj.check_out_images['back']
        return None

    def get_check_in_time(self, obj):
        return datetime2timestamp(obj.check_in_time)

    def get_check_out_time(self, obj):
        if obj.check_out_time:
            return datetime2timestamp(obj.check_out_time)
        return -1

    def get_check_in_lane(self, obj):
        if obj.check_in_lane_id:
            return str(obj.check_in_lane_id)
        return None

    def get_check_out_lane(self, obj):
        if obj.check_out_lane_id:
            return str(obj.check_out_lane_id)
        return None

    class Meta:
        model = ParkingSession
        fields = ('id', 'card_id', 'card_label', 'card_type', 'fee', 'vehicle_type', 'vehicle_number',
                  'check_in_lane', 'check_in_alpr_vehicle_number', 'check_in_front_image', 'check_in_back_image', 'check_in_time',
                  'check_out_lane', 'check_out_alpr_vehicle_number', 'check_out_front_image', 'check_out_back_image', 'check_out_time',)

#2018Dec13
class ParkingSessionSlotSerializer(serializers.Serializer):
    slot_id = serializers.IntegerField(help_text=u'Mã ô đậu')
    name = serializers.CharField(help_text=u'Tên ô đậu')
    prefix = serializers.CharField(help_text=u'Tiền tố gởi cổng Com')
    suffixes = serializers.CharField(help_text=u'Hậu tố gởi cổng Com')
    numlength = serializers.IntegerField(help_text=u'Chiều dài chuỗi số gởi cổng Com')
    hascheckkey = serializers.IntegerField(help_text=u'Có gởi kí số kiểm tra')
    slottotal = serializers.IntegerField(help_text=u'Tổng số ô đậu')
    currentslot = serializers.IntegerField(help_text=u'Số ô đang có xe')
    plankslot = serializers.IntegerField(help_text=u'Số ô trống')
    vehicletypesid = serializers.CharField(help_text=u'Mã loại phương tiện')
    vehicletypesidname = serializers.CharField(help_text=u'Tên loại phương tiện')
#2018Dec13
class CardCheckOutSerializer(serializers.Serializer):
    terminal_id = serializers.IntegerField(help_text='Terminal ID of check out process')
    lane_id = serializers.IntegerField(help_text='Lane ID of check out process')
    operator_id = serializers.IntegerField(help_text='Operator ID makes check out process')
    alpr_vehicle_number = serializers.CharField(help_text='ALPR vehicle number')
    front_thumb = serializers.FileField(help_text='Check out front thumbnail')
    back_thumb = serializers.FileField(help_text='Check out back thumbnail')
    parking_fee = serializers.IntegerField(help_text='Phi gui xe vang lai',required=False, blank=True)
    check_out_time = serializers.DateTimeField(help_text='Check out time', format="%Y-%d-%m %H:%M:%s")
    parking_fee_details = serializers.CharField(help_text='Chi tiet tinh phi gui xe vang lai',required=False, blank=True)
    extra1_thumb = serializers.FileField(help_text='Extra1 out thumbnail', required=False)
    extra2_thumb = serializers.FileField(help_text='Extra2 out thumbnail', required=False)

class CardExceptionCheckOutSerializer(serializers.Serializer):
    terminal_id = serializers.IntegerField(help_text='Terminal ID of check out process')
    lane_id = serializers.IntegerField(help_text='Lane ID of check out process')
    operator_id = serializers.IntegerField(help_text='Operator ID makes check out process')
    notes = serializers.CharField(help_text='Notes and reason')
    parkingfee = serializers.IntegerField(help_text='Phi')
    lock_card = serializers.IntegerField(help_text='0 - Nonlock<br/>1 - Lock')


class CheckInImageHostSerializer(serializers.ModelSerializer):
    id = serializers.IntegerField(source='id', help_text='System ID of terminal')
    terminal_id = serializers.CharField(source='terminal_id', help_text='ID characters of terminal', read_only=True, required=False)
    name = serializers.CharField(source='name', help_text='Name of terminal', read_only=True, required=False)
    status = serializers.IntegerField(source='status', help_text='0 - Disabled | 1 - Enabled', read_only=True, required=False)
    ip = serializers.CharField(source='ip', help_text='IP of terminal', required=False, read_only=True)
    timeout = serializers.SerializerMethodField('is_time_out')

    def is_time_out(self, obj):
        delta = get_now_utc() - obj.last_check_health
        if delta.total_seconds() > 300:
            return True
        return False

    class Meta:
        model = Terminal
        fields = ('id', 'terminal_id', 'name', 'status', 'ip', 'timeout')


class UserLoginSerializer(serializers.Serializer):
    id = serializers.IntegerField(help_text='ID of user', required=False, read_only=True)
    is_staff = serializers.BooleanField(help_text='Is staff', required=False, read_only=True)
    is_admin = serializers.BooleanField(help_text='Is administrator', required=False, read_only=True)
    display_name = serializers.CharField(help_text='Display Name', required=False, read_only=True)
    staff_id = serializers.CharField(help_text='Staff ID', required=False, read_only=True)
    username = serializers.CharField(help_text='Username')
    password = serializers.CharField(help_text='Password')
    lane_id = serializers.IntegerField(help_text='Lane ID')
    shift_id = serializers.IntegerField(help_text='Shift ID of this login', required=False, read_only=True)


class UserCardLoginSerializer(serializers.Serializer):
    id = serializers.IntegerField(help_text='ID of user', required=False, read_only=True)
    is_staff = serializers.BooleanField(help_text='Is staff', required=False, read_only=True)
    is_admin = serializers.BooleanField(help_text='Is administrator', required=False, read_only=True)
    display_name = serializers.CharField(help_text='Display Name', required=False, read_only=True)
    staff_id = serializers.CharField(help_text='Staff ID', required=False, read_only=True)
    username = serializers.CharField(help_text='Username', required=False, read_only=True)
    card_id = serializers.CharField(help_text='Card ID')
    lane_id = serializers.IntegerField(help_text='Lane ID')
    shift_id = serializers.IntegerField(help_text='Shift ID of this login', required=False, read_only=True)


class UserLogoutSerializer(serializers.Serializer):
    shift_id = serializers.IntegerField(help_text='Shift ID of this login')
    lane_id = serializers.IntegerField(help_text='Lane ID')
    user_id = serializers.IntegerField(help_text='User ID')
    revenue = serializers.IntegerField(help_text='Revenue')
    begin_timestamp = serializers.IntegerField(help_text='Begin timestamp', required=False, read_only=True)
    end_timestamp = serializers.IntegerField(help_text='End timestamp', required=False, read_only=True)
    num_check_in = serializers.IntegerField(help_text='Number of check in', required=False, read_only=True)
    num_check_out = serializers.IntegerField(help_text='Number of check out', required=False, read_only=True)


class ImageReplicationSerializer(serializers.Serializer):
    front_image = serializers.CharField(help_text='Front image path')
    back_image = serializers.CharField(help_text='Back image path')
    card_id = serializers.CharField(help_text='Card Id used for checking in images')


class CardTypeSerializer(serializers.ModelSerializer):
    id = serializers.IntegerField(source='id', help_text='ID of card type')
    name = serializers.CharField(source='name', help_text='Name of card type')

    class Meta:
        model = CardType
        fields = ('id', 'name')


class VehicleTypeSerializer(serializers.ModelSerializer):
    id = serializers.IntegerField(source='id', help_text='ID of vehicle type')
    name = serializers.CharField(source='name', help_text='Name of vehicle type')

    class Meta:
        model = VehicleType
        fields = ('id', 'name')


class UpdateVehicleNumberSerializer(serializers.Serializer):
    vehicle_number = serializers.CharField(help_text='Vehicle register number', required=False)
    check_in_alpr_vehicle_number = serializers.CharField(help_text='Check in ALPR vehicle number', required=False)
    check_out_alpr_vehicle_number = serializers.CharField(help_text='Check out ALPR vehicle number', required=False)


class StatisticsSerializer(serializers.Serializer):
    time_from = serializers.IntegerField(help_text='From timestamp', required=True)
    time_to = serializers.IntegerField(help_text='To timestamp', required=True)
    terminal_id = serializers.IntegerField(help_text='Terminal', required=False)

class ParkingFeeSerializer(serializers.Serializer):
    vehicleType = serializers.IntegerField(help_text='VehicleType')
    from_time = serializers.DateTimeField(help_text='FromTime')
    to_time = serializers.DateTimeField(help_text='ToTime')

##
# CLAIM PROMOTION
##

# POST /claim-promotion
# Create ClaimPromotionLog instance
# Create parking_fee_session
# Return final_fee, description
class ClaimPromotionSerializer(serializers.Serializer):
    parking_session_id = serializers.IntegerField(help_text='Parking session ID', required=True)
    user_id = serializers.IntegerField(help_text='User ID', default=0)
    # card_id = serializers.CharField(help_text='Card ID', required=True)
    # vehicle_number = serializers.CharField(help_text='Vehicle number', max_length=100, required=True)
    # vehicle_type = serializers.IntegerField(help_text='Vehicle type', default=0, required=False)
    amount_a = serializers.IntegerField(help_text='Amount A', default=0, required=True)
    amount_b = serializers.IntegerField(help_text='Amount B', default=0, required=True)
    amount_c = serializers.IntegerField(help_text='Amount C', default=0, required=True)
    amount_d = serializers.IntegerField(help_text='Amount D', default=0, required=True)
    amount_e = serializers.IntegerField(help_text='Amount E', default=0, required=True)
    data = serializers.CharField(help_text='Claim Promotion JSON data (should be a JSON list)', max_length=2000, required=True)
    client_time = serializers.DateTimeField(help_text='Submit time of client (YYYY-mm-dd HH:MM)',
                                            format="%Y-%d-%m %H:%M", required=False)
    notes = serializers.CharField(help_text='Notes', max_length=1000, default='', required=False)
##2018May15
class ClaimPromotionCallBill(serializers.Serializer):
    data = serializers.CharField(help_text='Claim Promotion JSON data (should be a JSON list)', max_length=2000,
                                 required=True)
    checkintime=serializers.CharField(help_text='String check in time (yyyyMMddHHmmss)', max_length=14,
                                 required=True)
    claimtime = serializers.CharField(help_text='String Claim time (yyyyMMddHHmmss)', max_length=14,
                                        required=True)
    vehicletype = serializers.IntegerField(help_text='Vehicletype', default=0,
                                      required=True)
##
class ClaimPromotionBillSerializer(serializers.ModelSerializer):
    class Meta:
        # model = ClaimPromotionBill
        model = ClaimPromotionBillV2


class ClaimPromotionCouponSerializer(serializers.ModelSerializer):
    class Meta:
        # model = ClaimPromotionCoupon
        model = ClaimPromotionCouponV2


class ClaimPromotionTenantModelSerializer(serializers.ModelSerializer):
    class Meta:
        model = ClaimPromotionTenant
        # fields = ('id', 'name', 'short_name')


class CustomerModelSerializer(serializers.ModelSerializer):
    class Meta:
        model = Customer
        # fields = ('id', 'name', 'short_name')


class ClaimPromotionVoucherModelSerializer(serializers.ModelSerializer):
    class Meta:
        model = ClaimPromotionVoucher
#2018Jun07
class ClaimPromotionSearchSerializer_new(serializers.ModelSerializer):
    id = serializers.CharField(help_text='Transaction id')
    parking_session_id = serializers.IntegerField(help_text='ID of parking session')
    vehicle_number = serializers.SerializerMethodField('get_vehicle_number')
    vehicle_type = serializers.SerializerMethodField('get_vehicle_type')
    user_id = serializers.IntegerField(help_text='User id')
    amount_a = serializers.IntegerField(help_text='Amount a')
    amount_b = serializers.IntegerField(help_text='Amount b')
    amount_c = serializers.IntegerField(help_text='Amount c')
    amount_d = serializers.IntegerField(help_text='Amount d')
    amount_e = serializers.IntegerField(help_text='Amount e')
    client_time = serializers.SerializerMethodField('get_client_time')
    server_time = serializers.SerializerMethodField('get_server_time')
    used = serializers.BooleanField(help_text='Used')
    notes = serializers.CharField(help_text='Notes')
    total = serializers.IntegerField(help_text='Amount a')
    promotion_bills = serializers.SerializerMethodField('get_promotion_bills')#ClaimPromotionBillSerializer(many=True)
    promotion_coupons =serializers.SerializerMethodField('get_promotion_coupons')# ClaimPromotionCouponSerializer(many=True)
    # 2018Jun01
    gate_name = serializers.SerializerMethodField('get_gate_name')
    check_in_time = serializers.SerializerMethodField('get_chek_in_time')
    def get_promotion_bills(self,obj):
        pk = ClaimPromotionBillV2.objects.filter(claim_promotion=obj["id"])
        if pk:
            res=[]
            for it in pk:
                rs={}
                rs["company_info"]=it.company_info
                rs["bill_number"]=it.bill_number
                rs["bill_amount"]=it.bill_amount
                rs["date"]=it.date
                rs["notes"]=it.notes
                res.append(rs);
            return  res;
        return  None;
    def get_promotion_coupons(self,obj):
        pk = ClaimPromotionCouponV2.objects.filter(claim_promotion=obj["id"])
        if pk:
            res = []
            for it in pk:
                rs = {}
                rs["company_info"] = it.company_info
                rs["bill_number"] = it.bill_number
                rs["bill_amount"] = it.bill_amount
                rs["date"] = it.date
                rs["notes"] = it.notes
                res.append(rs);
            return res;
        return  None;
    def get_gate_name(self, obj):
        if obj["user_id"]:
            us = UserProfile.objects.filter(id=obj["user_id"])
            if us:
                return us[0]
            return ''
        return ''

    def get_chek_in_time(self, obj):
        pk = ParkingSession.objects.filter(id=obj["parking_session_id"])
        if pk:
            r = pk[0]
            if r:
                ckintime = r.check_in_time;
                return datetime2timestamp(ckintime)
            return -1
        return -1
    # 2018Jun01
    def get_vehicle_number(self, obj):
        pk = ParkingSession.objects.filter(id=obj["parking_session_id"])
        if pk:
            r = pk[0]
            if r:
                return r.vehicle_number
            return None
        return  None

    def get_vehicle_type(self, obj):
        pk = ParkingSession.objects.filter(id=obj["parking_session_id"])
        if pk:
            r = pk[0]
            if r:
                return r.vehicle_type
            return  None
        return None
    def get_client_time(self, obj):
        if obj['client_time']:
            return datetime2timestamp(obj['client_time'])
        return -1

    def get_server_time(self, obj):
        if obj['server_time']:
            return datetime2timestamp(obj['server_time'])
        return -1

    class Meta:
        model = ClaimPromotionV2
        fields = ('id',
                  # 'card_id', 'parking_session_id', 'parking_fee_session_id',
                  'vehicle_number','parking_session_id',
                  'vehicle_type',
                  #2018Jun01
                  'gate_name','check_in_time','total',
                  # 2018Jun01
                  'user_id', 'amount_a', 'amount_b',
                  'amount_c', 'amount_d', 'amount_e', 'client_time', 'server_time', 'used', 'notes',
                  'promotion_bills', 'promotion_coupons')
#2018Jun07
class ClaimPromotionSearchSerializer(serializers.ModelSerializer):
    id = serializers.CharField(source='id', help_text='Transaction id')
    # card_id = serializers.CharField(source='card_id', help_text='Card id')
    parking_session_id = serializers.IntegerField(source='parking_session_id',
                                                  help_text='ID of parking session')
    # parking_fee_session_id = serializers.IntegerField(source='parking_fee_session_id',
    #                                                   help_text='ID of parking fee session')
    vehicle_number = serializers.SerializerMethodField('get_vehicle_number')
    vehicle_type = serializers.SerializerMethodField('get_vehicle_type')
    user_id = serializers.IntegerField(source='user_id', help_text='User id')
    amount_a = serializers.IntegerField(source='amount_a', help_text='Amount a')
    amount_b = serializers.IntegerField(source='amount_b', help_text='Amount b')
    amount_c = serializers.IntegerField(source='amount_c', help_text='Amount c')
    amount_d = serializers.IntegerField(source='amount_d', help_text='Amount d')
    amount_e = serializers.IntegerField(source='amount_e', help_text='Amount e')
    client_time = serializers.SerializerMethodField('get_client_time')
    server_time = serializers.SerializerMethodField('get_server_time')
    used = serializers.BooleanField(source='used', help_text='Used')
    notes = serializers.CharField(source='notes', help_text='Notes')
    promotion_bills = ClaimPromotionBillSerializer(many=True)
    promotion_coupons = ClaimPromotionCouponSerializer(many=True)
    # 2018Jun01
    gate_name = serializers.SerializerMethodField('get_gate_name')
    check_in_time = serializers.SerializerMethodField('get_chek_in_time')

    def get_gate_name(self, obj):
        if obj.user_id:
            us = UserProfile.objects.filter(id=obj.user_id)
            if us:
                return us[0]
            return ''
        return ''

    def get_chek_in_time(self, obj):
        pk = ParkingSession.objects.filter(id=obj.parking_session_id)
        if pk:
            r = pk[0]
            ckintime = r.check_in_time;
            return datetime2timestamp(ckintime)
        return -1
    # 2018Jun01
    def get_vehicle_number(self, obj):
        return obj.parking_session.vehicle_number

    def get_vehicle_type(self, obj):
        return obj.parking_session.vehicle_type

    def get_client_time(self, obj):
        if obj.client_time:
            return datetime2timestamp(obj.client_time)
        return -1

    def get_server_time(self, obj):
        if obj.server_time:
            return datetime2timestamp(obj.server_time)
        return -1

    class Meta:
        model = ClaimPromotionV2
        fields = ('id',
                  # 'card_id', 'parking_session_id', 'parking_fee_session_id',
                  'vehicle_number','parking_session_id',
                  'vehicle_type',
                  #2018Jun01
                  'gate_name','check_in_time',
                  # 2018Jun01
                  'user_id', 'amount_a', 'amount_b',
                  'amount_c', 'amount_d', 'amount_e', 'client_time', 'server_time', 'used', 'notes',
                  'promotion_bills', 'promotion_coupons')
##
# In case client need to learn server time
##
class TimeInfoSerializer(serializers.Serializer):
    utc_time = serializers.DateTimeField(help_text='Current UTC time')
    local_time = serializers.DateTimeField(help_text='Local Asia/Hochiminh time')
##2017-12-19
class BillStateInfoSerializer(serializers.Serializer):
    result=serializers.CharField(source='result', help_text='Bill not exists')
##
# ##2018-02-09
# class ActiveCheckInTimeSerializer(serializers.Serializer):
#     check_in_time = serializers.DateTimeField(help_text='Check in time',required=True)
#     cancel_time = serializers.DateTimeField(help_text='Cancel time',required=False )
#     expirate_time=serializers.DateTimeField(help_text='Expirate time',required=False)
# ##
#2018Oct17
class BlackListSerializer(serializers.Serializer):
    parking_session_id = serializers.IntegerField(help_text='ID of parking session')
    image_path = serializers.CharField(help_text='Path of image')
    vehiclenumber = serializers.CharField(help_text='Vehicle number')
    gate = serializers.IntegerField(help_text='Gate Id')
    user = serializers.IntegerField(help_text='Id of User login')
    stateparking = serializers.IntegerField(help_text='state parking')
class NotifiedParmSerializer(serializers.Serializer):
    gatename = serializers.CharField(help_text='Gate Name')
    datestr = serializers.CharField(help_text='Date String', required=False)
    duration = serializers.IntegerField(help_text='Duration')
#2018Oct17
#2018May21
class ForcedBarierSerializer(serializers.Serializer):
    user = serializers.CharField(help_text='Name of User login')
    terminal = serializers.CharField(help_text='Gate Name')
    lane = serializers.CharField(help_text='Lane Name')
    front_thumb = serializers.FileField(help_text='front thumb')
    back_thumb = serializers.FileField(help_text='back thumb')
class VoucherSerializer(serializers.Serializer):
    voucher_type = serializers.CharField(help_text='voucher_type', required=False,
                                    blank=True)
    voucher_amount = serializers.CharField(help_text='voucher_amount', required=False,
                                    blank=True)
    parking_fee = serializers.IntegerField(help_text='parking_fee', required=False, blank=True)
    actual_fee = serializers.IntegerField(help_text='actual_fee', required=False, blank=True)
    check_in_time = serializers.DateTimeField(help_text='check_in_time', format="%Y-%d-%m %H:%M:%s")

class VoucherParamSerializer(serializers.Serializer):
    check_in_time = serializers.DateTimeField(help_text='check_in_time', format="%Y-%d-%m %H:%M:%s")
#2018May21
