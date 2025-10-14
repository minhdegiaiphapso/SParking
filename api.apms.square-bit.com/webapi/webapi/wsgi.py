"""
WSGI config for webapi project.

It exposes the WSGI callable as a module-level variable named ``application``.

For more information on this file, see
https://docs.djangoproject.com/en/1.6/howto/deployment/wsgi/
"""

import os
import sys

from webapi.settings import PROJECT_ROOT

container_path = os.path.abspath(os.path.join(PROJECT_ROOT, '..'))
sys.path.append(os.path.join(container_path, 'parking'))

# print PROJECT_ROOT

os.environ.setdefault("DJANGO_SETTINGS_MODULE", "webapi.settings")
# Make Django returns https prefix.
# http://security.stackexchange.com/questions/8964/trying-to-make-a-django-based-site-use-https-only-not-sure-if-its-secure
# os.environ.setdefault('wsgi.url_scheme', 'https')
# os.environ.setdefault('HTTPS', "on")

from django.core.wsgi import get_wsgi_application
application = get_wsgi_application()
