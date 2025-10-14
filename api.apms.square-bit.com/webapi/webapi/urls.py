from django.conf.urls import patterns, include, url

from app import views
urlpatterns = patterns(
    'app.views',
    ##Urls using for Mobile APP
    url('api/mobile/', include('app.mobile_urls')),
    ##End Mobile APP
    url(r'^api/health/', 'get_health'),
    url(r'^api/clearcache/', 'clearcache'),
    url(r'^api/parking-name/', 'get_parking_name'),
    url(r'^api/cards/available/$', 'get_available_card'),
    url(r'^api/global-config/$', 'get_global_config'),
    url(r'^api/cards/$', views.CardListView.as_view(), name='card-list'),
    url(r'^api/cards/collect/$', views.CardCollectedView.as_view(), name='card-collect'),
    url(r'^api/cards/plate-collect/$', views.PlateCollectedView.as_view(), name='plate-collect'),
    url(r'^api/cards/(?P<card_id>[-\w]+)/$', views.CardDetailView.as_view(), name='card-detail'),
    url(r'^api/cards/(?P<card_id>[-\w]+)/checkin/$', views.CardCheckInView.as_view(), name='card-checkin-detail'),
    url(r'^api/cards/(?P<card_id>[-\w]+)/checkout/$', views.CardCheckOutView.as_view(), name='card-checkout-detail'),
    url(r'^api/cards/(?P<card_id>[-\w]+)/exception-checkout/$', views.CardExceptionCheckOutView.as_view(), name='card-exception-checkout-detail'),
    url(r'^api/cards/(?P<card_id>[-\w]+)/imagehosts/$', views.CardCheckInImageView.as_view(), name='card-checkinimage-detail'),
    url(r'^api/terminal-groups/$', views.TerminalGroupListView.as_view(), name='terminal-group-list'),
    url(r'^api/terminals/$', views.TerminalListView.as_view(), name='terminal-list'),
    url(r'^api/terminals/(?P<id>[0-9]+)/$', views.TerminalDetailView.as_view(), name='terminal-detail'),
    url(r'^api/terminals/(?P<id>[0-9]+)/health/$', views.TerminalHealthView.as_view(), name='terminal-health-detail'),
    url(r'^api/terminals/(?P<id>[0-9]+)/lanes/$', views.TerminalLaneView.as_view(), name='terminal-lane-list'),
    url(r'^api/terminals/(?P<id>[0-9]+)/timeout/$', views.TerminalTimeOutView.as_view(), name='terminal-timeout-detail'),
    url(r'^api/lanes/$', views.LaneListView.as_view(), name='lane-list'),
    url(r'^api/lanes/(?P<id>[0-9]+)/$', views.LaneDetailView.as_view(), name='lane-detail'),
    url(r'^api/cameras/$', views.CameraListView.as_view(), name='camera-list'),
    url(r'^api/cameras/(?P<id>[0-9]+)/$', views.CameraDetailView.as_view(), name='camera-detail'),
    url(r'^api/users/login/$', views.UserLoginView.as_view(), name='login-detail'),
    url(r'^api/users/login-by-card/$', views.UserCardLoginView.as_view(), name='login-card-detail'),
    url(r'^api/users/logout/$', views.UserLogoutView.as_view(), name='logout-detail'),
    url(r'^api/parking-sessions/$', views.CardCheckInSearchView.as_view(), name='checkin-search-list'),
    url(r'^api/parking-sessions/search/$', views.ParkingSessionSearchView.as_view(), name='parking-session-search-list'),
    url(r'^api/parking-sessions/(?P<id>[0-9]+)/$', views.ParkingSessionUpdateView.as_view(), name='parking-session-update'),
    url(r'^api/image-replication/$', views.ImageReplicationView.as_view(), name='image-replication-detail'),
    url(r'^api/card-types/$', views.CardTypeListView.as_view(), name='card-type-list'),
    url(r'^api/vehicle-types/$', views.VehicleTypeListView.as_view(), name='vehicle-type-list'),
    url(r'^api/vehicle-type-categories/$', 'get_vehicle_type_category'),
    url(r'^api/statistics/$', views.StatisticsView.as_view(), name='statistics'),
    url(r'^api/claim-promotion/search/$', views.ClaimPromotionSearchView.as_view(), name='claim-promotion-search'),
    url(r'^api/claim-promotion/$', views.ClaimPromotionCreateView.as_view(), name='claim-promotion'),
    url(r'^api/claim-reduction/$', views.ClaimPromotionCallReduction.as_view(), name='claim-reduction'),
    url(r'^api/time-info/$', views.TimeInfoView.as_view(), name='time-info'),
    url(r'^api/claim-promotion/tenants$', views.ClaimPromotionTenantListView.as_view(), name='claim-promotion-tenant'),
    url(r'^api/claim-promotion/vouchers', views.ClaimPromotionVoucherListView.as_view(), name='claim-promotion-voucher'),
    url(r'^api/calculateparkingfee/$', views.CalculateParkingFeeView.as_view(), name='calculateParking'),
    #chechbill exists --2017-12-07
    url(r'^api/checkbill-info/(?P<billdate>[-\w]+)/(?P<company>[-\w]+)/(?P<billcode>[-\w]+)$', views.CheckBillInfoView.as_view(), name='checkbill-info'),
    # 2018Dec13
    url(r'^api/sessionslots/$', views.ParkingSessionSlotView.as_view(), name='sessionslots'),
    # 2018Dec13
    url(r'^api/forcedbarier/$', views.ForcedBarierView.as_view(), name='forcedBarier'),

    url(r'^api/findandnotifyblacklist/$', views.FindAndNotifyBlacklistView.as_view(), name='findandnotifyblacklist'),
    url(r'^api/findandnotifytoservice/$', views.FindAndNotifyToService.as_view(), name='findandnotifytoservice'),
    url(r'^api/recallfee/', 'get_recallfee'),
    url(r'^api/cards/(?P<card_id>[-\w]+)/voucher/$', views.CreateVoucher.as_view(), name='card-voucher-detail'),
    url(r'^api/cards/(?P<card_id>[-\w]+)/deletevoucher/$', views.DeleteVoucher.as_view(), name='card-voucher-delete'),
)


urlpatterns += patterns(
    '',
    url(r'^api-auth/', include('rest_framework.urls', namespace='rest_framework')),
    url(r'^api/docs/', include('rest_framework_swagger.urls'))
)

