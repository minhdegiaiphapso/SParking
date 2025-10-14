# -*- coding: utf-8 -*-
from  .models import PartnerInvoice, InvoiceApiInitation, InvoiceConnector, InvoiceBuyer, \
    InvoiceTaxRule, RetailInvoice, ConsolidatedInvoice, ParkingSession, ParkingFeeSession, get_setting
from multiprocessing import  Process
from curencyService import docsotien
import  json
import requests
from datetime import datetime, time, timedelta
import  pytz
from pytz import timezone

TIME_ZONE = 'Asia/Saigon'
def format_datetime_to_api(dt_utc):
    # Nếu dt_utc không có tzinfo thì set UTC
    if dt_utc.tzinfo is None:
        dt_utc = dt_utc.replace(tzinfo=pytz.UTC)

    # Chuyển sang múi giờ +07:00
    tz = pytz.timezone("Asia/Ho_Chi_Minh")
    dt_local = dt_utc.astimezone(tz)

    # Format thủ công: yyyy-mm-ddTHH:MM:SS.mmm+07:00
    formatted = dt_local.strftime("%Y-%m-%dT%H:%M:%S.%f")[:-3]  # lấy mili giây
    offset = dt_local.strftime("%z")  # +0700
    offset = offset[:3] + ":" + offset[3:]  # đổi thành +07:00

    return formatted + offset


# # Ví dụ test
# dt = datetime(2025, 9, 19, 1, 18, 19, 849000, tzinfo=pytz.UTC)
# print(format_datetime_to_api(dt))
# # 👉 2025-09-19T08:18:19.849+07:00
def makeFromTo():
    now = datetime.now(tz=timezone(TIME_ZONE)).replace(hour=0,minute=0,second=0)
    fr = now + timedelta(days=-1)
    to = fr.replace(hour=23,minute=59,second=59)
    return fr, to
def get_fee(id):
    fee = 0
    pk = ParkingSession.objects.get(id = id)
    if pk.check_out_exception is not None:
        fee = pk.check_out_exception.parking_fee
    else:
        parking_fee_session = ParkingFeeSession.objects.all().filter(parking_session_id=id, session_type='OUT').order_by('-parking_fee')
        fee = parking_fee_session[0].parking_fee if parking_fee_session else 0
    return fee

def GetAfterIds():
    mns  = get_setting('check_issue_after_check_out', u'Sau check-out bao lâu (số phút) sẽ được kiểm tra để phát hành hóa đơn', 120)
    minutes = int(mns) * (-1)
    to = datetime.utcnow() + timedelta(minutes=minutes)
    fr = datetime.utcnow().replace(year=2025, month=6, day=25, hour=0, minute=0, second=0)
    ids = ParkingSession.objects.filter(
        check_out_time__lte=to,
        check_out_time__gte = fr
    ).values_list('id', flat=True)

    # Lọc các id KHÔNG nằm trong danh sách parkingrefid của RetailInvoice
    used_ids = set(map(int, RetailInvoice.objects.filter(
        parkingrefid__in=map(str, ids)
    ).values_list('parkingrefid', flat=True)))

    unused_ids = [i for i in ids if i not in used_ids]
    return unused_ids

def LastDatePakingIds():
    fr, to = makeFromTo()
    # fr  = fr.replace(year = 2024, month = 7, day = 27)
    # to = to.replace(year = 2024, month = 7, day = 27)
    ids = ParkingSession.objects.filter(
        check_out_time__gte=fr,
        check_out_time__lte=to
    ).values_list('id', flat=True)

    # Lọc các id KHÔNG nằm trong danh sách parkingrefid của RetailInvoice
    used_ids = set(map(int, RetailInvoice.objects.filter(
        parkingrefid__in=map(str, ids)
    ).values_list('parkingrefid', flat=True)))

    unused_ids = [i for i in ids if i not in used_ids]
    result = []
    totalfee = 0
    for id in unused_ids:
        fee = get_fee(id)
        if fee > 0:
            totalfee+= fee
            result.append(id)
    return ",".join(map(str, result)), totalfee

