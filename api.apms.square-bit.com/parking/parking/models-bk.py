# -*- coding: utf-8 -*-
import datetime
from audit_log.models.managers import AuditLog
from django.db.models.aggregates import Sum
from django.contrib.auth.models import User
from django.db import models
import jsonfield
import pytz
import sys
from site_settings.settings import TIME_ZONE
from common import VEHICLE_STATUS_CHOICE, CARD_STATUS

CARD_TYPE = ()

VEHICLE_TYPE_CATEGORY = (
    # (100000000, u'Tất cả'),
    (100000000, u'Bất kỳ'),
    (1000001, u'Xe máy'),
    (2000101, u'Ô tô'),
    (5000401, u'Tải giao hàng'),
    (4000301, u'XM- Giao hàng'),
)

VEHICLE_TYPE = ()

TICKET_STATUS = (
    (0, u'Không dùng'),
    (1, u'Đang dùng'),
)

LANE_DIRECTION = (
    (0, u'Vào'),
    (1, u'Ra'),
)

CAMERA_POSITION = (
    (0, u'Trước'),
    (1, u'Sau'),
)


class CardType(models.Model):
    id = models.IntegerField(primary_key=True)
    name = models.CharField(max_length=100, verbose_name=u"Tên")

    def __unicode__(self):
        return self.name

    class Meta:
        verbose_name_plural = u'Loại thẻ'

##2018Dec13
class Slot(models.Model):
    id = models.AutoField(primary_key=True)
    name = models.CharField(max_length=100, verbose_name=u"Tên")
    slottotal=models.IntegerField(verbose_name=u"Tổng số ô đậu")
    prefix = models.CharField(max_length=20, verbose_name=u"Tiền tố gởi cổng Com")
    suffixes = models.CharField(max_length=20, verbose_name=u"Hậu tố gởi cổng Com")
    numlength = models.IntegerField(verbose_name=u"Chiều dài chuỗi số gởi cổng Com")
    hascheckkey=models.IntegerField(verbose_name=u"Có gởi kí số kiểm tra")
    def __unicode__(self):
        return self.name
    class Meta:
        verbose_name = u'Loại Ô đậu'
        verbose_name_plural = u'Loại Ô đậu'
##2018Dec13
##2018Dec14
# class WaitingCheckIn(models.Mode):
#     cardcode=models.CharField(primary_key=True)
#     createdtime=models.DateTimeField(verbose_name=u"Thời điểm tạo")
#     waitingstaus=models.IntegerField(verbose_name=u"Trạng thái chờ")
#     customerinfo=models.TextField(verbose_name=u"Thông tin khách hàng")
#     waitingsecondamount=models.IntegerField(verbose_name=u"Số giây chờ")
# class WaitingCheckOut(models.Mode):
#     cardcode=models.CharField(primary_key=True)
#     createdtime=models.DateTimeField(verbose_name=u"Thời điểm tạo")
#     waitingtime = models.DateTimeField(verbose_name=u"Thời điểm chờ")
#     waitingstaus=models.IntegerField(verbose_name=u"Trạng thái chờ")
#     customerinfo=models.TextField(verbose_name=u"Thông tin khách hàng")
##2018Dec14
class VehicleType(models.Model):
    id = models.IntegerField(primary_key=True)
    category = models.IntegerField(choices=VEHICLE_TYPE_CATEGORY, default=VEHICLE_TYPE_CATEGORY[0][0],
                                   verbose_name='Nhóm chính')
    slot=models.ForeignKey(Slot,verbose_name='Ô đậu',db_column="slot_id")
    name = models.CharField(max_length=100, verbose_name=u"Tên")

    def __unicode__(self):
        return self.name

    class Meta:
        verbose_name = u'Loại xe'
        verbose_name_plural = u'Loại xe'


class Card(models.Model):
    id = models.AutoField(primary_key=True)
    card_id = models.CharField(max_length=128, unique=True, db_index=True, verbose_name=u"Mã thẻ")
    card_label = models.CharField(max_length=128, unique=True, db_index=True, verbose_name=u"Tên thẻ")
    status = models.IntegerField(choices=CARD_STATUS, verbose_name=u"Trạng thái")
    vehicle_type = models.IntegerField(choices=VEHICLE_TYPE, verbose_name=u"Loại xe", help_text=u'ID loại xe')
    card_type = models.IntegerField(choices=CARD_TYPE, verbose_name=u"Loại thẻ", help_text=u'ID loại thẻ')
    # cardtype = models.ForeignKey(CardType, verbose_name=u"Loại thẻ", help_text=u'ID loại thẻ', db_column="card_type")
    note = models.CharField(max_length=2000, null=True, blank=True, default=None, verbose_name='Ghi chú')

    audit_log = AuditLog()

    def __unicode__(self):
        return self.card_label


    class Meta:
        verbose_name = u'Thẻ'
        verbose_name_plural = u'Thẻ'

class admin_log(models.Model):
    id = models.AutoField(primary_key=True)
    action_time = models.CharField(max_length=2000, null=True, blank=True, default=None, verbose_name=u"Ngày tháng")
    # user_id = models.CharField(max_length=128, unique=True, db_index=True, verbose_name=u"Tài khoản thực hiện")
    user = models.ForeignKey(User, verbose_name='User ID', unique=True, db_index=True)
    content_type_id = models.IntegerField(max_length=128, unique=True, db_index=True, verbose_name=u"Tên Nhân Viên")
    object_id = models.IntegerField(max_length=128, unique=True, db_index=True, verbose_name=u"Tài Khoản thực hiện")
    object_repr = models.IntegerField(max_length=128, verbose_name=u"Mục Tiêu Điều Chỉnh", help_text=u'Mục Tiêu Điều Chỉnh')
    action_flag = models.IntegerField(max_length=128, verbose_name=u"Thao Tác", help_text=u'Thao Tác')
    change_message = models.CharField(max_length=2000, null=True, blank=True, default=None, verbose_name=u'Nội Dung')

    def __unicode__(self):
        return self.id


    class Meta:
        verbose_name = u'Tài khoản thực hiện'
        verbose_name_plural = u'Tài khoản thực hiện'
        db_table = 'django_admin_log'


class TerminalGroup(models.Model):
    id = models.AutoField(primary_key=True)
    name = models.CharField(max_length=200, verbose_name=u'Tên')

    def __unicode__(self):
        return self.name

    class Meta:
        verbose_name_plural = u'Cổng'
        verbose_name = u'Cổng'


class Terminal(models.Model):
    id = models.AutoField(primary_key=True)
    name = models.CharField(max_length=200)
    terminal_id = models.CharField(max_length=50, db_index=True, verbose_name='ID')
    ip = models.CharField(max_length=50, blank=True)
    version = models.CharField(max_length=50, blank=True, null=True)
    status = models.IntegerField()
    last_check_health = models.DateTimeField(blank=True, db_index=True)
    terminal_group = models.ForeignKey(TerminalGroup, blank=True, null=True)

    def __unicode__(self):
        return self.name

    class Meta:
        verbose_name = 'Trạm'
        verbose_name_plural = 'Trạm'


