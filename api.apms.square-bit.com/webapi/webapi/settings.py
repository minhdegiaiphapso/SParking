"""
Django settings for webapi project.

For more information on this file, see
https://docs.djangoproject.com/en/1.6/topics/settings/

For the full list of settings and their values, see
https://docs.djangoproject.com/en/1.6/ref/settings/
"""

# Build paths inside the project like this: os.path.join(BASE_DIR, ...)
import os
import sys
from host_settings import *

BASE_DIR = os.path.dirname(os.path.dirname(__file__))
PROJECT_ROOT = os.path.abspath(os.path.join(BASE_DIR))
IS_TEST = 'test' in sys.argv

# Quick-start development settings - unsuitable for production
# See https://docs.djangoproject.com/en/1.6/howto/deployment/checklist/

# SECURITY WARNING: keep the secret key used in production secret!
SECRET_KEY = 'zw0xy5c0l2v#ko641!=5$#frw(#(4+ju4gj0hjl!440kiwkdp5'

ALLOWED_HOSTS = ['*']

# Application definition

INSTALLED_APPS = (
    'django.contrib.auth',
    'django.contrib.contenttypes',
    'django.contrib.sessions',
    'django.contrib.messages',
    'django.contrib.staticfiles',
    'rest_framework',
    'rest_framework_swagger',
    'parking',
    'webapi',
)

MIDDLEWARE_CLASSES = (
    'django.contrib.sessions.middleware.SessionMiddleware',
    'django.middleware.common.CommonMiddleware',
    'django.middleware.csrf.CsrfViewMiddleware',
    'django.contrib.auth.middleware.AuthenticationMiddleware',
    'django.contrib.messages.middleware.MessageMiddleware',
    'django.middleware.clickjacking.XFrameOptionsMiddleware',
)

TEST_RUNNER = 'app.runnerstest.MyTestRunner'

ROOT_URLCONF = 'webapi.urls'

WSGI_APPLICATION = 'webapi.wsgi.application'

# Database
# https://docs.djangoproject.com/en/1.6/ref/settings/#databases
if IS_TEST:
    DATABASES = {
        'default': {
            'ENGINE': 'django.db.backends.sqlite3',
            'NAME': os.path.join(BASE_DIR, 'db.sqlite3'),
        }
    }

    MEDIA_ROOT = '/tmp'
    # CACHES = {
    #     'default': {
    #         'BACKEND': 'django.core.cache.backends.locmem.LocMemCache',
    #     }
    # }
else:
    DATABASES = {
        'default': {
            'ENGINE': 'django.db.backends.mysql',
            'NAME': DB_SETTINGS.get('NAME', 'spk'),
            'USER': DB_SETTINGS.get('USER', 'root'),
            'PASSWORD': DB_SETTINGS.get('PASSWORD', '@MinhDe81$123'),
            'HOST': DB_SETTINGS.get('HOST', '127.0.0.1'),
            'PORT': DB_SETTINGS.get('PORT', '3306'),
            'OPTIONS': {
                'init_command': 'SET default_storage_engine=INNODB,character_set_connection=utf8,collation_connection=utf8_unicode_ci',
                "connect_timeout": 1,
            }
        },
        'secondary': {
             'ENGINE': 'django.db.backends.mysql',
             'NAME': DB_SETTINGS.get('NAME', 'spkfee'),
             'USER': DB_SETTINGS.get('USER', 'root'),
             'PASSWORD': DB_SETTINGS.get('PASSWORD', '@MinhDe81$123'),
             'HOST': DB_SETTINGS.get('HOST', '127.0.0.1'),
             'PORT': DB_SETTINGS.get('PORT', '3306'),
             'OPTIONS': {
                'init_command': 'SET default_storage_engine=INNODB,character_set_connection=utf8,collation_connection=utf8_unicode_ci',
                "connect_timeout": 1,
            }
         },
    }

    MEDIA_ROOT = IMAGE_STORAGE_PATH
    # CACHES = {
    #     'default': {
    #         'BACKEND': 'django.core.cache.backends.memcached.MemcachedCache',
    #         'LOCATION': '127.0.0.1:11211',
    #     }
    # }

# DATABASE_ROUTERS = ['onemanga.router.OneMangaRouter']

# Internationalization
# https://docs.djangoproject.com/en/1.6/topics/i18n/

LANGUAGE_CODE = 'en-us'

TIME_ZONE = 'UTC'

USE_I18N = True

USE_L10N = True

USE_TZ = True


# Static files (CSS, JavaScript, Images)
# https://docs.djangoproject.com/en/1.6/howto/static-files/

STATIC_ROOT = '/data/static/api.apms.square-bit.com/'
STATIC_URL = '/static/'

# Swagger
SWAGGER_SETTINGS = {
    "exclude_namespaces": [], # List URL namespaces to ignore
    "api_version": '1.0',  # Specify your API's version
    "api_path": "/",  # Specify the path to your API not a root level
    "enabled_methods": [  # Specify which methods to enable in Swagger UI
        'get',
        'post',
        'put',
        'patch',
        'delete'
    ],
    "api_key": '', # An API key
    "is_authenticated": False,  # Set to True to enforce user authentication,
    "is_superuser": False,  # Set to True to enforce admin only access
}

REST_FRAMEWORK = {
    'DEFAULT_RENDERER_CLASSES': (
        'rest_framework.renderers.UnicodeJSONRenderer',
        'rest_framework.renderers.BrowsableAPIRenderer',
    ),
    'EXCEPTION_HANDLER': 'app.utils.custom_exception_handler',
}

APP_SECRET_KEY = 'GJHLM]On@e8&LbW~)t(0E*=-QzDvu1X,}+iIYLxg+|?;B~_M!h-E=,(P20Mh`A:#'