def getInvoiceToken():
    validate, token, series, tempid, maxamount, partnerid = False, None, None, None, None, None
    hour, minute, second = 0,5,0
    try:
        conn = InvoiceConnector.objects.filter(partner__activated = True, isvalid=True)
        if conn:
            conn = conn[0]
            validate = conn.isvalid
            token = conn.token
            series =conn.invoiceserie
            tempid = conn.invoicetemplate
            maxamount = conn.maxamount
            stime = conn.scheduletime
            partnerid = conn.partner.id
            if stime is not None:
                hour = stime.hour
                minute = stime.minute
                second = stime.second
    except Exception as ex:
       pass
    return validate, token, series, tempid, maxamount, partnerid, hour, minute, second
import uuid
def CallTaxInfo(fee, rule):
    vat, amount, amount1, VATRateName = 0, fee, fee, "KCT"
    if rule is not None:
        if rule.taxpercentage == 0 or rule.taxpercentage == 5 or rule.taxpercentage == 8 or rule.taxpercentage == 10:
            VATRateName = "%s"%rule.taxpercentage + '%'
        else:
            VATRateName = "KHAC: %s"%rule.taxpercentage + '%'
        pct = rule.taxpercentage
        if rule.feeincludesvat:
            amount = int((fee*100.0)/(100 + pct))
            vat = fee - amount
        else:
            vat = int(fee*pct/100.0)
            amount = fee
            amount1 = amount + vat
    return vat, amount, amount1, VATRateName, docsotien(amount1)

def CallTaxCamRanhInfo(fee, rule):
    vat, amount, amount1, VATRateName, VATPC = 0, fee, fee, "KCT", 0
    if rule is not None:
        if rule.taxpercentage == 0 or rule.taxpercentage == 5 or rule.taxpercentage == 8 or rule.taxpercentage == 10:
            VATRateName = "%s"%rule.taxpercentage + '%'
        else:
            VATRateName = "KHAC: %s"%rule.taxpercentage + '%'
        pct = rule.taxpercentage
        VATPC = pct
        if rule.feeincludesvat:
            amount = int((fee*100.0)/(100 + pct))
            vat = fee - amount
        else:
            vat = int(fee*pct/100.0)
            amount = fee
            amount1 = amount + vat
    return vat, amount, amount1, VATRateName, docsotien(amount1), VATPC

def makeInvoiceData(c, series, rule, buyer):
    vat, withoutVat, total, vatRateName, totalStr = CallTaxInfo(c.parkingfees, rule)
    datestr = c.invoicedate.strftime("%Y-%m-%d")
    data =  [
        {
            "RefID": c.refid,
            "InvSeries":series,
            "InvDate":datestr,
            "InvoiceName":u"Tổng hợp phí gửi xe ngày %s"%datestr,
            "CurrencyCode":"VND",
            "ExchangeRate":1,
            "IsTaxReduction43":False,
            "PaymentMethodName":"TM",
            "IsInvoiceSummary":False,
            "IsSendEmail":True if buyer is not None and buyer.receivername is not None and buyer.receiveremails is not None else False,
            "ReceiverName": buyer.receivername if buyer is not None else None,
            "ReceiverEmail": buyer.receiveremails if buyer is not None else None,
            "BuyerCode": buyer.code if buyer is not None else None,
            "BuyerRegantName": buyer.legalname if buyer is not None else None,
            "BuyerFullName": buyer.buyername if buyer is not None else None,
            "BuyerEmail": buyer.email if buyer is not None else None,
            "TotalSaleAmount": withoutVat,
            "TotalAmount": total,
            "TotalAmountInWords":u"%s"%totalStr,
            "OriginalInvoiceDetail":[
                {
                    "ItemType": 1,
                    "LineNumber": 1,
                    "SortOrder": 1,
                    "ItemCode": datestr,
                    "ItemName": u"Tổng hợp phí gửi xe ngày %s"%datestr,
                    "UnitName": u"Tổng phí gửi xe trong ngày",
                    "Quantity": 1,
                    "UnitPrice":total,
                    "AmountWithoutVATOC":withoutVat,
                    "VATAmountOC":vat,
                    "VATRateName":vatRateName
                }
            ]
        }   ]
    return data