class Lane(models.Model):
    id = models.AutoField(primary_key=True)
    name = models.CharField(max_length=200)
    direction = models.IntegerField(choices=LANE_DIRECTION)
    enabled = models.BooleanField()
    vehicle_type = models.IntegerField(choices=VEHICLE_TYPE)

    terminal = models.ForeignKey(Terminal)

    def __unicode__(self):
        return self.name

    class Meta:
        verbose_name = 'Làn'
        verbose_name_plural = 'Làn'


class Camera(models.Model):
    id = models.AutoField(primary_key=True)
    name = models.CharField(max_length=200)
    ip = models.CharField(max_length=50)
    position = models.IntegerField(choices=CAMERA_POSITION)
    direction = models.IntegerField(choices=LANE_DIRECTION)
    serial_number = models.CharField(max_length=200)
    lane = models.ForeignKey(Lane)

    def __unicode__(self):
        return self.name

    class Meta:
        verbose_name = 'Camera'

class CheckOutExceptionInfo(models.Model):
    id = models.AutoField(primary_key=True)
    notes = models.CharField(max_length=4000, verbose_name='Ghi chú')
    parking_fee = models.IntegerField(verbose_name='Phí', default=0)

    def __unicode__(self):
        return self.notes

    class Meta:
        verbose_name = u'Cho ra ngoại lệ'
        verbose_name_plural = u'Cho ra ngoại lệ'


class ParkingSession(models.Model):
    id = models.AutoField(primary_key=True)
    card = models.ForeignKey(Card, verbose_name=u'Tên thẻ')
    vehicle_type = models.IntegerField(choices=VEHICLE_TYPE, db_index=True, verbose_name=u'Loại xe')
    vehicle_number = models.CharField(max_length=20, db_index=True, verbose_name=u'Biển số xe')
    check_in_alpr_vehicle_number = models.CharField(max_length=20, verbose_name=u'Nhận dạng biển số lúc vào')
    check_in_operator = models.ForeignKey(User, related_name='parkingsession_checkinoperators',
                                          verbose_name=u'Người cho vào')
    check_in_time = models.DateTimeField(db_index=True, verbose_name=u'Thời điểm vào')
    check_in_images = jsonfield.JSONField(max_length=500, verbose_name=u'Hình ảnh cho vào')
    check_in_lane = models.ForeignKey(Lane, related_name='parkingsession_checkinlanes')
    check_out_alpr_vehicle_number = models.CharField(max_length=20, null=True, verbose_name=u'Nhận dạng biển số lúc ra')
    check_out_operator = models.ForeignKey(User, null=True, related_name='parkingsession_checkoutoperators',
                                           verbose_name=u'Người cho ra')
    check_out_time = models.DateTimeField(null=True, db_index=True, verbose_name=u'Thời điểm ra')
    check_out_images = jsonfield.JSONField(max_length=500, null=True, blank=True, verbose_name=u'Hình ảnh cho ra')
    check_out_lane = models.ForeignKey(Lane, null=True, related_name='parkingsession_checkoutlanes')
    duration = models.IntegerField(null=True, verbose_name=u'Thời gian gửi')
    check_out_exception = models.ForeignKey(CheckOutExceptionInfo, null=True, blank=True,
                                            verbose_name=u'Cho ra ngoại lệ')

    class Meta:
        verbose_name = u'Cho ra ngoại lệ'
        verbose_name_plural = u'Cho ra ngoại lệ'

    def __unicode__(self):
        return str(self.card.card_label)


class CardStatus(models.Model):
    id = models.AutoField(primary_key=True)
    card = models.ForeignKey(Card, 'card_id')
    parking_session = models.ForeignKey(ParkingSession, db_constraint=False)
    status = models.IntegerField(db_index=True, choices=CARD_STATUS)


class CheckInImage(models.Model):
    id = models.AutoField(primary_key=True)
    parking_session = models.ForeignKey(ParkingSession, db_constraint=False)
    terminal = models.ForeignKey(Terminal)


class UserCard(models.Model):
    id = models.AutoField(primary_key=True)
    user = models.ForeignKey(User)
    card = models.ForeignKey(Card, 'card_id')


class UserProfile(models.Model):
    id = models.AutoField(primary_key=True)
    user = models.OneToOneField(User)
    fullname = models.CharField(max_length=500, verbose_name='Tên đầy đủ', default="")
    staff_id = models.CharField(max_length=10, verbose_name='Mã nhân viên', default="")
    birthday = models.DateField(verbose_name='Ngày sinh', null=True, blank=True)
    card = models.ForeignKey(Card, verbose_name='Mã thẻ', null=True, blank=True, unique=True)

    def __unicode__(self):
        return self.fullname

    class Meta:
        verbose_name = u'Nhân viên'
        verbose_name_plural = u'Thông tin chi tiết'

        permissions = (
            ("export_unprotected_excel", "Export unprotected Excel"),
            ("pause_resume_cancel_vehicle_registration", "Pause/Resume/Cancel Vehicle Registration"),
            ("view_search_customer", "View Search Customer"),
            ("change_customer_vehicle_registration_expired_date", "Change customer vehicle registration expired date"),
        )


class UserShift(models.Model):
    id = models.AutoField(primary_key=True)
    user = models.ForeignKey(User)
    lane = models.ForeignKey(Lane)
    begin = models.DateTimeField()
    end = models.DateTimeField(null=True, blank=True)
    info = jsonfield.JSONField(max_length=500, null=True, blank=True)

    def __unicode__(self):
        return self.id


class ParkingSetting(models.Model):
    key = models.CharField(primary_key=True, max_length=100)
    name = models.CharField(max_length=500, verbose_name=u'Tên')
    value = models.CharField(max_length=1000, verbose_name=u'Giá trị',
                             help_text=u'Thận trọng khi thay đổi! 0 (False), 1(True) cho dạng Đúng/Sai, hoặc các giá trị khác')
    notes = models.CharField(max_length=1000, verbose_name=u'Ghi chu', null=True, blank=True)

    def __unicode__(self):
        return self.name

    class Meta:
        verbose_name_plural = u'Thiết lập bãi giữ xe'


class ImageReplicationSetting(models.Model):
    id = models.AutoField(primary_key=True)
    sour_ip = models.CharField(max_length=30, verbose_name=u'IP gốc')
    dest_ip_list = models.CharField(max_length=2000, verbose_name=u'Danh sách IP sao chép ảnh')

    def __unicode__(self):
        return str(self.sour_ip)

    class Meta:
        verbose_name = u'Cấu hình sao chép ảnh'
        verbose_name_plural = u'Cấu hình sao chép ảnh'


