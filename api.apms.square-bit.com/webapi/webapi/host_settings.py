__author__ = 'ndhoang'

# SECURITY WARNING: don't run with debug turned on in production!
DEBUG = True

TEMPLATE_DEBUG = True

# Database connection settings
DB_SETTINGS = {
    'HOST': '127.0.0.1',
    'PORT': '3306',
    'USER': 'root',
    'PASSWORD': '@MinhDe81$123'
}

LOG_SERVER = '192.168.1.41:8888'

# Specifies a place to storage images
IMAGE_STORAGE_PATH = '/data/upload/apms'

# Enable memory cache to boost performance
MEM_CACHE_ENABLE = False

# LOGGING = {
#     'disable_existing_loggers': False,
#     'version': 1,
#     'handlers': {
#         'console': {
#             # logging handler that outputs log messages to terminal
#             'class': 'logging.StreamHandler',
#             'level': 'DEBUG', # message level to be written to console
#         },
#         'file': {
#             'level': 'ERROR',
#             'class': 'logging.FileHandler',
#             'filename': '/var/log/apms-webapi.log',
#         },
#     },
#     'loggers': {
#         '': {
#             # this sets root level logger to log debug and higher level
#             # logs to console. All other loggers inherit settings from
#             # root level logger.
#             'handlers': ['console', 'file'],
#             'level': 'DEBUG',
#             'propagate': False, # this tells logger to send logging message
#                                 # to its parent (will send if set to True)
#         },
#         'parking.logger': {
#             'handlers': ['console', 'file'],
#             'level': 'ERROR',
#         },
#     },
# }
