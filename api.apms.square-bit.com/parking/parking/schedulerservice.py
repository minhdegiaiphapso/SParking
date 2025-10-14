# -*- coding: utf-8 -*-

from datetime import datetime, time, timedelta
import  time as times
from pytz import timezone
from threading import  Thread
TIME_ZONE = 'Asia/Saigon'
class SPSchedulerType:
    __sType=["everyday", "everydayofweek", "everydayofmonth", "everythefirstdayofmonth", "everythelastdayofmonth", "afteratime"]
    __dow=[0,1, 2, 3, 4, 5, 6] ## 0=Monday, ..., 6=Sunday
    def makeEveryDay(self,atTime):
        if isinstance(atTime, time):
            return ("everyday", (atTime.hour, atTime.minute, atTime.second))
        return  None
    def makeEveryDayOfWeek(self,dow, atTime):
        if dow in self.__dow and isinstance(atTime, time):
            return ("everydayofweek", (dow,(atTime.hour, atTime.minute, atTime.second)))
        return  None
    def makeEveryDayOfMonth(self, day, atTime):
        if isinstance(day,int) and day >0 and day <32 and isinstance(atTime, time):
            return ("everydayofmonth", (day,(atTime.hour, atTime.minute, atTime.second)))
        return  None
    def makeEveryTheFirstDayOfMonth(self, atTime):
        if  isinstance(atTime, time):
            return ("everythefirstdayofmonth", (1,(atTime.hour, atTime.minute, atTime.second)))
        return None
    def makeEveryTheLastDayOfMonth(self, atTime):
        if isinstance(atTime, time):
            return ("everythelastdayofmonth",  (31,(atTime.hour, atTime.minute, atTime.second)))
        return None
    def makeAfterATime(self, hours, minutes, seconds):
        if isinstance(hours, int) and hours >=0 and hours<=23 and isinstance(minutes, int) and minutes>=0 and minutes<=59 and isinstance(seconds, int) and seconds>=0 and seconds <=59:
            return ("afteratime", (hours, minutes, seconds))
        return None