def TryDoCallInvoice(c, maxamount, token, series, tempid, partnerid):
    amount = c.amountrequested
    while amount <= maxamount:
        amount += 1
        c.amountrequested = amount
        c.save()
        try:
            partner = PartnerInvoice.objects.get(id = partnerid)#TESTMISAINVOICE
            if partner.activated and (partner.code == "TESTMISAINVOICE" or partner.code == "MISAINVOICE"):
                c_api = InvoiceApiInitation.objects.filter(partner = partner, target = 2)
                if c_api:
                    c_api = c_api[0]
                    headers = {
                        "Content-Type":"application/json",
                         "Authorization":"Bearer %s"%token
                    }
                    urlmisa = c_api.url

                    rules = InvoiceTaxRule.objects.filter(mode =1, activated = True)
                    if rules:
                        rule = rules[0]
                    else:
                        rule = None
                    buyers = InvoiceBuyer.objects.filter(mode =1)
                    if buyers:
                        buyer = buyers[0]
                    else:
                        buyer = None
                    invoicedata = makeInvoiceData(c, series, rule, buyer)

                    sendData = {
                        "SignType": 2,
                        "InvoiceData": invoicedata,
                        "PublishInvoiceData": None
                    }
                    data = str(json.dumps(sendData, ensure_ascii=False))
                    c.contentrequest = data
                    c.save()
                    x = requests.post(url=urlmisa, data=c.contentrequest, headers=headers)
                    ct = x.content
                    st = x.status_code
                    if st == 200:
                        try:
                            jsd = json.loads(ct)
                            if "success" in jsd and jsd["success"] is True and "publishInvoiceResult" in jsd and jsd["publishInvoiceResult"] is not None:
                                res = json.loads(jsd["publishInvoiceResult"])
                                if res:
                                    res  = res[0]
                                    if 'TransactionID' in res and res['TransactionID'] is not None:
                                        c.transactionid = res['TransactionID']
                                        c.iscompleted = True
                                        c.save()
                                        break
                                    else:
                                        pass

                        except Exception as ex:
                            pass
                    else:
                        pass

        except Exception as eee:
            pass
def DoConsolidated():
    ids, totalfee = LastDatePakingIds()
    c = ConsolidatedInvoice.objects.filter(parkingids = ids)
    if c:
        return
    validate, token, series, tempid, maxamount, partnerid, hour, minute, second = getInvoiceToken()
    if validate is not True:
        return
    now = datetime.now()
    fr = now + timedelta(days=-1)
    refId = str(uuid.uuid4())
    c = ConsolidatedInvoice()
    c.refid = refId
    c.parkingids = ids
    c.parkingfees = totalfee
    c.amountrequested = 0
    c.reqestedtime  = now
    c.invoicedate = fr.date()
    c.iscompleted = False
    c.save()
    Process(target=TryDoCallInvoice, args=(c, maxamount, token, series, tempid, partnerid)).start()

