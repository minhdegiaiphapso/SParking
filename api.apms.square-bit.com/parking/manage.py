# -*- coding: utf-8 -*-
#!/usr/bin/env python
import os
import sys
from datetime import datetime, timedelta, date,time
from io import  BytesIO
from reportlab.pdfgen import canvas
from reportlab.lib import colors
from reportlab.lib.pagesizes import letter, mm, inch, A0, A2, A3, A4
from reportlab.platypus import Spacer, Image, Paragraph, BaseDocTemplate, SimpleDocTemplate, Table, TableStyle
from reportlab.lib.styles import getSampleStyleSheet,ParagraphStyle,TA_CENTER
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.pdfbase import pdfmetrics
from django.db import connections
from apscheduler.schedulers.background import BackgroundScheduler
scheduler = BackgroundScheduler()
job = None
now = datetime.now()
def clean_param(param):
    if hasattr(param, '_get_pk_val'):
        # has a pk value -- must be a model
        return str(param._get_pk_val())

    if callable(param):
        # it's callable, should call it.
        return str(param())

    return str(param)
def QuerySecond( proc_name, *proc_params):
    new_params = [clean_param(param) for param in proc_params]
    cursor = connections['secondary'].cursor()
    ret = cursor.execute("%s %s(%s)" % ("CALL",
                                        proc_name,
                                        ', '.join('%s' for x in new_params)),
                         new_params)

    rows = cursor.fetchall()
    retVal = []
    for row in rows:
        retVal.append(row)
    return retVal
def natConfig():
    configdata = QuerySecond("getnatconfig")
    if not configdata:
        return {"FolderShared":"\\\\172.16.0.1\\SharingNATTest","UserName":"nhide","Password":"123","RepeatSecond":10,"ScheduleTime":"22:00:00"}
    else:
        return {"FolderShared": configdata[0][1], "UserName": configdata[0][2], "Password": configdata[0][3], "RepeatSecond": configdata[0][4], "ScheduleTime": configdata[0][5]}
# natConfig=natConfig()
def add_pdfdata_fee(data, newdata, rowHeights):  # Them sheet vao workbook
    HEADER = ('STT','Ngày tác động','Tác động bở','Nơi tác động','Thao Tác','Mục tiêu tác động')
    tmpdata=[]
    pdfmetrics.registerFont(TTFont('Arial', 'Arial.ttf'))
    styleN = ParagraphStyle(
        name='Normal',
        fontName='Arial',
        fontSize=7,
    )
    styleT = ParagraphStyle(
        name='Normal',
        fontName='Arial',
        fontSize=7,
        textColor='green'
    )
    for i, column in enumerate(HEADER):  # Viet table header
        tmpdata.append(Paragraph(HEADER[i], styleT))
    data.append(tmpdata)
    rowHeights.append(40)
    # Write data
    stt = 1
    for d in newdata:  # Prepare data
        try:
            tmpdata = []
            tmpdata.append(stt)
            tmpdata.append(Paragraph(d[0], styleN))
            tmpdata.append(Paragraph(d[1], styleN))
            tmpdata.append(Paragraph(d[2], styleN))
            tmpdata.append(Paragraph(d[3], styleN))
            tmpdata.append(Paragraph(d[4], styleN))
            data.append(tmpdata)
            rowHeights.append(30)
            stt=stt+1
        except Exception as e:
            a=1