class Attendance(models.Model):
    id = models.AutoField(primary_key=True)
    user = models.ForeignKey(User)
    time_in = models.DateTimeField()
    time_out = models.DateTimeField(null=True, blank=True)
    total_time_of_date = models.FloatField(null=True, blank=True)
    parking_session = models.ForeignKey(ParkingSession, null=True, on_delete=models.SET_NULL, db_constraint=False)


class CheckOutException(models.Model):
    id = models.AutoField(primary_key=True)
    parking_session = models.ForeignKey(ParkingSession, db_constraint=False)
    notes = models.CharField(max_length=2000, verbose_name='Ghi chú')

    def __unicode__(self):
        return u"Cho ra ngoại lệ"

    class Meta:
        verbose_name = u'Cho ra ngoại lệ'
        verbose_name_plural = u'Cho ra ngoại lệ'


class ReportData(models.Model):
    id = models.AutoField(primary_key=True)
    time = models.DateTimeField(db_index=True)
    check_in = models.CharField(max_length=4000)
    check_out = models.CharField(max_length=4000)


class Server(models.Model):
    id = models.IntegerField(primary_key=True)
    name = models.CharField(max_length=200)
    ip = models.CharField(max_length=50)

    def __unicode__(self):
        return self.name


class Building(models.Model):
    name = models.CharField(max_length=255, verbose_name='Tên tòa nhà')
    address = models.CharField(max_length=500, verbose_name='Địa chỉ tòa nhà', blank=True, null=True, )

    def __unicode__(self):
        return u'%s' % self.name

    class Meta:
        verbose_name = u'Tòa nhà'
        verbose_name_plural = u'Tòa nhà'


class Company(models.Model):
    name = models.CharField(max_length=255, verbose_name='Tên công ty')
    address = models.CharField(max_length=500, verbose_name='Địa chỉ', blank=True, null=True, )
    phone = models.CharField(max_length=255, verbose_name='ĐT', blank=True, null=True, )
    email = models.EmailField(max_length=255, verbose_name='Email', blank=True, null=True)
    representative_name = models.CharField(max_length=255, verbose_name='Tên người đại diện', blank=True, null=True)
    representative_phone = models.CharField(max_length=255, verbose_name='SĐT người đại diện', blank=True, null=True)

    def __unicode__(self):
        return u'%s' % self.name

    class Meta:
        verbose_name = u'Công ty'
        verbose_name_plural = u'Công ty'


# CUSTOMER_TYPE = (
#     ('CDN', u'Loại khách hàng'),
#     ('KVP', u'Khối văn phòng'),
#     ('DB', u'Đặc biệt'),
#     ('MP', u'Miễn phí'))


class CustomerType(models.Model):
    name = models.CharField(max_length=255, verbose_name='Tên loại khách hàng')

    # type = models.CharField(max_length=20, choices=CUSTOMER_TYPE, default=CUSTOMER_TYPE[0][0], verbose_name='Loại')

    def __unicode__(self):
        return self.name

    class Meta:
        verbose_name = u'Loại khách hàng'
        verbose_name_plural = u'Loại khách hàng'


class Apartment(models.Model):
    address = models.CharField(max_length=255, verbose_name='Địa chỉ căn hộ')
    owner_name = models.CharField(max_length=255, verbose_name='Tên chủ hộ', blank=True, null=True)
    owner_phone = models.CharField(max_length=255, verbose_name='ĐT chủ hộ', blank=True, null=True)
    owner_email = models.EmailField(max_length=255, blank=True, null=True, verbose_name='Email chủ hộ')

    def __unicode__(self):
        return u'%s' % self.address

    class Meta:
        verbose_name = u'Căn hộ'
        verbose_name_plural = u'Căn hộ'


class Customer(models.Model):
    id = models.AutoField(primary_key=True)
    apartment = models.ForeignKey(Apartment, verbose_name='Căn hộ', blank=True, null=True)
    building = models.ForeignKey(Building, verbose_name='Tòa nhà', blank=True, null=True)
    company = models.ForeignKey(Company, verbose_name='Công ty', blank=True, null=True)
    # Thong tin chinh
    customer_type = models.ForeignKey(CustomerType, verbose_name='Loại khách hàng', null=True, blank=True,
                                      on_delete=models.SET_NULL)

    customer_name = models.CharField(verbose_name='Tên KH', max_length=255)
    customer_id = models.CharField(verbose_name='CMND/Hộ chiếu', max_length=255, blank=False)

    customer_birthday = models.DateField(verbose_name='Ngày sinh', null=True, blank=True)
    customer_avatar = models.ImageField(verbose_name='Ảnh đại điện', blank=True, upload_to=True)
    customer_phone = models.CharField(verbose_name='ĐT nhà', max_length=255, blank=True, default="")
    customer_mobile = models.CharField(verbose_name='Di động', max_length=255, blank=True, default="")
    customer_email = models.EmailField(verbose_name='Email', max_length=255, blank=True, default="")
    # Thong tin hoa don
    order_register_name = models.CharField(verbose_name='Tên cty/cá nhân', max_length=100, blank=True, default="")
    order_register_address = models.CharField(verbose_name='Địa chỉ', max_length=200, blank=True, default="")
    order_tax_code = models.CharField(verbose_name='Mã số thuế', max_length=50, blank=True, default="")
    # Thong tin bao nhac phi
    messaging_via_sms = models.BooleanField(verbose_name='Qua SMS', default=False)
    messaging_via_phone = models.BooleanField(verbose_name='Gọi điện', default=False)
    messaging_via_email = models.BooleanField(verbose_name='Email', default=False)
    messaging_via_apart_mail = models.BooleanField(verbose_name='Thư căn hộ', default=False)
    messaging_via_wiper_mail = models.BooleanField(verbose_name='Thư gắn gạt nước xe', default=False)
    messaging_sms_phone = models.CharField(max_length=255, verbose_name='SĐT', blank=True, default="")
    messaging_phone = models.CharField(verbose_name='SĐT', max_length=255, blank=True, default="")
    messaging_email = models.CharField(verbose_name='Email', max_length=255, blank=True, default="")
    messaging_address = models.CharField(verbose_name='Địa chỉ', max_length=255, blank=True, default="")
    staff = models.ForeignKey(User, verbose_name='NV đăng ký', db_constraint=False, blank=True, null=True,
                              on_delete=models.SET_NULL)

    audit_log = AuditLog()

    def __unicode__(self):
        return self.customer_name

    class Meta:
        verbose_name = u'Khách hàng'
        verbose_name_plural = u'Khách hàng'
        unique_together = ('customer_id', 'customer_name')


