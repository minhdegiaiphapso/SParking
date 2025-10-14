import datetime
import re
import csv, codecs, cStringIO
from subprocess import Popen, PIPE
import traceback
import requests
import sys
import json
from django.utils.timezone import utc
from dateutil import tz
from parking.parking.models import CardStatus, ParkingSession, Card
from rest_framework.views import exception_handler
from webapi.host_settings import LOG_SERVER
from parking.parking.views import  calculate_parking_fee
import pytz

__author__ = 'ndhoang'


def get_now_utc():
    return datetime.datetime.utcnow().replace(microsecond=0, tzinfo=utc)
###########2017-12-15
def canchanges(checkout_date, cancel_date, expirate_date):
    if expirate_date is None:
        ed = None
    else:
        ed = (datetime.datetime(year=expirate_date.year,
                                month=expirate_date.month,
                                day=expirate_date.day, ).replace(hour=0, minute=0, second=0,
                                                                 tzinfo=tz.gettz('Asia/Ho_Chi_Minh')).astimezone(
            pytz.utc) + datetime.timedelta(days=1))
        if ed<checkout_date:
            return  True
    if cancel_date is None:
        cd = None
    else:
        cd = datetime.datetime(year=cancel_date.year,
                               month=cancel_date.month,
                               day=cancel_date.day, ).replace(hour=0, minute=0, second=0,
                                                              tzinfo=tz.gettz('Asia/Ho_Chi_Minh')).astimezone(pytz.utc)
        if cd<checkout_date:
            return True;
    return  None
def get_activecheckin(checkin_date, cancel_date, expirate_date):
    drt=checkin_date
    ckout=get_now_utc();
    if expirate_date is None:
        ed=None
    else:

        ed=(datetime.datetime(year=expirate_date.year,
            month=expirate_date.month,
            day=expirate_date.day,).replace(hour=0,minute=0,second=0,tzinfo=tz.gettz('Asia/Ho_Chi_Minh')).astimezone(pytz.utc)+datetime.timedelta(days=1))

    if cancel_date is None:
        cd=None
    else:

        cd =datetime.datetime(year=cancel_date.year,
            month=cancel_date.month,
            day=cancel_date.day,).replace(hour=0,minute=0,second=0,tzinfo=tz.gettz('Asia/Ho_Chi_Minh')).astimezone(pytz.utc)

    if ed is not None:
        if(ed>=ckout):
            drt=checkin_date
        elif ed>checkin_date:
            drt=ed
        else:
            drt=checkin_date
    else:
        drt=checkin_date
    if cd is not None and cd<ckout:
        if  cd> checkin_date:
            if ed is  None:
                drt=cd
            elif cd<ed:
                drt=cd
        elif drt is None:
            drt=checkin_date
    #fee=calculate_parking_fee("", 100, drt, ckout)
    return drt # [{'checkin':checkin_date,'actual_checkin':drt,'checkout':ckout,'cancel':cancel_date,'actual_cancel':cd,'expirate':expirate_date,'actual_expirate':ed,'fee':fee[0]}]
def convertutc(currentt_date):
    return currentt_date.astimezone(pytz.utc)
def convertdatetime(date):
    return datetime.datetime(year=date.year,
            month=date.month,
            day=date.day,).replace(hour=0,minute=0,second=0,tzinfo=utc)
def get_now():
    return get_now_utc().astimezone(tz.gettz('Asia/Ho_Chi_Minh')).replace(tzinfo=utc)


def datetime2timestamp(time):
    milestone = datetime.datetime(year=2014, month=7, day=1, tzinfo=utc)
    return (time - milestone).total_seconds()


def timestamp2datetime(timestamp):
    milestone = datetime.datetime(year=2014, month=7, day=1, tzinfo=utc)
    return milestone + datetime.timedelta(seconds=timestamp)


def card_status_to_dict(obj):
    rs = dict()
    rs['id'] = obj.id
    rs['card_id'] = obj.card_id
    rs['parking_session_id'] = obj.parking_session_id
    rs['status'] = obj.status
    return rs


def dict_to_card_status(d):
    rs = CardStatus()
    rs.id = d['id']
    rs.card_id = d['card_id']
    rs.parking_session_id = d['parking_session_id']
    rs.status = d['status']
    return rs


def card_to_dict(obj):
    rs = dict()
    rs['id'] = obj.id
    rs['card_id'] = obj.card_id
    rs['card_label'] = obj.card_label
    rs['status'] = obj.status
    rs['vehicle_type'] = obj.vehicle_type
    return rs