class TransactionFeePDF(object):
    """"""

    # ----------------------------------------------------------------------
    def __init__(self):
        """Constructor"""
        self.width, self.height = letter
        self.styles = getSampleStyleSheet()
    # ----------------------------------------------------------------------
    # def coord(self, x, y, unit=1):
    #     """
    #     http://stackoverflow.com/questions/4726011/wrap-text-in-a-table-reportlab
    #     Helper class to help position flowables in Canvas objects
    #     """
    #     x, y = x * unit, self.height - y * unit
    #     return x, y
    #
    # # ----------------------------------------------------------------------
    def run(self):
        """
        Run the report
        """
        myConfig = natConfig()
        seconds = myConfig["RepeatSecond"] if myConfig["RepeatSecond"] and myConfig["RepeatSecond"] >= 10 and myConfig[
                                                                                                                  "RepeatSecond"] <= 3600 else 3600
        now = datetime.now()
        checkdate = (now + timedelta(days=-1)).replace(hour=23, minute=59, second=59)
        if now + timedelta(seconds=-seconds) <= checkdate:
            now = checkdate

        _from_time = datetime(year = now.year, month= now.month, day= now.day)
        _to_time=datetime(year = now.year, month= now.month, day= now.day, hour=23, minute=59, second=59)
        newdata = QuerySecond('gethistoryaccess', _from_time.strftime('%Y-%m-%d %H:%M:%S'),
                                   _to_time.strftime('%Y-%m-%d %H:%M:%S'))
        if not newdata:
            pass
        else:
            try:
                a = Image("parking/static/image/logo_report.png", 0.75 * inch, 0.5 * inch)
            except Exception as e:
                a = ''
            data = [[a,u'        BÁO CÁO LỊCH SỬ TÁC ĐỘNG    ngày lập: %s' % (now.strftime("%d/%m/%Y %H:%M:%S")),'','','',''],
                    ['',u'        Từ ngày: %s đến ngày: %s' % (_from_time.strftime("%d/%m/%Y %H:%M:%S"),_to_time.strftime("%d/%m/%Y %H:%M:%S")),'','','',''],
                    ['','','','','','']]
            rowHeights = []
            add_pdfdata_fee(data, newdata, rowHeights)
            filepath= "\\\\172.16.0.1\\SharingNATTest\\ConfigFeeChangedEvent_%s.pdf"%(now.strftime("%Y%b%d"))
            if os.path.isfile(filepath):
                os.remove(filepath)
            self.doc = SimpleDocTemplate(filepath, pagesize=A4, rightMargin=30, leftMargin=30, topMargin=30,
                                         bottomMargin=30, unicode="utf-8")
            self.story = [Spacer(1, 1)]
            self.createLineItems(data)
            self.doc.build(self.story, onFirstPage=self.addPageNumber, onLaterPages=self.addPageNumber)
            with open(filepath,'r') as f:
                f.read()
    # ----------------------------------------------------------------------
    def addPageNumber(self, canvas, doc):
        """
        Create the document
        """
        page_num = canvas.getPageNumber()
        text = "Trang %s" % page_num
        canvas.drawRightString(200 * mm, 5 * mm, text)

    # ----------------------------------------------------------------------
    def createLineItems(self,data):
        """
        Create the line items
        """
        colwidths = (30, 80, 80, 100, 60, None)
        pdfmetrics.registerFont(TTFont('Arial', 'Arial.ttf'))
        GRID_STYLE = TableStyle([
            ('SPAN', (0, 0), (0, 2)),
            ('SPAN', (1, 0), (5, 0)),
            ('SPAN', (2, 0), (5, 0)),
            ('FONTSIZE', (0, 0), (1, 2), 12),
            ('INNERGRID', (0, 4), (-1, -1), 0.05, colors.green),
            ('BOX', (0, 3), (-1, -1), 0.05, colors.black),
            ('TEXTCOLOR', (0, 3), (5, 3), colors.green),
            ('FONTSIZE', (0, 3), (5, 3), 7),
            ('FONTSIZE', (0, 4), (4, -1), 7),
            ('FONTSIZE', (5, 4), (5, -1), 7),
            ('FONTNAME', (0, 0), (-1, -1), 'Arial'),
            ('ALIGN', (0, 0), (-1, -1), 'LEFT'),
            ('VALIGN', (0, 0), (-1, -1), 'MIDDLE'),
        ])
        table = Table(data,colWidths=colwidths,rowHeights=None,style=GRID_STYLE)
        self.story.append(table)
def tick():
    myConfig = natConfig()
    seconds = myConfig["RepeatSecond"] if myConfig["RepeatSecond"] and myConfig["RepeatSecond"] >= 10 and myConfig["RepeatSecond"] <= 3600 else 3600
    now = datetime.now()
    checkdate=(now+timedelta(days=-1)).replace(hour=23,minute=59,second=59)
    if now+timedelta(seconds=-seconds) <= checkdate:
        now = checkdate
    curdate = now.strftime("%Y%m%d")
    ScheduleTime=myConfig["ScheduleTime"] if myConfig["ScheduleTime"] else "23:59:59"
    comparedate=datetime.strptime("%s %s"%(curdate,myConfig["ScheduleTime"]),"%Y%m%d %H:%M:%S")
    if now >= comparedate:
        nat=TransactionFeePDF()
        nat.run()
        #print('On tick! time: %s')
def start_job():
    global job
    myConfig = natConfig()
    seconds = myConfig["RepeatSecond"] if myConfig["RepeatSecond"] and myConfig["RepeatSecond"] >= 10 and myConfig["RepeatSecond"] <= 3600 else 3600
    job = scheduler.add_job(tick, 'interval', seconds=seconds)
    try:
        scheduler.start()
    except:
        pass
if __name__ == "__main__":
    os.environ.setdefault("DJANGO_SETTINGS_MODULE", "site_settings.settings")
    from django.core.management import execute_from_command_line
    #start_job()
    execute_from_command_line(sys.argv)