class SPSchedulerBackground():
    __sType = SPSchedulerType()
    __jobList =[]
    __isRunning = False
    __task= None
    def AddEveryDayJob(self, jobId, atTime, Job):
        if isinstance(jobId, str) and jobId not in self.__jobList:
            pr = self.__sType.makeEveryDay(atTime)
            if pr is None:
                return False
            self.__jobList.append({"JobId":jobId, "Param":pr, "Job":Job})
            return  True
        return False
    def AddEveryDayOfWeekJob(self,jobId, dow, atTime, Job):
        if isinstance(jobId, str) and jobId not in self.__jobList:
            pr = self.__sType.makeEveryDayOfWeek(dow, atTime)
            if pr is None:
                return False
            self.__jobList.append({"JobId":jobId, "Param":pr, "Job":Job})
            return  True
        return False
    def AddEveryDayOfMonth(self, jobId, day, atTime, Job):
        if isinstance(jobId, str) and jobId not in self.__jobList:
            pr = self.__sType.makeEveryDayOfMonth(day, atTime)
            if pr is None:
                return False
            self.__jobList.append({"JobId":jobId, "Param":pr, "Job":Job})
            return  True
        return False
    def AddEveryTheFirstDayOfMonth(self, jobId, atTime, Job):
        if isinstance(jobId, str) and jobId not in self.__jobList:
            pr = self.__sType.makeEveryTheFirstDayOfMonth(atTime)
            if pr is None:
                return False
            self.__jobList.append({"JobId":jobId, "Param":pr, "Job":Job})
            return  True
        return False
    def AddEveryTheLastDayOfMonth(self, jobId, atTime, Job):
        if isinstance(jobId, str) and jobId not in self.__jobList:
            pr = self.__sType.makeEveryTheLastDayOfMonth(atTime)
            if pr is None:
                return False
            self.__jobList.append({"JobId":jobId, "Param":pr, "Job":Job})
            return  True
        return False
    def AddAfterATime(self, jobId, hours, minutes, seconds, Job):
        if isinstance(jobId, str) and jobId not in self.__jobList:
            pr = self.__sType.makeAfterATime(hours, minutes, seconds)
            if pr is None:
                return False
            self.__jobList.append({"JobId":jobId, "Param":pr, "Job":Job})
            return  True
        return False
    def StartScheduler(self, startAt):
        if not self.IsRunning:
            [self.makeNextExcuteTime(jobItem, startAt) for jobItem in self.__jobList]
            self.__task = Thread(target=self.run)
            self.__task.daemon = True
            self.__isRunning = True
            self.__task.start()
    def StopShceduler(self, ):
        if self.__isRunning:
            self.__isRunning = False
    def PrintPlan(self):
        def strJob(jobitem):
            if "NextTime" in jobitem and isinstance(jobitem["NextTime"], datetime):
                return  "Plan '%s': %s"%(jobitem["JobId"], jobitem["NextTime"].strftime("%d/%m/%Y %H:%M:%S"))
            else:
                return  "'%s': %s"%(jobitem["JobId"], "No plan")

        if self.__jobList:
            print ('\n'.join([ strJob(jobitem) for jobitem in self.__jobList]))
        else:
            print ('No Jobs')
    def ChangeTimeByJobId(self, jobId, hour, minute, second):
        [self.changetime(jobItem, hour, minute, second) for jobItem in self.__jobList if "JobId" in jobItem and jobItem["JobId"] == jobId]
    @property
    def IsRunning(self):
        return self.__isRunning
    def run(self):
        while self.IsRunning:
            for jobItem in self.__jobList:
                self.executeJob(jobItem)
            times.sleep(1)
    @classmethod
    def changetime(cls, jobItem, hour, minute, second):
        if cls.IsRunning and "JobId" in jobItem and "Param" in jobItem and "Job" in jobItem and "NextTime" in jobItem :
            now = datetime.now(tz= timezone(TIME_ZONE))
            h, m, s = jobItem["NextTime"].hour, jobItem["NextTime"].minute, jobItem["NextTime"].second
            if h!= hour or m != minute or s!=second:
                jobItem["NextTime"] = jobItem["NextTime"].replace(hour = hour, minute = minute, second = second)
                if now >=jobItem["NextTime"]:
                    jobItem["NextTime"] = cls.setNext(jobItem["Param"], jobItem["NextTime"])
                print (u'Next plan of "%s": %s' % (jobItem["JobId"], jobItem["NextTime"].strftime("%d/%m/%Y %H:%M:%S")))
    @classmethod
    def executeJob(cls, jobItem):
        if cls.IsRunning and "JobId" in jobItem and "Param" in jobItem and "Job" in jobItem and "NextTime" in jobItem :
            now = datetime.now(tz= timezone(TIME_ZONE))
            if now >=jobItem["NextTime"]:
                jobItem["NextTime"] = cls.setNext(jobItem["Param"], jobItem["NextTime"])
                if "Executing" in jobItem and jobItem["Executing"]:
                    return
                jobItem["Executing"] = True
                jobItem["Job"]()
                jobItem["Executing"] = False
                print (u'Next plan of "%s": %s' % (jobItem["JobId"], jobItem["NextTime"].strftime("%d/%m/%Y %H:%M:%S")))
    @classmethod
    def setNext(cls, jobParam, currentTime):
        if jobParam[0]== "everyday":
            return currentTime+ timedelta(days=1)
        if jobParam[0] == "everydayofweek":
            return currentTime + timedelta(days=7)
        if jobParam[0] == "everydayofmonth":
            dayA = jobParam[1][0]
            ld = cls.lastDate(currentTime)
            nd = ld + timedelta(days = 1)
            lld = cls.lastDate(nd)
            while dayA > lld.day:
                lld = cls.lastDate(lld)
            return lld.replace(day = dayA)
        if jobParam[0] == "everythefirstdayofmonth":
            return cls.lastDate(currentTime) + timedelta(days = 1)
        if jobParam[0] == "everythelastdayofmonth":
            nd = cls.lastDate(currentTime) + timedelta(days=1)
            return cls.lastDate(nd)
        if jobParam[0] == "afteratime":
            return currentTime + timedelta(hours=jobParam[1][0], minutes=jobParam[1][1], seconds=jobParam[1][2])
    @classmethod
    def makeNextExcuteTime(cls, jobItem, startAt):
        if "JobId" in jobItem and "Param" in jobItem and "Job" in jobItem and isinstance(startAt, datetime):
            jobParam = jobItem["Param"]
            now = startAt #startAt.astimezone(timezone(TIME_ZONE))
            if jobParam[0] == "everyday":
                ext = now.replace(hour=jobParam[1][0], minute= jobParam[1][1], second= jobParam[1][2])
                if ext <=now:
                    ext = ext + timedelta(days=1)
                jobItem["NextTime"] = ext
                jobItem["Executing"] = False
            if jobParam[0] == "everydayofweek":
                cdow = now.weekday()
                dow = jobParam[1][0]
                if dow > cdow:
                    d = dow - cdow
                    ext = now.replace(hour=jobParam[1][1][0], minute=jobParam[1][1][1],
                                      second=jobParam[1][1][2]) + timedelta(days=d)
                elif dow == cdow:
                    ext = now.replace(hour=jobParam[1][1][0], minute=jobParam[1][1][1], second=jobParam[1][1][2])
                    if ext <= now:
                        ext = ext + timedelta(days=7)
                else:
                    d = (dow + 7) - cdow
                    ext = now.replace(hour=jobParam[1][1][0], minute=jobParam[1][1][1],
                                      second=jobParam[1][1][2]) + timedelta(days=d)
                jobItem["NextTime"] = ext
                jobItem["Executing"] = False
            if jobParam[0]== "everydayofmonth":
                dayA = jobParam[1][0]
                llld = cls.lastDate(now)
                ext = None
                if dayA<=llld.day:
                    ext = now.replace(hour=jobParam[1][1][0], minute=jobParam[1][1][1], second=jobParam[1][1][2], day = dayA)
                if ext is not None and ext> now:
                    pass
                else:
                    ext = now.replace(hour=jobParam[1][1][0], minute=jobParam[1][1][1], second=jobParam[1][1][2])
                    ld = cls.lastDate(ext)
                    nd = ld + timedelta(days=1)
                    lld = cls.lastDate(nd)
                    while dayA > lld.day:
                        lld = cls.lastDate(lld)
                    ext = lld.replace(day = dayA)
                jobItem["NextTime"] = ext
                jobItem["Executing"] = False
            if jobParam[0]=="everythefirstdayofmonth":
                ext = now.replace(hour=jobParam[1][1][0], minute=jobParam[1][1][1], second=jobParam[1][1][2], day=1)
                if ext> now:
                    pass
                else:
                    ext = cls.lastDate(ext) + timedelta(days=1)
                jobItem["NextTime"] = ext
                jobItem["Executing"] = False
            if jobParam[0] == "everythelastdayofmonth":
                tmp = now.replace(hour=jobParam[1][1][0], minute=jobParam[1][1][1], second=jobParam[1][1][2])
                ext = cls.lastDate(tmp)
                if ext<=now:
                    tmp =  ext + timedelta (days=1)
                    ext = cls.lastDate(tmp)
                jobItem["NextTime"] = ext
                jobItem["Executing"] = False
            if jobParam[0] =="afteratime":
                ext = now + timedelta(hours=jobParam[1][0], minutes=jobParam[1][1], seconds = jobParam[1][2])
                jobItem["NextTime"] = ext
                jobItem["Executing"] = False
    @classmethod
    def lastDate(cls, currentDate):
        tmp = currentDate.replace(day = 28) + timedelta(days = 4)
        return tmp - timedelta(days= tmp.day)
