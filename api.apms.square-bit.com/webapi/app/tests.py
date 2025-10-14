# coding=utf-8
import os
import datetime
from django.contrib.auth.models import User
from parking.models import UserCard, UserProfile
from rest_framework import status
from rest_framework.test import APITestCase
from webapi.settings import MEDIA_ROOT
from utils import *

__author__ = 'ndhoang'


### Util methods ###
def clearcache(client):
    client.get('/api/clearcache/')


def create_card(client, card_id, card_label, vehicle_type, card_status=1, card_type=1):
    return client.post('/api/cards/', data={'card_id': card_id, 'card_label': card_label, 'vehicle_type': vehicle_type, 'status': card_status, 'card_type': card_type})


def create_terminal(client, terminal_id, terminal_name, terminal_status=1):
    return client.post('/api/terminals/', data={'terminal_id': terminal_id, 'name': terminal_name, 'status': terminal_status})


def create_lane(client, terminal_id, lane_name, vehicle_type, direction, enabled=True):
    return client.post('/api/lanes/', data={'terminal_id': terminal_id, 'name': lane_name, 'vehicle_type': vehicle_type, 'direction': direction, 'enabled': enabled})


def create_camera(client, lane_id, camera_name, camera_ip, position, direction, serial_number):
    return client.post('/api/cameras/', data={'lane_id': lane_id, 'name': camera_name, 'ip': camera_ip, 'position': position, 'direction': direction, 'serial_number': serial_number})
### End Util methods ###


### Test classes ###
class TerminalAPITests(APITestCase):
    def test_create_get(self):
        item_id = 'terminal0'
        item_name = 'Terminal 0'
        # Create a item
        response_create = create_terminal(self.client, item_id, item_name)
        self.assertEqual(response_create.status_code, status.HTTP_201_CREATED)
        # Get created item and verify
        response_get = self.client.get('/api/terminals/%d/' % response_create.data['id'])
        self.assertEqual(response_get.status_code, status.HTTP_200_OK)
        self.assertEqual(response_get.data['terminal_id'], item_id)
        self.assertEqual(response_get.data['name'], item_name)
        self.assertEqual(response_get.data['status'], 1)
        self.assertDictEqual(response_create.data, response_get.data)

    def test_list(self):
        # Test empty item
        response = self.client.get('/api/terminals/')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertIs(type(response.data), list)
        self.assertEqual(len(response.data), 0)
        # Create a item
        item_id = 'terminal0'
        item_name = 'Terminal 0'
        create_terminal(self.client, item_id, item_name)
        # Test list with added item
        response = self.client.get('/api/terminals/')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertIs(type(response.data), list)
        self.assertEqual(len(response.data), 1)
        self.assertEqual(response.data[0]['terminal_id'], item_id)

    def test_health(self):
        item_id = 'terminal0'
        item_name = 'Terminal 0'
        # Create a item with timeout is False
        response = create_terminal(self.client, item_id, item_name)
        self.assertEqual(response.data['timeout'], False)
        got_id = response.data['id']
        # Make a item timeout and check timeout status should be True
        response = self.client.put('/api/terminals/%d/timeout/' % got_id)
        self.assertEqual(response.data['id'], got_id)
        self.assertEqual(response.data['timeout'], True)
        # Verify current timeout status with get api
        response = self.client.get('/api/terminals/%d/' % got_id)
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(response.data['timeout'], True)
        # Call health api and check timeout status should be False
        response = self.client.put('/api/terminals/%d/health/' % got_id)
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(response.data['id'], got_id)
        self.assertEqual(response.data['timeout'], False)
        # Verify current timeout status with get api
        response = self.client.get('/api/terminals/%d/' % got_id)
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(response.data['timeout'], False)

    def test_update(self):
        item_id = 'terminal0'
        item_name = 'Terminal 0'
        new_item_name = 'New Terminal 0'
        # Create a item
        response = create_terminal(self.client, item_id, item_name, 1)
        self.assertEqual(response.status_code, status.HTTP_201_CREATED)
        self.assertEqual(response.data['name'], item_name)
        self.assertEqual(response.data['status'], 1)
        # Update with new parameters
        response = self.client.put('/api/terminals/%d/' % response.data['id'], {'name': new_item_name, 'status': 0})
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        # Verify
        self.assertEqual(response.data['name'], new_item_name)
        self.assertEqual(response.data['status'], 0)
        # Verify again with get api
        response = self.client.get('/api/terminals/%d/' % response.data['id'])
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(response.data['name'], new_item_name)
        self.assertEqual(response.data['status'], 0)

    def test_create_with_exist_terminal_id_should_be_update(self):
        item_id = 'terminal0'
        item_name = 'Terminal 0'
        new_item_name = 'New Terminal 0'
        # Create a item
        response = create_terminal(self.client, item_id, item_name, 1)
        self.assertEqual(response.status_code, status.HTTP_201_CREATED)
        self.assertEqual(response.data['name'], item_name)
        self.assertEqual(response.data['status'], 1)
        old_id = response.data['id']
        response = create_terminal(self.client, item_id, new_item_name, 0)
        self.assertEqual(response.status_code, status.HTTP_201_CREATED)
        self.assertEqual(response.data['id'], old_id)
        self.assertEqual(response.data['terminal_id'], item_id)
        self.assertEqual(response.data['name'], new_item_name)
        self.assertEqual(response.data['status'], 0)


