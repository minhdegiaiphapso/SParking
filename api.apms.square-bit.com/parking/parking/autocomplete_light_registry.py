# -*- coding: utf-8 -*-
import autocomplete_light
from models import *
__author__ = 'ndhoang'

# autocomplete_light.register(
#     Card,
#     search_fields=['card_label'],
#     attrs={
#         'data-autcomplete-minimum-characters': 1,
#         'placeholder': 'Card label ?',
#     },
#     widget_attrs={'data-widget-maximum-values': 4},
# )


class UserProfileAutocomplete(autocomplete_light.AutocompleteModelBase):
    model = UserProfile
    search_fields = ['staff_id', 'fullname']
    attrs = {
        'data-autocomplete-minimum-characters': 0,
        'placeholder': 'Name?',
    }
    widget_attrs = {'data-widget-maximum-values': 4}

    def choice_label(self, choice):
        return '%s | %s' % (choice.staff_id, choice.fullname)
    
autocomplete_light.register(UserProfile, UserProfileAutocomplete)


class CardAutocomplete(autocomplete_light.AutocompleteModelBase):
    model = Card
    search_fields = ['card_label']
    attrs = {
        'data-autocomplete-minimum-characters': 1,
        'placeholder': u'Mã thẻ?',
    }
    widget_attrs = {'data-widget-maximum-values': 10}

    def choices_for_request(self):
        # excluded_card_id = set()
        # for item in UserProfile.objects.values_list('card_id', flat=True):
        #     if item:
        #         excluded_card_id.add(item)
        # self.choices = self.choices.exclude(id__in=excluded_card_id)
        return super(CardAutocomplete, self).choices_for_request()

    def choice_label(self, choice):
        return choice.card_label


autocomplete_light.register(Card, CardAutocomplete)