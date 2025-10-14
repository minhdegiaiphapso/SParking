# -*- coding: utf-8 -*-

from django.conf.urls import patterns, include, url
from django.views.generic import RedirectView
from django.contrib import admin
from adminplus.sites import AdminSitePlus
from autocomplete_light import autodiscover
from parking.models import init_app_config

autodiscover()

admin.site = AdminSitePlus()
admin.autodiscover()
urlpatterns = patterns('',
                       url(r'^$', RedirectView.as_view(url='/admin/')),
                       url(r'^admin/', include(admin.site.urls)),
                       url(r'^admin/get_users', 'parking.support.get_users'),

                       url(r'^admin/get_card_labels', 'parking.support.get_card_labels'),
                       url(r'^autocomplete/', include('autocomplete_light.urls')),
                       url(r'^api/terminals/$', 'parking.views.get_terminals', name='get_terminals'),
                       url(r'^api/servers/$', 'parking.views.get_servers', name='get_servers'),
                       url(r'^api/google-chart/$', 'parking.views.get_google_chart_script', name='get_google_chart_script'),

                       # Support APIs
    ##2018-04-11
    url(r'^get/regisfeedetail/(?P<cardtype>\d+)/(?P<vehicletype>\d+)$', 'parking.views.get_regisfeedetail', name='get_regisfeedetail'),
                       url(r'^get/callfeecomplex/(?P<feeid>\d+)/(?P<checkin>\d{14})/(?P<checkout>\d{14})$', 'parking.views.callfeetestcomplex', name='callfeetestcomplex'),
                       url(r'^get/callfeeactive/(?P<feeid>\d+)/(?P<checkin>\d{14})/(?P<checkout>\d{14})/(?P<expireddate>\d{14})$', 'parking.views.callfeetestactive', name='callfeetestactive'),
                       # url(r'^get/callfee24h/(?P<feeid>\d+)/(?P<checkin>\d{14})/(?P<checkout>\d{14})$', 'parking.views.callfeetest24h', name='callfeetest24h'),
    # url(r'^get/callfee/(?P<feeid>\d+)$', 'parking.views.callfeetest', name='callfeetest'),
    url(r'^get/toolfeemenu/$', 'parking.views.get_toolfeemenus', name='get_toolfeemenus'),
                       url(r'^get/getactivepermission/(?P<menuid>\d+)$', 'parking.views.get_activepermission',
        name='get_activepermission'),
                       url(r'^get/getrootpermission/$', 'parking.views.get_rootpermission', name='get_rootpermission'),
                       url(r'^get/grouppermission/(?P<menuid>\d+)$', 'parking.views.get_grouppermission', name='get_grouppermission'),
                       url(r'^get/activeredemtion/(?P<groupid>\d+)/(?P<vehicletypeid>\d+)$', 'parking.views.get_activeredemption', name='get_activeredemption'),
                       url(r'^get/tenantsbygroup/(?P<id>\d+)$', 'parking.views.get_tenantsbygroup', name='get_tenantsbygroup'),
                       url(r'^get/gettenantgroup/$', 'parking.views.get_tenantgroup', name='get_tenantgroup'),  #get_settenant
    url(r'^get/gettenantgroupactive/$', 'parking.views.get_tenantgroupactive', name='get_tenantgroupactive'),  #get_settenant
    url(r'^get/settenants/$', 'parking.views.get_settenant', name='get_settenant'),  #get_settenant
    url(r'^get/samplelist/$', 'parking.views.get_samplelist', name='get_samplelist'),
                       url(r'^get/downloadfeeresult/$', 'parking.views.downloadfeeresult', name='downloadfeeresult'),
                       url(r'^get/downloadtemplate/$', 'parking.views.downloadtemplate', name='downloadtemplate'),
                       url(r'^get/invalidimport/$', 'parking.views.downloadinvalidimport', name='downloadinvalidimport'),
                       url(r'^get/cardtypes/$', 'parking.views.get_card_types', name='get_card_types'),
                       url(r'^get/vehicletypes/$', 'parking.views.get_vehicle_types', name='get_vehicle_types'),
                       url(r'^get/samplefeeregistation/$', 'parking.views.get_regisfeesample', name='get_regisfeesample'),
                       url(r'^get/regisfeesimilar/$', 'parking.views.get_regisfeesimilar', name='get_regisfeesample'),
                       url(r'^get/regisredemptionsimilar/$', 'parking.views.get_regisredemptionsimilar', name='get_regisfeesample'),
                       url(r'^get/samplefees/(?P<id>\d+)/(?P<typeid>\d+)$', 'parking.views.get_samplefees', name='get_samplefees'),
                       url(r'^post/samplefees/$', 'parking.views.post_samplefees', name='post_samplefees'),
                       url(r'^get/detailfee24h/(?P<sampleid>\d+)$', 'parking.views.get_detailfee24h', name='detailfee24h'),
                       url(r'^get/detailfeeNN/(?P<sampleid>\d+)$', 'parking.views.get_detailfeeNN', name='detailfeeNN'),
                       url(r'^get/detailfeecomplex/(?P<sampleid>\d+)$', 'parking.views.get_detailcomplex', name='get_detailcomplex'),  #get_detailredemtion
    url(r'^get/detailredemption/(?P<sampleid>\d+)$', 'parking.views.get_detailredemtion', name='get_detailredemtion'),  #get_detailredemtion
    url(r'^post/changepermission/$', 'parking.views.post_changepermission', name='post_changepermission'),
    url(r'^post/changepermissionroot/$', 'parking.views.post_changepermissionroot', name='post_changepermissionroot'),
                       url(r'^post/fee24h/$', 'parking.views.post_sample24h', name='post_sample24h'),
                       url(r'^post/complex/$', 'parking.views.post_complex', name='post_complex'),
                       url(r'^post/redemption/$', 'parking.views.post_redemption', name='post_redemption'),
                       url(r'^get/formularbill/$', 'parking.views.get_formulabill', name='get_formulabill'),  #post_formulafee
    url(r'^get/formularfee/$', 'parking.views.get_formulafee', name='get_formulafee'),  #post_formulafee
    url(r'^post/formulafee/$', 'parking.views.post_formulafee', name='post_formulafee'),  #post_removeitem
    url(r'^post/changestate/$', 'parking.views.post_changestate', name='post_changestate'),#copysample
                       url(r'^post/copysample/$', 'parking.views.post_copysample', name='post_copysample'),
                       url(r'^post/copyfromfee/$', 'parking.views.post_copyfromfee', name='post_copyfromfee'),
                       url(r'^post/copyfrombill/$', 'parking.views.post_copyfrombill', name='post_copyfrombill'),
                       url(r'^post/removeitem/$', 'parking.views.post_removeitem', name='post_removeitem'),
                       url(r'^post/removeitem1/$', 'parking.views.post_removeitem1', name='post_removeitem1'),
                       url(r'^post/formulabill/$', 'parking.views.post_formulabill', name='post_formulabill'),
                       url(r'^post/callredemtion/$', 'parking.views.post_callredemtion', name='post_callredemtion'),  #sampleregis,callredemtion
    url(r'^post/redemptionregis/$', 'parking.views.post_redemptionregis', name='post_redemptionregis'),  #sampleregis
    url(r'^post/redemptionregis/$', 'parking.views.post_redemptionregis', name='post_redemptionregis'),  #sampleregis
    url(r'^post/sampleregis/$', 'parking.views.post_sampleregis', name='post_sampleregis'),  #sampleregis
    url(r'^post/sampleregissimilar/$', 'parking.views.post_sampleregissimilar', name='post_sampleregis'),
    url(r'^post/redemptionregissimilar/$', 'parking.views.post_redemptionregissimilar', name='post_redemptionregissimilar'),
    url(r'^post/changegrouptenant/$', 'parking.views.post_changegrouptenant', name='post_changegrouptenant'),  #sampleregis
    url(r'^get/specialdate/$', 'parking.views.get_specialdate', name='get_specialdate'),  #posttocalandreport
    url(r'^post/specialdate/$', 'parking.views.post_specialdate', name='post_specialdate'),  #post_specialdate
    url(r'^post/posttenantgroup/$', 'parking.views.post_tenantgroup', name='post_tenantgroup'),
                       url(r'^post/posttocalandreport/$', 'parking.views.post_tocalandreport', name='post_tocalandreport'),  #posttocalandreport
    ##
    url(r'^get/currentuser/$', 'parking.views.get_current_user', name='get_current_user'),
                       url(r'^get/vehicleregistration/(?P<vehicle_registration_id>\d+)/$', 'parking.views.get_vehicle_registration_info', name='get_vehicle_registration_info'),
                       url(r'^get/ticketpayment/(?P<ticket_payment_id>\d+)/$', 'parking.views.get_ticket_payment_info', name='get_ticket_payment_info'),
                       url(r'^get/newexpireddate/(?P<day>\d{1,2})/(?P<month>\d{1,2})/(?P<year>\d{4})/(?P<month_duration>\d+)/(?P<day_duration>\d+)/(?P<level_fee>\d+)/$', 'parking.views.get_new_expired_date', name='get_new_expired_date'),
                       ##2017-12-29
    url(r'^get/get_fee/(?P<fdate>\d{8})/(?P<tdate>\d{8})/(?P<feepermonth>\d+)/$', 'parking.views.get_fee', name='get_fee'),
                       url(r'^post/renewalregistry/$', 'parking.views.post_renewalregistry', name='post_renewalregistry'),
                       url(r'^post/registry/$', 'parking.views.post_registry',name='post_registry'),
                       url(r'^admin/support/configfee/$', 'parking.views.configfee', name='configfee'),
                       ##
    url(r'^get/depositactionfeelist/(?P<vehicle_registration_id>\d+)/$', 'parking.views.get_deposit_action_fee_list', name='get_deposit_action_fee_list'),
                       url(r'^get/depositactionfee/(?P<deposit_action_fee_id>\d+)/$', 'parking.views.get_deposit_action_fee', name='get_deposit_action_fee'),
                       url(r'^checkvalid/(?P<vehicle_registration_id>\d+)/$', 'parking.views.check_validity', name='check_validity'),

                       # Views
    url(r'^admin/report/VehicleRegistrationStatus/$', 'parking.report.render_report_vehicle_registration_status', name='render_report_vehicle_registration_status'),
                       ## 2018-03-20
    url(r'^admin/report/User-List/$', 'parking.report.render_report_user_list', name='render_report_user_list'),
                       ## 2018-03-20
                       # url(r'^admin/report/vanglai_cbd/$', 'parking.report.render_report_vanglai_cbd',
                       #     name='render_report_vanglai_cbd'),
                       # url(r'^admin/report/thang_cbd/$', 'parking.report.render_report_thang_cbd',
                       #     name='render_report_thang_cbd'),
                       url(r'^admin/report/BarierForced/$', 'parking.report.render_report_barier_forced',name='render_report_barier_forced'),
                       url(r'^admin/report/ParkingFee/$', 'parking.report.render_report_parking_fee', name='render_report_parking_fee'),
    url(r'^admin/report/ParkingInTheYard/$', 'parking.report.render_report_parking_intheyard', name='render_report_parking_intheyard'),
                       url(r'^admin/report/ParkingFee_New/$', 'parking.report.render_report_parking_fee_new',
                           name='render_report_parking_fee_new'),

                       url(r'^admin/report/CardStatus/$', 'parking.report.render_report_card_status_change', name='render_report_card_status_change'),

                       url(r'^admin/report/ParkingHourly/$', 'parking.report.render_report_parking_hourly', name='render_report_parking_hourly'),
                       url(r'^admin/report/ParkingHourlyNew/$', 'parking.report.render_report_parking_hourly_new', name='render_report_parking_hourly_new'),
                       url(r'^admin/report/ParkingSession/$', 'parking.report.render_report_parking_session', name='render_report_parking_session'),
                       url(r'^admin/report/ParkingSessionCancellation/$', 'parking.report.render_report_parking_session_cancellation', name='render_report_parking_session_cancellation'),

                       #render_report_Configfee_history
                       url(r'^admin/report/configfee_history/$', 'parking.report.render_report_Configfee_history',
                           name='render_report_Transaction_history'),
                       url(r'^admin/report/Report-Card/$', 'parking.report.render_report_card', name='render_report_card'),

                       url(r'^admin/report/permission_configfee/$', 'parking.report.render_report_permission_configfee', name='render_report_permission_configfee'),
                       url(r'^admin/report/PermissionConfigFee/$', 'parking.report.render_report_permission_configfee',
                           name='render_report_permission_configfee'),
                       url(r'^admin/report/ParkingRedemptionNew/$', 'parking.report.render_report_parking_redemption_new', name='render_report_parking_redemption_new'),


    url(r'^admin/log/customer/(?P<customer_id>\d+)/$', 'parking.views.render_customer_log', name='render_customer_vehicle_registration_log'),
                       url(r'^admin/log/vehicleregistration/(?P<vehicle_registration_id>\d+)/$', 'parking.views.render_vehicle_registration_log', name='render_vehicle_registration_log'),

                       url(r'^admin/customer/Search/$', 'parking.views.render_search_customer', name='render_search_customer'),
                       url(r'^admin/customer/BulkImport/$', 'parking.views.render_bulk_import_customer', name='render_bulk_import_customer'),

                       url(r'^admin/receipt/action/(?P<type>[0,1])/(?P<payment_id>\d+)/$', 'parking.views.render_receipt_action_form', name='render_receipt_action_form'),
                       url(r'^admin/setting/upload/(?P<key>\w+)/$', 'parking.views.render_setting_upload', name='render_setting_upload'),

                       # PDF
    url(r'^pdf/ticket-payment/(?P<ticket_payment_id>\d+)(?:/(?P<print_for_company>[0,1]))?/$', 'parking.views.pdf_ticket_payment', name='pdf_ticket_payment'),
                       url(r'^pdf/deposit-payment/(?P<ticket_payment_id>\d+)(?:/(?P<print_for_company>[0,1]))?/$', 'parking.views.pdf_ticket_payment', name='pdf_ticket_payment'),
                       url(r'^test/clear-cache/$', 'parking.views.clear_cache', name='clear_cache'),

                       url(r'^changelog/$', 'parking.views.get_change_notes', name='get_change_notes'),


                       )
try:
    init_app_config()
except:
    pass