def dict_to_card(d):
    rs = Card()
    rs.id = d['id']
    rs.card_id = d['card_id']
    rs.card_label = d['card_label']
    rs.status = d['status']
    rs.vehicle_type = d['vehicle_type']
    return rs


def parking_session_to_dict(obj):
    rs = dict()
    rs['id'] = obj.id
    rs['card_id'] = obj.card_id
    rs['vehicle_type'] = obj.vehicle_type
    rs['vehicle_sub_type'] = obj.vehicle_sub_type
    rs['vehicle_number'] = obj.vehicle_number
    rs['check_in_alpr_vehicle_number'] = obj.check_in_alpr_vehicle_number
    rs['check_in_operator_id'] = obj.check_in_operator_id
    rs['check_in_time'] = datetime2timestamp(obj.check_in_time)
    rs['check_in_images'] = dict(obj.check_in_images)
    rs['check_in_lane_id'] = obj.check_in_lane_id
    return rs


def dict_to_parking_session(d):
    rs = ParkingSession()
    rs.id = d['id']
    rs.card_id = d['card_id']
    rs.vehicle_type = d['vehicle_type']
    rs.vehicle_sub_type = d['vehicle_sub_type']
    rs.vehicle_number = d['vehicle_number']
    rs.check_in_alpr_vehicle_number = d['check_in_alpr_vehicle_number']
    rs.check_in_operator_id = d['check_in_operator_id']
    rs.check_in_time = timestamp2datetime(d['check_in_time'])
    rs.check_in_images = d['check_in_images']
    rs.check_in_lane_id = d['check_in_lane_id']
    return rs


def get_param(request, param_name, default_val=None):
    if param_name in request.QUERY_PARAMS:
        return request.QUERY_PARAMS[param_name]
    return default_val


class FakeStrictRedis(object):
    def __init__(self):
        self.db = dict()

    def get(self, name):
        if name in self.db:
            return self.db[name]
        return None

    def set(self, name, value, ex=None):
        self.db[name] = value

    def delete(self, name):
        if name in self.db:
            del self.db[name]

    def incr(self, name):
        if name in self.db:
            self.db[name] += 1
        else:
            self.db[name] = 1
        return self.db[name]

    def decr(self, name):
        if name in self.db:
            self.db[name] -= 1
        else:
            self.db[name] = 0
        return self.db[name]

    def flushall(self):
        self.db.clear()


my_ip = None


def get_my_ip():
    global my_ip
    if not my_ip:
        output = Popen(["ifconfig", "eth0"], stdout=PIPE).communicate()[0]
        ip = re.search(r'inet addr:(\S+)', output)
        if ip:
            my_ip = ip.group(1)
        else:
            my_ip = None
    return my_ip


def log_exception(status_code):
    exc_type, exc_value, exc_traceback = sys.exc_info()
    lines = traceback.format_exception(exc_type, exc_value, exc_traceback)
    detail = ''.join('!! ' + line for line in lines)  # Log it or whatever here
    log_data = {'action': 1, 'actor': get_my_ip(), 'int_param1': status_code, 'string_param1': detail}
    try:
        requests.post('http://' + LOG_SERVER + '/apms.server', data={'json': json.dumps(log_data)}, timeout=1)
    except:
        print(log_data)
    return exc_value


def custom_exception_handler(exc):
    # Call REST framework's default exception handler first,
    # to get the standard error response.
    response = exception_handler(exc)
    status_code = 500
    if response:
        status_code = response.status_code
    log_exception(status_code)

    return response


class UnicodeWriter:
    """
    A CSV writer which will write rows to CSV file "f",
    which is encoded in the given encoding.
    """
    def __init__(self, f, dialect=csv.excel, encoding="utf-8", **kwds):
        # Redirect output to a queue
        self.queue = cStringIO.StringIO()
        self.writer = csv.writer(self.queue, dialect=dialect, **kwds)
        self.stream = f
        self.encoder = codecs.getincrementalencoder(encoding)()

    def writerow(self, row):
        self.writer.writerow([s.encode("utf-8") for s in row])
        # Fetch UTF-8 output from the queue ...
        data = self.queue.getvalue()
        data = data.decode("utf-8")
        # ... and reencode it into the target encoding
        data = self.encoder.encode(data)
        # write to the target stream
        self.stream.write(data)
        # empty queue
        self.queue.truncate(0)

    def writerows(self, rows):
        for row in rows:
            self.writerow(row)