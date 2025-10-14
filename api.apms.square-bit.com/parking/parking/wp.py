# -*- coding: utf-8 -*-
from django.utils.translation import ugettext_lazy as _
from django.core.urlresolvers import reverse

from wpadmin.utils import get_admin_site_name
from wpadmin.menu import items
from wpadmin.menu.menus import Menu

def get_version():
    default_version_number = '0.1_default'
    try:
        with open('changelog.txt') as f:
            firstline = f.readline()
            if firstline.find('VERSION') != -1:
                version_number = firstline.split('=')[1]
                return version_number
        return default_version_number
    except Exception as e:
        return default_version_number

class UserTopMenu(Menu):
    def my_user_check(self, user):
        """
        Custom helper method for hiding some menu items from not allowed users.
        """
        return user.groups.filter(name='users').exists()

    def init_with_context(self, context):
        admin_site_name = get_admin_site_name(context)

        # if 'django.contrib.sites' in settings.INSTALLED_APPS:
        # from django.contrib.sites.models import Site
        # site_name = Site.objects.get_current().name
        # site_url = 'http://' + Site.objects.get_current().domain
        # else:
        # site_name = _('Site')
        #     site_url = '/'

        self.children += [
            items.MenuItem(
                title=_(u"Green Parking"),
                icon='fa-leaf',
                url=reverse('%s:index' % admin_site_name),
                css_styles='font-size: 2.0em;',
                description=u'Trang chủ',
            ),
           items.MenuItem(
                title=u'Quản lý',
                description=u'Quản lý bãi xe',
                icon='fa-flag',
                children=[
                    items.MenuItem(
                        title=u'Thẻ',
                        url=reverse('admin:parking_card_changelist'),
                        description=u'Quản lý thẻ',
                    ),

                    items.MenuItem(
                        title=u'Báo cáo khoá thẻ',
                        url='/admin/report/CardStatus/',
                        description=u'Báo cáo khoá thẻ',
                    ),
                    items.MenuItem(
                        title=u'-----------------------------------',
                    ),
                    items.MenuItem(
                        title=u'Nhận dạng thẻ ePass',
                        url=reverse('admin:parking_epassprefixcheck_changelist'),
                        description=u'Nhận dạng thẻ ePass',
                    ),
                    items.MenuItem(
                        title=u'Sưu tập thẻ ePass',
                        url=reverse('admin:parking_epasscollected_changelist'),
                        description=u'Sưu tập thẻ ePass',
                    ),
                    items.MenuItem(
                        title=u'-----------------------------------',
                    ),
                    items.MenuItem(
                        title=u'Đối tác thanh toán ePass',
                        url=reverse('admin:parking_epasspartner_changelist'),
                        description=u'Đối tac thanh toán ePass',
                    ),
                    items.MenuItem(
                        title=u'API thanh toán ePass',
                        url=reverse('admin:parking_epassapi_changelist'),
                        description=u'API thanh toán ePass',
                    ),
                    items.MenuItem(
                        title=u'-----------------------------------',
                    ),
                    items.MenuItem(
                        title=u'Nhân viên',
                        icon='fa-group',
                        description=u'Quản lý nhân viên',
                        children=[
                            items.MenuItem(
                                title=u'Nhóm nhân viên',
                                url=reverse('admin:auth_group_changelist'),
                                description=u'Quản lý nhóm nhân viên',
                            ),
                            items.MenuItem(
                                title=u'Nhân viên',
                                url=reverse('admin:auth_user_changelist'),
                                description=u'Quản lý thông tin nhân viên',
                            ),
                            items.MenuItem(
                                title=u'B.c Lịch sử thao tác',
                                url='/admin/report/Transaction_history/',
                                description=u'B.c Lịch sử thao tác',
                            ),
                            items.MenuItem(
                                title=u'B.c Danh sách thẻ',
                                url='/admin/report/Report-Card/',
                                description=u'B.c Danh sách thẻ',
                            ),
                            items.MenuItem(
                                title=u'Báo cáo phân quyền',
                                url='/admin/report/Permission/',
                                description=u'Danh sách quyền các nhóm nhân viên',
                            ),
                            items.MenuItem(
                                title=u'Users List',
                                url='/admin/report/User-List/',
                                description=u'Danh sách người dùng hệ thống',
                            ),
                            items.MenuItem(
                                title=u'-----------------------------------',
                            ),
                            items.MenuItem(
                                title=u'Chấm công cá nhân',
                                url='/admin/Attendance/',
                                description=u'Thống kê số giờ công của từng nhân viên',
                            ),
                            items.MenuItem(
                                title=u'Chấm công tất cả',
                                url='/admin/AttendanceAllStaff/',
                                description=u'Thống kê số giờ công của tất cả nhân viên trong tháng',
                            ),
                            items.MenuItem(
                                title=u'-----------------------------------',
                            ),
                            items.MenuItem(
                                title=u'Tài khoản API',
                                url=reverse('admin:parking_apitoken_changelist'),
                                description=u'Tài khoản gọi API',
                            ),
                        ]
                    ),

                ]
            ),

            items.MenuItem(
                title=u'Khách hàng',
                description=u'Quản lý khách hàng',
                icon='fa-group',
                children=[
                    items.MenuItem(
                        title=u'Danh sách',
                        url=reverse('admin:parking_customer_changelist'),
                        description=u'Đăng ký thông tin khách hàng & thông tin xe',

                    ),
                    items.MenuItem(
                        title=u'Lọc khách hàng',
                        url='/admin/customer/Search/',
                        description=u'Lọc thông tin khách hàng',
                    ),

                    # items.MenuItem(
                    # items.MenuItem(
                    #     title=u'Nhập khách hàng',
                    #     url='/admin/customer/BulkImport/',
                    #     description=u'Nhắc phí',
                    # ),

                    items.MenuItem(
                        title=u'-----------------------------------',
                    ),

                    # items.MenuItem(
                    #     title=u'Báo cáo khách hàng',
                    #     url='/admin/report/ExpiredVehicleRegistrationCollection',
                    #     description=u'Nhắc phí',
                    # ),
                    # items.MenuItem(
                    #     title=u'Báo cáo phí vé tháng',
                    #     url='/admin/report/TicketPayment/',
                    #     description=u'Báo cáo phí vé tháng',
                    # ),
                    items.MenuItem(
                        title=u'Season Detail Report',
                        url='/admin/report/ParkingSession',
                        description=u'Season Detail Report',
                    ),
                    items.MenuItem(
                        title=u'Daily Record Compact Report',
                        url='/admin/report/daily_record_compact',
                        description=u'Daily Record Compact Report',
                    ),
                    items.MenuItem(
                        title=u'Season Parking Report',
                        url='/admin/report/ParkingSessionCancellation',
                        description=u'Báo cáo Season Parking Report',
                    ),
                    items.MenuItem(
                        title=u'Báo cáo tình trạng xe',
                        url='/admin/report/VehicleRegistrationStatus',
                        description=u'Báo cáo tình trạng xe khách hàng',
                    ),

                    items.MenuItem(
                        title=u'-----------------------------------',
                    ),

                    items.MenuItem(
                        title=u'Công ty',
                        url=reverse('admin:parking_company_changelist'),
                        description=u'Danh sách công ty',
                    ),


                    items.MenuItem(
                        title=u'Tòa nhà',
                        url=reverse('admin:parking_building_changelist'),
                        description=u'Danh sách tòa nhà',
                    ),

                    items.MenuItem(
                        title=u'Căn hộ',
                        url=reverse('admin:parking_apartment_changelist'),
                        description=u'Danh sách căn hộ',
                    ),

                ]
            ),

            #
            # items.MenuItem(
            #     title=u'Đăng ký xe',
            #     description=u'Quản lý đăng ký xe',
            #     icon='fa-car',
            #     children=[
            #
            #         items.MenuItem(
            #             title=u'Đăng ký xe',
            #             url=reverse('admin:parking_vehicleregistration_changelist'),
            #             description=u'Xe khách hàng',
            #         ),
            #
            # ]),

            items.MenuItem(
                title=u'Lượt xe',
                description=u'Thống kê lượng xe vào ra',
                icon='fa-bar-chart-o',
                children=[
                    items.MenuItem(
                        title=u'Cho ra ngoại lệ',
                        url=reverse('admin:parking_parkingsession_changelist'),
                        description=u'Quản lý cho ra ngoại lệ',
                    ),

                    items.MenuItem(
                        title=u'Tra cứu',
                        url='/admin/SearchParkingSession',
                        description=u'Tìm kiếm thông tin các lượt xe',
                    ),
                    items.MenuItem(
                        title=u'Lọc thông tin xe',
                        url=reverse('admin:parking_vehicleregistration_changelist'),
                        description=u'Lọc Xe khách hàng',
                    ),

                    items.MenuItem(
                        title=u'Thống kê',
                        url='/admin/Statistics/',
                        description=u'Thống kê lượng xe vào ra bãi xe theo loại khách hàng và loại xe',
                    ),
                    items.MenuItem(
                        title=u'-----------------------------------',
                    ),


                    items.MenuItem(
                        title=u'Thống kê theo tỉnh',
                        url='/admin/StatisticsByLocation/',
                        description=u'Thống kê lượng xe vào ra bãi xe theo tỉnh thành và loại xe',
                    ),
                    items.MenuItem(
                        title='Biểu đồ',
                        url='/admin/Chart/',
                        description=u'Vẽ biểu đồ đường của lượng ra vào bãi giữ xe',
                    ),
                    items.MenuItem(
                        title=u'Xuất dữ liệu',
                        url='/admin/ExportParkingSession/',
                        description=u'Kết xuất thông tin các lượt xe',
                    ),
                ]
            ),

            items.MenuItem(
                title=u'Báo cáo thu chi',
                icon='fa-money',
                description=u'Báo cáo thu chi',
                children=[
                    items.MenuItem(
                        title=u'Danh sách gia hạn',
                        url=reverse('admin:parking_ticketpayment_changelist'),
                        description=u'Lượt đóng tiền',
                    ),
                    items.MenuItem(
                        title=u'Danh sách cọc thẻ',
                        url=reverse('admin:parking_depositpayment_changelist'),
                        description=u'Danh sách cọc thẻ',
                    ),

                    items.MenuItem(
                        title=u'Danh sách phiếu thu',
                        url=reverse('admin:parking_receipt_changelist'),
                        description=u'Danh sách phiếu thu',
                    ),

                    items.MenuItem(
                        title=u'-----------------------------------',
                    ),

                    items.MenuItem(
                        title=u'Giảm trừ phí vãng lai',
                        url=reverse('admin:parking_feeadjustment_changelist'),
                        description=u'Danh sách giảm trừ phí vãng lai',
                    ),
                    items.MenuItem(
                        title=u'Báo cáo xe tồn bãi',
                        url='/admin/report/ParkingInTheYard/',
                        description=u'Báo cáo lưu lượt xe tồn bãi',
                    ),
                    items.MenuItem(
                        title=u'Báo trình trạng ghi nhận biển số xe',
                        url='/admin/report/ParkingVehicleNumberState/',
                        description=u'Báo trình trạng ghi nhận biển số xe',
                    ),
                    items.MenuItem(
                        title=u'Báo cáo phí vãng lai',
                        url='/admin/report/ParkingFee/',
                        description=u'Báo cáo tổng và chi tiết phí vãng lai',
                    ),
                    #
                    # items.MenuItem(
                    #     title=u'Báo cáo xuất hoá đơn',
                    #     url='/admin/report/OrderInfo/',
                    #     description=u'Báo cáo xuất hoá đơn',
                    # ),
                    items.MenuItem(
                        title=u'Báo cáo tổng doanh thu',
                        url='/admin/report/ParkingFee-TicketPayment/',
                        description=u'Báo cáo tổng doanh thu',
                    ),
                    items.MenuItem(
                        title=u'Báo cáo Hourly/Month',
                        url='/admin/report/ParkingHourlyNew',
                        description=u'Báo cáo Daily Record Moi',
                    ),
                    items.MenuItem(
                        title=u'B.C Redemption/Month',
                        url='/admin/report/ParkingRedemptionNew/',
                        description=u'Báo cáo Parking Redemption Moi',
                    ),
                    items.MenuItem(
                        title=u'-----------------------------------',
                    ),
                    items.MenuItem(
                        title=u'Báo cáo Claim Promotion',
                        url='/admin/report/ParkingRedemption/',
                        description=u'Báo cáo Claim Promotion',
                    ),
                    items.MenuItem(
                        title=u'Daily Record',
                        url='/admin/report/ParkingHourly',
                        description=u'Báo cáo Daily Record',
                    ),
                    items.MenuItem(
                        title=u'Báo cáo cưỡng bức Barier',
                        url='/admin/report/BarierForced',
                        description=u'Báo cáo cưỡng bức Barier',
                    )
                    , items.MenuItem(
                        title=u'Báo cáo biển số đen',
                        url='/admin/report/blacklist',
                        description=u'Báo cáo biển số đen',
                    )
                ]
            ),

            items.MenuItem(
                title=u'Cấu hình',
                icon='fa-cogs',
                description=u'Cấu hình hệ thống',
                children=[
                     items.MenuItem(
                        title=u'v{0}'.format(get_version()),
                        description=u'Version {0}'.format(get_version()),
                        icon='fa-none',
                    ),

                    items.MenuItem(
                        title=u'Thiết lập chung',
                        url=reverse('admin:parking_parkingsetting_changelist'),
                        description=u'Thiết lập cấu hình chung cho toàn bộ hệ thống',
                    ),

                    items.MenuItem(
                        title=u'-----------------------------------',
                    ),

                    items.MenuItem(
                        title=u'Sao chép ảnh',
                        url=reverse('admin:parking_imagereplicationsetting_changelist'),
                        description=u'Thiết lập cấu hình sao chép ảnh của hệ thống',
                    ),
                    items.MenuItem(
                        title=u'Loại thẻ',
                        url=reverse('admin:parking_cardtype_changelist'),
                        description=u'Quản lý loại thẻ',
                    ),
                    items.MenuItem(
                        title=u'Loại xe',
                        url=reverse('admin:parking_vehicletype_changelist'),
                        description=u'Quản lý loại xe',
                    ),
                    items.MenuItem(
                        title=u'Cổng',
                        url=reverse('admin:parking_terminalgroup_changelist'),
                        description=u'Quản lý Cổng',
                    ),
                    items.MenuItem(
                        title=u'Máy trạm',
                        url=reverse('admin:parking_terminal_changelist'),
                        description=u'Quản lý máy trạm',
                    ),

                    items.MenuItem(
                        title=u'Loại khách hàng',
                        url=reverse('admin:parking_customertype_changelist'),
                        description=u'Quản lý loại khách hàng',
                    ),

                    items.MenuItem(
                        title=u'-----------------------------------',
                    ),
                    #2018-01-05
                    items.MenuItem(
                        title=u'Cấu hình phí vãng lai',
                        url='/admin/support/configfee/',
                        description=u'Cấu hình phí vãng lai',
                    ),
                    #
                    # items.MenuItem(
                    #     title=u'Phí xe vãng lai',
                    #     url=reverse('admin:parking_parkingfee_changelist'),
                    #     description=u'Quản lý phí gửi xe',
                    # ),
                    items.MenuItem(
                        title=u'Phí vé tháng',
                        url=reverse('admin:parking_levelfee_changelist'),
                        description=u'Quản lý Mức phí vé tháng',
                    ),
                    items.MenuItem(
                        title=u'Phí cọc thẻ',
                        url=reverse('admin:parking_depositactionfee_changelist'),
                        description=u'Quản lý Mức phí đóng cọc thẻ',
                    ),

                    items.MenuItem(
                        title=u'-----------------------------------',
                    ),
                    items.MenuItem(
                        title=u'Loại gian hàng',
                        url=reverse('admin:parking_claimpromotiongrouptenant_changelist'),
                        description=u'Loại gian hàng',
                    ),
                    items.MenuItem(
                        title=u'Cấu hình Gian hàng',
                        url=reverse('admin:parking_claimpromotiontenant_changelist'),
                        description=u'Cấu hình Gian hàng',
                    ),
                    items.MenuItem(
                        title=u'Cấu hình Voucher',
                        url=reverse('admin:parking_claimpromotionvoucher_changelist'),
                        description=u'Cấu hình Voucher',
                    ),
                    items.MenuItem(
                        title=u'Cấu hình Ô đậu',
                        url=reverse('admin:parking_slot_changelist'),
                        description=u'Cấu hình Ô đậu',
                    ),
                    items.MenuItem(
                        title=u'Biển số đen',
                        url=reverse('admin:parking_vehiclebalcklist_changelist'),
                        description=u'Biển số đen',
                    ),
                    items.MenuItem(
                        title=u'-----------------------------------',
                    ),
                    items.MenuItem(
                        title=u'Quy tắc tính VAT',
                        url=reverse('admin:parking_invoicetaxrule_changelist'),
                        description=u'Quy tắc tính VAT',
                    ),
                    items.MenuItem(
                        title=u'Đối tác xuất hóa đơn',
                        url=reverse('admin:parking_partnerinvoice_changelist'),
                        description=u'Đối tác xuất hóa đơn',
                    ),
                    items.MenuItem(
                        title=u'Tài khoản chứng thực',
                        url=reverse('admin:parking_invoiceconnector_changelist'),
                        description=u'Tài khoản chứng thực',
                    ),
                    items.MenuItem(
                        title=u'Định nghĩa API hóa đơn',
                        url=reverse('admin:parking_invoiceapiinitation_changelist'),
                        description=u'Định nghĩa API hóa đơn',
                    ),
                    items.MenuItem(
                        title=u'Thông tin người mua',
                        url=reverse('admin:parking_invoicebuyer_changelist'),
                        description=u'Thông tin người mua',
                    ),
                    items.MenuItem(
                        title=u'-----------------------------------',
                    ),
                    items.MenuItem(
                        title=u'Hóa đơn bán lẻ',
                        url=reverse('admin:parking_retailinvoice_changelist'),
                        description=u'Hóa đơn bán lẻ',
                    ),
                    items.MenuItem(
                        title=u'Hóa đơn tổng hợp',
                        url=reverse('admin:parking_consolidatedinvoice_changelist'),
                        description=u'Hóa đơn tổng hợp',
                    ),

                ]
            ),

            #     items.MenuItem(
            #     icon='none',
            #     title=_(u"PHASE II"),
            #     css_styles='font-size: 2.0em;',
            #     description=u'Phase 2',
            # ),
            items.UserTools(
                css_styles='float: right;',
                is_user_allowed=lambda user: user.is_staff,
            ),
        ]


class UserLeftMenu(Menu):
    def is_user_allowed(self, user):
        """
        Only users that are in 'users' group are allowed to see this menu.
        """
        return user.groups.filter(name='users').count()

    def init_with_context(self, context):
        if self.is_user_allowed(context.get('request').user):
            admin_site_name = get_admin_site_name(context)

            self.children += [
                items.MenuItem(
                    title=_('Dashboard'),
                    icon='fa-tachometer',
                    url=reverse('%s:index' % admin_site_name),
                    description=_('Dashboard'),
                ),
            ]
