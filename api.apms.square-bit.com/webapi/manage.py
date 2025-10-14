#!/usr/bin/env python
import os
import sys

from webapi.settings import PROJECT_ROOT

container_path = os.path.abspath(os.path.join(PROJECT_ROOT, '..'))
sys.path.append(os.path.join(container_path, 'parking'))

if __name__ == "__main__":
    os.environ.setdefault("DJANGO_SETTINGS_MODULE", "webapi.settings")
    os.environ.setdefault('HTTPS', "off")

    from django.core.management import execute_from_command_line
    execute_from_command_line(sys.argv)