class LevelFee(models.Model):
    name = models.CharField(max_length=100, verbose_name='Tên mức phí', help_text='khách hàng - XM1')
    customer_type = models.ForeignKey(CustomerType, verbose_name='Loại khách hàng', null=True, blank=True,
                                      on_delete=models.SET_NULL)
    vehicle_type = models.ForeignKey(VehicleType, verbose_name='Loại xe', null=True, blank=True,
                                     on_delete=models.SET_NULL)
    fee = models.IntegerField(default=0, verbose_name='Phí')

    def __unicode__(self):
        return u'%s' % (self.name)

    class Meta:
        verbose_name = u'Mức phí'
        verbose_name_plural = 'Mức phí'


def get_duration(vehicle_registration_id, expired_date):
    today = datetime.date.today()
    current_remain_duration = 0
    if expired_date:
        current_remain_duration = expired_date - today
        current_remain_duration = current_remain_duration.days

    sum_duration = TicketPaymentDetail.objects.filter(vehicle_registration_id=vehicle_registration_id,
                                                      used=False).aggregate(Sum('duration'))
    if sum_duration['duration__sum']:
        sum_duration = sum_duration['duration__sum']
    else:
        sum_duration = 0
    result = current_remain_duration + sum_duration
    s = u'0 ngày'
    if result > 0:
        s = u"%s ngày" % result
    elif expired_date == today:
        s = u"hôm nay"

    return result, s


class VehicleRegistration(models.Model):
    # Thong tin the xe
    id = models.AutoField(primary_key=True)
    card = models.ForeignKey(Card, verbose_name='Tên thẻ', blank=True, null=True, unique= True)
    customer = models.ForeignKey(Customer, verbose_name="Khách hàng", null=True, on_delete=models.CASCADE)
    level_fee = models.ForeignKey(LevelFee, verbose_name='Mức phí', blank=True, null=True, on_delete=models.SET_NULL)
    # Ngay gio
    registration_date = models.DateTimeField(verbose_name='Ngày tạo', auto_now_add=True)

    first_renewal_effective_date = models.DateField(verbose_name='Ngày đăng ký', blank=True, null=True)
    last_renewal_date = models.DateField(verbose_name='Ngày đóng phí', blank=True, null=True)
    last_renewal_effective_date = models.DateField(verbose_name='Ngày hiệu lực', blank=True, null=True)

    start_date = models.DateField(verbose_name='Ngày bắt đầu hạn', blank=True, null=True,
                                  help_text=u'Thông tin này đuợc cập nhật tự động, chỉ nên thay đổi bằng tay khi thật sự cần thiết')
    expired_date = models.DateField(verbose_name='Hạn hiện tại', blank=True, null=True,
                                    help_text=u'Thông tin này đuợc cập nhật tự động, chỉ nên thay đổi bằng tay khi thật sự cần thiết')

    pause_date = models.DateField(verbose_name='Ngày tạm ngừng', blank=True, null=True)
    cancel_date = models.DateField(verbose_name='Ngày hủy', blank=True, null=True)

    # current_payment_detail_id = models.IntegerField(verbose_name='Chi tiết thanh toán hiện tại', blank=True, null=True)

    # Thong tin tai xe
    vehicle_driver_name = models.CharField(verbose_name='Tên lái xe', max_length=255)
    vehicle_driver_id = models.CharField(verbose_name='Ghi Chú', max_length=255, blank=True, default="")
    vehicle_driver_phone = models.CharField(max_length=255, verbose_name='SĐT lái xe', blank=True, default="")
    # Thong tin xe
    vehicle_type = models.ForeignKey(VehicleType, verbose_name='Loại xe', null=True)
    vehicle_number = models.CharField(max_length=255, verbose_name='Biển số')
    vehicle_paint = models.CharField(max_length=255, verbose_name='Biển số Phụ', blank=True, default="")
    vehicle_brand = models.CharField(max_length=255, verbose_name='Nhãn hiệu', blank=True, default="")

    status = models.IntegerField(verbose_name='Trạng thái', choices=VEHICLE_STATUS_CHOICE, default=3, blank=True)
    staff = models.ForeignKey(UserProfile, verbose_name='Người thực hiện', blank=True, null=True)

    audit_log = AuditLog()

    class Meta:
        verbose_name = u'Thông tin xe'
        verbose_name_plural = u'Thông tin xe'

    def get_status(self):
        return VEHICLE_STATUS_CHOICE[self.status][1]

    def __unicode__(self):
        s = ''
        if self.expired_date:
            expired_date = self.expired_date
            day = datetime.datetime(expired_date.year, expired_date.month, expired_date.day)
            s = ' (%s)' % pytz.utc.localize(day).astimezone(pytz.timezone(TIME_ZONE)).strftime('%d/%m/%Y')

        return u'%s%s' % (self.vehicle_number, #self.get_status(),
        #                                                      #get_duration(self.id, self.expired_date)[1],
                            s)
        return u'%s' % (self.vehicle_number if self.vehicle_number else '')


CALCULATION_METHOD = (('luot', 'Theo lượt'),
                      ('block', 'Theo block'))


class ParkingFee(models.Model):
    vehicle_type = models.ForeignKey(VehicleType, unique=True, verbose_name='Loại xe')  # Loai xe
    calculation_method = models.CharField(max_length=10,
                                          choices=CALCULATION_METHOD,
                                          verbose_name='Phương thức')
    min_calculation_time = models.IntegerField(default=0, verbose_name='Số phút tính phí tối thiểu')

    def __unicode__(self):
        return u'Phí %s vãng lai' % self.vehicle_type

    class Meta:
        verbose_name = u'Phí gửi xe vãng lai'
        verbose_name_plural = u'Phí gửi xe vãng lai'


### Phi gui xe theo luot


class TurnFee(models.Model):
    # TURN_FEE_CHOICES = (
    #     ('NGAY', 'Ngay'),
    #     ('DEM', 'Dem')
    # )

    parking_fee = models.ForeignKey(ParkingFee)

    day_start_time = models.TimeField(default='5:00:00', verbose_name=u"TG bắt đầu lượt ngày")
    day_end_time = models.TimeField(default='17:59:59', verbose_name="TG kết thúc lượt ngày")

    night_start_time = models.TimeField(default='18:00:00', verbose_name=u"TG bắt đầu lượt đêm")
    night_end_time = models.TimeField(default='23:00:00', verbose_name="TG kết thúc lượt đêm")

    # name = models.CharField(max_length=20, choices=TURN_FEE_CHOICES, verbose_name=u"Tên lượt")
    # is_overnight = models.BooleanField(verbose_name=u"Qua đêm") # Ap dung tinh phi qua dem
    # duration = models.IntegerField(default=0, null=True, verbose_name=u"Số giờ lưu bãi tối thiểu")

    day_fee = models.IntegerField(default=0, verbose_name=u"Phí lượt ngày")
    night_fee = models.IntegerField(default=0, verbose_name=u"Phí lượt đêm")
    overnight_fee = models.IntegerField(default=0, verbose_name=u'Phí qua đêm')

    def __unicode__(self):
        return u'Phí gửi %s theo lượt' % (self.parking_fee.vehicle_type.name)

    class Meta:
        verbose_name = u'Phí gửi xe theo lượt'
        verbose_name_plural = u'Phí gửi xe theo lượt'