class LaneAPITests(APITestCase):
    def setUp(self):
        response = create_terminal(self.client, 'terminal0', 'Terminal 0')
        self.terminal_id = response.data['id']
        response = create_terminal(self.client, 'terminal1', 'Terminal 1')
        self.terminal_id_1 = response.data['id']

    def test_create_get(self):
        item_name = 'Lane 0'
        # Create a item
        response_create = create_lane(self.client, self.terminal_id, item_name, 1000001, 0)
        self.assertEqual(response_create.status_code, status.HTTP_201_CREATED)
        # Get created item and verify
        response_get = self.client.get('/api/lanes/%d/' % response_create.data['id'])
        self.assertEqual(response_get.status_code, status.HTTP_200_OK)
        self.assertEqual(response_get.data['name'], item_name)
        self.assertEqual(response_get.data['terminal_id'], self.terminal_id)
        self.assertEqual(response_get.data['vehicle_type'], 1000001)
        self.assertEqual(response_get.data['direction'], 0)
        self.assertEqual(response_get.data['enabled'], True)
        self.assertDictEqual(response_create.data, response_get.data)

    def test_list(self):
        # Test empty list
        response = self.client.get('/api/lanes/')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertIs(type(response.data), list)
        self.assertEqual(len(response.data), 0)
        # Create a item
        item_name = 'Lane 0'
        create_lane(self.client, self.terminal_id, item_name, 1000001, 0)
        # Test list with added item
        response = self.client.get('/api/lanes/')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertIs(type(response.data), list)
        self.assertEqual(len(response.data), 1)
        self.assertEqual(response.data[0]['name'], item_name)

    def test_update(self):
        item_name = 'Lane 0'
        new_item_name = 'New Lane 0'
        # Create a lane
        response = create_lane(self.client, self.terminal_id, item_name, 1000001, 0)
        self.assertEqual(response.status_code, status.HTTP_201_CREATED)
        self.assertEqual(response.data['name'], item_name)
        self.assertEqual(response.data['terminal_id'], self.terminal_id)
        self.assertEqual(response.data['vehicle_type'], 1000001)
        self.assertEqual(response.data['direction'], 0)
        self.assertEqual(response.data['enabled'], True)
        item_id = response.data['id']
        # Update a lane with invalid terminal id => Should get Http400
        response = self.client.put('/api/lanes/%d/' % item_id, {'name': new_item_name, 'vehicle_type': 2000101, 'direction': 1, 'enabled': False, 'terminal_id': 100})
        self.assertEqual(response.status_code, status.HTTP_400_BAD_REQUEST)
        # Update a lane with valid parameters
        response = self.client.put('/api/lanes/%d/' % item_id, {'name': new_item_name, 'vehicle_type': 2000101, 'direction': 1, 'enabled': False, 'terminal_id': self.terminal_id_1})
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        # Verify response data
        self.assertEqual(response.data['name'], new_item_name)
        self.assertEqual(response.data['vehicle_type'], 2000101)
        self.assertEqual(response.data['direction'], 1)
        self.assertEqual(response.data['enabled'], False)
        self.assertEqual(response.data['terminal_id'], self.terminal_id_1)
        # Verify again with get api
        response = self.client.get('/api/lanes/%d/' % item_id)
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(response.data['name'], new_item_name)
        self.assertEqual(response.data['vehicle_type'], 2000101)
        self.assertEqual(response.data['direction'], 1)
        self.assertEqual(response.data['enabled'], False)
        self.assertEqual(response.data['terminal_id'], self.terminal_id_1)

    def test_create_with_exist_terminal_id_and_name_should_be_update(self):
        item_name = 'Lane 0'
        # Create a lane
        response = create_lane(self.client, self.terminal_id, item_name, 1000001, 0)
        self.assertEqual(response.status_code, status.HTTP_201_CREATED)
        self.assertEqual(response.data['name'], item_name)
        self.assertEqual(response.data['terminal_id'], self.terminal_id)
        self.assertEqual(response.data['vehicle_type'], 1000001)
        self.assertEqual(response.data['direction'], 0)
        self.assertEqual(response.data['enabled'], True)
        item_id = response.data['id']
        response = create_lane(self.client, self.terminal_id, item_name, 2000101, 1, False)
        self.assertEqual(response.status_code, status.HTTP_201_CREATED)
        self.assertEqual(response.data['id'], item_id)
        self.assertEqual(response.data['name'], item_name)
        self.assertEqual(response.data['vehicle_type'], 2000101)
        self.assertEqual(response.data['direction'], 1)
        self.assertEqual(response.data['enabled'], False)
        self.assertEqual(response.data['terminal_id'], self.terminal_id)


