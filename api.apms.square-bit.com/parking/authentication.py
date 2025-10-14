import ldap
from django.conf import settings
from django.contrib.auth import get_user_model
from django.contrib.auth.models import User


class ADAuthentication:
    """
    Authenticate against the settings ADMIN_LOGIN and ADMIN_PASSWORD.

    Use the login name and a hash of the password. For example:

    ADMIN_LOGIN = 'admin'
    ADMIN_PASSWORD = 'pbkdf2_sha256$30000$Vo0VlMnkR4Bk$qEvtdyZRWTcOsCnI/oQ7fVOu1XAURIZYoOZ3iq8Dr4M='
    """
    def check_ad_credentials(self, username, password):
        """Verifies credentials for username and password.
        Returns None on success or a string describing the error on failure
        # Adapt to your needs
        """
        # fully qualified AD user name
        LDAP_USERNAME = '%s' % username
        # your password
        LDAP_PASSWORD = password
        try:
            # build a client
            ldap_client = ldap.initialize(settings.LDAP_SERVER)
            # perform a synchronous bind
            ldap_client.set_option(ldap.OPT_REFERRALS, 0)
            ldap_client.simple_bind_s(LDAP_USERNAME, LDAP_PASSWORD)
        except ldap.INVALID_CREDENTIALS:
            ldap_client.unbind()
            return 'Wrong username ili password'
        except ldap.SERVER_DOWN:
            return 'AD server not awailable'
        ldap_client.unbind()
        return None

    def authenticate(self, username=None, password=None, **kwargs):
        UserModel = get_user_model()
        if username is None:
            username = kwargs.get(UserModel.USERNAME_FIELD)
        try:
            user = UserModel._default_manager.get_by_natural_key(username)

            # Check AD info
            message = self.check_ad_credentials(username, password)

            if message is None:
                return user
            else:
                user.is_active=0
                return user
        except UserModel.DoesNotExist:
            # Run the default password hasher once to reduce the timing
            # difference between an existing and a non-existing user (#20760).
            UserModel().set_password(password)

    def get_user(self, user_id):
        try:
            return User.objects.get(pk=user_id)
        except User.DoesNotExist:
            return None