class BlockFee(models.Model):
    parking_fee = models.ForeignKey(ParkingFee)
    # parking_fee = models.ForeignKey(ParkingFee, db_constraint=False)

    first_block_duration = models.IntegerField(default=0, verbose_name='TG block đầu')
    next_block_duration = models.IntegerField(default=0, verbose_name='TG block tiếp theo')

    first_block_fee = models.IntegerField(default=0, verbose_name='Phí block đầu')
    next_block_fee = models.IntegerField(default=0, verbose_name='Phí block tiếp theo')

    max_block_duration = models.IntegerField(default=0, verbose_name='Tổng TG block tối đa')
    max_block_fee = models.IntegerField(default=0, verbose_name='Phí block tối đa')

    in_day_block_fee = models.IntegerField(default=0, verbose_name='Phí block trong ngay')
    night_block_fee = models.IntegerField(default=0, verbose_name='Phí block ban dem')

    def __unicode__(self):
        return u'Phí gửi xe theo block phút'

    class Meta:
        verbose_name = u'Phí gửi xe theo block phút'
        verbose_name_plural = u'Phí gửi xe theo block phút'


class Receipt(models.Model):
    RECEIPT_TYPE = (
        (0, 'Gia hạn'),
        (1, 'Cọc thẻ'),
    )

    receipt_number = models.IntegerField(verbose_name=u'Số phiếu thu', unique=True)
    type = models.IntegerField(verbose_name=u'Loại', choices=RECEIPT_TYPE)
    ref_id = models.IntegerField(verbose_name=u'Mã tham chiếu')
    cancel = models.BooleanField(verbose_name=u'Hủy', default=False)
    notes = models.CharField(max_length='500', verbose_name=u'Lý do')
    action_date = models.DateTimeField(verbose_name='Thao tác mới nhất', auto_now=True)

    class Meta:
        verbose_name = u'Phiếu thu'
        verbose_name_plural = u'Phiếu thu'

    def __unicode__(self):
        return u'Phiếu thu %s' % self.receipt_number


class TicketPayment(models.Model):
    PAYMENT_METHOD_CHOICES = (
        ('TM', u'Tiền mặt'),
        ('CK', u'Chuyển khoản')
    )
    customer = models.ForeignKey(Customer, related_name='ticket_payment_customer', verbose_name=u'Khách hàng',
                                 null=True, blank=True, on_delete=models.SET_NULL)
    receipt_id = models.IntegerField(null=True, verbose_name=u'Mã phiếu thu')
    receipt_number = models.IntegerField(null=True, verbose_name=u'Số phiếu thu')
    payment_date = models.DateTimeField(verbose_name=u'Ngày đóng phí', auto_now_add=True)
    payment_fee = models.IntegerField(default=0, verbose_name=u'Tổng số tiền')
    payment_method = models.CharField(max_length=20, choices=PAYMENT_METHOD_CHOICES, verbose_name=u'Thanh toán',
                                      default=PAYMENT_METHOD_CHOICES[0][0])
    notes = models.CharField(max_length=200, verbose_name=u'Ghi chú', null=True, blank=True)
    staff = models.ForeignKey(UserProfile, verbose_name=u'Người thực hiện', null=True, blank=True,
                              on_delete=models.SET_NULL)

    def __unicode__(self):
        return u'Lượt %s' % self.pk

    class Meta:
        verbose_name = u'Danh sách gia hạn'
        verbose_name_plural = u'Danh sách gia hạn'


class TicketPaymentDetail(models.Model):
    DURATION_CHOICES = (
        (0, '0'),
        (30, '1 tháng'),
        (60, '2 tháng'),
        (90, '3 tháng'),
        (120, '4 tháng'),
        (150, '5 tháng'),
        (180, '6 tháng'),
        (210, '7 tháng'),
        (240, '8 tháng'),
        (270, '9 tháng'),
        (300, '10 tháng'),
        (330, '11 tháng'),
        (360, '1 năm'),
    )
    ticket_payment = models.ForeignKey(TicketPayment, verbose_name=u'Lượt thanh toán')
    vehicle_registration = models.ForeignKey(VehicleRegistration, verbose_name=u'Số phiếu', null=True, blank=True,
                                             on_delete=models.SET_NULL)
    vehicle_number = models.CharField(verbose_name=u'Biển số xe', max_length=255, blank=True, default="")
    level_fee = models.IntegerField(default=0, verbose_name=u'Phí tháng')

    effective_date = models.DateField(verbose_name=u'Ngày hiệu lực', blank=True, null=True)
    duration = models.IntegerField(choices=DURATION_CHOICES, default=DURATION_CHOICES[0][0],
                                   verbose_name=u'Số tháng gia hạn (làm tròn đến cuối tháng)')
    day_duration = models.IntegerField(default=0, verbose_name=u'Số ngày cộng thêm')
    old_expired_date = models.DateField(verbose_name=u'Hạn cũ', blank=True, null=True)
    expired_date = models.DateField(verbose_name=u'Hạn mới', blank=True, null=True)
    cancel_date = models.DateField(verbose_name='Ngày hủy', blank=True, null=True)
    cardnumber = models.IntegerField(verbose_name=u'Số thẻ', blank=True)
    payment_detail_fee = models.IntegerField(default=0, verbose_name=u'Thành tiền')
    used = models.BooleanField(verbose_name=u'Đã và đang dược dùng', default=False)

    def __unicode__(self):
        return u'Chi tiết thanh toán %s, %s' % (
        self.pk, self.ticket_payment.payment_date.astimezone(pytz.timezone(TIME_ZONE)).strftime("%d/%m/%Y %H:%M:%S"))

    class Meta:
        verbose_name = u'Chi tiết thanh toán vé tháng'
        verbose_name_plural = u'Chi tiết thanh toán vé tháng'