def makeRetailInvoiceData(c, series, rule, buyer):
    vat, withoutVat, total, vatRateName, totalStr = CallTaxInfo(c.parkingfee, rule)
    datestr = c.invoicedate.strftime("%Y-%m-%d")
    data =  [
        {
            "RefID": c.refid,
            "InvSeries":series,
            "InvDate":datestr,
            "InvoiceName":u"Phí lượt của ParkingID: %s"%c.parkingrefid,
            "CurrencyCode":"VND",
            "ExchangeRate":1,
            "IsTaxReduction43":False,
            "PaymentMethodName":"TM",
            "IsInvoiceSummary":False,
            "IsSendEmail":True if buyer is not None and buyer.receivername is not None and buyer.receiveremails is not None else False,
            "ReceiverName": buyer.receivername if buyer is not None else None,
            "ReceiverEmail": buyer.receiveremails if buyer is not None else None,
            "BuyerCode": buyer.code if buyer is not None else None,
            "BuyerRegantName": buyer.legalname if buyer is not None else None,
            "BuyerFullName": buyer.buyername if buyer is not None else None,
            "BuyerEmail": buyer.email if buyer is not None else None,
            "TotalSaleAmount": withoutVat,
            "TotalAmount": total,
            "TotalAmountInWords":u"%s"%totalStr,
            "OriginalInvoiceDetail":[
                {
                    "ItemType": 1,
                    "LineNumber": 1,
                    "SortOrder": 1,
                    "ItemCode": "ID: %s"%c.parkingrefid,
                    "ItemName": u"Lượt gửi xe có ID là: %s"%c.parkingrefid,
                    "UnitName": u"Lượt",
                    "Quantity": 1,
                    "UnitPrice":total,
                    "AmountWithoutVATOC":withoutVat,
                    "VATAmountOC":vat,
                    "VATRateName":vatRateName
                }
            ]
        }   ]
    return data

def makeRetailInvoiceDataCamRanh(c, series, rule, buyer, pk):
    nowd =datetime.utcnow()
    vat, withoutVat, total, vatRateName, totalStr, vatPC = CallTaxCamRanhInfo(c.parkingfee, rule)
    datestr = c.invoicedate.strftime("%Y-%m-%d")
    data = {
        "InvoiceDetails": [
            {
                "KeyDetail": "1",
                "tien_truoc_thue": withoutVat,
                "thue_suat": vatPC,
                "thue": vat,
                "tien_sau_thue": total,
                "loai_hd": "1",
                "nhom_dv": "0",
                "ten_nhom_dv": "Tại bãi giữ xe",
                "ma_dv": "1",
                "ten_dv": u"Phí gửi xe của ParkingID: %s"%c.parkingrefid,
                "bks": pk.check_in_alpr_vehicle_number,
                "so_ro_moc": "",
                "loi_yn": "0",
                "mien_phi_yn": "0",
                "dien_giai": u"Phí lượt gửi xe của ParkingID: %s"%c.parkingrefid,
                "dvt": "Lượt",
                "fnote1": None,
                "fnote2": None,
                "fnote3": None,
                "fnote4": None,
                "fnote5": None,
                "fnote6": None,
                "fnote7": None,
                "fnote8": None,
                "fnote9": None,
                "fnote10": None
            },
        ],
        "keyMaster": c.refid,
        "ma_kh": buyer.code if buyer is not None else None,
        "ten_kh": buyer.receivername if buyer is not None else None,
        "ma_so_thue": buyer.taxcode if buyer is not None else None,
        "dia_chi": buyer.address if buyer is not None else None,
        "ma_dvqhns": "",
        "e_mail": buyer.email if buyer is not None else None,
        "ma_tau": "",
        "ngay_ct": format_datetime_to_api(nowd),
        "ngay_lct": format_datetime_to_api(nowd),
        "gio_vao": format_datetime_to_api(pk.check_in_time),
        "gio_ra": format_datetime_to_api(pk.check_out_time) if pk.check_out_time is not None else format_datetime_to_api(nowd),
    }

    return data


def TryDoCallRetailInvoice(c, partnerid, token, maxamount):
    try:
        partner = PartnerInvoice.objects.get(id=partnerid)  # TESTMISAINVOICE
        if partner.activated and (partner.code == "TESTMISAINVOICE" or partner.code == "MISAINVOICE"):
            c_api = InvoiceApiInitation.objects.filter(partner=partner, target=2)
            if c_api:
                c_api = c_api[0]
                headers = {
                    "Content-Type": "application/json",
                    "Authorization": "Bearer %s" % token
                }
                urlmisa = c_api.url
                amount = c.amountrequested
                while amount <= maxamount:
                    amount += 1
                    c.amountrequested = amount
                    c.save()
                    x = requests.post(url=urlmisa, data=c.contentrequest, headers=headers)
                    ct = x.content
                    st = x.status_code
                    if st == 200:
                        try:
                            jsd = json.loads(ct)
                            if "success" in jsd and jsd["success"] is True and "publishInvoiceResult" in jsd and jsd[
                                "publishInvoiceResult"] is not None:
                                res = json.loads(jsd["publishInvoiceResult"])
                                if res:
                                    res = res[0]
                                    if 'TransactionID' in res and res['TransactionID'] is not None:
                                        c.transactionid = res['TransactionID']
                                        c.contentresponse = jsd
                                        c.iscompleted = True
                                        c.save()
                                        break
                                    else:
                                        pass

                        except Exception as ex:
                            pass
                    else:
                        pass

    except Exception as eee:
        pass
