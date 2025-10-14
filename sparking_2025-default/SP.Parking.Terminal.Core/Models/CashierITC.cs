using Cirrious.MvvmCross.ViewModels;
using Green.Devices.Dal.CashierCounter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Models
{
    public class CashierITC : MvxNotifyPropertyChanged
    {
        public Action<int> EndTransaction;
        private DateTime from;
        private DateTime to;
        private int timeoutSeconds;
        private int amount;
        private int total;
        private int bill;
      
        public DateTime From
        {
            get { return from; }
            set
            {
                if (from == value)
                    return;
                from = value;
                RaisePropertyChanged(() => From);
            }
        }
        public DateTime To
        {
            get { return to; }
            set
            {
                if (to == value)
                    return;
                to = value;
                RaisePropertyChanged(() => To);
            }
        }
        public int TimeOutSeconds
        {
            get { return timeoutSeconds; }
            set
            {
                if (timeoutSeconds == value)
                    return;
                timeoutSeconds = value;
                RaisePropertyChanged(() => TimeOutSeconds);
            }
        }
        public int Amount
        {
            get { return amount; }
            set
            {
                if (amount == value)
                    return;
                amount = value;
                RaisePropertyChanged(() => Amount);
            }
        }
        public int Total
        {
            get { return total; }
            set
            {
                if (total == value)
                    return;
                total = value;
                RaisePropertyChanged(() => Total);
            }
        }
        public int Bill
        {
            get { return bill; }
            set
            {
                if (bill == value)
                    return;
                bill = value;
                RaisePropertyChanged(() => Bill);

            }
        }
      
        public ItcCashierInfo Info { get; private set; }
        
        public CashierITC(ItcCashierInfo info)
        {
            Info = info;
            Info.PropertyChanged = ProChanged;
            this.Bill = Info.Bill;
            this.Amount = Info.Amount;
            this.total = Info.Total;
            this.TimeOutSeconds = Info.TimeOutSeconds;
            this.From = Info.From;
            this.To = Info.To;
        }
        private void ProChanged(ItcCashierInfo.CashierInfoField arg1, object arg2)
        {
            switch(arg1)
            {
                case ItcCashierInfo.CashierInfoField.Bill:
                    this.Bill =  (int)(arg2);
                    break;
                case ItcCashierInfo.CashierInfoField.Total:
                    this.Total = (int)(arg2);
                    break;
                case ItcCashierInfo.CashierInfoField.Amount:
                    this.Amount = (int)(arg2);
                    break;
                case ItcCashierInfo.CashierInfoField.TimeOut:
                    this.TimeOutSeconds = (int)(arg2);
                    break;
                case ItcCashierInfo.CashierInfoField.Doing:
                    if(!(bool)(arg2) && EndTransaction!=null)
                    {
                        EndTransaction(Total);
                    }
                    break;
                
            }
        }
    }
}
