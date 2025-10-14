    myapp = angular.module("myapp", ['ngRoute']);
    myapp.filter('usedstatus', function () {
      return function (item) {
          try
          {
                switch(item)
                {
                    case 1:
                       return "Chưa áp dụng";
                    case 0:
                       return "Chưa hoàn chỉnh";
                    case 2:
                       return "Đang dùng";
                    case 3:
                       return "Đã khóa";
                    case 4:
                       return "Đang dùng";
                    default:
                        return "Chưa rõ ràng";
                }
          }
          catch(err)
          {
                return "Chưa rõ ràng";
          }
      };
    });
    myapp.filter('minutes', function () {
      return function (item) {
          try
          {
            if(parseInt(item)>0)
                return item + ' phút';
            else
                return '';
          }
          catch(err)
          {
            return '';
          }
      };
    });
    myapp.filter('VND', function () {
      return function (item) {
          try
          {
            if(parseInt(item)>=0)
                return item + ' VNĐ';
            else
                return '';
          }
          catch(err)
          {
            return '';
          }
      };
    });
    myapp.filter('feetype',function(){
        return function (item) {
          try
          {
            switch(item)
            {
                case 1:
                    return "";
                case 2:
                    return "Lược 24h";
                case 3:
                    return "Phức hợp";
                case 4:
                    return "Redemption";
                default:
                    return "Chưa rõ";
            }
          }
          catch(err)
          {
            return "Chưa rõ";;
          }
      };
    });
    myapp.config(function ($interpolateProvider) {
    $interpolateProvider.startSymbol('[[').endSymbol(']]')});
    myapp.config(['$routeProvider',
        function($routeProvider) {
            $routeProvider.
                when('/configfee', {
                    templateUrl: static_url + 'Angular/TemplateHtml/test1.html',
                    controller: 'RouteController1'
                }).
                when('/testfee', {
                    templateUrl: static_url + 'Angular/TemplateHtml/test2.html',
                    controller: 'activesamples'
                })
                .
                when('/formulafee', {
                    templateUrl: static_url + 'Angular/TemplateHtml/formulafee.html',
                    controller: 'FormFee'
                })
                .
                when('/formulabill', {
                    templateUrl: static_url + 'Angular/TemplateHtml/formularbill.html',
                    controller: 'FormBill'
                })
                .
                when('/specialdate', {
                    templateUrl: static_url + 'Angular/TemplateHtml/specialdate.html',
                    controller: 'SpecialDate'
                }).
                when('/calcualteandreport', {
                    templateUrl: static_url + 'Angular/TemplateHtml/calculateandreportfee.html',
                    controller: 'calandreportfee'
                }).//grouptenant
                when('/configredemption', {
                    templateUrl: static_url + 'Angular/TemplateHtml/redemption.html',
                    controller: 'configredemption'
                }).
                 when('/grouptenant', {
                    templateUrl: static_url + 'Angular/TemplateHtml/tenantgroup.html',
                    controller: 'tenantgroup'
                }).//permission
                 when('/permission', {
                    templateUrl: static_url + 'Angular/TemplateHtml/permission.html',
                    controller: 'permission'
                }).
                when('/rootpermission', {
                    templateUrl: static_url + 'Angular/TemplateHtml/rootpermission.html',
                    controller: 'rootpermission'
                }).
                otherwise({
                    templateUrl: static_url + 'Angular/TemplateHtml/test1.html',
                    controller: 'RouteController1'
                });
        }]);
    myapp.controller("RouteController1", function($scope) {
        $scope.test="Thisorking test1"
    });
    myapp.controller("activesamples", function($scope,$location, $anchorScroll,dataService) {
        dataService.getactivepermission(2).then(function(data){
            $scope.mypermission=data.data;
        });
        $scope.removeitem=function(it,item, index)
        {
            $scope.loading=true;
            obj=[];
            obj.id=it.id;
            try{
                obj.idupdate=  item.detail[parseInt(index)+1].id;
            }
             catch(err)
            {
               obj.idupdate=-1;
            }
            obj.tablename="sampleactive";
            dataService.removeitem1(obj).then(function(data)
                {
                    if(data.data.result!="fail")
                    {
                        alert("Đã xóa thành công.");
                        $scope.getdetail(item);
                    }
                    else
                        alert(data.data.data);
                    $scope.loading=false;
                }
            );
        }
        $scope.sampleregis=[];
        $scope.loading=true;
        dataService.getsampleregitation().then(function(data){
                $scope.sampleregis=data.data;
                $scope.loading=false;
        });
        //getsampleregitationsimilar
        $scope.sampleregissimilar=[];
        $scope.loading=true;
        dataService.getsampleregitationsimilar().then(function(data){
                $scope.sampleregissimilar=data.data;
                $scope.loading=false;
        });
        $scope.convertdate = function(date) {
            var d = new Date(date),
                month = '' + (d.getMonth() + 1),
                day = '' + d.getDate(),
                year = d.getFullYear();

            if (month.length < 2) month = '0' + month;
            if (day.length < 2) day = '0' + day;

            return [year, month, day].join('-');
        }
        $scope.convertdate1 = function(date) {
            var d = new Date(date),
                month = '' + (d.getMonth() + 1),
                day = '' + d.getDate(),
                year = d.getFullYear();

            if (month.length < 2) month = '0' + month;
            if (day.length < 2) day = '0' + day;

            return [year, month, day].join('');
        }
        $scope.converttime = function(time) {
            return time.split(':').join('');
        }
        $scope.callfee=function(){
            $scope.loadingcall=true;
            var id=$scope.demo.id;
            //alert(id);
            var fromdate=$scope.convertdate1($scope.demo.checkindate)+$scope.converttime($scope.demo.checkintime);
            //alert(fromdate)
            var todate=$scope.convertdate1($scope.demo.checkoutdate)+$scope.converttime($scope.demo.checkouttime);
            //alert(todate);
            var expired;
            if($scope.demo.expireddate==undefined|| $scope.demo.expireddate==null)
                expired=todate;
            else
                expired=$scope.convertdate1($scope.demo.expireddate)+'000000'
            //alert(expired)
            dataService.getcallfee(id,fromdate,todate,expired).then(function(data){
                $scope.demo.detail=data.data;
                $scope.loadingcall=false;
        });
        }
        //post_sampleregis
        $scope.allsimilarfee=false;
        $scope.countsimilar=0;

        $scope.setsimilarfee=function(item)
        {
            if(item.checksimilar)
            {
                 l=$scope.sampleregis.length;
                 if($scope.countsimilar<l)
                    $scope.countsimilar=$scope.countsimilar+1;
            }
            else
            {
                if($scope.countsimilar>0)
                    $scope.countsimilar=$scope.countsimilar-1;
            }
        }
        $scope.ChkAllSimilarFee=function()
        {
            l=$scope.sampleregis.length;
            for(i=0;i<l;i++)
            {
                $scope.sampleregis[i].checksimilar=$scope.allsimilarfee;
            }
            if($scope.allsimilarfee)
                $scope.countsimilar=l;
            else
                $scope.countsimilar=0;
        }
        $scope.post_sampleregissimilar=function(item)
        {
            var tmp=$scope.sampleregissimilar;
            tmp.activedate=$scope.convertdate(item.activedate)
            var obj=JSON.stringify(tmp);
            $scope.loading=true;
            dataService.postSampleRegissimilar(obj).then(function(data){
                if(data.data.result!="fail")
                    {
                        $scope.loading=true;
                        dataService.getsampleregitationsimilar().then(function(data){
                                $scope.sampleregissimilar=data.data;
                                $scope.loading=false;
                        });
                        alert('Lư kết quả thành công.');
                    }
                else
                    alert(data.data.data);
                $scope.loading=false;
            });
        }
        $scope.post_sampleregis=function(item){
            $scope.loading=true;
            var id1=-1;
            var tmp={"activedate":$scope.convertdate(item.activedate),
                "cardtype":item.cardtype.id,"vehicletype":item.vehicletype.id,
                "sampleid":item.sampleset.sampleselected.id,
                "sampleid1":item.samplesset1.sampleselected.id};
            //console.log(tmp);
            var obj=JSON.stringify(tmp);
            //console.log(obj);
            dataService.postSampleRegis(obj).then(function(data){
                //console.log(data);
                if(data.data.result!="fail")
                    $scope.getdetail(item);
                else
                    alert(data.data.data);
                $scope.loading=false;
            });
        }
        $scope.getdetail=function(item)
        {
            var cardtype=item.cardtype.id;
            var vehicletype=item.vehicletype.id;
            dataService.regisfeedetail(cardtype,vehicletype).then(function(data)
            {
                item.detail=data.data;
                //console.log(item.detail);
                //return data.data;
            }
            );
        }
        $scope.scrollthis=function(index)
        {
//            alert(index);
//            $location.hash('item'+index);
//            $anchorScroll();
        }
        $scope.demo=[]
        $scope.calculate=function(it,item)
        {
             $scope.demo.cardtype=item.cardtype.name;
             $scope.demo.vehicletype=item.vehicletype.name;
             $scope.demo.samplename=it.samplename;
             $scope.demo.samplename1=it.samplename1;
             $scope.demo.id=it.id;
             $scope.demo.expireddate=null;
             $scope.demo.detail=[]
        }
    });
    myapp.controller('24hdetail',function($scope,dataService)
    {

         dataService.getactivepermission(1).then(function(data){
            $scope.mypermission=data.data;
        });
        $scope.getdetail=function(id){
            $scope.loading=true;
             dataService.getSampleFeeDetail24(id).then(function(data){
                $scope.detailfee24h=data.data;
                $scope.detailfee24h.canrepeat=data.data.canrepeat==1;
                $scope.exceptfee=data.data.exceptfee==1;

                $scope.blocks=data.data.blockhours;
                 $scope.loading=false;
            });
        }
        $scope.checkmoneybythis=function(val)
        {
            if (val === null|| val ==='undefined') {
                return true;
            }
            if( val!='')
            {
                //console.log(val);
                try{
                    if(parseInt(val)%1000==0)
                        return true;
                    else
                        return false;
                }
                catch(err)
                {
                   return false;
                }
            }
            return true;
        }
        $scope.post24h=function(sf){
            var obj=[];
            var data=[];
            data.blocks=$scope.blocks;
            data.after24hfee=$scope.detailfee24h.affterfee;
            data.after24htype=$scope.detailfee24h.canrepeat;

            data.exceptfee=$scope.exceptfee;
            obj.id=$scope.detailfee24h.id;
            obj.samplefeeid=$scope.detailfee24h.samplefeeid;
            obj.data=data;
            //console.log(obj);
            //dataService.postSample24h(obj);
            dataService.postSample24h(obj).then(function(data){
                if(data.data!="fail")
                {
                    alert('Đã cập nhật chi tiết phí lên CSDL thành công.');
                    sf.inused=1;
                }
                else
                    alert('Cập nhật chi tiết phí lên CSDL không thành công. Kiểm tra thông tin nhập.');
                //console.log(data);
            });
        }
    });
    myapp.controller('complexdetail',function($scope,dataService)
    {
         dataService.getactivepermission(1).then(function(data){
            $scope.mypermission=data.data;
        });
        $scope.formulavalid=function(index)
        {
            if(index>-1)
                return true;
            return false;
        }
        $scope.formulavalidclass=function(index)
        {
            if(index>-1)
                return "f-valid";
            return "f-invalid";
        }
        $scope.finindex=function(it,arr){
            var l=arr.length;
            for(i=0;i<l;i++)
            {
                if(it.id==arr[i].id)
                {
                    it=arr[i];
                    return i;
                }
            }
            return 0;
        }

        $scope.getdetail=function(id){
            $scope.loading=true;
             dataService.getSampleFeeDetailComplex(id).then(function(data){
                $scope.detailcomplex=data.data;
                $scope.loading=false;
            });
        }
        $scope.checkmoneybythis=function(val)
        {
            if (val === null|| val ==='undefined') {
                return true;
            }
            if( val!='')
            {
                //console.log(val);
                try{
                    if(parseInt(val)%1000==0)
                        return true;
                    else
                        return false;
                }
                catch(err)
                {
                   return false;
                }
            }
            return true;
        }
        $scope.postcomplex=function(sf){
            var obj=[];
            obj.data=JSON.stringify($scope.detailcomplex);
            console.log(obj);
            //dataService.postSample24h(obj);
            dataService.postcomplex(obj).then(function(data){
                if(data.data.result!="fail")
                {
                    alert('Đã cập nhật chi tiết phí lên CSDL thành công.');
                    sf.inused=1;
                }
                else
                    alert(data.data.data);
                //console.log(data);
            });
        }
    });
    myapp.controller('redemptiondetail',function($scope,dataService)
    {
         dataService.getactivepermission(1).then(function(data){
            $scope.mypermission=data.data;
        });
        $scope.formulavalid=function(index)
        {
            if(index>-1)
                return true;
            return false;
        }
        $scope.formulavalidclass=function(index)
        {
            if(index>-1)
                return "f-valid";
            return "f-invalid";
        }
        $scope.finindex=function(it,arr){
            var l=arr.length;
            for(i=0;i<l;i++)
            {
                if(it.id==arr[i].id)
                {
                    it=arr[i];
                    return i;
                }
            }
            return 0;
        }

        $scope.getdetail=function(id){
            $scope.loading=true;
             dataService.getSampleFeeDetailRedemption(id).then(function(data){
                $scope.detailsredempt=data.data;
                $scope.loading=false;
            });
        }
        $scope.checkmoneybythis=function(val)
        {
            if (val === null|| val ==='undefined') {
                return true;
            }
            if( val!='')
            {
                //console.log(val);
                try{
                    if(parseInt(val)%1000==0)
                        return true;
                    else
                        return false;
                }
                catch(err)
                {
                   return false;
                }
            }
            return true;
        }
        $scope.post_redemption=function(sf){
            var obj=[];
            obj.data=JSON.stringify($scope.detailsredempt);
            //console.log(obj);
            //dataService.postSample24h(obj);
            dataService.postredemption(obj).then(function(data){
                if(data.data!="fail")
                {
                    alert('Đã cập nhật chi tiết phí lên CSDL thành công.');
                     sf.inused=1;
                }
                else
                    alert('Cập nhật chi tiết phí lên CSDL không thành công. Kiểm tra thông tin nhập.');
                //console.log(data);
            });
        }
    });
    myapp.controller('myCtrl', function($scope,dataService) {
        dataService.getactivepermission(1).then(function(data){
            $scope.mypermission=data.data;
        });
        $scope.CurrentDate=function(){return new Date();}
        //this.changestate
        $scope.lockitem=function(sf)
        {
            $scope.loading=true;
            obj=[];
            obj.id=sf.id;
            obj.type="lock";
            obj.fname=sf.feename;
            obj.feetype=sf.feetype;
            dataService.changestate(obj).then(function(data)
                {
                    if(data.data.result!="fail")
                    {
                        alert("Đã khóa thành công.");
                        sf.inused=3;
                        sf.canlock=false;
                        sf.canunlock=true;
                    }
                    else
                        alert(data.data.data);
                     $scope.loading=false;
                }
            );
        }
        $scope.unlockitem=function(sf)
        {
            $scope.loading=true;
            obj=[];
            obj.id=sf.id;
            obj.type="unlock";
            obj.fname=sf.feename;
            obj.feetype=sf.feetype;
            dataService.changestate(obj).then(function(data)
                {
                    if(data.data.result!="fail")
                    {
                        alert("Đã mở khóa thành công.");
                        sf.inused=2;
                        sf.canunlock=false;
                        sf.canlock=true;
                    }
                    else
                        alert(data.data.data);
                     $scope.loading=false;
                }
            );
        }
        $scope.removeitem=function(sf)
        {
            $scope.loading=true;
            obj=[];
            obj.id=sf.id;
            obj.tablename="samplefee";
            dataService.removeitem(obj).then(function(data)
                {
                    if(data.data.result!="fail")
                    {
                        alert("Đã xóa thành công.");
                        sf.isvisible=false;
                        sf.showdetail=false;
                    }
                    else
                        alert(data.data.data);
                     $scope.loading=false;
                }
            );
        }
       $scope.datypeselected=[];
       $scope.datypeselectedr=[];
       $scope.complexdates=[];
       $scope.redempdates=[];
       $scope.weekdayresult=[];
       $scope.datetype=1;
       $scope.weekdaychoices=[
        {id:2,text:"Thứ 2",selected:false, isvisible:true},
        {id:3,text:"Thứ 3",selected:false, isvisible:true},
        {id:4,text:"Thứ 4",selected:false, isvisible:true},
        {id:5,text:"Thứ 5",selected:false, isvisible:true},
        {id:6,text:"Thứ 6",selected:false, isvisible:true},
        {id:7,text:"Thứ 7",selected:false, isvisible:true},
        {id:1,text:"Chủ nhật",selected:false, isvisible:true},
       ]
       $scope.weekdayresultr=[];
       $scope.datetyper=1;
       $scope.alldaysr=false;
       $scope.alldays=false;
       $scope.weekdaychoicesr=[
        {id:2,text:"Thứ 2",selected:false, isvisible:true},
        {id:3,text:"Thứ 3",selected:false, isvisible:true},
        {id:4,text:"Thứ 4",selected:false, isvisible:true},
        {id:5,text:"Thứ 5",selected:false, isvisible:true},
        {id:6,text:"Thứ 6",selected:false, isvisible:true},
        {id:7,text:"Thứ 7",selected:false, isvisible:true},
        {id:1,text:"Chủ nhật",selected:false, isvisible:true},
       ]
       $scope.weekdayschanger=function(){
            //alert("i'm change");
            var res=[];
            var l=$scope.weekdaychoicesr.length;
            //alert(l);
            for(i=0;i<l;i++)
            {
                if($scope.weekdaychoicesr[i].selected && $scope.weekdaychoicesr[i].isvisible)
                    res.push($scope.weekdaychoicesr[i].id);
            }
            $scope.weekdayresultr=res;

       }
       $scope.weekdayschange=function(){
            //alert("i'm change");
            var res=[];
            var l=$scope.weekdaychoices.length;
            //alert(l);
            for(i=0;i<l;i++)
            {
                if($scope.weekdaychoices[i].selected && $scope.weekdaychoices[i].isvisible)
                    res.push($scope.weekdaychoices[i].id);
            }
            $scope.weekdayresult=res;
            //alert(res);
       }
       $scope.weekdayschangeall=function(){
            //alert("i'm change");
            var res=[];
            var l=$scope.weekdaychoices.length;
            for(i=0;i<l;i++)
            {
                if( $scope.alldays)
                {
                    $scope.weekdaychoices[i].selected=true;
                }
                else
                {
                    $scope.weekdaychoices[i].selected=false;
                }

            }
            //alert(l);
            for(i=0;i<l;i++)
            {
                if($scope.weekdaychoices[i].selected && $scope.weekdaychoices[i].isvisible)
                    res.push($scope.weekdaychoices[i].id);
            }
            $scope.weekdayresult=res;
            //alert(res);
       }
        $scope.weekdayschangeallr=function(){
            //alert("i'm change");
            var res=[];
            var l=$scope.weekdaychoicesr.length;
            //alert(l);
            for(i=0;i<l;i++)
            {
                if( $scope.alldaysr)
                {
                    //alert(1);
                    $scope.weekdaychoicesr[i].selected=true;
                }
                else
                {
                    //alert(0);
                    $scope.weekdaychoicesr[i].selected=false;
                }

            }
            //alert(l);
            for(i=0;i<l;i++)
            {
                if($scope.weekdaychoicesr[i].selected && $scope.weekdaychoicesr[i].isvisible)
                    res.push($scope.weekdaychoicesr[i].id);
            }
            $scope.weekdayresultr=res;
            //alert(res);
       }
       function contains(a, obj) {
            for (var i = 0; i < a.length; i++) {
                if (a[i] === obj) {
                    return true;
                }
            }
            return false;
        }
       $scope.delcomplexdate=function(){
            var l=$scope.complexdates.length;

            if(l>0)
            {
                data=$scope.complexdates[l-1];
                $scope.complexdates.pop();
                dt=data.dayresult;

                if(dt==[0])
                {
                    $scope.datypeselected=$scope.datypeselected.filter(item=>item!=0);
                    //console.log($scope.datypeselecte);
                }
                else
                {
                    $scope.datypeselected=$scope.datypeselected.filter(item=>!dt.includes(item));
                    //console.log($scope.datypeselecte);
                    var l=$scope.weekdaychoices.length;
                    for(i=0;i<l;i++)
                    {
                         //console.log(i);
                        if(contains(dt,$scope.weekdaychoices[i].id))
                        {
                            $scope.weekdaychoices[i].isvisible=true;
                            $scope.weekdaychoices[i].selected=false;
                        }
                    }
                }
            }
       }
       $scope.addcomplexdate=function(){
            if($scope.datetype==0)
            {
                if(contains($scope.datypeselected,0))
                    return;
                $scope.complexdates.push({
                    name:"ngày lễ",
                    datedesc:"ngày lễ",
                    amounttime1:0,
                    amounttime2:0,
                    amounttime3:0,
                    dayresult:[0]
                });
                $scope.datypeselected.push(0);
            }
            else
            {
                var l=$scope.weekdayresult.length;
                if(l<1)
                    return;
                var des=[];

                var res=[];

                for(i=0;i<l;i++)
                {
                    var it=$scope.weekdayresult[i];
                    res.push(it);
                    $scope.datypeselected.push(it);
                    switch(it)
                    {
                        case 2:
                            des.push('T2');
                            break;
                        case 3:
                            des.push('T3');
                            break;
                        case 4:
                            des.push('T4');
                            break;
                        case 5:
                            des.push('T5');
                            break;
                        case 6:
                            des.push('T6');
                            break;
                        case 7:
                            des.push('T7');
                            break;
                        case 1:
                            des.push('CN');
                            break;
                    }
                }
                var l=$scope.weekdaychoices.length;
                for(i=0;i<l;i++)
                {
                    if(contains($scope.weekdayresult,$scope.weekdaychoices[i].id))
                        $scope.weekdaychoices[i].isvisible=false;
                }
                $scope.complexdates.push({
                    name:"ngày thường",
                    datedesc:des,
                    amounttime1:0,
                    amounttime2:0,
                    amounttime3:0,
                    dayresult:res
                });
                $scope.weekdayresult=[];
            }
            //console.log(JSON.stringify($scope.complexdates));
       }
       $scope.delredempdate=function(){
            var l=$scope.redempdates.length;
            if(l>0)
            {
                data=$scope.redempdates[l-1];
                $scope.redempdates.pop();
                dt=data.dayresult;

                if(dt==[0])
                {
                    $scope.datypeselectedr=$scope.datypeselectedr.filter(item=>item!=0);
                    //console.log($scope.datypeselecte);
                }
                else
                {
                    $scope.datypeselectedr=$scope.datypeselectedr.filter(item=>!dt.includes(item));
                    //console.log($scope.datypeselecte);
                    var l=$scope.weekdaychoicesr.length;
                    for(i=0;i<l;i++)
                    {
                         //console.log(i);
                        if(contains(dt,$scope.weekdaychoicesr[i].id))
                        {
                            $scope.weekdaychoicesr[i].isvisible=true;
                            $scope.weekdaychoicesr[i].selected=false;
                        }
                    }
                }
            }
       }
       $scope.addredempdate=function(){
            if($scope.datetyper==0)
            {

                if(contains($scope.datypeselectedr,0))
                    return;
                $scope.redempdates.push({
                    name:"ngày lễ",
                    datedesc:"ngày lễ",
                    amounttime:0,
                    dayresult:0
                });
                $scope.datypeselectedr.push(0);
            }
            else
            {
                var l=$scope.weekdayresultr.length;
                if(l<1)
                    return;
                var des=[];

                var res=[];

                for(i=0;i<l;i++)
                {
                    var it=$scope.weekdayresultr[i];
                    res.push(it);
                    $scope.datypeselectedr.push(it);
                    switch(it)
                    {
                        case 2:
                            des.push('T2');
                            break;
                        case 3:
                            des.push('T3');
                            break;
                        case 4:
                            des.push('T4');
                            break;
                        case 5:
                            des.push('T5');
                            break;
                        case 6:
                            des.push('T6');
                            break;
                        case 7:
                            des.push('T7');
                            break;
                        case 1:
                            des.push('CN');
                            break;
                    }
                }
                var l=$scope.weekdaychoicesr.length;
                for(i=0;i<l;i++)
                {
                    if(contains($scope.weekdayresultr,$scope.weekdaychoicesr[i].id))
                        $scope.weekdaychoicesr[i].isvisible=false;
                }
                $scope.redempdates.push({
                    name:"Ngày thường",
                    datedesc:des,
                    amounttime:0,
                    dayresult:res
                });
                $scope.weekdayresultr=[];
            }
            //console.log(JSON.stringify($scope.complexdates));
       }
       $scope.SampleFees=[];
       $scope.loading=true;
       dataService.getSampleFees(0,0).then(function(data){
                $scope.SampleFees=data.data;
                $scope.loading=false;
        });
        $scope.showtotalfee=function(item)
        {
            if(item==1)
                return true;
            return false;
        }
        $scope.addsample=[];
        $scope.addsample.timestartr="00:00:00";
        $scope.addsample.subtracttolerance=true;
        $scope.addsample.subtractfree=true;
        $scope.addsample.postsamplefees=function(){
            $scope.loading=true;
            var obj=[];
            var data=[];
            if($scope.addsample.feetypeselected.id==3)
            {
                obj.type=4;
                obj.callname=$scope.addsample.feename;
                obj.startdate=$scope.addsample.timestartr;
                obj.datelist=JSON.stringify($scope.redempdates);
            }
            else
            {
                if($scope.addsample.feetypeselected.id==2)
                {
                    obj.type=1;
                    data.callname=$scope.addsample.feename;
                    data.startdate=$scope.addsample.timestart;
                    data.freetime=$scope.addsample.freetime;
                    data.subtractfree=$scope.addsample.subtractfree;
                    data.subtracttolerance=$scope.addsample.subtracttolerance;
                    data.tolerancetime=$scope.addsample.tolerancetime;
                    data.datelist=JSON.stringify($scope.complexdates);
                    obj.data=data;
                }
                else
                {
                    if($scope.addsample.typemethodselected.id==2)
                    {
                        obj.type=2;
                        data.callname=$scope.addsample.feename;
                        data.freetime=$scope.addsample.freetime;
                        data.types24h=$scope.addsample.type24h;
                        data.after24hfee=$scope.addsample.aftertype24hfee;
                        data.after24htype=$scope.addsample.repeattype24h;
                        //alert(data.after24htype);
                        data.except=$scope.addsample.except;
                        obj.data=data;
                    }
                    else
                    {
                        obj.type=3;
                        data.callname=$scope.addsample.feename;
                        data.totalfee=$scope.addsample.feebytime;
                        data.feebyday=$scope.addsample.feebyday;
                        data.startdate=$scope.addsample.timestart;
                        data.startnight=$scope.addsample.timebeginnight;
                        data.freetime=$scope.addsample.freetime;
                        obj.data=data;
                    }
                }
            }
            dataService.postSampleFees(obj).then(function(data){
                if(data.data!="fail" && data.data!="failname")
                    $scope.SampleFees.splice( 0, 0, data.data );
                else if(data.data=="failname")
                {
                    alert("Tên này đã tồn tại. Nhập tên khác.");
                }
                else
                    alert("Không thể lưu kết quả. Vui lòng thử lại!");
                $scope.loading=false;
            });
        }
        $scope.copyitem=function(sf)
        {
            $scope.loading=true;
            obj=[];
            obj.id=sf.id;
            obj.copyname=sf.copyname;
            dataService.copysample(obj).then(function(data)
                {
                    if(data.data!="fail" && data.data!="failname")
                    {
                         $scope.SampleFees.splice( 0, 0, data.data )
                    }
                    else if(data.data=="failname")
                    {
                        alert("Tên này đã tồn tại. Nhập tên khác.");
                    }
                    else
                        alert("Không thể lưu kết quả. Vui lòng thử lại!");
                     $scope.loading=false;
                }
            );
        }
        $scope.addsample.feetypes = [
        { id: 1, name: 'Theo từng lượt', issimple:true },
        { id: 2, name: 'Dạng phức hợp',issimple:false },
        { id: 3, name: 'Dạng Redemption',issimple:false }
        ];
        $scope.addsample.feetypeselected = $scope.addsample.feetypes[0];
        $scope.addsample.typemethods = [
        { id: 1, name: 'Ngày - đêm', issettotal:false },
        { id: 2, name: 'Theo từng vòng 24h',issettotal:true }
        ];
        $scope.addsample.typemethodselected = $scope.addsample.typemethods[0];
        $scope.addsample.typemethodselectedchange=function(){
            if($scope.addsample.typemethodselected.id==2 && $scope.addsample.feetypeselected.id == 1)
            {
                $scope.addsample.type24h=[];
                $scope.addsample.type24h.push({id:1,hour:'',money:''});
            }
            else
            {
                $scope.addsample.type24h=[];
                $scope.addsample.type24h.push({id:1,hour:1,money:''});
            }
        }

        $scope.addsample.timestart="00:00:00";
        $scope.addsample.timebeginnight="00:00:00";
        $scope.addsample.freetime='';
        $scope.addsample.tolerancetime='';
        $scope.addsample.feebytime='';
        $scope.addsample.feebyday='';
        $scope.addsample.aftertype24hfee='';
        $scope.addsample.type24h=[];
        $scope.addsample.type24h.push({id:1,hour:1,money:''});
        $scope.addsample.addtype24=function(){
            var l=$scope.addsample.type24h.length;
            if(l<24)
                $scope.addsample.type24h.push({id:l+1,hour:'',money:''});
        }
        $scope.addsample.deltype24=function(){
            var l=$scope.addsample.type24h.length;
            if(l>1)
                $scope.addsample.type24h.pop();
        }
        $scope.addsample.Isdeltype24=function()
        {
             var l=$scope.addsample.type24h.length;
             if(l>1)
                return true;
             else
                return false;
        }
        $scope.addsample.Isaddtype24=function()
        {
             var l=$scope.addsample.type24h.length;
             if(l<24)
                return true;
             else
                return false;
        }

        $scope.addsample.checkmoney= function(type, index/*$event*/) {
            //var val = event.target.value;
            var val='';
            switch(type)
            {
                case 1:
                    val=$scope.addsample.feebytime;
                    break;
                case 2:
                    val=$scope.addsample.type24h[index-1].money;
                    break;
                case 3:
                    val=$scope.addsample.aftertype24hfee;
                    break;
            }
            //console.log(val);
            if (val === null|| val ==='undefined') {
                return true;
            }
            if( val!='')
            {
               // console.log(val);
                try{
                    if(parseInt(val)%1000==0)
                        return true;
                    else
                        return false;
                }
                catch(err)
                {
                   return false;
                }
            }
            return true;
        }
        $scope.addsample.expression=function()
        {
            if($scope.addsample.freetime!=''||$scope.addsample.tolerancetime!='')
            {
                try{
                    if(parseInt($scope.addsample.freetime)<parseInt($scope.addsample.tolerancetime))
                        return false;
                    else
                        return true;
                }
                catch(err)
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }
    });
    myapp.controller('FormFee',function($scope,dataService){
        dataService.getactivepermission(5).then(function(data){
            $scope.mypermission=data.data;
        });
        $scope.add=[];
        $scope.add.fomrulatype=[{"id":0,"name":"Một block theo giờ"},{"id":1,"name":"Một block trọn gói"},{"id":2,"name":"Nhiều block"}];
        $scope.add.fomrulatypeselected=$scope.add.fomrulatype[0];
        $scope.add.detail=[];
        var des=[{"id":0,"des":"trên giờ"},{"id":1,"des":"trên block"}];
        $scope.add.detail.push({"hours":0,'money':0,"des":des,"desselected":des[0]});
        $scope.add.adddetail=function()
        {
            var des=[{"id":0,"des":"trên giờ"},{"id":1,"des":"trên block"}];
            $scope.add.detail.push({"hours":0,'money':0,"des":des,"desselected":des[0]});
        }
        $scope.add.removedetail=function(){
            var l=$scope.add.detail.length;
            if(l>1)
            {
                $scope.add.detail.pop();
            }
        }
        $scope.checkmoney=function(val)
        {
            if (val === null|| val ==='undefined') {
                return true;
            }
            if( val!='')
            {
               // console.log(val);
                try{
                    if(parseInt(val)%1000==0)
                        return true;
                    else
                        return false;
                }
                catch(err)
                {
                   return false;
                }
            }
            return true;
        }
        $scope.formulas=[];
        dataService.getFormulaFees().then(function(data){
            $scope.formulas=data.data;
            //console.log(data.data);
        });
        $scope.add.postformulafee=function()
        {
            obj=[];
            obj.feetypes=$scope.add.fomrulatypeselected.id;
            obj.callname=$scope.add.callname;
            obj.detail=JSON.stringify($scope.add.detail);
            obj.fullfee=$scope.add.blockfee;
            //console.log(obj);
            dataService.postFormulaFee(obj).then(function(data)
                {
                    if(data.data!="fail")
                        $scope.formulas.splice( 0, 0, data.data );
                    //console.log(data);
                }
            );
        }
        $scope.removeitem=function(item)
        {
            obj=[];
            obj.id=item.id;
            obj.tablename="feeformula";
            dataService.removeitem(obj).then(function(data)
                {
                    if(data.data.result!="fail")
                    {
                        alert("Đã xóa thành công.");
                        item.isvisible=false;
                    }
                    else
                        alert(data.data.data);
                }
            );
        }
    });
    //calandreportfee
    myapp.controller('calandreportfee',function($scope,dataService){
        dataService.getactivepermission(4).then(function(data){
            $scope.mypermission=data.data;
        });
        $scope.samples=[];
        $scope.isdownload=false;
        $scope.samples=[]

        dataService.gelistSample().then(function(data){
            $scope.samples=data.data;
            $scope.samples.sampleselected=$scope.samples.samples[0];
        });
        $scope.posttocalandreport=function()
        {
            $scope.loading=true;
            obj=[];
            obj.file=document.getElementById('fileSelect').files[0],
            obj.sampleselected=JSON.stringify($scope.samples.sampleselected);
            console.log(obj);
            dataService.posttocalandreport(obj).then(function(data)
                {
                    $scope.loading=false;
                   if(data.data=="ok")
                   {
                        $scope.isdownload=true;
                         $scope.invaliddata=false;
                        alert('Import và tính kết quả thành công.')
                   }
                   else
                   {
                        $scope.isdownload=false;
                        if (data.data=="faildata")
                        {
                            $scope.invaliddata=true;
                            alert('File import có nội dung không phù hợp. Vui lòng chọn lại file import!')
                        }
                        else
                        {
                            $scope.invaliddata=false;
                            alert('Import không thành công. Vui lòng chọn lại file import!')
                        }
                   }
                   //console.log($scope.isdownload)
                }
            );
        }
    });
    //configredemption
    myapp.controller('configredemption',function($scope,dataService){
        dataService.getactivepermission(3).then(function(data){
            $scope.mypermission=data.data;
        });
        $scope.redemptregissimilar=[];
        $scope.loading=true;
        dataService.getredemptregitationsimilar().then(function(data){
                $scope.redemptregissimilar=data.data;
                $scope.loading=false;
        });
        $scope.removeitem=function(it,item, index)
        {
            obj=[];
            obj.id=it.id;

            try{
                obj.idupdate=  item.activesamples.redemption[parseInt(index)+1].id;
            }
             catch(err)
            {
               obj.idupdate=-1;
            }
            obj.tablename="redemptionactive";
            dataService.removeitem1(obj).then(function(data)
                {
                    if(data.data.result!="fail")
                    {
                        alert("Đã xóa thành công.");
                        $scope.getdetailconfig(item);
                    }
                    else
                        alert(data.data.data);
                }
            );
        }
        $scope.group=[];
        $scope.groups=[];
        $scope.checkmoney=function(val)
        {
            if (val === null|| val ==='undefined') {
                return true;
            }
            if( val!='')
            {
               // console.log(val);
                try{
                    if(parseInt(val)%1000==0)
                        return true;
                    else
                        return false;
                }
                catch(err)
                {
                   return false;
                }
            }
            return true;
        }

        dataService.gettenantgroup().then(function(data){
            $scope.group=data.data;
            var l = $scope.group.length;
            $scope.groups.push({"id":-1,"groupname":"-Chọn-"});
            for(i=0;i<l;i++)
            {
                $scope.groups.push({"id":data.data[i].id,"groupname":data.data[i].groupname});
            }
            $scope.groupselected=$scope.groups[0];

        });
        dataService.gettenantgroupactive().then(function(data){
            $scope.groupactive=data.data;
        });
        $scope.addgroups=[];
        $scope.adgrouptenant=function(){
            if($scope.groupselected.id!=-1)
            {
                $scope.addgroups.push({"group":$scope.groupselected,"billamount":0});
                var i = $scope.groups.indexOf($scope.groupselected);
                if(i != -1) {
                    $scope.groups.splice(i, 1);
                }
                $scope.groupselected=$scope.groups[0];
            }
        }
        $scope.delgrouptenant=function(){
            var l=$scope.addgroups.length;
            if(l>0)
            {
                $scope.groups.push($scope.addgroups[l-1].group);
                $scope.addgroups.pop();
            }
        }
        $scope.tenants=[]
        //getdetail
        $scope.getdetail=function(item)
        {
            //alert(item.id);
            //console.log(item);
            dataService.gettenantsbygroup(item.id).then(function(data)
                {
                    //console.log(data.data);
                    item.detail=data.data;
                    //return data.data;
                }
            );
            //settenants
        }
        //getactiveredemtion
        $scope.getdetailconfig=function(item)
        {
            //alert(item.id);
            //console.log(item);
            dataService.getactiveredemtion(item.id,item.vehicleid).then(function(data)
                {
                    //console.log(data.data);
                    item.activesamples=data.data;
                    item.activesamples.samples.sampleselected=item.activesamples.samples.samples[0];
                    item.activesamples.samples.sampleselected1=item.activesamples.samples.samples[0];
                    //return data.data;
                }
            );
            //settenants
        }
        //post_redemtionregis
        $scope.convertdate = function(date) {
            var d = new Date(date),
                month = '' + (d.getMonth() + 1),
                day = '' + d.getDate(),
                year = d.getFullYear();

            if (month.length < 2) month = '0' + month;
            if (day.length < 2) day = '0' + day;

            return [year, month, day].join('-');
        }
        $scope.convertdate1 = function(date) {
            var d = new Date(date),
                month = '' + (d.getMonth() + 1),
                day = '' + d.getDate(),
                year = d.getFullYear();

            if (month.length < 2) month = '0' + month;
            if (day.length < 2) day = '0' + day;

            return [year, month, day].join('');
        }
        $scope.converttime = function(time) {

            return time.split(':').join('');
        }
        $scope.postcallredemtion=function(){
            var obj=[];
            redemtiontime=$scope.convertdate1($scope.demo.redemptiondate)+$scope.converttime($scope.demo.redemptiontime);
            checkintime=$scope.convertdate1($scope.demo.checkindate)+$scope.converttime($scope.demo.checkintime);
            vehicleid=$scope.demo.vehicleselected.id;
            obj=JSON.stringify({"vehicleid":vehicleid,"checkintime":checkintime,"redemtiontime":redemtiontime,"groups":$scope.addgroups})
            dataService.postcallredemtion(obj).then(function(data){
                //console.log(data);
                if(data.data.result!="fail")
                    $scope.callresult=data.data.data;
                else
                    alert(data.data.data);
                //console.log(data);
            });
        }
        $scope.post_sampleregissimilar=function(item)
        {
            //console.log(item);
            //alert('begin now');
            var tmp=$scope.redemptregissimilar;
            tmp.activedate=$scope.convertdate(item.activedate)
            var obj=JSON.stringify(tmp);
            $scope.loading=true;
            //console.log(tmp);
            dataService.postRedemptRegissimilar(obj).then(function(data){
                if(data.data.result!="fail")
                    {
                        $scope.loading=true;
                        dataService.getredemptregitationsimilar().then(function(data){
                                $scope.sampleregissimilar=data.data;
                                $scope.loading=false;
                        });
                        alert('Lư kết quả thành công.');
                    }
                else
                    alert(data.data.data);
                $scope.loading=false;
            });
        }
        $scope.post_redemtionregis=function(item){
            //var obj=[];
            var id1=-1;
            //console.log(item.activesamples.samples);
            var tmp={"activedate":$scope.convertdate(item.activesamples.samples.activedate),
                "tenantgroupid":item.id,
                "sampleid":item.activesamples.samples.sampleselected.id,
                "sampleid1":item.activesamples.samples.sampleselected.id,
                //"sampleid1":item.activesamples.samples.sampleselected1.id,
                "vehicleid":item.vehicleid,
                }

            //console.log(tmp);
            var obj=JSON.stringify(tmp);
            //console.log(obj);
            dataService.postredemptionregis(obj).then(function(data){
                //console.log(data);
                if(data.data.result!="fail")
                    $scope.getdetailconfig(item);
                else
                    alert(data.data.data);
                //console.log(data);
            });
        }
        //getvehicletypes
        $scope.demo=[]
        dataService.getvehicletypes().then(function(data){
            $scope.demo.vehicles=data.data;
        });
    });
    //tenantgroup
    myapp.controller('tenantgroup',function($scope,dataService){
        dataService.getactivepermission(7).then(function(data){
            $scope.mypermission=data.data;
        });
        $scope.add=[];
        $scope.group=[];
        dataService.gettenantgroup().then(function(data){
            $scope.group=data.data;
        });
        $scope.add.posttenantgroup=function()
        {
            obj=[];
            obj.callname=$scope.add.groupname;
            dataService.posttenantgroup(obj).then(function(data)
                {
                    if(data.data!="fail")
                        $scope.group.splice( 0, 0, data.data );

                }
            );
            //settenants
        }

        $scope.tenants=[]
        $scope.settenants=function()
        {
            //alert('set tenants');
            dataService.settenants().then(function(data)
                {
                    $scope.tenants=data.data;
                    //console.log(data);
                }
            );

        }
        $scope.savechange=function()
        {
            obj=[];
            obj=JSON.stringify($scope.tenants);
           // alert('save change');
            //console.log(obj);
            dataService.changegrouptenant(obj).then(function(data)
                {
                    if(data.data.result!="fail")
                    {
                        alert("Lưu thay đổi thành công.")
                        $scope.settenants();
                    }
                    else
                    {
                        alert(data.data.result.data);
                    }
                }
            );
            //settenants
        }
        //getdetail
        $scope.getdetail=function(item)
        {
            //alert(item.id);
            //console.log(item);
            dataService.gettenantsbygroup(item.id).then(function(data)
                {
                    //console.log(data.data);
                    item.detail=data.data;
                    //return data.data;
                }
            );
            //settenants
        }
        $scope.removeitem=function(item)
        {
            obj=[];
            obj.id=item.id;
            obj.tablename="tenantgroup";
            dataService.removeitem(obj).then(function(data)
                {
                    if(data.data.result!="fail")
                    {
                        alert("Đã xóa thành công.");
                        item.isvisible=false;
                    }
                    else
                        alert(data.data.data);
                }
            );
        }
    });
    myapp.controller('permission',function($scope,dataService){
        dataService.getactivepermission(9).then(function(data){
            $scope.mypermission=data.data;
        });
        $scope.menus=[];
        dataService.gettoolfeemenu().then(function(data){
            $scope.menus=data.data;
        });
        //getdetail
        $scope.getdetail=function(item)
        {
            dataService.getgrouppermission(item.id).then(function(data)
                {
                    item.listpermission=data.data;
                }
            );
        }
        $scope.changepermission1=function(item)
        {

            item.isedit=item.iseditall;
        }
        $scope.changepermission11=function(item)
        {

            if(!item.isedit)
                item.iseditall=false ;
        }
        $scope.changepermission2=function(item)
        {

            item.isdel=item.isdelall;
        }
        $scope.changepermission22=function(item)
        {

            if(!item.isdel)
                item.isdelall=false ;
        }
        $scope.setactiveuser=function(it,itd)
        {
            if(itd.isactive)
            {
                itd.isadd=it.isadd;
                itd.isdel=it.isdel;
                itd.isdelall=it.isdelall;
                itd.isedit=it.isedit;
                itd.iseditall=it.iseditall;
            }
        }
        $scope.postchangepermission=function(item){
            obj=[];
            obj=JSON.stringify(item);
            dataService.postchangepermission(obj).then(function(data)
                {
                    if(data.data.result!="fail")
                    {
                        alert("Lưu thay đổi thành công.")
                    }
                    else
                    {
                        alert(data.data.data);
                    }
                }
            );
        }
    });
    myapp.controller('rootpermission',function($scope,dataService){
        $scope.groups=[];
        dataService.getrootpermission().then(function(data){
            $scope.groups=data.data;
        });
        $scope.postchangepermission=function(){
            obj=[];
            console.log($scope.groups);
            obj=JSON.stringify($scope.groups);
            dataService.postchangepermissionroot(obj).then(function(data)
                {
                    if(data.data.result!="fail")
                    {
                        alert("Lưu thay đổi thành công.")
                    }
                    else
                    {
                        alert(data.data.data);
                    }
                }
            );
        }
    });
    myapp.controller('FormBill',function($scope,dataService){
        dataService.getactivepermission(6).then(function(data){
            $scope.mypermission=data.data;
        });
        $scope.add=[];
        $scope.add.detail=[];
        $scope.add.detail.push({"billamount":0,'deductionamount':0});
        $scope.add.adddetail=function()
        {
            $scope.add.detail.push({"billamount":0,'deductionamount':0});
        }
        $scope.add.removedetail=function(){
            var l=$scope.add.detail.length;
            if(l>1)
            {
                $scope.add.detail.pop();
            }
        }
        $scope.checkmoney=function(val)
        {
            if (val === null|| val ==='undefined') {
                return true;
            }
            if( val!='')
            {
               // console.log(val);
                try{
                    if(parseInt(val)%1000==0)
                        return true;
                    else
                        return false;
                }
                catch(err)
                {
                   return false;
                }
            }
            return true;
        }
        $scope.formulas=[];
        dataService.getFormulaBill().then(function(data){
            $scope.formulas=data.data;
            //console.log(data.data);
        });
        $scope.add.postformulabill=function()
        {
            obj=[];
            obj.callname=$scope.add.callname;
            obj.detail=JSON.stringify($scope.add.detail);
            dataService.postFormulaBill(obj).then(function(data)
                {
                    if(data.data!="fail")
                        $scope.formulas.splice( 0, 0, data.data );
                }
            );
        }
        $scope.removeitem=function(item)
        {
            obj=[];
            obj.id=item.id;
            obj.tablename="billformula";
            dataService.removeitem(obj).then(function(data)
                {
                    if(data.data!="fail")
                    {
                        alert("Đã xóa thành công.");
                        item.isvisible=false;
                    }
                    else
                        alert(data.data.data);
                }
            );
        }
    });
    myapp.controller('SpecialDate',function($scope,dataService){
        dataService.getactivepermission(8).then(function(data){
            $scope.mypermission=data.data;
        });
        $scope.removeitem=function(sf)
        {
            $scope.loading=true;
            obj=[];
            obj.id=sf.id;
            obj.tablename="specialdate";
            dataService.removeitem(obj).then(function(data)
                {
                    if(data.data.result!="fail")
                    {
                        alert("Đã xóa thành công.");
                        sf.isvisible=false;
                        sf.showdetail=false;
                    }
                    else
                        alert(data.data.data);
                     $scope.loading=false;
                }
            );
        }
        $scope.add=[];
        $scope.convertdate = function(date) {
            var d = new Date(date),
                month = '' + (d.getMonth() + 1),
                day = '' + d.getDate(),
                year = d.getFullYear();

            if (month.length < 2) month = '0' + month;
            if (day.length < 2) day = '0' + day;

            return [year, month, day].join('-');
        }
        $scope.checkpercent=function(val)
        {
            if (val === null|| val ===undefined) {
                return true;
            }
            if( val!='')
            {
                //console.log(val);
                try{
                    v=parseInt(val);
                    if(v <=-100 || v >500)
                        return false;
                    else
                        return true;
                }
                catch(err)
                {
                   return false;
                }
            }
            return true;
        }
        $scope.specialdates=[];
        dataService.getSpecialDate().then(function(data){
            $scope.specialdates=data.data;
            //console.log(data.data);
        });
        $scope.add.postSpecialDate=function()
        {
            obj=[];
            obj.callname=$scope.add.callname;
            obj.dateactive=$scope.convertdate($scope.add.dateactive);
            obj.percentupordown=$scope.add.percentupordown;
            console.log(obj);
            dataService.postSpecialDate(obj).then(function(data)
                {
                    if(data.data!="fail")
                        $scope.specialdates.splice( 0, 0, data.data );
                }
            );
        }
    });
    myapp.directive('ensureExpression', ['$parse', function($parse) {
        return {
            restrict: 'A',
            require: 'ngModel',
            controller: function () { },
            scope: true,
            link: function (scope, element, attrs, ngModelCtrl) {
                scope.validate = function () {
                    var booleanResult = $parse(attrs.ensureExpression)(scope);
                    ngModelCtrl.$setValidity('expression', booleanResult);
                };
                scope.$watch(attrs.ngModel, function(value) {
                    scope.validate();
                });
            }
        };
    }]);
    myapp.directive('ensureWatch', ['$parse', function ($parse) {
        return {
            restrict: 'A',
            require: 'ensureExpression',
            link: function (scope, element, attrs, ctrl) {
                angular.forEach(attrs.ensureWatch.split(",").filter(function (n) { return !!n; }), function (n) {
                    scope.$watch(n, function () {
                        scope.validate();
                    });
                });
            }
        };
    }]);
    myapp.directive('samplecomplex', function() {
        var directive = {};
        directive.restrict = 'E';
        directive.templateUrl =static_url + 'Angular/TemplateHtml/SampleComplexDetail.html';
        return directive;
    });
    myapp.directive('sample24h', function() {
        //console.log(dataService);
        var directive = {};
        directive.restrict = 'E';
        directive.templateUrl =static_url + 'Angular/TemplateHtml/Sample24hDetail.html';
        return directive;
    });
    myapp.directive('sampleredemtion', function() {
        //console.log(dataService);
        var directive = {};
        directive.restrict = 'E';
        directive.templateUrl =static_url + 'Angular/TemplateHtml/redemptiondetail.html';
        return directive;
    });
    myapp.service('dataService', function($http) {
        //get/getrootpermission
        this.getrootpermission = function() {
            return $http({
                method: 'GET',
                url: url_root+'/get/getrootpermission/',
                dataType: 'json',
                contentType: "application/json"
             });
        };
        //get/getactivepermission/
        this.getactivepermission = function(menuid) {
            return $http({
                method: 'GET',
                url: url_root+'/get/getactivepermission/'+menuid,
                dataType: 'json',
                contentType: "application/json"
             });
        };
        //get/grouppermission/
        this.getgrouppermission = function(menuid) {
            return $http({
                method: 'GET',
                url: url_root+'/get/grouppermission/'+menuid,
                dataType: 'json',
                contentType: "application/json"
             });
        };
        //get/toolfeemenu/
        this.gettoolfeemenu = function() {
            return $http({
                method: 'GET',
                url: url_root+'/get/toolfeemenu/',
                dataType: 'json',
                contentType: "application/json"
             });
        };
        //get/vehicletypes/
        this.getvehicletypes = function() {
            return $http({
                method: 'GET',
                url: url_root+'/get/vehicletypes/',
                dataType: 'json',
                contentType: "application/json"
             });
        };
        //get/activeredemtion/
        this.getactiveredemtion = function(groupid,vehicletypeid) {
            return $http({
                method: 'GET',
                url: url_root+'/get/activeredemtion/'+groupid+'/'+vehicletypeid,
                dataType: 'json',
                contentType: "application/json"
             });
        };
        //Set tenants
        this.settenants = function() {
            return $http({
                method: 'GET',
                url: url_root+'/get/settenants/',
                dataType: 'json',
                contentType: "application/json"
             });
        };
        //Lấy tenant by group
        this.gettenantsbygroup = function(id) {
            return $http({
                method: 'GET',
                url: url_root+'/get/tenantsbygroup/'+id,
                dataType: 'json',
                contentType: "application/json"
             });
        };
        //Lấy group tannet
        this.gettenantgroup = function() {
            return $http({
                method: 'GET',
                url: url_root+'/get/gettenantgroup/',
                dataType: 'json',
                contentType: "application/json"
             });
        };
         //Lấy group tannet active
        this.gettenantgroupactive = function() {
            return $http({
                method: 'GET',
                url: url_root+'/get/gettenantgroupactive/',
                dataType: 'json',
                contentType: "application/json"
             });
        };
        //Lấy Ngạch hóa đơn;
        this.getFormulaBill = function() {
            return $http({
                method: 'GET',
                url: url_root+'/get/formularbill/',
                dataType: 'json',
                contentType: "application/json"
             });
        };
        //Lấy list sample active;
        this.gelistSample = function() {
            return $http({
                method: 'GET',
                url: url_root+'/get/samplelist/',
                dataType: 'json',
                contentType: "application/json"
             });
        };
        //get/regisfeesimilar
        //Lấy đăng ký similar
        this.getsampleregitationsimilar = function() {
            return $http({
                method: 'GET',
                url: url_root+'/get/regisfeesimilar/',
                dataType: 'json',
                contentType: "application/json"
             });
        };
        //Lấy đăng ký similar redemption
        this.getredemptregitationsimilar = function() {
            return $http({
                method: 'GET',
                url: url_root+'/get/regisredemptionsimilar/',
                dataType: 'json',
                contentType: "application/json"
             });
        };
        //Lấy phương tiện và thẻ đăng ký;
        this.getsampleregitation = function() {
            return $http({
                method: 'GET',
                url: url_root+'/get/samplefeeregistation/',
                dataType: 'json',
                contentType: "application/json"
             });
        };
        //Lấy chi tiết mẫu đăng ký;
        this.regisfeedetail = function(cardtype,vehicletype) {
            return $http({
                method: 'GET',
                url: url_root+'/get/regisfeedetail/'+cardtype+'/'+vehicletype,
                dataType: 'json',
                contentType: "application/json"
             });
        };
         //Lấy kết quả tính phí;
        this.getcallfee = function(id,from,to,expired) {
            return $http({
                method: 'GET',
                url: url_root+'/get/callfeeactive/'+id+'/'+from+'/'+to+'/'+expired,
                dataType: 'json',
                contentType: "application/json"
             });
        };
        //Lấy ngày đặc biệt;
        this.getSpecialDate = function() {
            return $http({
                method: 'GET',
                url: url_root+'/get/specialdate/',
                dataType: 'json',
                contentType: "application/json"
             });
        };
        //Lấy formula fee;
        this.getFormulaFees = function() {
            return $http({
                method: 'GET',
                url: url_root+'/get/formularfee/',
                dataType: 'json',
                contentType: "application/json"
             });
        };
        //Lấy sample fee;
        this.getSampleFees = function(id,typeid) {
            return $http({
                method: 'GET',
                url: url_root+'/get/samplefees/'+id+'/'+typeid,
                dataType: 'json',
                contentType: "application/json"
             });
        };
        //Lấy chi tiết sample fee loại 24h detail;
        this.getSampleFeeDetail24 = function(feeid) {
            return $http({
                method: 'GET',
                url: url_root+'/get/detailfee24h/'+feeid,
                dataType: 'json',
                contentType: "application/json"
             });
        };
        //Lấy chi tiết sample fee loại complex detail;
        this.getSampleFeeDetailComplex = function(feeid) {
            return $http({
                method: 'GET',
                url: url_root+'/get/detailfeecomplex/'+feeid,
                dataType: 'json',
                contentType: "application/json"
             });
        };
        //Lấy chi tiết sample fee loại redemption;
        this.getSampleFeeDetailRedemption = function(feeid) {
            return $http({
                method: 'GET',
                url: url_root+'/get/detailredemption/'+feeid,
                dataType: 'json',
                contentType: "application/json"
             });
        };
        this.postchangepermission=function(obj){
            var form=new FormData();
            form.append("datas",obj);
             return $http({
                method: 'POST',
                url: url_root+'/post/changepermission/',
                data:form,
                dataType: 'json',
                contentType: "application/json",
                headers: { 'Content-Type': undefined},
                transformRequest: angular.identity
             });
        };
        this.postchangepermissionroot=function(obj){
            var form=new FormData();
            form.append("datas",obj);
             return $http({
                method: 'POST',
                url: url_root+'/post/changepermissionroot/',
                data:form,
                dataType: 'json',
                contentType: "application/json",
                headers: { 'Content-Type': undefined},
                transformRequest: angular.identity
             });
        };
        //copy samples copysample
        this.copysample=function(obj)
        {
            var form=new FormData();
            form.append("id",obj.id);
            form.append("copyname",obj.copyname);
            return $http({
                method: 'POST',
                url: url_root+'/post/copysample/',
                data:form,
                dataType: 'json',
                contentType: "application/json",
                headers: { 'Content-Type': undefined},
                transformRequest: angular.identity
             });
        }
        //change lock or unlock item
         this.changestate = function(obj){
            //console.log(obj);
            var form=new FormData();
            form.append("id",obj.id);
            form.append("type",obj.type);
            form.append("fname",obj.fname);
            form.append("feetype",obj.feetype);
            return $http({
                method: 'POST',
                url: url_root+'/post/changestate/',
                data:form,
                dataType: 'json',
                contentType: "application/json",
                headers: { 'Content-Type': undefined},
                transformRequest: angular.identity
             });
        };
        //remove item removeitem
         this.removeitem = function(obj){
            //console.log(obj);
            var form=new FormData();
            form.append("id",obj.id);
            form.append("tablename",obj.tablename);
            return $http({
                method: 'POST',
                url: url_root+'/post/removeitem/',
                data:form,
                dataType: 'json',
                contentType: "application/json",
                headers: { 'Content-Type': undefined},
                transformRequest: angular.identity
             });
        };
        //remove item removeitem1
         this.removeitem1 = function(obj){
            //console.log(obj);
            var form=new FormData();
            form.append("id",obj.id);
            form.append("idupdate",obj.idupdate);
            form.append("tablename",obj.tablename);
            return $http({
                method: 'POST',
                url: url_root+'/post/removeitem1/',
                data:form,
                dataType: 'json',
                contentType: "application/json",
                headers: { 'Content-Type': undefined},
                transformRequest: angular.identity
             });
        };
        this.postSampleFees = function(obj){
            //console.log(obj);
            var form=new FormData();

            var type=obj.type;
            var data=obj.data;
            switch(type)
            {
                case 1://dang phuc hop

                    form.append("feetype","complex");
                    form.append("callname",data.callname);
                    form.append("startdate",data.startdate);
                    form.append("freetime",data.freetime);
                    form.append("subtractfree",data.subtractfree);
                    form.append("subtracttolerance",data.subtracttolerance);
                    form.append("tolerancetime",data.tolerancetime);
                    form.append("datelist",data.datelist);
                    break;
                case 2://dang don gian vong 24h
                    form.append("feetype","simple24")
                    form.append("callname",data.callname);
                    form.append("freetime",data.freetime);
                    form.append("blocks",JSON.stringify(data.types24h));
                    //form.append("hours",hours);
                    //form.append("moneys",moneys);
                    form.append("except",data.except);
                    form.append("after24hfee",data.after24hfee);

                    form.append("after24htype",data.after24htype);
                    // alert(data.after24htype);
                    break;
                case 3:// dang don gian
                    form.append("feetype","simple")
                    form.append("callname",data.callname);
                    form.append("totalfee",data.totalfee);
                    form.append("feebyday",data.feebyday);
                    form.append("startdate",data.startdate);
                    form.append("startnight",data.startnight);
                    form.append("freetime",data.freetime);
                    break;
                case 4:
                    form.append("feetype","redemption")
                    form.append("callname",obj.callname);
                    form.append("startdate",obj.startdate);
                    form.append("datelist",obj.datelist);
                    break;
            }
            return $http({
                method: 'POST',
                url: url_root+'/post/samplefees/',
                data:form,
                dataType: 'json',
                contentType: "application/json",
                headers: { 'Content-Type': undefined},
                transformRequest: angular.identity
             });
        };
        this.postSample24h = function(obj){
            //console.log(obj);
            var form=new FormData();
            var id=obj.id;
            //var sampleid=obj.sampleid;
            var hours=[]
            var moneys=[]
            data=obj.data;
            form.append("blocks",JSON.stringify(data.blocks));
            form.append("id",id);
            form.append("except",data.exceptfee);
            form.append("after24hfee",data.after24hfee);
            form.append("after24htype",data.after24htype);
            //alert(data.after24htype);

            //console.log(form);
            return $http({
                method: 'POST',
                url: url_root+'/post/fee24h/',
                data:form,
                dataType: 'json',
                contentType: "application/json",
                headers: { 'Content-Type': undefined},
                transformRequest: angular.identity
             });
        };
        //changegrouptenant
        this.changegrouptenant = function(obj){
            //console.log(obj);
            var form=new FormData();
            form.append("datas",obj);
            return $http({
                method: 'POST',
                url: url_root+'/post/changegrouptenant/',
                data:form,
                dataType: 'json',
                contentType: "application/json",
                headers: { 'Content-Type': undefined},
                transformRequest: angular.identity
             });
        };
        //postSampleRegisimilars
        this.postRedemptRegissimilar=function(obj){
            //console.log(obj);
            var form=new FormData();
            form.append("datas",obj);
            return $http({
                method: 'POST',
                url: url_root+'/post/redemptionregissimilar/',
                data:form,
                dataType: 'json',
                contentType: "application/json",
                headers: { 'Content-Type': undefined},
                transformRequest: angular.identity
             });
        };
        this.postSampleRegissimilar = function(obj){
            //console.log(obj);
            var form=new FormData();
            form.append("datas",obj);
            return $http({
                method: 'POST',
                url: url_root+'/post/sampleregissimilar/',
                data:form,
                dataType: 'json',
                contentType: "application/json",
                headers: { 'Content-Type': undefined},
                transformRequest: angular.identity
             });
        };
        //postSampleRegis
        this.postSampleRegis = function(obj){
            //console.log(obj);
            var form=new FormData();
            form.append("datas",obj);
            return $http({
                method: 'POST',
                url: url_root+'/post/sampleregis/',
                data:form,
                dataType: 'json',
                contentType: "application/json",
                headers: { 'Content-Type': undefined},
                transformRequest: angular.identity
             });
        };
        //post_redemtionregis
        this.postredemptionregis = function(obj){
            //console.log(obj);
            var form=new FormData();
            form.append("datas",obj);
            return $http({
                method: 'POST',
                url: url_root+'/post/redemptionregis/',
                data:form,
                dataType: 'json',
                contentType: "application/json",
                headers: { 'Content-Type': undefined},
                transformRequest: angular.identity
             });
        };
        //post_callredemtion
        this.postcallredemtion = function(obj){
            //console.log(obj);
            var form=new FormData();
            form.append("datas",obj);
            return $http({
                method: 'POST',
                url: url_root+'/post/callredemtion/',
                data:form,
                dataType: 'json',
                contentType: "application/json",
                headers: { 'Content-Type': undefined},
                transformRequest: angular.identity
             });
        };
        this.postcomplex = function(obj){
            //console.log(obj);
            var form=new FormData();
            form.append("datas",obj.data);
            return $http({
                method: 'POST',
                url: url_root+'/post/complex/',
                data:form,
                dataType: 'json',
                contentType: "application/json",
                headers: { 'Content-Type': undefined},
                transformRequest: angular.identity
             });
        };
        this.postredemption = function(obj){
            //console.log(obj);
            var form=new FormData();
            form.append("datas",obj.data);
            return $http({
                method: 'POST',
                url: url_root+'/post/redemption/',
                data:form,
                dataType: 'json',
                contentType: "application/json",
                headers: { 'Content-Type': undefined},
                transformRequest: angular.identity
             });
        };
        this.postFormulaFee = function(obj){
            //console.log(obj);
            var form=new FormData();
            form.append("feetypes",obj.feetypes);
            form.append("callname",obj.callname);
            form.append("detail ",obj.detail);
            form.append("fullfee ",obj.fullfee);
            return $http({
                method: 'POST',
                url: url_root+'/post/formulafee/',
                data:form,
                dataType: 'json',
                contentType: "application/json",
                headers: { 'Content-Type': undefined},
                transformRequest: angular.identity
             });
        };
        this.postFormulaBill = function(obj){
            var form=new FormData();
            form.append("callname",obj.callname);
            form.append("detail ",obj.detail);
            return $http({
                method: 'POST',
                url: url_root+'/post/formulabill/',
                data:form,
                dataType: 'json',
                contentType: "application/json",
                headers: { 'Content-Type': undefined},
                transformRequest: angular.identity
             });
        };
        this.posttenantgroup = function(obj){
            var form=new FormData();
            form.append("callname",obj.callname);
            return $http({
                method: 'POST',
                url: url_root+'/post/posttenantgroup/',
                data:form,
                dataType: 'json',
                contentType: "application/json",
                headers: { 'Content-Type': undefined},
                transformRequest: angular.identity
             });
        };
        this.postSpecialDate = function(obj){
            var form=new FormData();
            form.append("callname",obj.callname);
            form.append("dateactive",obj.dateactive);
            form.append("percentupordown",obj.percentupordown);
            //console.log(obj);
            return $http({
                method: 'POST',
                url: url_root+'/post/specialdate/',
                data:form,
                dataType: 'json',
                contentType: "application/json",
                headers: { 'Content-Type': undefined},
                transformRequest: angular.identity
             });
        };
        //posttocalandreport
        this.posttocalandreport = function(obj){
            var form=new FormData();
            form.append("file",obj.file);
            form.append("sample",obj.sampleselected);
            return $http({
                method: 'POST',
                url: url_root+'/post/posttocalandreport/',
                data:form,
                dataType: 'json',
                contentType: "application/json",
                headers: { 'Content-Type': undefined},
                transformRequest: angular.identity
             });
        };
    });