def TryDoCallRetailInvoiceCamRanh(c, partnerid, token, maxamount):
    try:
        partner = PartnerInvoice.objects.get(id=partnerid)  # TESTMISAINVOICE
        if partner.activated and partner.code == "FAST_CAMRANH_INVOICE" :
            c_api = InvoiceApiInitation.objects.filter(partner=partner, target=2)
            if c_api:
                c_api = c_api[0]
                headers = {
                    "Content-Type": "application/json",
                    "Authorization": "Bearer %s" % token
                }
                urlmisa = c_api.url
                amount = c.amountrequested
                while amount <= maxamount:
                    amount += 1
                    c.amountrequested = amount
                    c.save()
                    x = requests.post(url=urlmisa, data=c.contentrequest, headers=headers)
                    ct = x.content
                    st = x.status_code
                    if st == 200:
                        try:
                            jsd = json.loads(ct)
                            c.contentresponse = jsd
                            c.iscompleted = True
                            c.save()
                            break
                        except Exception as ex:
                            pass
                    else:
                        pass

    except Exception as eee:
        pass

def TryDoCallMultiRetailInvoice(dts, partnerid, token, maxamount, rule, series):
    try:
        partner = PartnerInvoice.objects.get(id=partnerid)  # TESTMISAINVOICE
        if partner.activated and (partner.code == "TESTMISAINVOICE" or partner.code == "MISAINVOICE"):
            c_api = InvoiceApiInitation.objects.filter(partner=partner, target=2)
            if c_api:
                c_api = c_api[0]
                headers = {
                    "Content-Type": "application/json",
                    "Authorization": "Bearer %s" % token
                }
                urlmisa = c_api.url
                for c in dts:
                    invoicedata = makeRetailInvoiceData(c, series, rule, None)
                    sendData = {
                        "SignType": 2,
                        "InvoiceData": invoicedata,
                        "PublishInvoiceData": None
                    }
                    data = str(json.dumps(sendData, ensure_ascii=False))
                    c.contentrequest = data
                    c.save()
                    amount = c.amountrequested
                    while amount <= maxamount:
                        amount += 1
                        c.amountrequested = amount
                        c.save()
                        x = requests.post(url=urlmisa, data=c.contentrequest, headers=headers)
                        ct = x.content
                        st = x.status_code
                        if st == 200:
                            try:
                                jsd = json.loads(ct)
                                if "success" in jsd and jsd["success"] is True and "publishInvoiceResult" in jsd and jsd[
                                    "publishInvoiceResult"] is not None:
                                    res = json.loads(jsd["publishInvoiceResult"])
                                    if res:
                                        res = res[0]
                                        if 'TransactionID' in res and res['TransactionID'] is not None:
                                            c.transactionid = res['TransactionID']
                                            c.contentresponse = jsd
                                            c.iscompleted = True
                                            c.save()
                                            break
                                        else:
                                            pass
                            except Exception as ex:
                                pass
                        else:
                            pass
        elif partner.activated and partner.code == 'FAST_CAMRANH_INVOICE':
            c_api = InvoiceApiInitation.objects.filter(partner=partner, target=2)

            if c_api:
                c_api = c_api[0]
                headers = {
                    "Content-Type": "application/json",
                    "Authorization": "Bearer %s" % token
                }
                urlmisa = c_api.url
                for c in dts:
                    pk = ParkingSession.objects.get(id=c.parkingrefid)
                    invoicedata = makeRetailInvoiceDataCamRanh(c, series, rule, None, pk)

                    data = str(json.dumps(invoicedata, ensure_ascii=False))
                    c.contentrequest = data
                    c.save()
                    amount = c.amountrequested
                    while amount <= maxamount:
                        amount += 1
                        c.amountrequested = amount
                        c.save()
                        x = requests.post(url=urlmisa, data=c.contentrequest, headers=headers)
                        ct = x.content
                        st = x.status_code
                        if st == 200:
                            try:
                                jsd = json.loads(ct)
                                c.contentresponse = jsd
                                c.iscompleted = True
                                c.save()
                                break

                            except Exception as ex:
                                pass
                        else:
                            pass
    except Exception as eee:
        pass

