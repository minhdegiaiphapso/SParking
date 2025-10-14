# -*- coding: utf-8 -*-

from django.core.management.base import BaseCommand, CommandError
from parking.models import init_app_config, update_or_create_superuser

class Command(BaseCommand):
    help = "Initialize GPMS Parking's app config"

    def handle(self, *args, **options):
        try:
            init_app_config()
            update_or_create_superuser()
            self.stdout.write('Successfully init app config')
        except Exception as e:
            print e
            raise CommandError('python manage.py bootstrap init app config failed')
