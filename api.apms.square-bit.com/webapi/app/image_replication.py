from Queue import Queue
import json
from threading import Thread
import pika
from pika.exceptions import AMQPConnectionError
from parking.models import ImageReplicationSetting
import time
import logging

__author__ = 'ndhoang'


class ImageReplicationController:
    def __init__(self):
        self.logger = logging.getLogger('parking.logger')
        self.task_queue = Queue()
        self.port_client = '9191'
        self.connection = None
        self.channel = None
        self.broadcast_queue_names = None
        self.second_server_queue = 'SecondaryServer'
        # self.replication_settings = {
        #     '192.168.1.110': ['192.168.1.123', '192.168.1.140'],
        #     '192.168.1.123': ['192.168.1.110', '192.168.1.140'],
        #     '192.168.1.140': ['192.168.1.110', '192.168.1.123']
        # }
        self.replication_settings = dict()
        for record in ImageReplicationSetting.objects.all():
            sour_ip = record.sour_ip.strip()
            dest_ips = list()
            dest_ip_raw = record.dest_ip_list.split('|')
            for ip_raw in dest_ip_raw:
                dest_ips.append(ip_raw.strip())
            self.replication_settings[sour_ip] = dest_ips
        self.try_connect_server()
        t = Thread(target=self.run)
        t.daemon = True
        t.start()

    def run(self):
        while True:
            item = self.task_queue.get(block=True)
            while True:
                try:
                    self.channel.basic_publish(exchange='', routing_key=item['queue_name'], body=item['message'])
                    break
                except Exception, ex:
                    self.logger.error(str(ex.message))
                    while not self.try_connect_server():
                        print 'Fail to connect. Retry in 1 second.'
                        time.sleep(1)
            self.task_queue.task_done()

    def try_connect_server(self):
        try:
            self.connection = pika.BlockingConnection(pika.ConnectionParameters(host='localhost'))
            self.channel = self.connection.channel()
            self.broadcast_queue_names = set()
            self.channel.queue_declare(queue=self.second_server_queue)
            for key, list_repl_ips in self.replication_settings.iteritems():
                self.broadcast_queue_names.add(key)
                self.channel.queue_declare(queue='Imagerepl.' + key)
                for queue_name in list_repl_ips:
                    self.broadcast_queue_names.add(queue_name)
                    self.channel.queue_declare(queue='Imagerepl.' + queue_name)
            return True
        except AMQPConnectionError:
            return False

    def get_replicate_terminals(self, from_ip):
        if self.replication_settings and from_ip in self.replication_settings:
            return self.replication_settings[from_ip]
        else:
            return []

    def replicate_check_in(self, from_ip, front_image, back_image, card_id):
        message_obj = {
            'type': 'add',
            'host': from_ip + ':' + self.port_client,
            'front_image': front_image,
            'back_image': back_image,
            'card_id': card_id
        }
        message = json.dumps(message_obj)
        self.__publish_message(self.second_server_queue, message)
        self.__publish_message(from_ip, message)
        for queue_name in self.get_replicate_terminals(from_ip):
            self.__publish_message(queue_name, message)

    def replicate_check_out(self, front_image, back_image):
        message_obj = {
            'type': 'add',
            'front_image': front_image,
            'back_image': back_image,
        }
        message = json.dumps(message_obj)
        self.__publish_message(self.second_server_queue, message)

    def delete(self, front_image, back_image):
        message_obj = {
            'type': 'del',
            'front_image': front_image,
            'back_image': back_image
        }

        message = json.dumps(message_obj)

        if self.broadcast_queue_names:
            for queue_name in self.broadcast_queue_names:
                self.__publish_message(queue_name, message)

    def __publish_message(self, queue_name, message):
        if queue_name == self.second_server_queue:
            fix_queue_name = self.second_server_queue
        else:
            fix_queue_name = 'Imagerepl.' + queue_name
        self.task_queue.put({'queue_name': fix_queue_name, 'message': message})