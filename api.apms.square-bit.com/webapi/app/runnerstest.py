from django.test.runner import DiscoverRunner
from teamcity.unittestpy import TeamcityTestRunner

__author__ = 'ndhoang'


class MyTestRunner(DiscoverRunner):
    def run_suite(self, suite, **kwargs):
        return TeamcityTestRunner().run(suite)