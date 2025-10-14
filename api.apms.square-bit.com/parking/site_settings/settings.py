"""
Django settings for parking project.

For more information on this file, see
https://docs.djangoproject.com/en/1.6/topics/settings/

For the full list of settings and their values, see
https://docs.djangoproject.com/en/1.6/ref/settings/
"""

# Build paths inside the project like this: os.path.join(BASE_DIR, ...)
import os
from django.conf.global_settings import TEMPLATE_CONTEXT_PROCESSORS as TCP
BASE_DIR = os.path.dirname(os.path.dirname(__file__))


# Quick-start development settings - unsuitable for production
# See https://docs.djangoproject.com/en/1.6/howto/deployment/checklist/

# SECURITY WARNING: keep the secret key used in production secret!
SECRET_KEY = 'b0$!h9zi7hnje7^f!+9zi-jokgan@7woc9eo-sxh&(*w0#0o#o'

# SECURITY WARNING: don't run with debug turned on in production!
DEBUG = True

TEMPLATE_DEBUG = True

ALLOWED_HOSTS = ['*']

STATIC_URL = '/static/'

TEMPLATE_CONTEXT_PROCESSORS = TCP + (
    'django.core.context_processors.request',
    'django.core.context_processors.csrf',
)


# Application definition

INSTALLED_APPS = (
    'parking',
    'autocomplete_light',
    'wpadmin',
    'adminplus',
    'django.contrib.admin',
    'django.contrib.auth',
    'django.contrib.contenttypes',
    'django.contrib.sessions',
    'django.contrib.messages',
    'django.contrib.staticfiles',
)

MIDDLEWARE_CLASSES = (
    'django.contrib.sessions.middleware.SessionMiddleware',
    'django.middleware.common.CommonMiddleware',
    'django.middleware.csrf.CsrfViewMiddleware',
    'django.contrib.auth.middleware.AuthenticationMiddleware',
    'django.contrib.messages.middleware.MessageMiddleware',
    'django.middleware.clickjacking.XFrameOptionsMiddleware',
    'audit_log.middleware.UserLoggingMiddleware',
)

AUTHENTICATION_BACKENDS = (
    'django.contrib.auth.backends.ModelBackend',
    'parking.authentication.ADAuthentication'
)

ROOT_URLCONF = 'site_settings.urls'

WSGI_APPLICATION = 'site_settings.wsgi.application'

WPADMIN = {
    'admin': {
        'admin_site': 'parking.admin.admin',
        'title': 'Django admin panel',
        'menu': {
            'top': 'parking.wp.UserTopMenu',
            # 'left': 'wpadmin.menu.menus.BasicLeftMenu',
        },
        'dashboard': {
            'breadcrumbs': False,
        },
        'custom_style': STATIC_URL + 'css/greenparking.css',
    }
}

# Database
# https://docs.djangoproject.com/en/1.6/ref/settings/#databases

DATABASES = {
    'default': {
        'ENGINE': 'django.db.backends.mysql',
        'NAME': 'spk',
        'USER': 'root',
        'PASSWORD': '@MinhDe81$123',
        'HOST': '127.0.0.1',
        'PORT': '3306',
        'OPTIONS': {
            'init_command': 'SET default_storage_engine=INNODB,character_set_connection=utf8,collation_connection=utf8_unicode_ci'
        },
    },
    'secondary': {
        'ENGINE': 'django.db.backends.mysql',
        'NAME': 'spkfee',
        'USER': 'root',
        'PASSWORD': '@MinhDe81$123',
        'HOST': '127.0.0.1',
        'PORT': '3306',
        'OPTIONS': {
            'init_command': 'SET default_storage_engine=INNODB,character_set_connection=utf8,collation_connection=utf8_unicode_ci'
        },
    },
}

AD_DOMAIN = "hcm.keppelland.com"
LDAP_SERVER = 'ldap://%s' % AD_DOMAIN

# DATABASE_ROUTERS = ['parking.dbrouter.GPMSDBRouter']

# Internationalization
# https://docs.djangoproject.com/en/1.6/topics/i18n/

LANGUAGE_CODE = 'vi'

TIME_ZONE = 'Asia/Saigon'

USE_I18N = True

USE_L10N = True

USE_TZ = True


# Static files (CSS, JavaScript, Images)
# https://docs.djangoproject.com/en/1.6/howto/static-files/

STATIC_URL = '/static/'
STATIC_ROOT = '/data/static/admin.apms.square-bit.com'

TEMPLATE_DIRS = (
    os.path.join(BASE_DIR, 'templates'),
)

# Cho phep tim template trong cac app
# Load checkoutwithexception.html
TEMPLATE_LOADERS = ('django.template.loaders.filesystem.Loader',
 'django.template.loaders.app_directories.Loader')

# LOGGING = {
#     'disable_existing_loggers': False,
#     'version': 1,
#     'handlers': {
#         'console': {
#             # logging handler that outputs log messages to terminal
#             'class': 'logging.StreamHandler',
#             'level': 'DEBUG', # message level to be written to console
#         },
#     },
#     'loggers': {
#         '': {
#             # this sets root level logger to log debug and higher level
#             # logs to console. All other loggers inherit settings from
#             # root level logger.
#             'handlers': ['console'],
#             'level': 'DEBUG',
#             'propagate': False, # this tells logger to send logging message
#                                 # to its parent (will send if set to True)
#         },
#         'django.db': {
#             # django also has database level logging
#         },
#     },
# }

DATE_INPUT_FORMATS = (
    '%d/%m/%Y',
)