def DoAgaimRetail():
    validate, token, series, tempid, maxamount, partnerid, hour, minute, second = getInvoiceToken()
    if validate is not True:
        return False
    rts  =RetailInvoice.objects.filter(iscompleted = 0, amountrequested__lte = maxamount)
    if rts:
        rules = InvoiceTaxRule.objects.filter(mode=2, activated=True)
        if rules:
            rule = rules[0]
        else:
            rule = None
        Process(target=TryDoCallMultiRetailInvoice, args=(rts, partnerid, token, maxamount, rule, series)).start()

def DoPlanRetail():
    ids = GetAfterIds()
    if ids:
        validate, token, series, tempid, maxamount, partnerid, hour, minute, second = getInvoiceToken()
        if validate is not True:
            return False
        slt = []
        rules = InvoiceTaxRule.objects.filter(mode=2, activated=True)
        if rules:
            rule = rules[0]
        else:
            rule = None
        for parkingId in ids:
            try:
                pk = ParkingSession.objects.get(id=parkingId)
                parkingFee = get_fee(parkingId)
                if parkingFee <=0:
                    continue
                chk = RetailInvoice.objects.filter(parkingrefid=str(parkingId), parkingcompleted=True)
                if chk:
                    continue

                now = datetime.now()
                c = RetailInvoice()
                c.parkingrefid = str(parkingId)
                c.parkingfee = parkingFee
                c.refid = str(uuid.uuid4())
                c.buyer = None
                c.amountrequested = 0
                c.reqestedtime = now
                c.parkingcompleted = 1
                c.invoicedate = now.date()
                c.iscompleted = False
                c.save()
                # invoicedata = makeRetailInvoiceData(c, series, rule, None)
                # sendData = {
                #     "SignType": 2,
                #     "InvoiceData": invoicedata,
                #     "PublishInvoiceData": None
                # }
                # data = str(json.dumps(sendData, ensure_ascii=False))
                # c.contentrequest = data
                # c.save()
                slt.append(c)
                #Process(target=TryDoCallRetailInvoice, args=(c, partnerid, token, maxamount)).start()
            except Exception as ex:
                pass
        if slt:
            Process(target=TryDoCallMultiRetailInvoice, args=(slt, partnerid, token, maxamount, rule, series)).start()