class ParkingFeeSession(models.Model):
    TYPE_CHOICES = (
        ('IN', 'Check in'),
        ('OUT', 'Check out'),
    )

    parking_session = models.ForeignKey(ParkingSession, verbose_name=u'Lượt xe', db_index=True)
    card_id = models.CharField(max_length=255, verbose_name=u'Mã thẻ', blank=True, default="")
    vehicle_number = models.CharField(max_length=255, verbose_name=u'Biển số xe', blank=True, default="")
    parking_fee = models.IntegerField(verbose_name=u'Phí gửi xe', default=0)
    parking_fee_detail = models.CharField(max_length='1000', verbose_name=u'Thông tin thêm')
    calculation_time = models.DateTimeField(verbose_name=u'Thời điểm tính', db_index=True, auto_now_add=True)
    payment_date = models.DateTimeField(verbose_name=u'Thời điểm thanh toán', auto_now_add=True)
    session_type = models.CharField(max_length=10, choices=TYPE_CHOICES, db_index=True, verbose_name=u'Loai lượt vãng lai')
    vehicle_type = models.ForeignKey(VehicleType, verbose_name=u'Loai xe')
    is_vehicle_registration = models.BooleanField(verbose_name='Xe đăng ký vé tháng?', default=False)

    def __unicode__(self):
        return '%s' % (self.id)

    class Meta:
        verbose_name = u'Lượt vãng lai'
        verbose_name_plural = u'Lượt vãng lai'


class PauseResumeHistory(models.Model):
    TYPE_CHOICES = (
        (0, u'Tạm ngừng'),
        (1, u'Tiếp tục')
    )
    vehicle_registration = models.ForeignKey(VehicleRegistration, verbose_name='Đăng ký xe', null=True, blank=True,
                                             db_constraint=False)
    expired_date = models.DateField(verbose_name='Thời hạn', null=True, blank=True)
    request_date = models.DateField(verbose_name='Ngày yêu cầu', auto_now_add=True)
    start_date = models.DateField(verbose_name='Ngày hiệu lực trở lại', null=True, blank=True)
    request_type = models.IntegerField(verbose_name='Loại yêu cầu', choices=TYPE_CHOICES)
    request_notes = models.CharField(max_length=200, verbose_name='Ghi chú', blank=True, default="")
    remain_duration = models.IntegerField(verbose_name='Số ngày còn lại', default=0)
    used = models.BooleanField(verbose_name='Da su dung record nay', default=False)

    def __unicode__(self):
        return u"%s" % (self.vehicle_registration if self.vehicle_registration else '')

    class Meta:
        verbose_name = u'Lịch sử hủy/tạm ngừng/tiếp tục'
        verbose_name_plural = u'Lịch sử hủy/tạm ngừng/tiếp tục'


class DepositActionFee(models.Model):
    name = models.CharField(max_length=100, verbose_name='Tên phí cọc thẻ', help_text='Cọc thẻ ban đầu, Mất thẻ')
    customer_type = models.ForeignKey(CustomerType, verbose_name='Loại khách hàng', null=True, blank=True,
                                      on_delete=models.SET_NULL)
    vehicle_type = models.ForeignKey(VehicleType, verbose_name='Loại xe', null=True, blank=True,
                                     on_delete=models.SET_NULL)
    fee = models.IntegerField(default=0, verbose_name='Phí')

    def __unicode__(self):
        return u'%s' % (self.name)

    class Meta:
        verbose_name = u'Phí cọc thẻ'
        verbose_name_plural = 'Phí cọc thẻ'


class DepositPayment(models.Model):
    PAYMENT_METHOD_CHOICES = (
        ('TM', u'Tiền mặt'),
        ('CK', u'Chuyển khoản')
    )
    customer = models.ForeignKey(Customer, related_name='deposit_payment_customer', verbose_name=u'Khách hàng',
                                 null=True, blank=True, on_delete=models.SET_NULL)
    receipt_id = models.IntegerField(null=True, verbose_name=u'Ma phiếu thu')
    receipt_number = models.IntegerField(null=True, verbose_name=u'Số phiếu thu')
    payment_date = models.DateTimeField(verbose_name=u'Thanh toán lần cuối', auto_now=True)
    payment_fee = models.IntegerField(default=0, verbose_name=u'Tổng số tiền')
    payment_method = models.CharField(max_length=20, choices=PAYMENT_METHOD_CHOICES, verbose_name=u'Thanh toán',
                                      default=PAYMENT_METHOD_CHOICES[0][0])
    notes = models.CharField(max_length=200, verbose_name=u'Ghi chú', blank=True, default="")
    staff = models.ForeignKey(UserProfile, verbose_name=u'Người thực hiện', null=True, blank=True,
                              on_delete=models.SET_NULL)

    def __unicode__(self):
        return u'Mã cọc %s' % self.pk

    class Meta:
        verbose_name = u'Danh sách cọc thẻ'
        verbose_name_plural = u'Danh sách cọc thẻ'


class DepositPaymentDetail(models.Model):
    deposit_payment = models.ForeignKey(DepositPayment, verbose_name=u'Lượt thanh toán')
    vehicle_registration = models.ForeignKey(VehicleRegistration, verbose_name=u'Vé xe', null=True, blank=True,
                                             on_delete=models.SET_NULL)
    vehicle_number = models.CharField(verbose_name=u'Biển số xe', max_length=255, blank=True, default="")
    deposit_action_fee = models.ForeignKey(DepositActionFee, verbose_name=u'Phí cọc thẻ', null=True, blank=True,
                                           on_delete=models.SET_NULL)
    deposit_payment_detail_fee = models.IntegerField(verbose_name=u'Thành tiền', default=0)

    def __unicode__(self):
        return u'Chi tiết cọc thẻ %s' % (self.pk)

    class Meta:
        verbose_name = u'Chi tiết đóng cọc thẻ'
        verbose_name_plural = u'Chi tiết đóng cọc thẻ'


##
# CLAIM PROMOTION
##


class ClaimPromotion(models.Model):
    id = models.CharField(primary_key=True, max_length=100)
    parking_session = models.ForeignKey(ParkingSession, verbose_name='Parking Session ID', null=True, blank=True)
    # parking_fee_session = models.ForeignKey(ParkingFeeSession, verbose_name='Parking Fee Session ID', null=True, blank=True)
    # card = models.ForeignKey(Card,verbose_name='Card ID', null=True, blank=True)
    # vehicle_number = models.CharField(verbose_name='Vehicle number', max_length=100)
    # vehicle_type = models.IntegerField(verbose_name='Vehicle type', default=0)
    user = models.ForeignKey(User, verbose_name='User ID', null=True, blank=True)
    amount_a = models.IntegerField(verbose_name='Amount A', default=0)
    amount_b = models.IntegerField(verbose_name='Amount B', default=0)
    amount_c = models.IntegerField(verbose_name='Amount C', default=0)
    amount_d = models.IntegerField(verbose_name='Amount D', default=0)
    amount_e = models.IntegerField(verbose_name='Amount E', default=0)
    client_time = models.DateField(verbose_name='Client calculation time', null=True, blank=True)
    server_time = models.DateTimeField(verbose_name='Server created time', auto_now_add=True, db_index=True)
    used = models.BooleanField(verbose_name='Claim Promotion has been used?', default=False, db_index=True)
    notes = models.TextField(verbose_name='Notes', max_length=1000, null=True, blank=True)

