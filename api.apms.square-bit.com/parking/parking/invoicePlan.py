# -*- coding: utf-8 -*-
from schedulerservice import SPSchedulerBackground
from invoiceService import getInvoiceToken, DoConsolidated, \
    DoSetToken, DoRetailInvoice,DoPlanRetail,DoAgaimRetail
import datetime
from threading import  Thread

spBackground = SPSchedulerBackground()
validate, token, series, tempid, maxamount, partnerid, hour, minute, second = getInvoiceToken()
# def ConsolidatedInvoiceForLastDate():
#     DoConsolidated()
#     #spBackground.ChangeTimeByJobId("AutoConsolidatedInvoice", 3,5,5)
#     print 'success'

def DailyConsolidatedInvoice():
    task = Thread(target=DoConsolidated)
    task.daemon = True
    task.start()

def WeeklySetToken():
    task = Thread(target=DoSetToken)
    task.daemon = True
    task.start()

def AutoRetail():
    task = Thread(target=DoPlanRetail)
    task.daemon = True
    task.start()

def AutoRetailAgain():
    task = Thread(target=DoAgaimRetail)
    task.daemon = True
    task.start()
#spBackground.AddEveryDayJob(jobId="AutoConsolidatedInvoice", atTime=datetime.time(hour=hour,minute=minute,second=second), Job=DailyConsolidatedInvoice)
spBackground.AddEveryDayOfWeekJob("AutoSetToken", dow=3, atTime=datetime.time(hour=0,minute=0,second=0), Job=WeeklySetToken)
spBackground.AddAfterATime(jobId = "AutoRetailAfterCheckOut", hours=0, minutes=2, seconds=45, Job=AutoRetail)

spBackground.AddAfterATime(jobId = "AutoRetailAgain", hours=0, minutes=10, seconds=15, Job=AutoRetailAgain)

def SetTimeConsolidatedInvoice(hour, minute, second):
    spBackground.ChangeTimeByJobId("AutoConsolidatedInvoice", hour, minute, second)
