__author__ = 'ndhoang'


class GPMSDBRouter(object):
    def db_for_read(self, model, **hints):
        """
        Reads go to secondary.
        """
        return 'secondary'

    def db_for_write(self, model, **hints):
        """
        Writes always go to primary.
        """
        return 'default'

    def allow_relation(self, obj1, obj2, **hints):
        """
        Allow relations if a model in the auth app is involved.
        """
        return True