class ClaimPromotionV2(models.Model):
    old_id = models.CharField(max_length=250, null=True, blank=True, unique=True)
    parking_session = models.ForeignKey(ParkingSession, verbose_name='Parking Session ID', null=True, blank=True)
    user = models.ForeignKey(User, verbose_name='User ID', null=True, blank=True)
    amount_a = models.IntegerField(verbose_name='Amount A', default=0)
    amount_b = models.IntegerField(verbose_name='Amount B', default=0)
    amount_c = models.IntegerField(verbose_name='Amount C', default=0)
    amount_d = models.IntegerField(verbose_name='Amount D', default=0)
    amount_e = models.IntegerField(verbose_name='Amount E', default=0)
    client_time = models.DateField(verbose_name='Client calculation time', null=True, blank=True)
    server_time = models.DateTimeField(verbose_name='Server created time', db_index=True) # auto_now_add=True
    used = models.BooleanField(verbose_name='Claim Promotion has been used?', default=False, db_index=True)
    notes = models.TextField(verbose_name='Notes', max_length=1000, null=True, blank=True)

class ClaimPromotionBill(models.Model):
    claim_promotion = models.ForeignKey(ClaimPromotion, null=True, blank=True, related_name="promotion_bills_v1")
    company_info = models.CharField(verbose_name='Company info', max_length=512, null=True, blank=True)
    date = models.DateTimeField(verbose_name='Date', null=True, blank=True)
    bill_number = models.CharField(verbose_name='Bill number', max_length=250, null=True, blank=True)
    bill_amount = models.IntegerField(verbose_name='Bill amount', default=0)
    notes = models.TextField(verbose_name='Notes', max_length=1000, null=True, blank=True)


class ClaimPromotionBillV2(models.Model):
    claim_promotion = models.ForeignKey(ClaimPromotionV2, null=True, blank=True, related_name="promotion_bills")
    company_info = models.CharField(verbose_name='Company info', max_length=512, null=True, blank=True)
    date = models.DateTimeField(verbose_name='Date', null=True, blank=True)
    bill_number = models.CharField(verbose_name='Bill number', max_length=250, null=True, blank=True)
    bill_amount = models.IntegerField(verbose_name='Bill amount', default=0)
    notes = models.TextField(verbose_name='Notes', max_length=1000, null=True, blank=True)


class ClaimPromotionCoupon(models.Model):
    claim_promotion = models.ForeignKey(ClaimPromotion, null=True, blank=True, related_name="promotion_coupons_v1")
    company_info = models.CharField(verbose_name='Company info', max_length=512, null=True, blank=True)
    coupon_code =  models.CharField(verbose_name='Company info', max_length=250, null=True, blank=True)
    coupon_amount =  models.IntegerField(verbose_name='Coupon amount', default=0)
    notes = models.TextField(verbose_name='Notes', max_length=1000, null=True, blank=True)


class ClaimPromotionCouponV2(models.Model):
    claim_promotion = models.ForeignKey(ClaimPromotionV2, null=True, blank=True, related_name="promotion_coupons")
    company_info = models.CharField(verbose_name='Company info', max_length=512, null=True, blank=True)
    coupon_code =  models.CharField(verbose_name='Company info', max_length=250, null=True, blank=True)
    coupon_amount =  models.IntegerField(verbose_name='Coupon amount', default=0)
    notes = models.TextField(verbose_name='Notes', max_length=1000, null=True, blank=True)
##2018Oct17
class VehicleBalcklist(models.Model):
    vehicle_type=models.ForeignKey(VehicleType,verbose_name='Loại xe', null=True, blank=True,
                                     on_delete=models.SET_NULL, db_column="vehicle_type")
    vehicle_number=models.CharField(verbose_name='Biển số', max_length=50, null=False, blank=False,unique=True)
    notes = models.TextField(verbose_name='Ghi chú', max_length=1000, null=True, blank=True)
    class Meta:
        verbose_name = u'Biển số đen'
        verbose_name_plural = u'Biển số đen'
    def __unicode__(self):
        return self.vehicle_number

class CurrentBalcklistState(models.Model):
    blacklist = models.ForeignKey(VehicleBalcklist, verbose_name='Biển số đen', null=True, blank=True,
                                     on_delete=models.SET_NULL,db_column="blacklist")
    gate = models.ForeignKey(Terminal, verbose_name='Cổng kiểm soát', null=True, blank=True,db_column="gate")
    user = models.ForeignKey(User, verbose_name='Nhân viên kiểm soát', null=True, blank=True,db_column="user")
    date = models.DateTimeField(verbose_name='Thời điểm', null=True, blank=True)
    stateparking = models.IntegerField(verbose_name='Trình trạng ra vào')
    state =models.IntegerField(verbose_name='Trạng thái')
    notes = models.TextField(verbose_name='Ghi chú', max_length=1000, null=True, blank=True)
    class Meta:
        verbose_name = u'Thực trạng biển số đen'
        verbose_name_plural = u'Thực trạng biển số đen'

    def __unicode__(self):
        return self.VehicleBalcklist.vehicle_number
##
#2018Aug14
class ClaimPromotionGroupTenant(models.Model):

    groupname = models.CharField(verbose_name='Tên loại', max_length=512, db_index=True,help_text='Claimed sẽ không thành công nếu tên chứa các ký tự đặc biệt: ` , \', \", ~, +, -, *, /, &, ^, %, $, ...')
    updated = models.DateTimeField(auto_now=True)
    class Meta:
        verbose_name = u'Loại gian hàng kinh doanh'
        verbose_name_plural = u'Loại gian hàng kinh doanh'

    def __unicode__(self):
        return self.groupname


#2018Aug14
class ClaimPromotionTenant(models.Model):
    group_tenant = models.ForeignKey(ClaimPromotionGroupTenant, verbose_name=u"Loại gian hàng",
                                     db_column="group_tenant")
    name = models.CharField(verbose_name='Tên đầy đủ', max_length=512, db_index=True,help_text='Claimed sẽ không thành công nếu tên chứa các ký tự đặc biệt: ` , \', \", ~, +, -, *, /, &, ^, %, $, ...')
    short_name = models.CharField(verbose_name='Tên viết tắt', max_length=512, db_index=True,help_text='Claimed sẽ không thành công nếu tên chứa các ký tự đặc biệt: ` , \', \", ~, +, -, *, /, &, ^, %, $, ...')
    updated = models.DateTimeField(auto_now=True)
    class Meta:
        verbose_name = u'Gian hàng kinh doanh'
        verbose_name_plural = u'Gian hàng kinh doanh'

    def __unicode__(self):
        return u'Gian hàng %s' % (self.short_name)
