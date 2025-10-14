# -*- coding: utf-8 -*-
__author__ = 'Nghia'

VEHICLE_STATUS_CHOICE = (
    (0, u'Hủy'),
    (1, u'Đang dùng'),
    (2, u'Tạm ngừng'),
    (3, u'Chưa đăng ký')
)

VEHICLE_STATUS_COLOR_VALUE_DICT = {
    0: {"color": "gray", "value": u"Hủy"},
    1: {"color": "blue", "value": u"Đang dùng"},
    2: {"color": "brown", "value": u"Tạm ngừng"},
    3: {"color": "green", "value": u"Chưa đăng ký"},
}

CARD_STATUS = (
    (0, u'Không dùng'),
    (1, u'Đang dùng'),
    (2, u'Khoá'),
)