class CameraAPITests(APITestCase):
    def setUp(self):
        response = create_terminal(self.client, 'terminal0', 'Terminal 0')
        terminal_id = response.data['id']
        response = create_lane(self.client, terminal_id, 'Lane 0', 1, 0)
        self.lane_id = response.data['id']
        response = create_lane(self.client, terminal_id, 'Lane 1', 1, 0)
        self.lane_id_1 = response.data['id']

    def test_create_get(self):
        item_name = 'Camera 0'
        item_ip = '192.168.1.129'
        # Create a item
        response_create = create_camera(self.client, self.lane_id, item_name, item_ip, 0, 0, 'abcd')
        self.assertEqual(response_create.status_code, status.HTTP_201_CREATED)
        # Get created item and verify
        response_get = self.client.get('/api/cameras/%d/' % response_create.data['id'])
        self.assertEqual(response_get.status_code, status.HTTP_200_OK)
        self.assertEqual(response_get.data['name'], item_name)
        self.assertEqual(response_get.data['ip'], item_ip)
        self.assertEqual(response_get.data['position'], 0)
        self.assertEqual(response_get.data['direction'], 0)
        self.assertEqual(response_get.data['serial_number'], 'abcd')
        self.assertEqual(response_get.data['lane_id'], self.lane_id)
        self.assertDictEqual(response_create.data, response_get.data)

    def test_list(self):
        # Test empty list
        response = self.client.get('/api/cameras/')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertIs(type(response.data), list)
        self.assertEqual(len(response.data), 0)
        # Create a item
        item_name = 'Camera 0'
        item_ip = '192.168.1.129'
        create_camera(self.client, self.lane_id, item_name, item_ip, 0, 0, 'abcd')
        # Test list with added item
        response = self.client.get('/api/cameras/')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertIs(type(response.data), list)
        self.assertEqual(len(response.data), 1)
        self.assertEqual(response.data[0]['name'], item_name)

    def test_update(self):
        item_name = 'Camera 0'
        item_ip = '192.168.1.129'
        new_item_name = 'New Lane 0'
        new_item_ip = '192.168.1.119'
        # Create a camera
        response = create_camera(self.client, self.lane_id, item_name, item_ip, 0, 0, 'abcd')
        self.assertEqual(response.status_code, status.HTTP_201_CREATED)
        self.assertEqual(response.data['name'], item_name)
        self.assertEqual(response.data['ip'], item_ip)
        self.assertEqual(response.data['lane_id'], self.lane_id)
        item_id = response.data['id']
        # Update a camera with invalid lane id => Should get Http400
        response = self.client.put('/api/cameras/%d/' % item_id, {'name': new_item_name, 'ip': new_item_ip, 'lane_id': 100, 'position': 1, 'direction': 1, 'serial_number': 'dcba'})
        self.assertEqual(response.status_code, status.HTTP_400_BAD_REQUEST)
        # Update a camera with valid parameters
        response = self.client.put('/api/cameras/%d/' % item_id, {'name': new_item_name, 'ip': new_item_ip, 'lane_id': self.lane_id_1, 'position': 1, 'direction': 1, 'serial_number': 'dcba'})
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        # Verify response data
        self.assertEqual(response.data['name'], new_item_name)
        self.assertEqual(response.data['ip'], new_item_ip)
        self.assertEqual(response.data['position'], 1)
        self.assertEqual(response.data['direction'], 1)
        self.assertEqual(response.data['serial_number'], 'dcba')
        self.assertEqual(response.data['lane_id'], self.lane_id_1)
        # Verify again with get api
        response = self.client.get('/api/cameras/%d/' % item_id)
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(response.data['name'], new_item_name)
        self.assertEqual(response.data['ip'], new_item_ip)
        self.assertEqual(response.data['position'], 1)
        self.assertEqual(response.data['direction'], 1)
        self.assertEqual(response.data['serial_number'], 'dcba')
        self.assertEqual(response.data['lane_id'], self.lane_id_1)