class ClaimPromotionVoucher(models.Model):
    name = models.CharField(verbose_name='Tên', max_length=512, db_index=True)
    short_value =  models.CharField(verbose_name='Giá trị viết tắt', max_length=512, db_index=True)
    value = models.IntegerField(verbose_name='Giá trị', default=0)
    updated = models.DateTimeField(auto_now=True)

    class Meta:
        verbose_name = u'Voucher'
        verbose_name_plural = u'Voucher'

    def __unicode__(self):
        return u'Voucher %s' % (self.short_value)


class FeeAdjustment(models.Model):
    # vehicle_type = models.IntegerField(choices=VEHICLE_TYPE, verbose_name=u"Loại xe", help_text=u'ID loại xe')
    vehicle_type = models.ForeignKey(VehicleType, verbose_name=u"Loại xe", help_text=u'ID loại xe', db_column="vehicle_type")
    time = models.DateField(verbose_name='Ngày chỉnh', blank=True, null=True)
    fee =  models.IntegerField(verbose_name='Phí', default=0)
    remark = models.TextField(verbose_name='Ghi chú', max_length=1000, null=True, blank=True)
    reload(sys)
    sys.setdefaultencoding('utf-8')
    class Meta:
        verbose_name = u'Điều chỉnh phí'
        verbose_name_plural = u'Điều chỉnh phí'

    def __unicode__(self):
        return '%s' % self.vehicle_type

def get_setting(key, name, default_value, notes=''):
    try:
        setting_item = ParkingSetting.objects.get(key=key)
        return setting_item.value
    except ParkingSetting.DoesNotExist:
        ParkingSetting.objects.create(key=key, name=name, value=default_value, notes=notes)
        return default_value


def decode_vehicle_type(vehicle_type):
    end = vehicle_type / 10000
    begin = vehicle_type - end * 10000
    return begin, end


def encode_vehicle_type(begin, end):
    return end * 10000 + begin


def get_storaged_vehicle_type(vehicle_type):
    return decode_vehicle_type(vehicle_type)[1]


def load_vehicle_type():
    try:
        if VehicleType.objects.all().count() == 0:
            for cat in VEHICLE_TYPE_CATEGORY:
                VehicleType.objects.create(id=cat[0], category=cat[0], name=cat[1])
                # VehicleType.objects.create(id=0, name='Unknown')
                # VehicleType.objects.create(id=1, name='Car')
                # VehicleType.objects.create(id=2, name='Bike')
                # VehicleType.objects.create(id=3, name='Electric Bicycle')
        global VEHICLE_TYPE
        vehicles_type = VehicleType.objects.all()
        a = list()
        for vehicle_type in vehicles_type:
            a.append((vehicle_type.id, vehicle_type.name))
        VEHICLE_TYPE = tuple(a)
        return VEHICLE_TYPE
    except:
        return ()


def load_nghia_vehicle_type():
    # vehicle_type_data = list()
    vehicle_type_dict = dict()
    for type in VehicleType.objects.all():
        # vehicle_type_data.append({'vehicle_id': str(type.id), 'vehicle_name': type.name})
        vehicle_type_dict[get_storaged_vehicle_type(type.id)] = type.id
    return vehicle_type_dict

def load_vehicle_type_name():
    # vehicle_type_data = list()
    vehicle_type_dict = dict()
    for type in VehicleType.objects.all():
        # vehicle_type_data.append({'vehicle_id': str(type.id), 'vehicle_name': type.name})
        vehicle_type_dict[get_storaged_vehicle_type(type.id)] = type.name
    return vehicle_type_dict

def load_card_type():
    try:
        if CardType.objects.all().count() == 0:
            CardType.objects.create(id=0, name=u'Khách vãng lai')
            CardType.objects.create(id=1, name=u'Nhân viên Kiến Việt')
            CardType.objects.create(id=2, name=u'Nhân viên AEON')
            CardType.objects.create(id=3, name=u'Quản trị hệ thống')
        global CARD_TYPE
        cards_type = CardType.objects.all()
        a = list()
        for card_type in cards_type:
            a.append((card_type.id, card_type.name))
        CARD_TYPE = tuple(a)
        return CARD_TYPE
    except:
        return ()


load_vehicle_type()
load_card_type()


def init_app_config():
    # TODO: init app config
    # Begin Init settings
    get_setting('parking_name', u'Tên bãi xe', 'Green Parking')
    get_setting('car_limit_slot', u'Số chỗ ô tô', 500)
    get_setting('bike_limit_slot', u'Số chỗ xe máy', 5000)
    get_setting('log_server', u'Địa chỉ máy chủ Log', '192.168.1.41')
    get_setting('days_of_maintain_data', u'Số ngày lưu giữ dữ liệu', 60)
    get_setting('can_check_in_out_when_in_effective_range', u'Chỉ được vào ra khi đăng ký xe còn hiệu lực', 1)
    get_setting('max_vehicle_registration_debt_days', u'Số ngày nợ vé tháng tối đa', 10)
    get_setting('invoice_pdf_form', u'Mẫu phiếu thu PDF', 'invoice-form.pdf')
    get_setting('num_parking_fee_collection_day_before_expired', u'Số ngày nhắc phí trước khi hết hạn', 10)
    get_setting('can_renew_different_vehicle_registration_expired_date',
                u'Cho phép gia hạn các xe đăng ký khác thời hạn',
                0)
    get_setting('next_receipt_number', u'Số phiếu thu tiếp theo', 1)
    get_setting('print_receipt_number', u'In số phiếu thu', 1)
    get_setting('order_report_template', u'Mẫu nội dung xuất hoá đơn',
                u'{"ve_thang": "Công ty KV thu hộ phí giữ xe từ {valid_from} - {valid_to}", "coc_the": "Thu hộ phí cọc thẻ"}',
                'Trường bắt buộc:\n "ve_thang" - nội dung xuất hóa đơn vé tháng.\n "coc_the" - nội dung xuất hóa đơn cọc thẻ.\nCác truờng thông tin có thể sử dụng (đóng trong cặp dấu {}): {valid_from}, {valid_to}.')
    get_setting('logo_report', u'Mẫu logo dùng trong báo cáo', 'logo_report.png')
    # End Init settings

def update_or_create_superuser():
    try:
        user = User.objects.get(username="admin")
    except User.DoesNotExist:
        # Create default superuser
        user = User.objects.create_user('admin', 'admin@squarebitinc.com', 'nopass')
        user.is_staff = True
        user.is_superuser = True
        user.save()
        print "Create superuser successfully"
    finally:
        try:
            user = User.objects.get(username="admin")
            UserProfile.objects.create(user=user, fullname='Quản Trị Viên', staff_id='ADMIN0001')
            print "Update userprofile successfully."
        except Exception as e:
            print "Update userprofile failed, maybe userprofile did exist!"
