"""
WSGI config for parking project.

It exposes the WSGI callable as a module-level variable named ``application``.

For more information on this file, see
https://docs.djangoproject.com/en/1.6/howto/deployment/wsgi/
"""

import os
os.environ.setdefault("DJANGO_SETTINGS_MODULE", "site_settings.settings")
from parking.invoicePlan import  spBackground
from datetime import date, datetime, time, timedelta
from pytz import timezone
# from settings import TIME_ZONE
if not spBackground.IsRunning:
    spBackground.StartScheduler(datetime.now(tz= timezone('Asia/Saigon')))
    spBackground.PrintPlan()
from django.core.wsgi import get_wsgi_application
application = get_wsgi_application()
