from django.conf.urls import patterns, include, url
from django.conf import settings
from django.conf.urls.static import static

from  . import mobile_views
urlpatterns = patterns(
    '',
    url(r'^parking-images/(?P<parking_id>[0-9]+)/$', mobile_views.get_parking_images, name='get_parking_images'),
    url(r'^revenue-report/$', mobile_views.revenue_report, name='revenue-report'),
    url(r'^user-login/$', mobile_views.UserLoginView.as_view(), name='user-login-mobile'),
    url(r'^card-login/$', mobile_views.UserCardLoginView.as_view(), name='card-login-mobile'),
    url(r'^owner-certificate/$', mobile_views.CerificationView.as_view(), name='owner-certificate-mobile'),
    url(r'^shift-assignment/$', mobile_views.ShiftAssignmentView.as_view(), name='shift-assignment'),
    url(r'^check-cus/$', mobile_views.CheckCusView.as_view(), name='check-cus'),
    url(r'^edit-session/$', mobile_views.EditSessionView.as_view(), name='edit-session'),
    url(r'^cancel-session/$', mobile_views.CancelSessionView.as_view(), name='cancel-session'),
    url(r'^exception-session-out/$', mobile_views.OutSessionExceptionView.as_view(), name='exception-session-out'),
    url(r'^parking-search/$', mobile_views.MobileSearchView.as_view(), name='parking-search'),
    url(r'^retail-invoice/$', mobile_views.RetailInvoiceView.as_view(), name='retail-invoice'),
    url(r'^login-api/$', mobile_views.login_api, name='login_api'),
    url(r'^log-invoce-retail/$', mobile_views.invoice_log_retail, name='log-invoce-retail'),
    url(r'^log-invoice-consoliddate/$', mobile_views.invoice_log_consoliddate, name='log-invoice-consoliddate'),
    url(r'^sync-invoice/$', mobile_views.sync_invoice, name='sync-invoice'),
)