class CardAPITests(APITestCase):
    def test_create_get(self):
        item_id = 'card0'
        # Create a item
        response_create = create_card(self.client, item_id, '0', 1)
        self.assertEqual(response_create.status_code, status.HTTP_201_CREATED)
        # Get created item and verify
        response_get = self.client.get('/api/cards/%s/' % item_id)
        self.assertEqual(response_get.status_code, status.HTTP_200_OK)
        self.assertEqual(response_get.data['card_id'], item_id)
        self.assertEqual(response_get.data['card_label'], '0')
        self.assertEqual(response_get.data['vehicle_type'], 1)
        self.assertEqual(response_get.data['status'], 1)
        self.assertEqual(response_get.data['card_type'], 1)
        self.assertDictEqual(response_create.data, response_get.data)

    # def test_list(self):
    #     # Test empty item
    #     response = self.client.get('/api/cards/')
    #     self.assertEqual(response.status_code, status.HTTP_200_OK)
    #     self.assertIs(type(response.data), list)
    #     self.assertEqual(len(response.data), 0)
    #     # Create a item
    #     item_id = 'card0'
    #     create_card(self.client, item_id, '0', 1)
    #     # Test list with added item
    #     response = self.client.get('/api/cards/')
    #     self.assertEqual(response.status_code, status.HTTP_200_OK)
    #     self.assertIs(type(response.data), list)
    #     self.assertEqual(len(response.data), 1)
    #     self.assertEqual(response.data[0]['card_id'], item_id)

    def test_update(self):
        item_id = 'card0'
        # Create a item
        response = create_card(self.client, item_id, '0', 1)
        self.assertEqual(response.status_code, status.HTTP_201_CREATED)
        self.assertEqual(response.data['card_id'], item_id)
        self.assertEqual(response.data['card_label'], '0')
        self.assertEqual(response.data['vehicle_type'], 1)
        self.assertEqual(response.data['status'], 1)
        self.assertEqual(response.data['card_type'], 1)
        # Update with new parameters
        response = self.client.put('/api/cards/%s/' % item_id, {'status': 2, 'card_label': '1', 'vehicle_type': 2, 'card_type': 2})
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        # Verify
        self.assertEqual(response.data['card_id'], item_id)
        self.assertEqual(response.data['card_label'], '1')
        self.assertEqual(response.data['vehicle_type'], 2)
        self.assertEqual(response.data['status'], 2)
        self.assertEqual(response.data['card_type'], 2)
        # Verify again with get api
        response = self.client.get('/api/cards/%s/' % item_id)
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(response.data['card_id'], item_id)
        self.assertEqual(response.data['card_label'], '1')
        self.assertEqual(response.data['vehicle_type'], 2)
        self.assertEqual(response.data['status'], 2)
        self.assertEqual(response.data['card_type'], 2)


class LoginAPITest(APITestCase):
    def setUp(self):
        response = create_terminal(self.client, 'terminal0', 'Terminal 0')
        terminal_id = response.data['id']
        response = create_lane(self.client, terminal_id, 'Lane 0', 1, 0)
        self.lane_id = response.data['id']
        self.user = User.objects.create_user('testuser', email='testuser@test.com', password='testing')
        self.card = create_card(self.client, 'card0', '0', 1).data
        self.card1 = create_card(self.client, 'card1', '1', 1).data
        UserProfile.objects.create(user=self.user, card_id=1, fullname='test', birthday=datetime.datetime.now())
        self.user.save()

    def test_login_username_password_success(self):
        # Login success should return data with valid user id
        response = self.client.post('/api/users/login/', {'username': 'testuser', 'password': 'testing', 'lane_id': self.lane_id})
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(response.data['id'], self.user.id)

    def test_login_username_password_fail(self):
        # Invalid username
        response = self.client.post('/api/users/login/', {'username': 'invalid_username', 'password': 'testing', 'lane_id': self.lane_id})
        self.assertEqual(response.status_code, status.HTTP_400_BAD_REQUEST)
        # Invalid password
        response = self.client.post('/api/users/login/', {'username': 'testuser', 'password': 'invalid_password', 'lane_id': self.lane_id})
        self.assertEqual(response.status_code, status.HTTP_400_BAD_REQUEST)
        # Both
        response = self.client.post('/api/users/login/', {'username': 'invalid_username', 'password': 'invalid_password', 'lane_id': self.lane_id})
        self.assertEqual(response.status_code, status.HTTP_400_BAD_REQUEST)

    def test_login_by_card_success(self):
        # Login success should return data with valid user id
        response = self.client.post('/api/users/login-by-card/', {'card_id': 'card0', 'lane_id': self.lane_id})
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(response.data['id'], self.user.id)

    def test_login_by_card_fail(self):
        # Login success should return data with valid user id
        response = self.client.post('/api/users/login-by-card/', {'card_id': 'card1', 'lane_id': self.lane_id})
        self.assertEqual(response.status_code, status.HTTP_400_BAD_REQUEST)