def DoRetailInvoice(parkingId, parkingFee, parkingcompleted = 1, hasBuyer = False, code = None, legalName = None,
                    buyername = None, taxcode = None, address = None, phone= None,
                    email = None, receivername = None, receiveremails = None):
    try:
        validate, token, series, tempid, maxamount, partnerid, hour, minute, second = getInvoiceToken()
        if validate is not True:
            return False
        pk = ParkingSession.objects.get(id = parkingId)
        chk = RetailInvoice.objects.filter(parkingrefid = str(parkingId), parkingcompleted=True)
        partner = PartnerInvoice.objects.get(id=partnerid)  # TESTMISAINVOICE

        if chk:
            return False
        rules = InvoiceTaxRule.objects.filter(mode=2, activated=True)
        if rules:
            rule = rules[0]
        else:
            rule = None
        if hasBuyer is True:
            buyer = InvoiceBuyer()
            buyer.mode = 2
            buyer.code = code
            buyer.legalName = legalName
            buyer.buyername = buyername
            buyer.taxcode = taxcode
            buyer.address =address
            buyer.phone = phone
            buyer.email = email
            buyer.receivername = receivername
            buyer.receiveremails = receiveremails
            buyer.save()
        else:
            buyer = None
        now = datetime.now()
        c = RetailInvoice()
        c.parkingrefid = str(parkingId)
        c.parkingfee =parkingFee
        c.refid = str(uuid.uuid4())
        c.buyer = buyer
        c.amountrequested = 0
        c.reqestedtime = now
        c.parkingcompleted = parkingcompleted
        c.invoicedate = now.date()
        c.iscompleted = False
        c.save()
        if partner.activated and (partner.code == "TESTMISAINVOICE" or partner.code == "MISAINVOICE"):
            invoicedata = makeRetailInvoiceData(c, series, rule, buyer)
            sendData = {
                "SignType": 2,
                "InvoiceData": invoicedata,
                "PublishInvoiceData": None
            }
            data = str(json.dumps(sendData, ensure_ascii=False))
            c.contentrequest = data
            c.save()
            Process(target=TryDoCallRetailInvoice, args=(c, partnerid, token, maxamount)).start()
            print('Success:%s'%parkingId)
            return True
        elif partner.activated and partner.code == 'FAST_CAMRANH_INVOICE':
            invoicedata = makeRetailInvoiceDataCamRanh(c, series, rule, buyer, pk)
            data = str(json.dumps(invoicedata, ensure_ascii=False))
            c.contentrequest = data
            c.save()
            Process(target=TryDoCallRetailInvoiceCamRanh, args=(c, partnerid, token, maxamount)).start()
            print('Success:%s' % parkingId)
            return True
    except Exception as ex:
        return False
def make_authenticate(obj):
    urlmisaToken = ""
    if obj.partner.code == 'TESTMISAINVOICE' or obj.partner.code == 'MISAINVOICE':
        if obj.partner.code == 'TESTMISAINVOICE':
            urlmisaToken = "https://testapi.meinvoice.vn/api/integration/auth/token"
        elif obj.partner.code == 'MISAINVOICE':
            urlmisaToken = "https://api.meinvoice.vn/api/integration/auth/token"
        sendData = {
            "appid": obj.appid,
            "taxcode": obj.taxcode,
            "username": obj.username,
            "password": obj.password
        }
        data = str(json.dumps(sendData, ensure_ascii=False))
        headers = {'Content-Type': "application/json"}
        x = requests.post(url=urlmisaToken, data=data, headers=headers)
        ct = x.content
        st = x.status_code
        if st == 200:
            try:
                jsd = json.loads(ct)
                if "success" in jsd and jsd["success"] is True and "data" in jsd and jsd["data"] is not None:
                    obj.token = jsd["data"]
                    obj.isvalid = True
                    obj.lastupdate = datetime.datetime.now()
                    obj.save()
            except Exception as ex:
                pass
    elif obj.partner.code == 'FAST_CAMRANH_INVOICE':
        urlmisaToken = "https://14.248.85.18:9987/camranhs/oauth2/token"
        data = {
            "client_secret": "f1f9424a-925f-4fa9-9007-ad3adf8c18b9",
            "grant_type": "client_credentials",
            "client_id": "camranhport"
        }
        headers = {
            "Content-Type": "application/x-www-form-urlencoded"
        }
        x = requests.post(url=urlmisaToken, data=data, headers=headers)
        ct = x.content
        st = x.status_code
        if st == 200:
            try:
                jsd = json.loads(ct)
                if "access_token" in jsd and jsd["access_token"] is not None:
                    obj.token = jsd["access_token"]
                    obj.isvalid = True
                    obj.lastupdate = datetime.datetime.now()
                    obj.save()
            except Exception as ex:
                pass

def DoSetToken():
    try:
        conns = InvoiceConnector.objects.filter(partner__activated=True)
        if conns:
            conn = conns[0]
            Process(target=make_authenticate, args=(conn)).start()
    except:
        pass