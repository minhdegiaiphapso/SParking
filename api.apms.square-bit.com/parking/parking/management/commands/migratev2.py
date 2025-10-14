# -*- coding: utf-8 -*-

from django.core.management.base import BaseCommand, CommandError
from parking.models import ClaimPromotion, ClaimPromotionBill, ClaimPromotionCoupon, ClaimPromotionV2, ClaimPromotionBillV2, ClaimPromotionCouponV2

class Command(BaseCommand):
    help = "Migrate ClaimPromotion tables"

    def handle(self, *args, **options):
        try:
            old_claim_promotions = ClaimPromotion.objects.all()
            # print "old_claim_promotions: ", old_claim_promotions

            for old_claim_promotion in old_claim_promotions:
                # Clone it into ClaimPromotionV2
                try:
                    new_claim_promotion = ClaimPromotionV2.objects.create(
                        old_id = old_claim_promotion.id,
                        parking_session = old_claim_promotion.parking_session,
                        user = old_claim_promotion.user,
                        amount_a = old_claim_promotion.amount_a,
                        amount_b = old_claim_promotion.amount_b,
                        amount_c = old_claim_promotion.amount_c,
                        amount_d = old_claim_promotion.amount_d,
                        amount_e = old_claim_promotion.amount_e,
                        client_time = old_claim_promotion.client_time,
                        server_time = old_claim_promotion.server_time,
                        used = old_claim_promotion.used,
                        notes = old_claim_promotion.notes
                    )

                    # print "ClaimPromotionV2 created, id", new_claim_promotion.id

                    # Clone its Bills into ClaimPromotionBillV2
                    old_claim_promotion_bills = ClaimPromotionBill.objects.filter(claim_promotion_id=old_claim_promotion.id)
                    for old_claim_promotion_bill in old_claim_promotion_bills:
                        # new_claim_promotion_bill = \
                        ClaimPromotionBillV2.objects.create(
                            claim_promotion=new_claim_promotion,
                            company_info=old_claim_promotion_bill.company_info,
                            date=old_claim_promotion_bill.date,
                            bill_number=old_claim_promotion_bill.bill_number,
                            bill_amount=old_claim_promotion_bill.bill_amount,
                            notes=old_claim_promotion_bill.notes,
                        )

                    # Clone its Coupons into ClaimPromotionCouponV2
                    old_claim_promotion_coupons = ClaimPromotionCoupon.objects.filter(
                        claim_promotion_id=old_claim_promotion.id)

                    for old_claim_promotion_coupon in old_claim_promotion_coupons:
                        # new_claim_promotion_coupon
                        ClaimPromotionCouponV2.objects.create(
                            claim_promotion=new_claim_promotion,
                            company_info=old_claim_promotion_coupon.company_info,
                            coupon_code=old_claim_promotion_coupon.coupon_code,
                            coupon_amount=old_claim_promotion_coupon.coupon_amount,
                            notes=old_claim_promotion_coupon.notes
                        )

                    # print "    Its bill & coupons migrated"

                except Exception as ex:
                    print "Migrate ClaimPromotion failed, id", old_claim_promotion.id, "error ", ex
                    continue


            self.stdout.write('Migrations done!')
        except Exception as e:
            print e
            raise CommandError('python manage.py migratev2 failed')