class CheckInAPITest(APITestCase):
    def setUp(self):
        clearcache(self.client)
        self.card = create_card(self.client, 'card0', '0', 1).data
        self.card1 = create_card(self.client, 'card1', '1', 1).data
        self.locked_card = create_card(self.client, 'lockedcard', 'lock', 1, 2).data
        self.terminal = create_terminal(self.client, 'terminal0', 'Terminal 0').data
        self.terminal1 = create_terminal(self.client, 'terminal1', 'Terminal 1').data
        self.lane = create_lane(self.client, self.terminal['id'], 'Lane 0', 1, 0).data
        self.user = User.objects.create_user('testuser', email='testuser@test.com', password='testing')
        self.user.save()
        self.root_path = os.path.dirname(os.path.dirname(__file__))

    def test_create_get(self):
        vehicle_number = '1234'
        alpr_vehicle_number = 'raw-1234'
        vehicle_type = 1000001
        sour_front_path = self.root_path + '/front.jpg'
        sour_back_path = self.root_path + '/back.jpg'
        # Check in
        with open(sour_front_path, 'rb') as front_f, open(sour_back_path, 'rb') as back_f:
            response = self.client.post('/api/cards/card0/checkin/', {'terminal_id': self.terminal['id'], 'lane_id': self.lane['id'], 'operator_id': self.user.id, 'vehicle_type': vehicle_type, 'vehicle_number': vehicle_number, 'alpr_vehicle_number': alpr_vehicle_number, 'front_thumb': front_f, 'back_thumb': back_f})
        self.assertEqual(response.status_code, status.HTTP_201_CREATED)
        # Verify with get api info
        response = self.client.get('/api/cards/card0/checkin/')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(response.data['card_label'], self.card['card_label'])
        self.assertEqual(response.data['terminal_id'], self.terminal['id'])
        self.assertEqual(response.data['lane_id'], self.lane['id'])
        self.assertEqual(response.data['operator_id'], self.user.id)
        self.assertEqual(response.data['vehicle_type'], vehicle_type / 10000)
        self.assertEqual(response.data['vehicle_number'], vehicle_number)
        self.assertEqual(response.data['alpr_vehicle_number'], alpr_vehicle_number)
        self.assertEqual(len(response.data['image_hosts']), 1)
        self.assertEqual(response.data['image_hosts'][0]['id'], self.terminal['id'])
        # Verify: Thumbnails uploaded
        dest_front_path = MEDIA_ROOT + '/' + response.data['front_image_path']
        dest_back_path = MEDIA_ROOT + '/' + response.data['back_image_path']
        self.assertTrue(os.path.isfile(dest_front_path))
        self.assertTrue(os.path.isfile(dest_back_path))
        self.assertEqual(os.path.getsize(dest_front_path), os.path.getsize(sour_front_path))
        self.assertEqual(os.path.getsize(dest_back_path), os.path.getsize(sour_back_path))
        os.remove(dest_front_path)
        os.remove(dest_back_path)

    def test_checkin_inuse_card(self):
        vehicle_number = '1234'
        alpr_vehicle_number = 'raw-1234'
        vehicle_type = 1000001
        sour_front_path = self.root_path + '/front.jpg'
        sour_back_path = self.root_path + '/back.jpg'
        with open(sour_front_path, 'rb') as front_f, open(sour_back_path, 'rb') as back_f:
            response = self.client.post('/api/cards/card0/checkin/', {'terminal_id': self.terminal['id'], 'lane_id': self.lane['id'], 'operator_id': self.user.id, 'vehicle_type': vehicle_type, 'vehicle_number': vehicle_number, 'alpr_vehicle_number': alpr_vehicle_number, 'front_thumb': front_f, 'back_thumb': back_f})
            self.assertEqual(response.status_code, status.HTTP_201_CREATED)
            dest_front_path = MEDIA_ROOT + '/' + response.data['front_image_path']
            dest_back_path = MEDIA_ROOT + '/' + response.data['back_image_path']
            response = self.client.post('/api/cards/card0/checkin/', {'terminal_id': self.terminal['id'], 'lane_id': self.lane['id'], 'operator_id': self.user.id, 'vehicle_type': vehicle_type, 'vehicle_number': vehicle_number, 'alpr_vehicle_number': alpr_vehicle_number, 'front_thumb': front_f, 'back_thumb': back_f})
            self.assertEqual(response.status_code, status.HTTP_400_BAD_REQUEST)
            self.assertEqual(response.data['detail'], 'Card is in use')
            os.remove(dest_front_path)
            os.remove(dest_back_path)

    def test_checkin_locked_card(self):
        vehicle_number = '1234'
        alpr_vehicle_number = 'raw-1234'
        vehicle_type = 1000001
        sour_front_path = self.root_path + '/front.jpg'
        sour_back_path = self.root_path + '/back.jpg'
        with open(sour_front_path, 'rb') as front_f, open(sour_back_path, 'rb') as back_f:
            response = self.client.post('/api/cards/lockedcard/checkin/', {'terminal_id': self.terminal['id'], 'lane_id': self.lane['id'], 'operator_id': self.user.id, 'vehicle_type': vehicle_type, 'vehicle_number': vehicle_number, 'alpr_vehicle_number': alpr_vehicle_number, 'front_thumb': front_f, 'back_thumb': back_f})
        self.assertEqual(response.status_code, status.HTTP_400_BAD_REQUEST)
        self.assertEqual(response.data['detail'], 'Card is locked')

    def test_submit_image_host(self):
        vehicle_number = '1234'
        alpr_vehicle_number = 'raw-1234'
        vehicle_type = 1000001
        sour_front_path = self.root_path + '/front.jpg'
        sour_back_path = self.root_path + '/back.jpg'
        # Check in
        with open(sour_front_path, 'rb') as front_f, open(sour_back_path, 'rb') as back_f:
            response = self.client.post('/api/cards/card0/checkin/', {'terminal_id': self.terminal['id'], 'lane_id': self.lane['id'], 'operator_id': self.user.id, 'vehicle_type': vehicle_type, 'vehicle_number': vehicle_number, 'alpr_vehicle_number': alpr_vehicle_number, 'front_thumb': front_f, 'back_thumb': back_f})
            dest_front_path = MEDIA_ROOT + '/' + response.data['front_image_path']
            dest_back_path = MEDIA_ROOT + '/' + response.data['back_image_path']
            os.remove(dest_front_path)
            os.remove(dest_back_path)
        self.assertEqual(response.status_code, status.HTTP_201_CREATED)
        # Submit image host
        response = self.client.post('/api/cards/card0/imagehosts/', {'id': self.terminal1['id']})
        self.assertEqual(response.status_code, status.HTTP_201_CREATED)
        # Verify list image host from get info api
        response = self.client.get('/api/cards/card0/checkin/')
        self.assertEqual(len(response.data['image_hosts']), 2)
        self.assertEqual(response.data['image_hosts'][1]['id'], self.terminal1['id'])

    def test_check_in_increase_current_num_slots(self):
        vehicle_number = '1234'
        alpr_vehicle_number = 'raw-1234'
        vehicle_type = 1000001
        sour_front_path = self.root_path + '/front.jpg'
        sour_back_path = self.root_path + '/back.jpg'
        # Check in first time
        with open(sour_front_path, 'rb') as front_f, open(sour_back_path, 'rb') as back_f:
            response = self.client.post('/api/cards/card0/checkin/', {'terminal_id': self.terminal['id'], 'lane_id': self.lane['id'], 'operator_id': self.user.id, 'vehicle_type': vehicle_type, 'vehicle_number': vehicle_number, 'alpr_vehicle_number': alpr_vehicle_number, 'front_thumb': front_f, 'back_thumb': back_f})
            self.assertEqual(response.status_code, status.HTTP_201_CREATED)
            # Current number of slots should be 1
            self.assertEqual(response.data['current_num_slots'], 1)
            dest_front_path = MEDIA_ROOT + '/' + response.data['front_image_path']
            dest_back_path = MEDIA_ROOT + '/' + response.data['back_image_path']
            os.remove(dest_front_path)
            os.remove(dest_back_path)
        # Check in next card
        with open(sour_front_path, 'rb') as front_f, open(sour_back_path, 'rb') as back_f:
            response = self.client.post('/api/cards/card1/checkin/', {'terminal_id': self.terminal['id'], 'lane_id': self.lane['id'], 'operator_id': self.user.id, 'vehicle_type': vehicle_type, 'vehicle_number': vehicle_number, 'alpr_vehicle_number': alpr_vehicle_number, 'front_thumb': front_f, 'back_thumb': back_f})
            self.assertEqual(response.status_code, status.HTTP_201_CREATED)
            # Current number of slots should be increased to 2
            self.assertEqual(response.data['current_num_slots'], 2)
            dest_front_path = MEDIA_ROOT + '/' + response.data['front_image_path']
            dest_back_path = MEDIA_ROOT + '/' + response.data['back_image_path']
            os.remove(dest_front_path)
            os.remove(dest_back_path)

    def test_check_in_with_same_vehicle_number(self):
        vehicle_number = '1234'
        alpr_vehicle_number = 'raw-1234'
        vehicle_type = 1000001
        sour_front_path = self.root_path + '/front.jpg'
        sour_back_path = self.root_path + '/back.jpg'
        # Check in first time
        with open(sour_front_path, 'rb') as front_f, open(sour_back_path, 'rb') as back_f:
            response = self.client.post('/api/cards/card0/checkin/', {'terminal_id': self.terminal['id'], 'lane_id': self.lane['id'], 'operator_id': self.user.id, 'vehicle_type': vehicle_type, 'vehicle_number': vehicle_number, 'alpr_vehicle_number': alpr_vehicle_number, 'front_thumb': front_f, 'back_thumb': back_f})
            self.assertEqual(response.status_code, status.HTTP_201_CREATED)
            # Flag vehicle number exist should be False
            self.assertEqual(response.data['vehicle_number_exist'], False)
            dest_front_path = MEDIA_ROOT + '/' + response.data['front_image_path']
            dest_back_path = MEDIA_ROOT + '/' + response.data['back_image_path']
            os.remove(dest_front_path)
            os.remove(dest_back_path)
        # Check in next card with the same vehicle number
        with open(sour_front_path, 'rb') as front_f, open(sour_back_path, 'rb') as back_f:
            response = self.client.post('/api/cards/card1/checkin/', {'terminal_id': self.terminal['id'], 'lane_id': self.lane['id'], 'operator_id': self.user.id, 'vehicle_type': vehicle_type, 'vehicle_number': vehicle_number, 'alpr_vehicle_number': alpr_vehicle_number, 'front_thumb': front_f, 'back_thumb': back_f})
            self.assertEqual(response.status_code, status.HTTP_201_CREATED)
            # Flag vehicle number exist should be True
            self.assertEqual(response.data['vehicle_number_exist'], True)
            dest_front_path = MEDIA_ROOT + '/' + response.data['front_image_path']
            dest_back_path = MEDIA_ROOT + '/' + response.data['back_image_path']
            os.remove(dest_front_path)
            os.remove(dest_back_path)


class CheckOutAPITest(APITestCase):
    def setUp(self):
        clearcache(self.client)
        self.card = create_card(self.client, 'card0', '0', 1).data
        self.locked_card = create_card(self.client, 'lockedcard', 'lock', 1, 2).data
        self.terminal = create_terminal(self.client, 'terminal0', 'Terminal 0').data
        self.terminal1 = create_terminal(self.client, 'terminal1', 'Terminal 1').data
        self.lane = create_lane(self.client, self.terminal['id'], 'Lane 0', 1, 0).data
        self.checkout_lane = create_lane(self.client, self.terminal['id'], 'Lane 1', 1, 1).data
        self.user = User.objects.create_user('testuser', email='testuser@test.com', password='testing')
        self.user.save()
        self.root_path = os.path.dirname(os.path.dirname(__file__))
        # Check in
        vehicle_number = '1234'
        alpr_vehicle_number = 'raw-1234'
        vehicle_type = 1000001
        sour_front_path = self.root_path + '/front.jpg'
        sour_back_path = self.root_path + '/back.jpg'
        with open(sour_front_path, 'rb') as front_f, open(sour_back_path, 'rb') as back_f:
            response = self.client.post('/api/cards/card0/checkin/', {'terminal_id': self.terminal['id'], 'lane_id': self.lane['id'], 'operator_id': self.user.id, 'vehicle_type': vehicle_type, 'vehicle_number': vehicle_number, 'alpr_vehicle_number': alpr_vehicle_number, 'front_thumb': front_f, 'back_thumb': back_f})
            dest_front_path = MEDIA_ROOT + '/' + response.data['front_image_path']
            dest_back_path = MEDIA_ROOT + '/' + response.data['back_image_path']
            os.remove(dest_front_path)
            os.remove(dest_back_path)

    def test_create(self):
        # Check out
        alpr_vehicle_number = 'raw-1234'
        sour_front_path = self.root_path + '/front.jpg'
        sour_back_path = self.root_path + '/back.jpg'
        with open(sour_front_path, 'rb') as front_f, open(sour_back_path, 'rb') as back_f:
            response = self.client.post('/api/cards/card0/checkout/', {'terminal_id': self.terminal['id'], 'lane_id': self.checkout_lane['id'], 'operator_id': self.user.id, 'alpr_vehicle_number': alpr_vehicle_number, 'front_thumb': front_f, 'back_thumb': back_f})
            dest_front_path = MEDIA_ROOT + '/' + response.data['front_image_path']
            dest_back_path = MEDIA_ROOT + '/' + response.data['back_image_path']
        self.assertEqual(response.status_code, status.HTTP_201_CREATED)
        # Verify: Checkin info has been removed
        response = self.client.get('/api/cards/card0/checkin/')
        self.assertEqual(response.status_code, status.HTTP_400_BAD_REQUEST)
        self.assertEqual(response.data['detail'], 'Card is not in use')
        # Verify: Thumbnails uploaded
        self.assertTrue(os.path.isfile(dest_front_path))
        self.assertTrue(os.path.isfile(dest_back_path))
        self.assertEqual(os.path.getsize(dest_front_path), os.path.getsize(sour_front_path))
        self.assertEqual(os.path.getsize(dest_back_path), os.path.getsize(sour_back_path))
        os.remove(dest_front_path)
        os.remove(dest_back_path)

    def test_update_after_check_out(self):
        # Check out
        alpr_vehicle_number = 'raw-1234'
        sour_front_path = self.root_path + '/front.jpg'
        sour_back_path = self.root_path + '/back.jpg'
        with open(sour_front_path, 'rb') as front_f, open(sour_back_path, 'rb') as back_f:
            response = self.client.post('/api/cards/card0/checkout/', {'terminal_id': self.terminal['id'], 'lane_id': self.checkout_lane['id'], 'operator_id': self.user.id, 'alpr_vehicle_number': alpr_vehicle_number, 'front_thumb': front_f, 'back_thumb': back_f})
            dest_front_path = MEDIA_ROOT + '/' + response.data['front_image_path']
            dest_back_path = MEDIA_ROOT + '/' + response.data['back_image_path']
            os.remove(dest_front_path)
            os.remove(dest_back_path)
        self.assertEqual(response.status_code, status.HTTP_201_CREATED)
        # Check in again should be prevented
        vehicle_number = '1234'
        alpr_vehicle_number = 'raw-1234'
        vehicle_type = 1000001
        sour_front_path = self.root_path + '/front.jpg'
        sour_back_path = self.root_path + '/back.jpg'
        with open(sour_front_path, 'rb') as front_f, open(sour_back_path, 'rb') as back_f:
            response = self.client.put('/api/cards/card0/checkin/', {'terminal_id': self.terminal['id'], 'lane_id': self.lane['id'], 'operator_id': self.user.id, 'vehicle_type': vehicle_type, 'vehicle_number': vehicle_number, 'alpr_vehicle_number': alpr_vehicle_number, 'front_thumb': front_f, 'back_thumb': back_f})
        self.assertEqual(response.status_code, status.HTTP_400_BAD_REQUEST)


class ParkingSessionSearchAPITest(APITestCase):
    def setUp(self):
        clearcache(self.client)
        self.card = create_card(self.client, 'card0', '0', 1).data
        self.card1 = create_card(self.client, 'card1', '1', 1).data
        self.locked_card = create_card(self.client, 'lockedcard', 'lock', 1, 2).data
        self.terminal = create_terminal(self.client, 'terminal0', 'Terminal 0').data
        self.terminal1 = create_terminal(self.client, 'terminal1', 'Terminal 1').data
        self.lane = create_lane(self.client, self.terminal['id'], 'Lane 0', 1, 0).data
        self.checkout_lane = create_lane(self.client, self.terminal['id'], 'Lane 1', 1, 1).data
        self.user = User.objects.create_user('testuser', email='testuser@test.com', password='testing')
        self.user.save()
        self.root_path = os.path.dirname(os.path.dirname(__file__))
        # Check in
        sour_front_path = self.root_path + '/front.jpg'
        sour_back_path = self.root_path + '/back.jpg'

        self.create_time = int(datetime2timestamp(get_now_utc()))
        with open(sour_front_path, 'rb') as front_f, open(sour_back_path, 'rb') as back_f:
            response = self.client.post('/api/cards/card0/checkin/', {'terminal_id': self.terminal['id'], 'lane_id': self.lane['id'], 'operator_id': self.user.id, 'vehicle_type': 1000001, 'vehicle_number': '1234', 'alpr_vehicle_number': 'abcd-1234', 'front_thumb': front_f, 'back_thumb': back_f})
            dest_front_path = MEDIA_ROOT + '/' + response.data['front_image_path']
            dest_back_path = MEDIA_ROOT + '/' + response.data['back_image_path']
            os.remove(dest_front_path)
            os.remove(dest_back_path)
        with open(sour_front_path, 'rb') as front_f, open(sour_back_path, 'rb') as back_f:
            response = self.client.post('/api/cards/card1/checkin/', {'terminal_id': self.terminal['id'], 'lane_id': self.lane['id'], 'operator_id': self.user.id, 'vehicle_type': 2000101, 'vehicle_number': '4321', 'alpr_vehicle_number': 'abcd-4321', 'front_thumb': front_f, 'back_thumb': back_f})
            dest_front_path = MEDIA_ROOT + '/' + response.data['front_image_path']
            dest_back_path = MEDIA_ROOT + '/' + response.data['back_image_path']
            os.remove(dest_front_path)
            os.remove(dest_back_path)

    def test_search_by_card_id(self):
        response = self.client.get('/api/parking-sessions/?card_id=card0')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(len(response.data), 1)
        self.assertEqual(response.data[0]['card_label'], self.card['card_label'])

    def test_search_by_card_label(self):
        response = self.client.get('/api/parking-sessions/?card_label=0')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(len(response.data), 1)
        self.assertEqual(response.data[0]['card_label'], self.card['card_label'])

    def test_search_by_vehicle_number(self):
        response = self.client.get('/api/parking-sessions/?vehicle_number=4321')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(len(response.data), 1)
        self.assertEqual(response.data[0]['card_label'], self.card1['card_label'])

    def test_search_by_vehicle_type(self):
        response = self.client.get('/api/parking-sessions/?vehicle_type=1000001')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(len(response.data), 1)
        self.assertEqual(response.data[0]['card_label'], self.card['card_label'])
        response = self.client.get('/api/parking-sessions/?vehicle_type=2000101')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(len(response.data), 1)
        self.assertEqual(response.data[0]['card_label'], self.card1['card_label'])
        response = self.client.get('/api/parking-sessions/?vehicle_type=100000000')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(len(response.data), 2)

    def test_search_by_check_in_time(self):
        response = self.client.get('/api/parking-sessions/?from_time=%d' % self.create_time)
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(len(response.data), 2)
        response = self.client.get('/api/parking-sessions/?from_time=%d' % (self.create_time + 10))
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(len(response.data), 0)
        response = self.client.get('/api/parking-sessions/?to_time=%d' % self.create_time)
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(len(response.data), 2)
        response = self.client.get('/api/parking-sessions/?to_time=%d' % (self.create_time - 10))
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(len(response.data), 0)
        response = self.client.get('/api/parking-sessions/?from_time=%d&to_time=%d' % (self.create_time - 10, self.create_time + 10))
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(len(response.data), 2)