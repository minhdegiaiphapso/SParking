-- MySQL dump 10.13  Distrib 5.7.12, for Win64 (x86_64)
--
-- Host: 172.16.0.10    Database: apmsdb
-- ------------------------------------------------------
-- Server version	5.5.55-0ubuntu0.14.04.1

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `auth_group`
--

DROP TABLE IF EXISTS `auth_group`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `auth_group` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(80) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `name` (`name`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `auth_group_permissions`
--

DROP TABLE IF EXISTS `auth_group_permissions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `auth_group_permissions` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `group_id` int(11) NOT NULL,
  `permission_id` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `group_id` (`group_id`,`permission_id`),
  KEY `auth_group_permissions_5f412f9a` (`group_id`),
  KEY `auth_group_permissions_83d7f98b` (`permission_id`),
  CONSTRAINT `group_id_refs_id_f4b32aac` FOREIGN KEY (`group_id`) REFERENCES `auth_group` (`id`),
  CONSTRAINT `permission_id_refs_id_6ba0f519` FOREIGN KEY (`permission_id`) REFERENCES `auth_permission` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `auth_permission`
--

DROP TABLE IF EXISTS `auth_permission`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `auth_permission` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(50) NOT NULL,
  `content_type_id` int(11) NOT NULL,
  `codename` varchar(100) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `content_type_id` (`content_type_id`,`codename`),
  KEY `auth_permission_37ef4eb4` (`content_type_id`),
  CONSTRAINT `content_type_id_refs_id_d043b34a` FOREIGN KEY (`content_type_id`) REFERENCES `django_content_type` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `auth_user`
--

DROP TABLE IF EXISTS `auth_user`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `auth_user` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `password` varchar(128) NOT NULL,
  `last_login` datetime NOT NULL,
  `is_superuser` tinyint(1) NOT NULL,
  `username` varchar(30) NOT NULL,
  `first_name` varchar(30) NOT NULL,
  `last_name` varchar(30) NOT NULL,
  `email` varchar(75) NOT NULL,
  `is_staff` tinyint(1) NOT NULL,
  `is_active` tinyint(1) NOT NULL,
  `date_joined` datetime NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `username` (`username`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `auth_user_groups`
--

DROP TABLE IF EXISTS `auth_user_groups`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `auth_user_groups` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `user_id` int(11) NOT NULL,
  `group_id` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `user_id` (`user_id`,`group_id`),
  KEY `auth_user_groups_6340c63c` (`user_id`),
  KEY `auth_user_groups_5f412f9a` (`group_id`),
  CONSTRAINT `group_id_refs_id_274b862c` FOREIGN KEY (`group_id`) REFERENCES `auth_group` (`id`),
  CONSTRAINT `user_id_refs_id_40c41112` FOREIGN KEY (`user_id`) REFERENCES `auth_user` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `auth_user_user_permissions`
--

DROP TABLE IF EXISTS `auth_user_user_permissions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `auth_user_user_permissions` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `user_id` int(11) NOT NULL,
  `permission_id` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `user_id` (`user_id`,`permission_id`),
  KEY `auth_user_user_permissions_6340c63c` (`user_id`),
  KEY `auth_user_user_permissions_83d7f98b` (`permission_id`),
  CONSTRAINT `permission_id_refs_id_35d9ac25` FOREIGN KEY (`permission_id`) REFERENCES `auth_permission` (`id`),
  CONSTRAINT `user_id_refs_id_4dc23c39` FOREIGN KEY (`user_id`) REFERENCES `auth_user` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `blacklistplate`
--

DROP TABLE IF EXISTS `blacklistplate`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `blacklistplate` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `PlateNumber` varchar(45) NOT NULL,
  `PlateStatus` int(11) NOT NULL,
  `TerminalId` int(11) DEFAULT NULL,
  `CreatedBy` int(11) NOT NULL,
  `Description` varchar(45) DEFAULT NULL,
  `Active` bit(1) NOT NULL,
  `LaneId` int(11) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `django_admin_log`
--

DROP TABLE IF EXISTS `django_admin_log`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `django_admin_log` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `action_time` datetime NOT NULL,
  `user_id` int(11) NOT NULL,
  `content_type_id` int(11) DEFAULT NULL,
  `object_id` longtext,
  `object_repr` varchar(200) NOT NULL,
  `action_flag` smallint(5) unsigned NOT NULL,
  `change_message` longtext NOT NULL,
  PRIMARY KEY (`id`),
  KEY `django_admin_log_6340c63c` (`user_id`),
  KEY `django_admin_log_37ef4eb4` (`content_type_id`),
  CONSTRAINT `content_type_id_refs_id_93d2d1f8` FOREIGN KEY (`content_type_id`) REFERENCES `django_content_type` (`id`),
  CONSTRAINT `user_id_refs_id_c0d12874` FOREIGN KEY (`user_id`) REFERENCES `auth_user` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `django_content_type`
--

DROP TABLE IF EXISTS `django_content_type`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `django_content_type` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(100) NOT NULL,
  `app_label` varchar(100) NOT NULL,
  `model` varchar(100) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `app_label` (`app_label`,`model`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `django_session`
--

DROP TABLE IF EXISTS `django_session`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `django_session` (
  `session_key` varchar(40) NOT NULL,
  `session_data` longtext NOT NULL,
  `expire_date` datetime NOT NULL,
  PRIMARY KEY (`session_key`),
  KEY `django_session_b7b81f0c` (`expire_date`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `gatenotified`
--

DROP TABLE IF EXISTS `gatenotified`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `gatenotified` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `blackliststate` int(11) DEFAULT NULL,
  `gatereceive` varchar(256) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_blackliststate` (`blackliststate`),
  CONSTRAINT `gatenotified_ibfk_1` FOREIGN KEY (`blackliststate`) REFERENCES `parking_currentbalckliststate` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_apartment`
--

DROP TABLE IF EXISTS `parking_apartment`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_apartment` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `address` varchar(255) NOT NULL,
  `owner_name` varchar(255) DEFAULT NULL,
  `owner_phone` varchar(255) DEFAULT NULL,
  `owner_email` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_attendance`
--

DROP TABLE IF EXISTS `parking_attendance`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_attendance` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `user_id` int(11) NOT NULL,
  `time_in` datetime NOT NULL,
  `time_out` datetime DEFAULT NULL,
  `total_time_of_date` double DEFAULT NULL,
  `parking_session_id` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `parking_attendance_6340c63c` (`user_id`),
  KEY `parking_attendance_f7022ad3` (`parking_session_id`),
  CONSTRAINT `attendance_parking_session_id_refs_id` FOREIGN KEY (`parking_session_id`) REFERENCES `parking_parkingsession` (`id`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `user_id_refs_id_52928c06` FOREIGN KEY (`user_id`) REFERENCES `auth_user` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_blockfee`
--

DROP TABLE IF EXISTS `parking_blockfee`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_blockfee` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `parking_fee_id` int(11) NOT NULL,
  `first_block_duration` int(11) NOT NULL,
  `next_block_duration` int(11) NOT NULL,
  `first_block_fee` int(11) NOT NULL,
  `next_block_fee` int(11) NOT NULL,
  `max_block_duration` int(11) NOT NULL,
  `max_block_fee` int(11) NOT NULL,
  `in_day_block_fee` int(11) NOT NULL,
  `night_block_fee` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `parking_blockfee_8f589dc5` (`parking_fee_id`),
  CONSTRAINT `parking_fee_id_refs_id_d56899d7` FOREIGN KEY (`parking_fee_id`) REFERENCES `parking_parkingfee` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_building`
--

DROP TABLE IF EXISTS `parking_building`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_building` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) NOT NULL,
  `address` varchar(500) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_camera`
--

DROP TABLE IF EXISTS `parking_camera`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_camera` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(200) NOT NULL,
  `ip` varchar(50) NOT NULL,
  `position` int(11) NOT NULL,
  `direction` int(11) NOT NULL,
  `serial_number` varchar(200) NOT NULL,
  `lane_id` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `parking_camera_e2fc017a` (`lane_id`),
  CONSTRAINT `lane_id_refs_id_72638a8c` FOREIGN KEY (`lane_id`) REFERENCES `parking_lane` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_card`
--

DROP TABLE IF EXISTS `parking_card`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_card` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `card_id` varchar(128) NOT NULL,
  `card_label` varchar(128) NOT NULL,
  `status` int(11) NOT NULL,
  `vehicle_type` int(11) NOT NULL,
  `card_type` int(11) NOT NULL,
  `note` varchar(2000) DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `card_id` (`card_id`),
  UNIQUE KEY `card_label` (`card_label`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_cardauditlogentry`
--

DROP TABLE IF EXISTS `parking_cardauditlogentry`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_cardauditlogentry` (
  `id` int(11) NOT NULL,
  `card_id` varchar(128) NOT NULL,
  `card_label` varchar(128) NOT NULL,
  `status` int(11) NOT NULL,
  `vehicle_type` int(11) NOT NULL,
  `card_type` int(11) NOT NULL,
  `note` varchar(2000) DEFAULT NULL,
  `action_user_id` int(11) DEFAULT NULL,
  `action_id` int(11) NOT NULL AUTO_INCREMENT,
  `action_date` datetime NOT NULL,
  `action_type` varchar(1) NOT NULL,
  PRIMARY KEY (`action_id`),
  KEY `parking_cardauditlogentry_5a576368` (`id`),
  KEY `parking_cardauditlogentry_ca196c13` (`card_id`),
  KEY `parking_cardauditlogentry_61693150` (`card_label`),
  KEY `parking_cardauditlogentry_7537981b` (`action_user_id`),
  CONSTRAINT `action_user_id_refs_id_3a0b44b5` FOREIGN KEY (`action_user_id`) REFERENCES `auth_user` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_cardstatus`
--

DROP TABLE IF EXISTS `parking_cardstatus`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_cardstatus` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `card_id` varchar(128) NOT NULL,
  `parking_session_id` int(11) NOT NULL,
  `status` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `parking_cardstatus_c70f998a` (`card_id`),
  KEY `parking_cardstatus_f7022ad3` (`parking_session_id`),
  KEY `parking_cardstatus_48fb58bb` (`status`),
  CONSTRAINT `cardstatus_parking_session_id_refs_id` FOREIGN KEY (`parking_session_id`) REFERENCES `parking_parkingsession` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `card_id_refs_card_id_908e35fb` FOREIGN KEY (`card_id`) REFERENCES `parking_card` (`card_id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_cardtype`
--

DROP TABLE IF EXISTS `parking_cardtype`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_cardtype` (
  `id` int(11) NOT NULL,
  `name` varchar(100) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_checkinimage`
--

DROP TABLE IF EXISTS `parking_checkinimage`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_checkinimage` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `parking_session_id` int(11) NOT NULL,
  `terminal_id` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `parking_checkinimage_f7022ad3` (`parking_session_id`),
  KEY `parking_checkinimage_ba6d4d82` (`terminal_id`),
  CONSTRAINT `checkinimage_parking_session_id_refs_id` FOREIGN KEY (`parking_session_id`) REFERENCES `parking_parkingsession` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `terminal_id_refs_id_cd498fe3` FOREIGN KEY (`terminal_id`) REFERENCES `parking_terminal` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_checkoutexception`
--

DROP TABLE IF EXISTS `parking_checkoutexception`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_checkoutexception` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `parking_session_id` int(11) NOT NULL,
  `notes` varchar(2000) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `parking_checkoutexception_f7022ad3` (`parking_session_id`),
  CONSTRAINT `checkoutexception_parking_session_id_refs_id` FOREIGN KEY (`parking_session_id`) REFERENCES `parking_parkingsession` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_checkoutexceptioninfo`
--

DROP TABLE IF EXISTS `parking_checkoutexceptioninfo`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_checkoutexceptioninfo` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `notes` varchar(4000) NOT NULL,
  `parking_fee` int(11) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_claimpromotion`
--

DROP TABLE IF EXISTS `parking_claimpromotion`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_claimpromotion` (
  `id` varchar(100) NOT NULL,
  `user_id` int(11) NOT NULL,
  `parking_session_id` int(11) NOT NULL,
  `amount_a` int(11) NOT NULL,
  `amount_b` int(11) NOT NULL,
  `amount_c` int(11) NOT NULL,
  `amount_d` int(11) NOT NULL,
  `amount_e` int(11) NOT NULL,
  `client_time` date DEFAULT NULL,
  `server_time` datetime NOT NULL,
  `used` tinyint(1) NOT NULL,
  `notes` longtext,
  PRIMARY KEY (`id`),
  KEY `parking_claimpromotion_99bfca51` (`server_time`),
  KEY `parking_claimpromotion_6ab4cc27` (`used`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_claimpromotion_logerror`
--

DROP TABLE IF EXISTS `parking_claimpromotion_logerror`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_claimpromotion_logerror` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `session_id` int(11) DEFAULT NULL,
  `server_time` datetime DEFAULT NULL,
  `client_time` datetime DEFAULT NULL,
  `sendata` varchar(2000) DEFAULT NULL,
  `amount_a` int(11) DEFAULT NULL,
  `amount_b` int(11) DEFAULT NULL,
  `amount_c` int(11) DEFAULT NULL,
  `amount_d` int(11) DEFAULT NULL,
  `amount_e` int(11) DEFAULT NULL,
  `user_id` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_claimpromotionbill`
--

DROP TABLE IF EXISTS `parking_claimpromotionbill`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_claimpromotionbill` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `claim_promotion_id` varchar(100) DEFAULT NULL,
  `company_info` varchar(250) DEFAULT NULL,
  `bill_number` varchar(250) DEFAULT NULL,
  `bill_amount` int(11) NOT NULL,
  `notes` longtext,
  `date` datetime NOT NULL,
  PRIMARY KEY (`id`),
  KEY `parking_claimpromotionbill_560eb439` (`claim_promotion_id`),
  CONSTRAINT `claim_promotion_id_refs_id_b12d69fe` FOREIGN KEY (`claim_promotion_id`) REFERENCES `parking_claimpromotion` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_claimpromotionbillv2`
--

DROP TABLE IF EXISTS `parking_claimpromotionbillv2`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_claimpromotionbillv2` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `claim_promotion_id` int(11) DEFAULT NULL,
  `company_info` varchar(512) DEFAULT NULL,
  `date` datetime DEFAULT NULL,
  `bill_number` varchar(250) DEFAULT NULL,
  `bill_amount` int(11) NOT NULL,
  `notes` longtext,
  PRIMARY KEY (`id`),
  KEY `parking_claimpromotionbillv2_560eb439` (`claim_promotion_id`),
  CONSTRAINT `claim_promotion_id_refs_id_14735cdc` FOREIGN KEY (`claim_promotion_id`) REFERENCES `parking_claimpromotionv2` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_claimpromotioncoupon`
--

DROP TABLE IF EXISTS `parking_claimpromotioncoupon`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_claimpromotioncoupon` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `claim_promotion_id` varchar(100) DEFAULT NULL,
  `company_info` varchar(250) DEFAULT NULL,
  `coupon_code` varchar(250) DEFAULT NULL,
  `coupon_amount` int(11) NOT NULL,
  `notes` longtext,
  PRIMARY KEY (`id`),
  KEY `parking_claimpromotioncoupon_560eb439` (`claim_promotion_id`),
  CONSTRAINT `claim_promotion_id_refs_id_910bcf46` FOREIGN KEY (`claim_promotion_id`) REFERENCES `parking_claimpromotion` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_claimpromotioncouponv2`
--

DROP TABLE IF EXISTS `parking_claimpromotioncouponv2`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_claimpromotioncouponv2` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `claim_promotion_id` int(11) DEFAULT NULL,
  `company_info` varchar(512) DEFAULT NULL,
  `coupon_code` varchar(250) DEFAULT NULL,
  `coupon_amount` int(11) NOT NULL,
  `notes` longtext,
  PRIMARY KEY (`id`),
  KEY `parking_claimpromotioncouponv2_560eb439` (`claim_promotion_id`),
  CONSTRAINT `claim_promotion_id_refs_id_e8fb3705` FOREIGN KEY (`claim_promotion_id`) REFERENCES `parking_claimpromotionv2` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_claimpromotiongrouptenant`
--

DROP TABLE IF EXISTS `parking_claimpromotiongrouptenant`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_claimpromotiongrouptenant` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `groupname` varchar(512) DEFAULT NULL,
  `updated` datetime NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_claimpromotiontenant`
--

DROP TABLE IF EXISTS `parking_claimpromotiontenant`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_claimpromotiontenant` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(500) DEFAULT NULL,
  `short_name` varchar(250) NOT NULL,
  `updated` datetime NOT NULL,
  `group_tenant` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `short_name` (`short_name`),
  KEY `group_tenant` (`group_tenant`),
  CONSTRAINT `parking_claimpromotiontenant_ibfk_1` FOREIGN KEY (`group_tenant`) REFERENCES `parking_claimpromotiongrouptenant` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_claimpromotionv2`
--

DROP TABLE IF EXISTS `parking_claimpromotionv2`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_claimpromotionv2` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `old_id` varchar(250) DEFAULT NULL,
  `parking_session_id` int(11) DEFAULT NULL,
  `user_id` int(11) DEFAULT NULL,
  `amount_a` int(11) NOT NULL,
  `amount_b` int(11) NOT NULL,
  `amount_c` int(11) NOT NULL,
  `amount_d` int(11) NOT NULL,
  `amount_e` int(11) NOT NULL,
  `client_time` date DEFAULT NULL,
  `server_time` datetime NOT NULL,
  `used` tinyint(1) NOT NULL,
  `notes` longtext,
  PRIMARY KEY (`id`),
  UNIQUE KEY `old_id` (`old_id`),
  KEY `parking_claimpromotionv2_f7022ad3` (`parking_session_id`),
  KEY `parking_claimpromotionv2_6340c63c` (`user_id`),
  KEY `parking_claimpromotionv2_99bfca51` (`server_time`),
  KEY `parking_claimpromotionv2_6ab4cc27` (`used`),
  CONSTRAINT `parking_session_id_refs_id_186596da` FOREIGN KEY (`parking_session_id`) REFERENCES `parking_parkingsession` (`id`),
  CONSTRAINT `user_id_refs_id_46ae2072` FOREIGN KEY (`user_id`) REFERENCES `auth_user` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_claimpromotionvoucher`
--

DROP TABLE IF EXISTS `parking_claimpromotionvoucher`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_claimpromotionvoucher` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(250) NOT NULL,
  `short_value` varchar(250) NOT NULL,
  `value` int(11) NOT NULL,
  `updated` datetime NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `short_value` (`short_value`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_company`
--

DROP TABLE IF EXISTS `parking_company`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_company` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) NOT NULL,
  `address` varchar(500) DEFAULT NULL,
  `phone` varchar(255) DEFAULT NULL,
  `email` varchar(255) DEFAULT NULL,
  `representative_name` varchar(255) DEFAULT NULL,
  `representative_phone` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_currentbalckliststate`
--

DROP TABLE IF EXISTS `parking_currentbalckliststate`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_currentbalckliststate` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `blacklist` int(11) DEFAULT NULL,
  `gate` int(11) DEFAULT NULL,
  `user` int(11) DEFAULT NULL,
  `date` datetime DEFAULT NULL,
  `stateparking` int(11) DEFAULT NULL,
  `state` int(11) DEFAULT NULL,
  `notes` text,
  `parking_id` int(11) DEFAULT NULL,
  `image_path` varchar(128) DEFAULT NULL,
  `inactive` tinyint(4) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_blacklist` (`blacklist`),
  KEY `fk_gate` (`gate`),
  KEY `fk_user` (`user`),
  CONSTRAINT `parking_currentbalckliststate_ibfk_1` FOREIGN KEY (`blacklist`) REFERENCES `parking_vehiclebalcklist` (`id`),
  CONSTRAINT `parking_currentbalckliststate_ibfk_2` FOREIGN KEY (`gate`) REFERENCES `parking_terminal` (`id`),
  CONSTRAINT `parking_currentbalckliststate_ibfk_3` FOREIGN KEY (`user`) REFERENCES `auth_user` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_customer`
--

DROP TABLE IF EXISTS `parking_customer`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_customer` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `apartment_id` int(11) DEFAULT NULL,
  `building_id` int(11) DEFAULT NULL,
  `company_id` int(11) DEFAULT NULL,
  `customer_type_id` int(11) DEFAULT NULL,
  `customer_name` varchar(255) NOT NULL,
  `customer_id` varchar(255) NOT NULL,
  `customer_birthday` date DEFAULT NULL,
  `customer_avatar` varchar(100) NOT NULL,
  `customer_phone` varchar(255) NOT NULL,
  `customer_mobile` varchar(255) NOT NULL,
  `customer_email` varchar(255) NOT NULL,
  `order_register_name` varchar(100) NOT NULL,
  `order_register_address` varchar(200) NOT NULL,
  `order_tax_code` varchar(50) NOT NULL,
  `messaging_via_sms` tinyint(1) NOT NULL,
  `messaging_via_phone` tinyint(1) NOT NULL,
  `messaging_via_email` tinyint(1) NOT NULL,
  `messaging_via_apart_mail` tinyint(1) NOT NULL,
  `messaging_via_wiper_mail` tinyint(1) NOT NULL,
  `messaging_sms_phone` varchar(255) NOT NULL,
  `messaging_phone` varchar(255) NOT NULL,
  `messaging_email` varchar(255) NOT NULL,
  `messaging_address` varchar(255) NOT NULL,
  `staff_id` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `customer_id` (`customer_id`,`customer_name`),
  KEY `parking_customer_fd482c61` (`apartment_id`),
  KEY `parking_customer_f0aa7168` (`building_id`),
  KEY `parking_customer_0316dde1` (`company_id`),
  KEY `parking_customer_27f1b859` (`customer_type_id`),
  KEY `parking_customer_f0a7d083` (`staff_id`),
  CONSTRAINT `apartment_id_refs_id_3fc53cc8` FOREIGN KEY (`apartment_id`) REFERENCES `parking_apartment` (`id`),
  CONSTRAINT `building_id_refs_id_2144bedd` FOREIGN KEY (`building_id`) REFERENCES `parking_building` (`id`),
  CONSTRAINT `company_id_refs_id_032eb715` FOREIGN KEY (`company_id`) REFERENCES `parking_company` (`id`),
  CONSTRAINT `customer_type_id_refs_id_0a712d3a` FOREIGN KEY (`customer_type_id`) REFERENCES `parking_customertype` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_customerauditlogentry`
--

DROP TABLE IF EXISTS `parking_customerauditlogentry`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_customerauditlogentry` (
  `id` int(11) NOT NULL,
  `apartment_id` int(11) DEFAULT NULL,
  `building_id` int(11) DEFAULT NULL,
  `company_id` int(11) DEFAULT NULL,
  `customer_type_id` int(11) DEFAULT NULL,
  `customer_name` varchar(255) NOT NULL,
  `customer_id` varchar(255) NOT NULL,
  `customer_birthday` date DEFAULT NULL,
  `customer_avatar` varchar(100) NOT NULL,
  `customer_phone` varchar(255) NOT NULL,
  `customer_mobile` varchar(255) NOT NULL,
  `customer_email` varchar(255) NOT NULL,
  `order_register_name` varchar(100) NOT NULL,
  `order_register_address` varchar(200) NOT NULL,
  `order_tax_code` varchar(50) NOT NULL,
  `messaging_via_sms` tinyint(1) NOT NULL,
  `messaging_via_phone` tinyint(1) NOT NULL,
  `messaging_via_email` tinyint(1) NOT NULL,
  `messaging_via_apart_mail` tinyint(1) NOT NULL,
  `messaging_via_wiper_mail` tinyint(1) NOT NULL,
  `messaging_sms_phone` varchar(255) NOT NULL,
  `messaging_phone` varchar(255) NOT NULL,
  `messaging_email` varchar(255) NOT NULL,
  `messaging_address` varchar(255) NOT NULL,
  `staff_id` int(11) DEFAULT NULL,
  `action_user_id` int(11) DEFAULT NULL,
  `action_id` int(11) NOT NULL AUTO_INCREMENT,
  `action_date` datetime NOT NULL,
  `action_type` varchar(1) NOT NULL,
  PRIMARY KEY (`action_id`),
  KEY `parking_customerauditlogentry_5a576368` (`id`),
  KEY `parking_customerauditlogentry_fd482c61` (`apartment_id`),
  KEY `parking_customerauditlogentry_f0aa7168` (`building_id`),
  KEY `parking_customerauditlogentry_0316dde1` (`company_id`),
  KEY `parking_customerauditlogentry_27f1b859` (`customer_type_id`),
  KEY `parking_customerauditlogentry_f0a7d083` (`staff_id`),
  KEY `parking_customerauditlogentry_7537981b` (`action_user_id`),
  CONSTRAINT `action_user_id_refs_id_46a39779` FOREIGN KEY (`action_user_id`) REFERENCES `auth_user` (`id`),
  CONSTRAINT `apartment_id_refs_id_ae968bd6` FOREIGN KEY (`apartment_id`) REFERENCES `parking_apartment` (`id`),
  CONSTRAINT `building_id_refs_id_52da1633` FOREIGN KEY (`building_id`) REFERENCES `parking_building` (`id`),
  CONSTRAINT `company_id_refs_id_491bd70a` FOREIGN KEY (`company_id`) REFERENCES `parking_company` (`id`),
  CONSTRAINT `customer_type_id_refs_id_be7a61f1` FOREIGN KEY (`customer_type_id`) REFERENCES `parking_customertype` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_customertype`
--

DROP TABLE IF EXISTS `parking_customertype`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_customertype` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_depositactionfee`
--

DROP TABLE IF EXISTS `parking_depositactionfee`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_depositactionfee` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(100) NOT NULL,
  `customer_type_id` int(11) DEFAULT NULL,
  `vehicle_type_id` int(11) DEFAULT NULL,
  `fee` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `parking_depositactionfee_27f1b859` (`customer_type_id`),
  KEY `parking_depositactionfee_62b0e080` (`vehicle_type_id`),
  CONSTRAINT `customer_type_id_refs_id_e9eee219` FOREIGN KEY (`customer_type_id`) REFERENCES `parking_customertype` (`id`),
  CONSTRAINT `vehicle_type_id_refs_id_35717ce3` FOREIGN KEY (`vehicle_type_id`) REFERENCES `parking_vehicletype` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_depositpayment`
--

DROP TABLE IF EXISTS `parking_depositpayment`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_depositpayment` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `customer_id` int(11) DEFAULT NULL,
  `receipt_id` int(11) DEFAULT NULL,
  `receipt_number` int(11) DEFAULT NULL,
  `payment_date` datetime NOT NULL,
  `payment_fee` int(11) NOT NULL,
  `payment_method` varchar(20) NOT NULL,
  `notes` varchar(200) NOT NULL,
  `staff_id` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `parking_depositpayment_09847825` (`customer_id`),
  KEY `parking_depositpayment_f0a7d083` (`staff_id`),
  CONSTRAINT `customer_id_refs_id_62faf686` FOREIGN KEY (`customer_id`) REFERENCES `parking_customer` (`id`),
  CONSTRAINT `staff_id_refs_id_67ee49f9` FOREIGN KEY (`staff_id`) REFERENCES `parking_userprofile` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_depositpaymentdetail`
--

DROP TABLE IF EXISTS `parking_depositpaymentdetail`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_depositpaymentdetail` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `deposit_payment_id` int(11) NOT NULL,
  `vehicle_registration_id` int(11) DEFAULT NULL,
  `vehicle_number` varchar(255) NOT NULL,
  `deposit_action_fee_id` int(11) DEFAULT NULL,
  `deposit_payment_detail_fee` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `parking_depositpaymentdetail_e1f6a2ff` (`deposit_payment_id`),
  KEY `parking_depositpaymentdetail_0c7531ef` (`vehicle_registration_id`),
  KEY `parking_depositpaymentdetail_977af448` (`deposit_action_fee_id`),
  CONSTRAINT `deposit_action_fee_id_refs_id_9da3a48f` FOREIGN KEY (`deposit_action_fee_id`) REFERENCES `parking_depositactionfee` (`id`),
  CONSTRAINT `deposit_payment_id_refs_id_4588b294` FOREIGN KEY (`deposit_payment_id`) REFERENCES `parking_depositpayment` (`id`),
  CONSTRAINT `vehicle_registration_id_refs_id_b6fd3cde` FOREIGN KEY (`vehicle_registration_id`) REFERENCES `parking_vehicleregistration` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_duplicatecheckin`
--

DROP TABLE IF EXISTS `parking_duplicatecheckin`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_duplicatecheckin` (
  `parkingsession_duplicate_id` int(11) NOT NULL,
  `parkingsession_id` int(11) NOT NULL,
  PRIMARY KEY (`parkingsession_duplicate_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_feeadjustment`
--

DROP TABLE IF EXISTS `parking_feeadjustment`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_feeadjustment` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `vehicle_type` int(11) DEFAULT NULL,
  `time` datetime DEFAULT NULL,
  `fee` int(11) DEFAULT NULL,
  `remark` varchar(1000) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_forcedbarier`
--

DROP TABLE IF EXISTS `parking_forcedbarier`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_forcedbarier` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `user` varchar(50) DEFAULT NULL,
  `teminal` varchar(128) DEFAULT NULL,
  `lan` varchar(128) DEFAULT NULL,
  `timestampe` mediumtext,
  `frontpath` varchar(256) DEFAULT NULL,
  `backpath` varchar(256) DEFAULT NULL,
  `forcedtime` datetime DEFAULT NULL,
  `note` varchar(4000) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_imagereplicationsetting`
--

DROP TABLE IF EXISTS `parking_imagereplicationsetting`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_imagereplicationsetting` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `sour_ip` varchar(30) NOT NULL,
  `dest_ip_list` varchar(2000) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_lane`
--

DROP TABLE IF EXISTS `parking_lane`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_lane` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(200) NOT NULL,
  `direction` int(11) NOT NULL,
  `enabled` tinyint(1) NOT NULL,
  `vehicle_type` int(11) NOT NULL,
  `terminal_id` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `parking_lane_ba6d4d82` (`terminal_id`),
  CONSTRAINT `terminal_id_refs_id_dfd0120e` FOREIGN KEY (`terminal_id`) REFERENCES `parking_terminal` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_levelfee`
--

DROP TABLE IF EXISTS `parking_levelfee`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_levelfee` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(100) NOT NULL,
  `customer_type_id` int(11) DEFAULT NULL,
  `vehicle_type_id` int(11) DEFAULT NULL,
  `fee` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `parking_levelfee_27f1b859` (`customer_type_id`),
  KEY `parking_levelfee_62b0e080` (`vehicle_type_id`),
  CONSTRAINT `customer_type_id_refs_id_865cebbd` FOREIGN KEY (`customer_type_id`) REFERENCES `parking_customertype` (`id`),
  CONSTRAINT `vehicle_type_id_refs_id_c97d2d38` FOREIGN KEY (`vehicle_type_id`) REFERENCES `parking_vehicletype` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_parkingfee`
--

DROP TABLE IF EXISTS `parking_parkingfee`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_parkingfee` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `vehicle_type_id` int(11) NOT NULL,
  `calculation_method` varchar(10) NOT NULL,
  `min_calculation_time` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `vehicle_type_id` (`vehicle_type_id`),
  CONSTRAINT `vehicle_type_id_refs_id_8886f46e` FOREIGN KEY (`vehicle_type_id`) REFERENCES `parking_vehicletype` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_parkingfeesession`
--

DROP TABLE IF EXISTS `parking_parkingfeesession`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_parkingfeesession` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `parking_session_id` int(11) NOT NULL,
  `card_id` varchar(255) NOT NULL,
  `vehicle_number` varchar(255) NOT NULL,
  `parking_fee` int(11) NOT NULL,
  `parking_fee_detail` varchar(1000) NOT NULL,
  `calculation_time` datetime NOT NULL,
  `payment_date` datetime NOT NULL,
  `session_type` varchar(10) NOT NULL,
  `vehicle_type_id` int(11) NOT NULL,
  `is_vehicle_registration` tinyint(1) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `parking_parkingfeesession_62b0e080` (`vehicle_type_id`),
  KEY `parking_parkingfeesession_162376bc` (`parking_session_id`),
  KEY `parking_parkingfeesession_2f4f7afb` (`calculation_time`),
  KEY `parking_parkingfeesession_9a0806f4` (`session_type`),
  CONSTRAINT `vehicle_type_id_refs_id_74836f14` FOREIGN KEY (`vehicle_type_id`) REFERENCES `parking_vehicletype` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_parkingsession`
--

DROP TABLE IF EXISTS `parking_parkingsession`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_parkingsession` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `card_id` int(11) NOT NULL,
  `vehicle_type` int(11) NOT NULL,
  `vehicle_number` varchar(20) NOT NULL,
  `check_in_alpr_vehicle_number` varchar(20) NOT NULL,
  `check_in_operator_id` int(11) NOT NULL,
  `check_in_time` datetime NOT NULL,
  `check_in_images` longtext NOT NULL,
  `check_in_lane_id` int(11) NOT NULL,
  `check_out_alpr_vehicle_number` varchar(20) DEFAULT NULL,
  `check_out_operator_id` int(11) DEFAULT NULL,
  `check_out_time` datetime DEFAULT NULL,
  `check_out_images` longtext,
  `check_out_lane_id` int(11) DEFAULT NULL,
  `duration` int(11) DEFAULT NULL,
  `check_out_exception_id` int(11) DEFAULT NULL,
  `check_out_timeRide` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `parking_parkingsession_c70f998a` (`card_id`),
  KEY `parking_parkingsession_62b0e080` (`vehicle_type`),
  KEY `parking_parkingsession_a0452cbd` (`vehicle_number`),
  KEY `parking_parkingsession_bf00c629` (`check_in_operator_id`),
  KEY `parking_parkingsession_76a4fa0c` (`check_in_time`),
  KEY `parking_parkingsession_257ac8cd` (`check_in_lane_id`),
  KEY `parking_parkingsession_d2fb539f` (`check_out_operator_id`),
  KEY `parking_parkingsession_a366583e` (`check_out_time`),
  KEY `parking_parkingsession_75302522` (`check_out_lane_id`),
  KEY `parking_parkingsession_1b13c6b0` (`check_out_exception_id`),
  CONSTRAINT `card_id_refs_id_0d93286a` FOREIGN KEY (`card_id`) REFERENCES `parking_card` (`id`),
  CONSTRAINT `check_in_lane_id_refs_id_5eb8bd10` FOREIGN KEY (`check_in_lane_id`) REFERENCES `parking_lane` (`id`),
  CONSTRAINT `check_in_operator_id_refs_id_008965fc` FOREIGN KEY (`check_in_operator_id`) REFERENCES `auth_user` (`id`),
  CONSTRAINT `check_out_exception_id_refs_id_2041a15a` FOREIGN KEY (`check_out_exception_id`) REFERENCES `parking_checkoutexceptioninfo` (`id`),
  CONSTRAINT `check_out_lane_id_refs_id_5eb8bd10` FOREIGN KEY (`check_out_lane_id`) REFERENCES `parking_lane` (`id`),
  CONSTRAINT `check_out_operator_id_refs_id_008965fc` FOREIGN KEY (`check_out_operator_id`) REFERENCES `auth_user` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_parkingsetting`
--

DROP TABLE IF EXISTS `parking_parkingsetting`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_parkingsetting` (
  `key` varchar(100) NOT NULL,
  `name` varchar(500) NOT NULL,
  `value` varchar(1000) NOT NULL,
  `notes` varchar(1000) DEFAULT NULL,
  PRIMARY KEY (`key`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_pauseresumehistory`
--

DROP TABLE IF EXISTS `parking_pauseresumehistory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_pauseresumehistory` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `vehicle_registration_id` int(11) DEFAULT NULL,
  `expired_date` date DEFAULT NULL,
  `request_date` date NOT NULL,
  `start_date` date DEFAULT NULL,
  `request_type` int(11) NOT NULL,
  `request_notes` varchar(200) NOT NULL,
  `remain_duration` int(11) NOT NULL,
  `used` tinyint(1) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `parking_pauseresumehistory_0c7531ef` (`vehicle_registration_id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_receipt`
--

DROP TABLE IF EXISTS `parking_receipt`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_receipt` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `receipt_number` int(11) NOT NULL,
  `type` int(11) NOT NULL,
  `ref_id` int(11) NOT NULL,
  `cancel` tinyint(1) NOT NULL,
  `notes` varchar(500) NOT NULL,
  `action_date` datetime NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_reportdata`
--

DROP TABLE IF EXISTS `parking_reportdata`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_reportdata` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `time` datetime NOT NULL,
  `check_in` varchar(4000) NOT NULL,
  `check_out` varchar(4000) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `parking_reportdata_7f12bbd9` (`time`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_server`
--

DROP TABLE IF EXISTS `parking_server`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_server` (
  `id` int(11) NOT NULL,
  `name` varchar(200) NOT NULL,
  `ip` varchar(50) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_slot`
--

DROP TABLE IF EXISTS `parking_slot`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_slot` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(100) NOT NULL,
  `prefix` varchar(20) DEFAULT NULL,
  `suffixes` varchar(20) DEFAULT NULL,
  `numlength` int(11) DEFAULT NULL,
  `hascheckkey` int(11) DEFAULT NULL,
  `slottotal` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `name` (`name`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_terminal`
--

DROP TABLE IF EXISTS `parking_terminal`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_terminal` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(200) NOT NULL,
  `terminal_id` varchar(50) NOT NULL,
  `ip` varchar(50) NOT NULL,
  `version` varchar(50) DEFAULT NULL,
  `status` int(11) NOT NULL,
  `last_check_health` datetime NOT NULL,
  `terminal_group_id` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `parking_terminal_0edfff90` (`terminal_id`),
  KEY `parking_terminal_d0502e57` (`last_check_health`),
  KEY `parking_terminal_bcb36e14` (`terminal_group_id`),
  CONSTRAINT `terminal_group_id_refs_id_62ac341d` FOREIGN KEY (`terminal_group_id`) REFERENCES `parking_terminalgroup` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_terminalgroup`
--

DROP TABLE IF EXISTS `parking_terminalgroup`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_terminalgroup` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(200) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_ticketpayment`
--

DROP TABLE IF EXISTS `parking_ticketpayment`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_ticketpayment` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `customer_id` int(11) DEFAULT NULL,
  `receipt_id` int(11) DEFAULT NULL,
  `receipt_number` int(11) DEFAULT NULL,
  `payment_date` datetime NOT NULL,
  `payment_fee` int(11) NOT NULL,
  `payment_method` varchar(20) NOT NULL,
  `notes` varchar(200) DEFAULT NULL,
  `staff_id` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `parking_ticketpayment_09847825` (`customer_id`),
  KEY `parking_ticketpayment_f0a7d083` (`staff_id`),
  CONSTRAINT `customer_id_refs_id_ed03a025` FOREIGN KEY (`customer_id`) REFERENCES `parking_customer` (`id`),
  CONSTRAINT `staff_id_refs_id_b3482d04` FOREIGN KEY (`staff_id`) REFERENCES `parking_userprofile` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_ticketpaymentdetail`
--

DROP TABLE IF EXISTS `parking_ticketpaymentdetail`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_ticketpaymentdetail` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `ticket_payment_id` int(11) NOT NULL,
  `vehicle_registration_id` int(11) DEFAULT NULL,
  `vehicle_number` varchar(255) NOT NULL,
  `level_fee` int(11) NOT NULL,
  `effective_date` date DEFAULT NULL,
  `duration` int(11) NOT NULL,
  `day_duration` int(11) NOT NULL,
  `old_expired_date` date DEFAULT NULL,
  `expired_date` date DEFAULT NULL,
  `payment_detail_fee` int(11) NOT NULL,
  `used` tinyint(1) NOT NULL,
  `cancel_date` date DEFAULT NULL,
  `cardnumber` varchar(128) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `parking_ticketpaymentdetail_1d0580c1` (`ticket_payment_id`),
  KEY `parking_ticketpaymentdetail_0c7531ef` (`vehicle_registration_id`),
  CONSTRAINT `ticket_payment_id_refs_id_8907a626` FOREIGN KEY (`ticket_payment_id`) REFERENCES `parking_ticketpayment` (`id`),
  CONSTRAINT `vehicle_registration_id_refs_id_958f5b38` FOREIGN KEY (`vehicle_registration_id`) REFERENCES `parking_vehicleregistration` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_turnfee`
--

DROP TABLE IF EXISTS `parking_turnfee`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_turnfee` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `parking_fee_id` int(11) NOT NULL,
  `day_start_time` time NOT NULL,
  `day_end_time` time NOT NULL,
  `night_start_time` time NOT NULL,
  `night_end_time` time NOT NULL,
  `day_fee` int(11) NOT NULL,
  `night_fee` int(11) NOT NULL,
  `overnight_fee` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `parking_turnfee_8f589dc5` (`parking_fee_id`),
  CONSTRAINT `parking_fee_id_refs_id_253d40af` FOREIGN KEY (`parking_fee_id`) REFERENCES `parking_parkingfee` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_usercard`
--

DROP TABLE IF EXISTS `parking_usercard`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_usercard` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `user_id` int(11) NOT NULL,
  `card_id` varchar(128) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `parking_usercard_6340c63c` (`user_id`),
  KEY `parking_usercard_c70f998a` (`card_id`),
  CONSTRAINT `card_id_refs_card_id_302e098a` FOREIGN KEY (`card_id`) REFERENCES `parking_card` (`card_id`),
  CONSTRAINT `user_id_refs_id_a58ada31` FOREIGN KEY (`user_id`) REFERENCES `auth_user` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_userprofile`
--

DROP TABLE IF EXISTS `parking_userprofile`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_userprofile` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `user_id` int(11) NOT NULL,
  `fullname` varchar(500) NOT NULL,
  `staff_id` varchar(10) NOT NULL,
  `birthday` date DEFAULT NULL,
  `card_id` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `user_id` (`user_id`),
  UNIQUE KEY `card_id` (`card_id`),
  CONSTRAINT `card_id_refs_id_c12068d4` FOREIGN KEY (`card_id`) REFERENCES `parking_card` (`id`),
  CONSTRAINT `user_id_refs_id_338d953e` FOREIGN KEY (`user_id`) REFERENCES `auth_user` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_usershift`
--

DROP TABLE IF EXISTS `parking_usershift`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_usershift` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `user_id` int(11) NOT NULL,
  `lane_id` int(11) NOT NULL,
  `begin` datetime NOT NULL,
  `end` datetime DEFAULT NULL,
  `info` longtext,
  PRIMARY KEY (`id`),
  KEY `parking_usershift_6340c63c` (`user_id`),
  KEY `parking_usershift_e2fc017a` (`lane_id`),
  CONSTRAINT `lane_id_refs_id_3d243462` FOREIGN KEY (`lane_id`) REFERENCES `parking_lane` (`id`),
  CONSTRAINT `user_id_refs_id_bc3c8b9a` FOREIGN KEY (`user_id`) REFERENCES `auth_user` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_vehiclebalcklist`
--

DROP TABLE IF EXISTS `parking_vehiclebalcklist`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_vehiclebalcklist` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `vehicle_type` int(11) DEFAULT NULL,
  `vehicle_number` varchar(10) DEFAULT NULL,
  `notes` text,
  PRIMARY KEY (`id`),
  KEY `fk_vehicleblacklist` (`vehicle_type`),
  CONSTRAINT `parking_vehiclebalcklist_ibfk_1` FOREIGN KEY (`vehicle_type`) REFERENCES `parking_vehicletype` (`id`) ON DELETE NO ACTION ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_vehicleregistration`
--

DROP TABLE IF EXISTS `parking_vehicleregistration`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_vehicleregistration` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `card_id` int(11) DEFAULT NULL,
  `customer_id` int(11) NOT NULL,
  `level_fee_id` int(11) DEFAULT NULL,
  `registration_date` datetime NOT NULL,
  `first_renewal_effective_date` date DEFAULT NULL,
  `last_renewal_date` date DEFAULT NULL,
  `last_renewal_effective_date` date DEFAULT NULL,
  `start_date` date DEFAULT NULL,
  `expired_date` date DEFAULT NULL,
  `pause_date` date DEFAULT NULL,
  `cancel_date` date DEFAULT NULL,
  `vehicle_driver_name` varchar(255) NOT NULL,
  `vehicle_driver_id` varchar(255) NOT NULL,
  `vehicle_driver_phone` varchar(255) NOT NULL,
  `vehicle_type_id` int(11) NOT NULL,
  `vehicle_number` varchar(255) NOT NULL,
  `vehicle_brand` varchar(255) NOT NULL,
  `vehicle_paint` varchar(255) NOT NULL,
  `status` int(11) NOT NULL,
  `staff_id` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `parking_vehicleregistration_c70f998a` (`card_id`),
  KEY `parking_vehicleregistration_09847825` (`customer_id`),
  KEY `parking_vehicleregistration_199e5e2f` (`level_fee_id`),
  KEY `parking_vehicleregistration_62b0e080` (`vehicle_type_id`),
  KEY `parking_vehicleregistration_f0a7d083` (`staff_id`),
  CONSTRAINT `card_id_refs_id_c55198c5` FOREIGN KEY (`card_id`) REFERENCES `parking_card` (`id`),
  CONSTRAINT `customer_id_refs_id_adef60b0` FOREIGN KEY (`customer_id`) REFERENCES `parking_customer` (`id`),
  CONSTRAINT `level_fee_id_refs_id_c61ca765` FOREIGN KEY (`level_fee_id`) REFERENCES `parking_levelfee` (`id`),
  CONSTRAINT `staff_id_refs_id_c4b0e0db` FOREIGN KEY (`staff_id`) REFERENCES `parking_userprofile` (`id`),
  CONSTRAINT `vehicle_type_id_refs_id_941a7abb` FOREIGN KEY (`vehicle_type_id`) REFERENCES `parking_vehicletype` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_vehicleregistrationauditlogentry`
--

DROP TABLE IF EXISTS `parking_vehicleregistrationauditlogentry`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_vehicleregistrationauditlogentry` (
  `id` int(11) NOT NULL,
  `card_id` int(11) DEFAULT NULL,
  `customer_id` int(11) NOT NULL,
  `level_fee_id` int(11) DEFAULT NULL,
  `registration_date` datetime NOT NULL,
  `first_renewal_effective_date` date DEFAULT NULL,
  `last_renewal_date` date DEFAULT NULL,
  `last_renewal_effective_date` date DEFAULT NULL,
  `start_date` date DEFAULT NULL,
  `expired_date` date DEFAULT NULL,
  `pause_date` date DEFAULT NULL,
  `cancel_date` date DEFAULT NULL,
  `vehicle_driver_name` varchar(255) NOT NULL,
  `vehicle_driver_id` varchar(255) NOT NULL,
  `vehicle_driver_phone` varchar(255) NOT NULL,
  `vehicle_type_id` int(11) NOT NULL,
  `vehicle_number` varchar(255) NOT NULL,
  `vehicle_brand` varchar(255) NOT NULL,
  `vehicle_paint` varchar(255) NOT NULL,
  `status` int(11) NOT NULL,
  `staff_id` int(11) DEFAULT NULL,
  `action_user_id` int(11) DEFAULT NULL,
  `action_id` int(11) NOT NULL AUTO_INCREMENT,
  `action_date` datetime NOT NULL,
  `action_type` varchar(1) NOT NULL,
  PRIMARY KEY (`action_id`),
  KEY `parking_vehicleregistrationauditlogentry_5a576368` (`id`),
  KEY `parking_vehicleregistrationauditlogentry_c70f998a` (`card_id`),
  KEY `parking_vehicleregistrationauditlogentry_09847825` (`customer_id`),
  KEY `parking_vehicleregistrationauditlogentry_199e5e2f` (`level_fee_id`),
  KEY `parking_vehicleregistrationauditlogentry_62b0e080` (`vehicle_type_id`),
  KEY `parking_vehicleregistrationauditlogentry_f0a7d083` (`staff_id`),
  KEY `parking_vehicleregistrationauditlogentry_7537981b` (`action_user_id`),
  CONSTRAINT `action_user_id_refs_id_86fdea6e` FOREIGN KEY (`action_user_id`) REFERENCES `auth_user` (`id`),
  CONSTRAINT `card_id_refs_id_d17facdc` FOREIGN KEY (`card_id`) REFERENCES `parking_card` (`id`),
  CONSTRAINT `customer_id_refs_id_0e2f1183` FOREIGN KEY (`customer_id`) REFERENCES `parking_customer` (`id`),
  CONSTRAINT `level_fee_id_refs_id_9fbc2c9b` FOREIGN KEY (`level_fee_id`) REFERENCES `parking_levelfee` (`id`),
  CONSTRAINT `staff_id_refs_id_ca7aea04` FOREIGN KEY (`staff_id`) REFERENCES `parking_userprofile` (`id`),
  CONSTRAINT `vehicle_type_id_refs_id_b23c7db4` FOREIGN KEY (`vehicle_type_id`) REFERENCES `parking_vehicletype` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_vehicletype`
--

DROP TABLE IF EXISTS `parking_vehicletype`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_vehicletype` (
  `id` int(11) NOT NULL,
  `category` int(11) NOT NULL,
  `name` varchar(100) NOT NULL,
  `slot_id` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_voucher`
--

DROP TABLE IF EXISTS `parking_voucher`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `parking_voucher` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `parking_session_id` int(11) DEFAULT NULL,
  `voucher_type` varchar(128) DEFAULT NULL,
  `Voucher_amount` int(11) DEFAULT NULL,
  `parking_fee` int(11) DEFAULT NULL,
  `actual_fee` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping events for database 'apmsdb'
--

--
-- Dumping routines for database 'apmsdb'
--
/*!50003 DROP FUNCTION IF EXISTS `getcurrentslot` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   FUNCTION `getcurrentslot`(slot_id int) RETURNS int(11)
BEGIN

RETURN 
	(SELECT count(p.id) FROM parking_parkingsession p
	where p.check_out_time is null and p.check_out_exception_id is null and p.vehicle_type in 
		(select FLOOR(vt.id/10000) as `vehicle_type` from parking_vehicletype vt where vt.slot_id=slot_id)
	);
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP FUNCTION IF EXISTS `getimage` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   FUNCTION `getimage`(pid int, state int) RETURNS varchar(512) CHARSET utf8
begin
	declare res varchar(512);
    set res= (
		select 
			case when state=0 then p.check_in_images else p.check_out_images end as `img`
        from parking_parkingsession p 
        where p.id=pid 
    );
    return res;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP FUNCTION IF EXISTS `getvehiclesofslot` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   FUNCTION `getvehiclesofslot`(slot_id int, t int) RETURNS varchar(2000) CHARSET utf8
BEGIN

RETURN 
(
	select 
    case when t=0 then group_concat(vt.id separator ', ')   else group_concat(vt.name separator ', ') end as `name` 
    from parking_vehicletype vt
    where vt.slot_id=slot_id
);
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `addcurrentbacklist` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `addcurrentbacklist`(pid int, imgp varchar(128),b int,g int, u int, d datetime,sp int, st int, n text(1000))
begin
	insert into parking_currentbalckliststate(`blacklist`, `gate`,`user`,`date`,`stateparking`,`state`,`notes`,`parking_id`,`image_path`)
		values(b,g,u,d,sp,st,n,pid,imgp);
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `addpayment` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `addpayment`(rcnumber int,note varchar(256),cus int, ttfee int, staff varchar(128))
begin
	declare cdate datetime;
    declare tid int;
	declare rcid int;
	declare sid int;
    set cdate=DATE_SUB(now(), INTERVAL 7 HOUR);
    set rcid= (select `id` from `parking_receipt` where `receipt_number`=rcnumber);
	set sid= (select id from `auth_user` where username=staff);
    if sid is null then
		set sid=staff;
    end if;
	set sid=(select id from `parking_userprofile` where user_id=sid);
	set SQL_SAFE_UPDATES = 0;
	insert into `parking_ticketpayment`(`customer_id`, `receipt_id`,`receipt_number`,`payment_date`,`payment_fee`,`payment_method`,`notes`, `staff_id`)
	values(cus,rcid,rcnumber,cdate,ttfee,'TM','Gia hạn',sid);
    set tid=Last_Insert_Id();
	update `parking_receipt` set `ref_id`=tid
    where `id`=rcid;
	set SQL_SAFE_UPDATES = 1;
    select 1;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `addreceipt` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `addreceipt`(rcnumber int,note varchar(256))
begin
	declare cdate datetime;
    set cdate=DATE_SUB(now(), INTERVAL 7 HOUR);
    set SQL_SAFE_UPDATES = 0;
	insert into `parking_receipt`(`receipt_number`,`type`,`ref_id`,`cancel`,`notes`,`action_date`)
	values(rcnumber,0,0,0,note,cdate);
   
    set SQL_SAFE_UPDATES = 1;
    select 1;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `autoupdateticket` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `autoupdateticket`()
begin
	set SQL_SAFE_UPDATES = 0;
	update parking_ticketpaymentdetail as pt
	inner join parking_vehicleregistration as v on v.id=pt.vehicle_registration_id
	inner join parking_card as c on c.id=v.card_id
	set pt.cardnumber=c.card_label
    where pt.cardnumber is null;
    set SQL_SAFE_UPDATES = 1;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `cancelvoucher` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `cancelvoucher`(cid varchar(100),citime datetime)
begin
	declare cardid int; 
    set cardid = (select id from parking_card where card_id = cid);
    set SQL_SAFE_UPDATES = 0;
    
	delete v from `parking_voucher` v
	inner join parking_parkingsession s on s.id=v.parking_session_id
	where  s.card_id=cardid and s.check_in_time=citime;
    set SQL_SAFE_UPDATES = 1;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `cardlist` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `cardlist`()
begin
	(select `parking_card`.* 
    from `parking_card`
    where not exists
		(select 1
			from `parking_vehicleregistration`
            where `parking_vehicleregistration`.`card_id`=`parking_card`.`id`
        )
	);
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `ceillingfee` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `ceillingfee`()
begin
	set SQL_SAFE_UPDATES = 0;
	update parking_ticketpaymentdetail
	set payment_detail_fee=1000+floor(payment_detail_fee/1000)*1000
	where mod(payment_detail_fee,1000)>0;
	update parking_ticketpayment
	set payment_fee=1000+floor(payment_fee/1000)*1000
	where mod(payment_fee,1000)>0;
	set SQL_SAFE_UPDATES = 1;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `checkbill` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `checkbill`(bd date, c varchar(128), bc varchar(128))
BEGIN
	if exists 
		(	select 1 
			from parking_claimpromotionbillv2 
			where company_info=c and cast(`date` as date)=bd and bill_number=bc
		) then
		select 'No' as `Result`;
	else 
		select 'Yes' as `Result`;
	end if;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `cusreport` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `cusreport`()
begin
	select pt.`name` as `cus_type`,pc.customer_name
    from parking_customer pc
    inner join parking_customertype pt on pt.id=pc.customer_type_id
    order by pt.name, pc.customer_name;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `deletedata` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `deletedata`()
begin
set SQL_SAFE_UPDATES = 0;
	delete  from parking_ticketpaymentdetail where duration=0 and day_duration=0 and cancel_date is null;
set SQL_SAFE_UPDATES = 1;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `findbacklist` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `findbacklist`(vnum varchar(10))
begin
	select * 
    from parking_vehiclebalcklist p
    where  length(vnum)>=4 and length(p.vehicle_number)>=4 and
		SUBSTRING_INDEX(p.vehicle_number,'-',-1)=SUBSTRING_INDEX(vnum,'-',-1);
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `findcurentbacklist` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `findcurentbacklist`(b int, d datetime)
begin
	select * 
    from parking_currentbalckliststate
    where blacklist=b and `date`>=d and state=0;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `forcedbarierget` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `forcedbarierget`(
	username varchar(50),
    address varchar(128),
    lane varchar(128),
    direction varchar(128),
    fromtime datetime, 
    totime datetime
)
begin
	select * 
    from `parking_forcedbarier`
    where `forcedtime` between fromtime and totime
		and (ifnull(address,'')='' or `teminal` = address)
        and `lan` like CONCAT('%', direction , '%')
        and `lan` like CONCAT('%', lane)
        and (ifnull(username,'')='' or `user`=username)
	order by forcedtime desc, lan;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `forcedbariersave` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `forcedbariersave`(
	username varchar(50),lane varchar(128), pcaddress varchar(128),timestp long,front varchar(256),back varchar(256),fdate datetime, nt varchar(4000)
)
begin
	insert into `parking_forcedbarier`(`user`,`lan`,`teminal`,`timestampe`,`frontpath`,`backpath`,`forcedtime`,`note`)
    values(username,lane,pcaddress,timestp,front,back,fdate,nt);
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getblacklistnotify` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getblacklistnotify`(gatename varchar(256), d datetime, duration int)
begin
	Select p.id, DATE_FORMAT(p.`date`,'%d/%m/%Y %H:%i:%s') as `parkingdate` , b.vehicle_number,v.`name` as `vehicle_type`, t.`name` as `gate`, u.fullname as `user`,
		case when p.stateparking = 0 then 'Check_In' else 'Check_Out' end `staeParking` ,p.notes,p.parking_id,getimage(p.parking_id,p.stateparking) as `image_path`
    from parking_currentbalckliststate p
    left join parking_vehiclebalcklist b on b.id=p.blacklist
    left join parking_vehicletype v on v.id=b.vehicle_type
    left join parking_terminal t on t.id=p.gate
    left join parking_userprofile u on u.user_id=p.user
    where p.`date` between DATE_SUB(d, INTERVAL 1 DAY) and DATE_add(d, INTERVAL 1 DAY) and (p.inactive!=1 or p.inactive is null);
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getblacklistreport` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getblacklistreport`(fromtime datetime, totime datetime)
begin
	#set fromtime=DATE_SUB(fromtime,interval 7 hour);
    #set totime=DATE_SUB(totime,interval 7 hour);
    select t.*
    from(
	Select p.id, DATE_FORMAT(p.`date`,'%d/%m/%Y %H:%i:%s') as `parkingdate` , b.vehicle_number,v.`name` as `vehicle_type`, t.`name` as `gate`, u.fullname as `user`,
		case when p.stateparking = 0 then 'Check_In' else 'Check_Out' end `staeParking` ,p.notes,p.parking_id,getimage(p.parking_id,p.stateparking) as `image_path`, 
        case when p.inactive =1 then 'Inactive' else 'Active' end as `state`
    from parking_currentbalckliststate p 
    left join parking_vehiclebalcklist b on b.id=p.blacklist
    left join parking_vehicletype v on v.id=b.vehicle_type
    left join parking_terminal t on t.id=p.gate
    left join parking_userprofile u on u.user_id=p.user
    where p.`date` between fromtime and totime 
    ) t
    where t.vehicle_number is not null and t.vehicle_type is not null and t.image_path is not null
    order by t.state,t.parkingdate desc;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getdrivernumberbysession` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getdrivernumberbysession`(fdate datetime, tdate datetime, tp int)
begin
	set fdate=DATE_SUB(fdate,interval 7 hour);
	set tdate=DATE_SUB(tdate,interval 7 hour);
	select t.* 
    from
    (	
		
        select pc.card_label,pt.name as `vehicletype`, 
			DATE_FORMAT(DATE_SUB(p.check_in_time, INTERVAL -7 HOUR),'%d/%m/%Y %H:%i:%s') as 'checkintime',
			DATE_FORMAT(DATE_SUB(p.check_out_time, INTERVAL -7 HOUR),'%d/%m/%Y %H:%i:%s') as 'checkouttime',
			ui.fullname as `usercheckin`,uo.fullname as `usercheckout`,
			p.vehicle_number,
			case when  p.vehicle_number !='' and p.vehicle_number like '% ' then 'Nhập biển số' else 'Tự động' end as `vnumberregistype`    
		from 
        (	select pk.* 
			from parking_parkingsession pk
            where ((pk.check_out_time is null and pk.check_in_time between fdate and tdate) or  (pk.check_out_time is not null and pk.check_out_time between fdate and tdate )) 
		) p  
		left join parking_card pc on pc.id=p.card_id
		left join parking_vehicletype pt on round(pt.id/10000)=p.vehicle_type
		left join parking_userprofile ui on ui.user_id=p.check_in_operator_id
		left join parking_userprofile uo on uo.user_id=p.check_out_operator_id
		
    ) t
    where (tp=1 and t.vnumberregistype='Nhập biển số') or (tp=0 and t.vnumberregistype='Tự động') or (tp=-1)
    order by t.vnumberregistype,t.checkintime,t.checkouttime;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getduoblecheckin` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getduoblecheckin`()
begin
select pg.* 
from parking_parkingsession pg
where #pg.check_in_time between fromdate and todate
		 pg.check_out_time is null 
	 and
        exists(select 1 from
		(select *from(
		select check_in_images, card_id,count(*) as 'count' 
		from parking_parkingsession 
		where #check_in_time between fromdate and todate
			 check_out_time is null
		group by card_id,check_in_images)t
		where t.count>1)tt 
		where tt.check_in_images=pg.check_in_images and tt.card_id=pg.card_id
        );

end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getduoblecheckinfull` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getduoblecheckinfull`(fromdate datetime,todate datetime)
begin
select pg.* 
from parking_parkingsession pg
where pg.check_in_time between fromdate and todate and pg.check_out_time is null
	and exists(select 1 from parking_parkingsession p where p.id <> pg.id and p.card_id=pg.card_id and p.check_in_time>=pg.check_in_time);

end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getduoblecheckout` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getduoblecheckout`()
begin
select pg.* 
from parking_parkingfeesession pg
inner join 
	(select *from(
	select parking_session_id,card_id,count(*) as 'count' 
	from parking_parkingfeesession 
	where session_type='out' and payment_date>='2018-01-01'
	group by parking_session_id,card_id)t
	where t.count>1)tt 
on tt.parking_session_id=pg.parking_session_id
where pg.session_type='out'
order by pg.parking_session_id;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getfarcards` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getfarcards`()
begin
	select card_id
    from parking_card
    where length(card_id)=24;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getGroup` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getGroup`()
begin
	select * from auth_group;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getGroupPermission` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getGroupPermission`()
begin
	select * from  auth_group_permissions;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getgroupuser` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getgroupuser`()
begin
select * from auth_group;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `gethistoryaccess` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `gethistoryaccess`(fromtime datetime, totime datetime)
begin
	select DATE_FORMAT( h.actiondate, '%d/%m/%Y %H:%i:%s') as actiontime,h.userid,h.username,h.target,h.useraction,h.content
    from historyaccess h
    where h.actiondate between fromtime and totime
    order by h.target,h.actiondate;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getparkinghourly` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getparkinghourly`(fromtime datetime, totime datetime)
begin
    #declare rank int;
    set fromtime=DATE_SUB(fromtime,interval 7 hour);
    set totime=DATE_SUB(totime,interval 7 hour);
    select DATE_FORMAT(DATE_SUB(ps.check_out_time, INTERVAL -7 HOUR),'%d%m%Y') as 'id',
		case when ps.vehicle_number like'%?%' or ifnull(ps.vehicle_number,'')=''  then '' else ps.vehicle_number end as `vehiclenumber`,
		pt.name as `vehicletype`,ct.name as `cardtype`,pc.card_label as `cardcode`,
		DATE_FORMAT(DATE_SUB(ps.check_in_time, INTERVAL -7 HOUR),'%Y-%m-%d %H:%i:%s') as 'checkintime',
		case when ifnull(cl.server_time,'')='' then '' else  DATE_FORMAT(DATE_SUB(cl.server_time, INTERVAL -7 HOUR),'%Y-%m-%d %H:%i:%s') end as 'Claimed',
        DATE_FORMAT(DATE_SUB(ps.check_out_time, INTERVAL -7 HOUR),'%Y-%m-%d %H:%i:%s') as 'checkouttime', 
		CONCAT(
				FLOOR(HOUR(TIMEDIFF(ps.check_out_time, ps.check_in_time)) / 24), ' - ',
				MOD(HOUR(TIMEDIFF(ps.check_out_time, ps.check_in_time)), 24), ':',
				MINUTE(TIMEDIFF(ps.check_out_time, ps.check_in_time)), ':',
				SECOND(TIMEDIFF(ps.check_out_time, ps.check_in_time)), '') as `ParkingDuration`,
		pf.parking_fee, abs(cl.amount_b)+abs(cl.amount_c)+abs(cl.amount_d) as `redemption`, 
        ui.fullname as `usercheckin`,uc.fullname as 'userclaim',uo.fullname as `usercheckout`,
		ti.name as `teminalin`,tou.name as `teminalout`
	from 
    (select pss.* from  parking_parkingsession pss where pss.check_out_time between fromtime and totime
		and pss.check_out_exception_id is null) ps
	left join 
    (select pff.* from  parking_parkingfeesession pff where pff.payment_date between fromtime and totime and pff.session_type='OUT' ) 
		pf on pf.parking_session_id=ps.id
	left join parking_vehicletype pt on round(pt.id/10000)=ps.vehicle_type
	left join parking_card pc on pc.id=ps.card_id
	left join parking_cardtype ct on ct.id=pc.card_type
	left join parking_userprofile ui on ui.user_id=ps.check_in_operator_id
	left join parking_userprofile uo on uo.user_id=ps.check_out_operator_id
	left join parking_lane li on li.id=ps.check_in_lane_id
	left join parking_lane lo on lo.id=ps.check_out_lane_id
	left join parking_terminal ti on ti.id=li.terminal_id
	left join parking_terminal tou on tou.id=lo.terminal_id
	left join parking_claimpromotionv2 cl on cl.parking_session_id=ps.id
	left join parking_userprofile uc on uc.user_id=cl.user_id
	order by ps.check_out_time;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getparkingsessionreportdetail` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getparkingsessionreportdetail`(fromtime datetime, totime datetime)
begin
	set fromtime=DATE_SUB(fromtime,interval 7 hour);
    set totime=DATE_SUB(totime,interval 7 hour);
	select t.*
    from( 
		select DATE_FORMAT(DATE_SUB(p.check_out_time, INTERVAL -7 HOUR),'%d/%m/%Y %H:%i:%s') as `paymentday`,pt.name as `vehicletype`,
			pc.card_label,p.vehicle_number,
			ui.fullname as `usercheckin`,
			DATE_FORMAT(DATE_SUB(p.check_in_time, INTERVAL -7 HOUR),'%d/%m/%Y %H:%i:%s') as 'checkintime',
			uo.fullname as `usercheckout`,
			DATE_FORMAT(DATE_SUB(p.check_out_time, INTERVAL -7 HOUR),'%d/%m/%Y %H:%i:%s') as 'checkouttime', 
			null as `exceptionout`, pt.id as `col1`,pc.card_type as `col2`,DATE_FORMAT(p.check_out_time,'%Y-%m-%d') as `col3`,null `col4`, pf.parking_fee
		from 
        (
			select * 
			from parking_parkingsession 
			where check_out_time between fromtime and totime and check_out_time is not null and check_out_exception_id is null 
        ) p
        left join  parking_parkingfeesession pf on pf.parking_session_id=p.id and  pf.session_type='OUT'
		
		left join parking_vehicletype pt on round(pt.id/10000)=p.vehicle_type
		left join parking_card pc on pc.id=p.card_id
		#left join parking_cardtype ct on ct.id=pc.card_type
		left join parking_userprofile ui on ui.user_id=p.check_in_operator_id
		left join parking_userprofile uo on uo.user_id=p.check_out_operator_id
		
		union 
		select  DATE_FORMAT(DATE_SUB(p.check_out_time, INTERVAL -7 HOUR),'%d/%m/%Y %H:%i:%s') as `paymentday`,pt.name as `vehicletype`,
			pc.card_label,p.vehicle_number,
			ui.fullname as `usercheckin`,
			DATE_FORMAT(DATE_SUB(p.check_in_time, INTERVAL -7 HOUR),'%d/%m/%Y %H:%i:%s') as 'checkintime',
			uo.fullname as `usercheckout`,
			DATE_FORMAT(DATE_SUB(p.check_out_time, INTERVAL -7 HOUR),'%d/%m/%Y %H:%i:%s') as 'checkouttime', 
			e.notes as `exceptionout`, pt.id as `col1`,pc.card_type as `col2`,DATE_FORMAT(p.check_out_time,'%Y-%m-%d') as `col3`,null `col4`, e.parking_fee
		from 
        (
			select * 
			from parking_parkingsession 
			where check_out_time between fromtime and totime and check_out_time is not null and check_out_exception_id is not null
        ) p
		left join parking_checkoutexceptioninfo e on e.id=p.check_out_exception_id
		left join parking_vehicletype pt on round(pt.id/10000)=p.vehicle_type
		left join parking_card pc on pc.id=p.card_id
		#left join parking_cardtype ct on ct.id=pc.card_type
		left join parking_userprofile ui on ui.user_id=p.check_in_operator_id
		left join parking_userprofile uo on uo.user_id=p.check_out_operator_id
		
		)t 
	order by t.exceptionout, t.checkouttime desc;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getparking_voucher` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getparking_voucher`(sid int)
Begin
	Select * from parking_voucher where parking_session_id=sid;
End ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getpaymentid` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getpaymentid`()
begin
	select max(id) from parking_ticketpayment;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getPCAddress` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getPCAddress`()
begin
	(select '-Tất cả-' as `name`,'' as `value`)
    union
    (
		select concat(t.ip,'-',t.name) as`name`, concat(t.ip,'-',t.name) as`value`
		from parking_terminal t
		order by t.ip,t.name
    );
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getpermissionbyuser` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getpermissionbyuser`(uid int)
begin
select up.id,up.permission_id,p.codename
from auth_user_user_permissions up
left join auth_permission p on p.id=up.permission_id
where up.user_id=uid and p.codename like '%reportdata%';
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getpermittionroot` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getpermittionroot`()
begin
    select t.*
	from 
	(SELECT p.id,t.id as `o_id`,t.name as `function`,
		case  when  p.codename like '%add%' then 'Can add' when p.codename like '%change%' then 'Can change' else '' end   as `description` 
    FROM auth_permission p
	JOIN django_content_type t on t.id=p.content_type_id
	where (p.codename like '%add%' or p.codename like '%change%')
		and t.id in (1,2,4,10,11,13,14,22,23,24,25,27,28,30,31,32,33,34,35,39,40,41,43,44,45,48,56)
	 union 
	 select (select ap.id from auth_permission ap where ap.content_type_id=p1.id limit 1) as `id`, p1.id as `o_id`,p1.name as `function`,'Can see' as `description` 
	 from django_content_type p1
	 where 
		p1.id in (1,2,4,10,11,13,14,22,23,24,25,27,28,30,31,32,33,34,35,39,40,41,43,44,45,48,56)
	)  t
	where t.id is not null and t.o_id is not null
	order by t.o_id,t.id;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getreceiptnumber` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getreceiptnumber`()
begin
	select ifnull(max(receipt_number),0)+1 as `receiptnumber` from `parking_receipt`;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getredempts` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getredempts`(fromtime datetime, totime datetime)
begin
	select cl.id as `transactionno`,ps.vehicle_number,pt.`name` as `vehicle_type`,pc.card_label as `cardcode`,us.fullname,
		DATE_FORMAT(DATE_SUB(ps.check_in_time, INTERVAL -7 HOUR),'%Y-%m-%d %H:%i:%S') as 'checkintime', 
        DATE_FORMAT(DATE_SUB(cl.server_time, INTERVAL -7 HOUR),'%Y-%m-%d %H:%i:%S') as 'claimtime',
       CONCAT(
			FLOOR(HOUR(TIMEDIFF(cl.server_time, ps.check_in_time)) / 24), ' - ',
			MOD(HOUR(TIMEDIFF(cl.server_time, ps.check_in_time)), 24), ':',
			MINUTE(TIMEDIFF(cl.server_time, ps.check_in_time)), ':',
			SECOND(TIMEDIFF(cl.server_time, ps.check_in_time)), '') as `ParkingDuration`,
		case when t.tennant is null then 'Không có hóa đơn' else t.tennant end as 'tennant',
		case when t.tennant is null then '' else t.bill_no end as 'bill_no',
		case when t.tennant is null then 0 else t.bill_amount end as 'bill_amount',
		case when t.tennant is null then 0 else t.total end as 'totalbill',
        cl.amount_a as `parkingfee`,abs(cl.amount_b)+abs(cl.amount_c)+abs(cl.amount_d) as `redemption`,
        #old#cl.amount_e as `remainingfee`, 
        case when cl.amount_a>=(abs(cl.amount_b)+abs(cl.amount_c)+abs(cl.amount_d)) then cl.amount_a-(abs(cl.amount_b)+abs(cl.amount_c)+abs(cl.amount_d)) else 0 end  as `remainingfee`,
        cl.notes as `remark`,pt.id as `vtype`,DATE_FORMAT(cl.server_time,'%Y-%m-%d') as `daycompare`
    from parking_claimpromotionv2 cl
    left join parking_parkingsession ps on ps.id=cl.parking_session_id
    left join parking_card pc on pc.id=ps.card_id
    left join parking_vehicletype pt on round(pt.id/10000)=ps.vehicle_type
    left join parking_userprofile us on us.user_id=cl.user_id
    left join 
    (
		SELECT claim_promotion_id,  GROUP_CONCAT(DISTINCT  company_info SEPARATOR '; ') as `tennant`,
			GROUP_CONCAT(bill_number SEPARATOR '; ') as `bill_no`,
			GROUP_CONCAT(bill_amount SEPARATOR '; ') as `bill_amount`, sum(bill_amount) as `total` 
		FROM parking_claimpromotionbillv2 GROUP BY claim_promotion_id
    ) t on t.claim_promotion_id=cl.id
    where DATE_SUB(cl.server_time, INTERVAL -7 HOUR) between fromtime and totime
    and cl.used=1
    order by cl.server_time;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getreportcompact` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getreportcompact`(fromtime datetime, totime datetime)
begin
	set fromtime=DATE_SUB(fromtime,interval 7 hour);
    set totime=DATE_SUB(totime,interval 7 hour);
	select v.name as `vehicle_type`, ct.name as `card_type`, c.card_label,
		DATE_FORMAT(DATE_SUB(t.check_in_time, INTERVAL -7 HOUR),'%d/%m/%Y %H:%i:%s') as `checkintime`,
		case when cl.id is not null then DATE_FORMAT(DATE_SUB(cl.server_time, INTERVAL -7 HOUR),'%d/%m/%Y %H:%i:%s') else '' end as 'claimed',
        DATE_FORMAT(DATE_SUB(t.check_out_time, INTERVAL -7 HOUR),'%d/%m/%Y %H:%i:%s') as `checkouttime`,
        t.duration,
		case when cl.id is not null then abs(cl.amount_b)+abs(cl.amount_c)+abs(cl.amount_d) else 0 end as redemption
        #,pf.parking_fee
    from
    (
		select p.* from parking_parkingsession p
		where p.check_in_time between fromtime and totime and p.check_in_time is not null
    ) t
    left join parking_card c on c.id=t.card_id
    left join parking_cardtype ct on ct.id=c.card_type
    left join parking_vehicletype v on round(v.id/10000)=t.vehicle_type
    left join parking_claimpromotionv2 cl on cl.parking_session_id=t.id
    #left join parking_parkingfeesession pf on pf.parking_session_id=t.id and pf.session_type='OUT'
    ;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getsamplefee` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getsamplefee`(sid varchar(128), ftype int)
begin
	Select * 
    from samplefee
    where (ifnull(sid,0)=0 or id=sid)
    and (ifnull(ftype,0)=0 or feetype=ftype);
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getslotbyvehicle` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getslotbyvehicle`(vid int)
begin
	select p.id,p.slottotal,getcurrentslot(p.id) as `currentslot`
    from parking_slot  p
    where exists(select 1 from parking_vehicletype vt where vt.id=vid and vt.slot_id=p.id);
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getslots` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getslots`()
begin
	select p.*,getcurrentslot(p.id) as `currentslot`,getvehiclesofslot(p.id,0) as `vehiclesid`,getvehiclesofslot(p.id,1) as `vehiclesname`
    from parking_slot  p;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getsysusers` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getsysusers`()
begin
	select *
	from
	(
		select u.id as `userid`,
			case when pu.id is not null then pu.fullname else u.username end as `username`,
			 case when g.id is not null then g.id else -1 end as `groupid`,
			case when g.id is not null then g.name else 'Others' end as `groupname`
		from auth_user u
		left join auth_user_groups ug on ug.user_id=u.id
		left join auth_group g on g.id=ug.group_id
		left join parking_userprofile pu on pu.user_id=u.id
        where u.is_active>0
    ) t
    order by t.groupid,t.userid;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getUser` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getUser`()
begin
	(select '-Tất cả-' as `name`,'' as `value`)
    union
    (
		select t.username as 'name',t.username as 'value'
		from auth_user t
		order by t.username
    );
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getuserbygroup` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getuserbygroup`(gid int)
begin
	select * 
    from(
		select us.id, case when ifnull(up.id,0)=0 then us.username else up.fullname end as `uname`,
			case when ifnull(ug.group_id,0)=0 then -1 else ug.group_id end as 'groupid'
		from auth_user us
		left join auth_user_groups ug on ug.user_id=us.id
		left join parking_userprofile up on up.user_id=us.id
        where us.is_active=1
        ) t
	where t.groupid=gid
    order by uname;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getuserbygroupid` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getuserbygroupid`(gid int)
begin	
	select u.id,
		case when p.id is not null then p.fullname else u.username end as `username`, g.group_id
    from auth_user u
    left join auth_user_groups g on g.user_id=u.id
    left join parking_userprofile p on p.user_id=u.id
    where g.id=gid
    order by u.id;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getuserlist` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getuserlist`()
begin
	select au.username,pu.fullname,ag.name as `groupname`,au.date_joined as `effectivedate`,
		case when au.is_active=1 then 'active' else 'deactive' end as `remark`,
		case when au.is_staff=1 then 'x' else '' end as `admin`
	from auth_user au
	left join auth_user_groups aug on aug.user_id=au.id
	left join auth_group ag on ag.id=aug.group_id
	left join parking_userprofile pu on pu.user_id=au.id
	order by au.is_staff desc, ag.id,au.is_active desc,au.date_joined;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getvanglaidetail` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getvanglaidetail`(fromtime datetime, totime datetime)
begin
    set fromtime=DATE_SUB(fromtime,interval 7 hour);
    set totime=DATE_SUB(totime,interval 7 hour);
    select 
		pt.name as `vehicletype`,pc.card_label as `cardcode`,case when ps.vehicle_number like'%?%' or ifnull(ps.vehicle_number,'')=''  then '' else ps.vehicle_number end as `vehiclenumber`,
		ui.fullname as `usercheckin`,
		DATE_FORMAT(DATE_SUB(ps.check_in_time, INTERVAL -7 HOUR),'%Y-%m-%d %H:%i:%s') as 'checkintime',
		uo.fullname as `usercheckout`,
        DATE_FORMAT(DATE_SUB(ps.check_out_time, INTERVAL -7 HOUR),'%Y-%m-%d %H:%i:%s') as 'checkouttime', 
        ec.notes,
				
		case when ps.check_out_exception_id is null then pf.parking_fee else ec.parking_fee end as `parking_fee`
	from 
    (select pss.* from  parking_parkingsession pss where pss.check_out_time between fromtime and totime) ps
	left join 
    (select pff.* from  parking_parkingfeesession pff where pff.payment_date between fromtime and totime and pff.session_type='OUT' ) 
		pf on pf.parking_session_id=ps.id
	left join parking_vehicletype pt on round(pt.id/10000)=ps.vehicle_type
	left join parking_card pc on pc.id=ps.card_id
    left join parking_cardtype ct on ct.id=pc.card_type
	left join parking_userprofile ui on ui.user_id=ps.check_in_operator_id
	left join parking_userprofile uo on uo.user_id=ps.check_out_operator_id
    LEFT JOIN parking_checkoutexceptioninfo ec on ec.id=ps.check_out_exception_id
    where ct.id=0
	order by ps.check_out_exception_id, ps.check_out_time;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getvehecleregitration` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getvehecleregitration`(pageindex int,pagenumber int, cus int)
BEGIN
	declare rr int;
    declare cd date;
    set cd=curdate();
    set @rr=0;
    select tt.*,@rr as 'TotalRows'
    from 
    (
		select t.*,@rr:=@rr+1 as 'RowNumber'
		from
		(
			select cr.`card_label` as 'card',c.`customer_name` as `customer`,t.`name` as `vehecletype`, v.vehicle_number,v.vehicle_brand,v.vehicle_driver_name,
				v.vehicle_driver_phone,
				DATE_FORMAT(v.registration_date, '%d/%m/%Y %H:%i:%s') as `registration_date`,      
				DATE_FORMAT(v.start_date, '%d/%m/%Y') as `start_date`, 
				DATE_FORMAT(v.expired_date, '%d-%m-%Y') as `expired_date` ,
				case 
					#when v.cancel_date is not null then 'Hủy'
					#when v.pause_date then 'Tạm ngừng'
					when v.expired_date=v.first_renewal_effective_date then 'Chưa đăng ký'
					when v.expired_date< cd then 'Hết hạn'
					else  'Đang dùng' end
				as `status`,
				pt.receipt_number,dt.duration,dt.payment_detail_fee as `totalfee`,dt.ticket_payment_id as `paymentid`
			from `parking_vehicleregistration` v
			left join `parking_vehicletype` t on (t.id=v.vehicle_type_id)
			left join `parking_customer` c on c.id=v.customer_id
			left join `parking_card` cr on cr.id=v.card_id
			left join `parking_ticketpaymentdetail` dt on dt.`vehicle_registration_id`=v.`id`
			left join `parking_ticketpayment` pt on pt.id=dt.ticket_payment_id
			where v.customer_id=cus  and  v.cancel_date is null and v.pause_date is null and dt.duration is not null and  dt.duration>0
			order by  v.start_date desc,c.customer_name
		) as t
    )as tt
    where tt.RowNumber >=(pageindex-1)*pagenumber +1 and tt.RowNumber<=pageindex*pagenumber;

end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getVehicleInParking` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getVehicleInParking`(
	reporttime datetime, 
    vtype int,
    clable int, 
    durationdays int, 
    filterby int
)
begin
declare rownum int;
set @rownum:=0;
set reporttime=DATE_SUB(reporttime, interval 7 hour);
select  @rownum:=@rownum + 1 as `Row_Number`,  p.id,c.card_label,ct.name as `cardtype`, pt.name as `vehicletype`,p.vehicle_number,
		p.check_in_alpr_vehicle_number,
        DATE_FORMAT(DATE_SUB(p.check_in_time, INTERVAL -7 HOUR),'%d/%m/%Y %H:%i:%s') as 'checkintime'
        ,DATE_FORMAT(DATE_SUB(p.check_out_time , INTERVAL -7 HOUR),'%d/%m/%Y %H:%i:%s') as 'checkouttime'
from parking_parkingsession p
left join parking_card c on c.id=p.card_id
left join parking_cardtype ct on ct.id=c.card_type
left join parking_vehicletype pt on round(pt.id/10000)=p.vehicle_type
where (p.check_out_time is null or p.check_out_time > reporttime) and p.check_in_time <=reporttime
	and (ifnull(clable,-1)=-1 or ct.id=clable)
    and (ifnull(vtype,0)=0 or pt.id=vtype)
	and (ifnull(filterby,-1)=-1 or ifnull(durationdays,0)=0 or 
		( durationdays >0 and 
			(
				(filterby=0 and durationdays = datediff(reporttime,p.check_in_time))
				or (filterby=1 and durationdays >= datediff(reporttime,p.check_in_time))
                or (filterby=2 and durationdays > datediff(reporttime,p.check_in_time))
                or (filterby=3 and durationdays <= datediff(reporttime,p.check_in_time))
                or (filterby=4 and durationdays < datediff(reporttime,p.check_in_time))
            )
		)
    )
    ;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getvehicletype` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getvehicletype`()
begin
	select vt.id,vt.category,vt.`name` as `vehiclename` from parking_vehicletype  vt;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `get_parkingsessionsearch` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `get_parkingsessionsearch`(
	fromtime datetime,
	totime datetime, 
	cardid int, 
	cardlabel varchar(50),
	vehiclenumber varchar(50),
	vehicletype int, 
	modefilter int, 
	terminal int,
	operator int, 
	pageindex int, 
	pagesize int 
    )
begin
	DECLARE LowerBound INT;
	DECLARE UpperBound INT;
	DECLARE rownum INT;
    set @rownum:=0;
	SET LowerBound = ((pageindex - 1) * pagesize) + 1;
	SET UpperBound = (pageindex  * pagesize);
    #set vehicletype=vehicletype/10000;
    select tt.*,@rownum as `total` from
    (
		select @rownum:=@rownum + 1 as `Row_Number` , t.id,c.card_id,c.card_label,c.card_type,p.vehicle_type,p.vehicle_number,
			p.check_in_alpr_vehicle_number,p.check_out_alpr_vehicle_number,p.check_in_images,
			p.check_out_images,p.check_in_time,p.check_out_time,p.check_in_lane_id,p.check_out_lane_id#, ifnull(f.parking_fee,0) as `fee` 
		from
		(
			select p.id
			from parking_parkingsession p
			#left join parking_card c on c.id=p.card_id
			where (	modefilter =1 and
					p.check_out_time between fromtime and totime and ifnull(p.check_out_time,0)!=0 
					and(ifnull(cardid,0)=0 or p.card_id=cardid)
					#and (ifnull(cardlabel,'')='' or c.card_label like Concat('%',cardlabel,'%'))
					and (ifnull(vehiclenumber,'')='' or p.vehicle_number like Concat('%',vehiclenumber,'%'))
					and (ifnull(vehicletype,0)=0 or p.vehicle_type=vehicletype)
                    and (ifnull(operator,0)=0 or p.check_out_operator_id=operator)
					and (ifnull(terminal,0)=0 or p.check_out_lane_id in (select l.id from parking_lane l where l.terminal_id=terminal))		
				 ) or
				 (  modefilter=0  and
					p.check_in_time between fromtime and totime and ifnull(p.check_out_time,0)=0 
					and(ifnull(cardid,0)=0 or p.card_id=cardid)
					#and (ifnull(cardlabel,'')='' or c.card_label like Concat('%',cardlabel,'%'))
					and (ifnull(vehiclenumber,'')='' or p.vehicle_number like Concat('%',vehiclenumber,'%'))
					and (ifnull(vehicletype,0)=0 or p.vehicle_type=vehicletype)
                    and (ifnull(operator,0)=0 or p.check_in_operator_id=operator)
					and (ifnull(terminal,0)=0 or p.check_in_lane_id in (select l.id from parking_lane l where l.terminal_id=terminal))		
				)
				or
				(	modefilter=2 and 
					(p.check_in_time between fromtime and totime) 
					and(ifnull(cardid,0)=0 or p.card_id=cardid)
					#and (ifnull(cardlabel,'')='' or c.card_label like Concat('%',cardlabel,'%'))
					and (ifnull(vehiclenumber,'')='' or p.vehicle_number like Concat('%',vehiclenumber,'%'))
					and (ifnull(vehicletype,0)=0 or p.vehicle_type=vehicletype)
                    and (ifnull(operator,0)=0 or p.check_in_operator_id=operator)
					
                    and (ifnull(terminal,0)=0 
						or p.check_in_lane_id in (select l.id from parking_lane l where l.terminal_id=terminal ) 
                        or (p.check_out_lane_id is not null and p.check_in_lane_id in (select l.id from parking_lane l where l.terminal_id=terminal ) )
					)	
				)
			order by p.id desc
		)t
        inner join parking_parkingsession p on p.id=t.id
        #left join parking_parkingfeesession f on f.parking_session_id=p.id and f.session_type='OUT'
        left join parking_card c on c.id=p.card_id
        where ifnull(cardlabel,'')='' or c.card_label=cardlabel #like Concat('%',cardlabel,'%')
    )tt 
    where tt.Row_Number between LowerBound and UpperBound;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `get_parkingsessionsearch_multi` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `get_parkingsessionsearch_multi`(
	fromtime datetime,
	totime datetime, 
	cardid int, 
	cardlabel varchar(50),
	vehiclenumber varchar(50),
	vehicletype int, 
	modefilter int, 
	terminal int,
	operator int, 
	pageindex int, 
	pagesize int 
    )
begin
	DECLARE LowerBound INT;
	DECLARE UpperBound INT;
	DECLARE rownum INT;
    declare maxid int;
    declare minid int;
    if 	modefilter =1 then
		set maxid=(select max(id) from parking_parkingsession where check_out_time<=totime);
		set minid=(select min(id) from parking_parkingsession where check_out_time>=fromtime);
	else
		set maxid=(select max(id) from parking_parkingsession where check_in_time<=totime);
		set minid=(select min(id) from parking_parkingsession where check_in_time>=fromtime);
	end if;
    set @rownum:=0;
	SET LowerBound = ((pageindex - 1) * pagesize) + 1;
	SET UpperBound = (pageindex  * pagesize);
    #set vehicletype=vehicletype/10000;
    select tt.*,@rownum as `total` from
    (
		select @rownum:=@rownum + 1 as `Row_Number` , t.id,c.card_id,c.card_label,c.card_type,p.vehicle_type,p.vehicle_number,
			p.check_in_alpr_vehicle_number,p.check_out_alpr_vehicle_number,p.check_in_images,
			p.check_out_images,p.check_in_time,p.check_out_time,p.check_in_lane_id,p.check_out_lane_id#, ifnull(f.parking_fee,0) as `fee` 
		from
		(
			select p.id
			from parking_parkingsession p
			#left join parking_card c on c.id=p.card_id
			where (	modefilter =1 and
					p.id between minid and maxid and ifnull(p.check_out_time,0)!=0 
					and(ifnull(cardid,0)=0 or p.card_id=cardid)
					#and (ifnull(cardlabel,'')='' or c.card_label like Concat('%',cardlabel,'%'))
					and (ifnull(vehiclenumber,'')='' or p.vehicle_number like Concat('%',vehiclenumber,'%'))
					and (ifnull(vehicletype,0)=0 or p.vehicle_type=vehicletype)
                    and (ifnull(operator,0)=0 or p.check_out_operator_id=operator)
					and (ifnull(terminal,0)=0 or p.check_out_lane_id in (select l.id from parking_lane l where l.terminal_id=terminal))		
				 ) or
				 (  modefilter=0  and
					p.id between minid and maxid and ifnull(p.check_out_time,0)=0 
					and(ifnull(cardid,0)=0 or p.card_id=cardid)
					#and (ifnull(cardlabel,'')='' or c.card_label like Concat('%',cardlabel,'%'))
					and (ifnull(vehiclenumber,'')='' or p.vehicle_number like Concat('%',vehiclenumber,'%'))
					and (ifnull(vehicletype,0)=0 or p.vehicle_type=vehicletype)
                    and (ifnull(operator,0)=0 or p.check_in_operator_id=operator)
					and (ifnull(terminal,0)=0 or p.check_in_lane_id in (select l.id from parking_lane l where l.terminal_id=terminal))		
				)
				or
				(	modefilter=2 and 
					(p.id between minid and maxid) 
					and(ifnull(cardid,0)=0 or p.card_id=cardid)
					#and (ifnull(cardlabel,'')='' or c.card_label like Concat('%',cardlabel,'%'))
					and (ifnull(vehiclenumber,'')='' or p.vehicle_number like Concat('%',vehiclenumber,'%'))
					and (ifnull(vehicletype,0)=0 or p.vehicle_type=vehicletype)
                    and (ifnull(operator,0)=0 or p.check_in_operator_id=operator)
					 and (ifnull(terminal,0)=0 
						or p.check_in_lane_id in (select l.id from parking_lane l where l.terminal_id=terminal ) 
                        or (p.check_out_lane_id is not null and p.check_in_lane_id in (select l.id from parking_lane l where l.terminal_id=terminal ) )
					)	
				)
			order by p.id desc
		)t
        inner join parking_parkingsession p on p.id=t.id
        #left join parking_parkingfeesession f on f.parking_session_id=p.id and f.session_type='OUT'
        left join parking_card c on c.id=p.card_id
        where ifnull(cardlabel,'')='' or c.card_label =cardlabel #like Concat('%',cardlabel,'%')
    )tt 
    where tt.Row_Number between LowerBound and UpperBound;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `get_redemptionsearch` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `get_redemptionsearch`(
	fromtime datetime,
	totime datetime, 
	cardid int, 	
	vehiclenumber varchar(50),
	vehicletype int, 
	pageindex int, 
	pagesize int 
    )
begin
	DECLARE LowerBound INT;
	DECLARE UpperBound INT;
	DECLARE rownum INT;
    set @rownum:=0;
	SET LowerBound = ((pageindex - 1) * pagesize) + 1;
	SET UpperBound = (pageindex  * pagesize);
    select tt.*,@rownum as `total` from
    (
		select @rownum:=@rownum + 1 as `Row_Number` ,t.* 
		from
		(   select u.*
			from(
				select c.*
				from parking_claimpromotionv2 c
				left join parking_parkingsession p on p.id=c.parking_session_id 
				where  c.server_time between fromtime and totime 
                )u
                where 
                (ifnull(vehiclenumber,'')='' and ifnull(vehicletype,0)=0 and ifnull(cardid,0)=0)
                or
                exists (select 1 
								from parking_parkingsession p 
                                where p.id=u.parking_session_id 
									and (ifnull(vehiclenumber,'')='' or p.vehicle_number like Concat('%',vehiclenumber,'%'))
									and (ifnull(vehicletype,0)=0 or p.vehicle_type=vehicletype)
									and (ifnull(cardid,0)=0 or p.card_id=cardid)
							)
			order by u.id desc
		)t
        
    )tt 
    where tt.Row_Number between LowerBound and UpperBound;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `grouppermissionreport` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `grouppermissionreport`()
begin
	select * from groupuserpermission order by menuid;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `parkingsessionreportdetail` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `parkingsessionreportdetail`(fd date, td date, ct nvarchar(128), mt nvarchar(128))
begin
	select ctm.`name` as `customer_type`,cm.`customer_name`,cp.`name` as `company`,pb.`name` as `building`,
		pv.vehicle_driver_name as `drive_name`,pv.vehicle_driver_id as `old_licence_plate`,pv.vehicle_number,vt.`name` as `vehicle_type`,
        pc.card_label as `card_code`,case ct.`name` when 'Thẻ vãng lai' then 'Thẻ tháng' else ct.`name` end as `card_type`,
        case when pd.effective_date< fd then fd else pd.effective_date end as `commencement_date`,
        case when  pd.expired_date>td or pd.old_expired_date>td then td else ifnull(pd.expired_date,pd.old_expired_date) end as `expiry_date`,
        lf.`name` as `fee_type`,lf.fee as `fee_type_no`,pt.receipt_id as `payment_no`,DATE_FORMAT(pt.payment_date, '%m/%d/%Y %H:%i:%s') as `payment_date`,
        case when  pd.cancel_date is null or pd.cancel_date>td then null  else pd.cancel_date end as `canceled_date`,
         case when  pd.cancel_date is null or pd.cancel_date>td then null else pt.receipt_number end as `canceled_reciept`,
        pt.payment_method as `method`, pt.notes as `remark`, 0 as `bill`, 0 as `canceled_fee`
    from parking_ticketpaymentdetail pd
	inner join parking_ticketpayment pt on pt.id=pd.ticket_payment_id
    inner join parking_vehicleregistration pv on pv.id=pd.vehicle_registration_id
    left join parking_levelfee lf on lf.id=pv.level_fee_id
    left join parking_card pc on pc.id=pv.card_id
    left join parking_cardtype ct on ct.id=pc.card_type
    left join parking_vehicletype vt on vt.id=pv.vehicle_type_id
    left join parking_customer cm on cm.id=pv.customer_id
    left join parking_customertype ctm on ctm.id=cm.customer_type_id
    left join parking_building pb on pb.id=cm.building_id
    left join parking_company cp on cp.id=cm.company_id
    where pd.effective_date is not null and pd.expired_date is not null 
		#and  ((pd.effective_date between fd and td) or (fd between pd.effective_date and pd.expired_date))
		#and pv.expired_date>=fd
        and (pd.expired_date>=fd or pd.old_expired_date>=fd)
        and pd.effective_date<=td
        and (ct='' or ctm.`name`=ct)
        and (mt='' or pt.payment_method=mt)
		and not exists(select 1 from parking_receipt rc where rc.id=pt.receipt_id and rc.cancel=1)
	order by ctm.`name`,cm.customer_name;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `renewalget` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `renewalget`(cus int)
begin
	declare cd date;
    set cd=curdate();
	select pv.id, pv.vehicle_number,pc.card_label,pt.name as `vehicletype`,
		case 
			#when pv.cancel_date is not null then 'Hủy'
			#when pv.pause_date then 'Tạm ngừng'
			when pv.expired_date=pv.first_renewal_effective_date then 'Chưa đăng ký'
			when pv.expired_date< cd then 'Hết hạn'
			else  'Đang dùng' end
		as `status`,
        case when  pv.expired_date is null then  DATE_FORMAT(CURDATE() , '%d/%m/%Y') 
			else case when DATE_ADD(pv.expired_date,INTERVAL 1 day) < CURDATE() then DATE_FORMAT(CURDATE() , '%d/%m/%Y') else DATE_FORMAT(DATE_ADD(pv.expired_date,INTERVAL 1 day) , '%d/%m/%Y')  end
            end as `activedate`,
        DATE_FORMAT(pv.expired_date, '%d/%m/%Y') as `expired_date` ,pf.fee
	from parking_vehicleregistration pv
    left join parking_card pc on pc.id=pv.card_id
    left join parking_vehicletype pt on pt.id=pv.vehicle_type_id
    left join parking_levelfee pf on pf.id=pv.level_fee_id
    where pv.customer_id=cus and  pv.cancel_date is null and pv.pause_date is null; # and pv.status in (1,3);
   
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `renewalupdate` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `renewalupdate`(rcnumber int,note varchar(256), staff varchar(128),idrg int, oldd date, newd date, fd int, lvfee int)
begin
	declare cdate datetime;
    declare rcid int;
	
	declare pmid int;
    declare drt int;
    set cdate=DATE_SUB(now(), INTERVAL 7 HOUR);
	set drt=TIMESTAMPDIFF(day,oldd,newd)+1; 
   
	
    set rcid= (select `id` from `parking_receipt` where `receipt_number`=rcnumber);
    
	set pmid= (select `id` from `parking_ticketpayment` where `receipt_id`=rcid);
    set SQL_SAFE_UPDATES = 0;
    insert into parking_ticketpaymentdetail(ticket_payment_id,vehicle_registration_id,vehicle_number,
			level_fee,effective_date,duration,day_duration,old_expired_date,expired_date,payment_detail_fee, used)
	select pmid,pv.id,pv.vehicle_number,lvfee,oldd,drt,0,pv.expired_date,newd,fd,0
    from `parking_vehicleregistration` pv
    where pv.id=idrg;
	update `parking_vehicleregistration`
    set `expired_date`=newd,
		`last_renewal_date`=cdate,
        `last_renewal_effective_date`=oldd ,
        `status`=1
	where id=idrg;
    set SQL_SAFE_UPDATES = 1;
    select 1;

end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `savevoucher` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `savevoucher`(cid varchar(100),citime datetime, vtype varchar(256), vamount int, fee int, afee int )
begin
	declare cardid int; 
	declare sessionid int;
    set cardid = (select id from parking_card where card_id = cid);
    if exists (select 1 from parking_parkingsession s where s.card_id=cardid and s.check_in_time=citime) then
        set sessionid=(select s.id from parking_parkingsession s where s.card_id=cardid and s.check_in_time=citime);
        if exists(select 1 from parking_voucher where parking_session_id=sessionid) then
			set SQL_SAFE_UPDATES = 0;
            Update parking_voucher 
            set voucher_type=vtype,
				Voucher_amount=vamount,
                parking_fee=fee,
                actual_fee=afee
			where parking_session_id=sessionid;
            set SQL_SAFE_UPDATES = 1;
		else
            insert into parking_voucher(parking_session_id,voucher_type,Voucher_amount,parking_fee,actual_fee)
            values(sessionid,vtype,vamount,fee,afee);
		end if;
        select 1;
    end if;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `selectblacklist` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `selectblacklist`()
begin
	select distinct SUBSTRING_INDEX(b.vehicle_number,'-',-1)
    from parking_vehiclebalcklist b;   
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `updateadjustment` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `updateadjustment`()
begin
	set SQL_SAFE_UPDATES = 0;
	update  apmsdb.parking_feeadjustment
	set fee=fee-2*fee;
	set SQL_SAFE_UPDATES = 1;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `vehicleregistrationsave` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `vehicleregistrationsave`(cus int, cr nvarchar(128),flevel int,vtype int, 
	v_num varchar(128), v_paint varchar(128), nt varchar(128),d_name nvarchar(128), 
    d_phone varchar(50), f_date date, t_date date, tt_fee decimal, staff nvarchar(128) )
BEGIN
    declare cid int;
    declare tid int;
    declare rid int;
    declare sid int;
    declare rcnum int;
    declare rcid int;
    declare fpm decimal;
	declare drt int;
    declare cdate datetime;
    DECLARE EXIT HANDLER FOR SQLEXCEPTION ROLLBACK;
	DECLARE EXIT HANDLER FOR SQLWARNING ROLLBACK;
    set cid= (select id from `parking_card` where card_label=cr);	
	set sid= (select id from `parking_userprofile` where user_id=staff);
    set fpm= (select fee from `parking_levelfee` where id=flevel);
    set drt=TIMESTAMPDIFF(day,f_date,t_date)+1; 
    set rcnum=1;
    set rcnum= (select max(receipt_number)+1 from `parking_receipt`);
    set cdate=now();
	set SQL_SAFE_UPDATES = 0;
    START TRANSACTION;
		insert into `parking_receipt`(`receipt_number`,`type`,`ref_id`,`cancel`,`notes`,`action_date`)
		values(rcnum,0,0,0,'Đăng ký mới',cdate);
		update `parking_receipt` set `ref_id`=`id` where `receipt_number`=rcnum;
		set rcid= (select `id` from `parking_receipt` where `receipt_number`=rcnum);
		insert into `parking_ticketpayment`(`customer_id`, `receipt_id`,`receipt_number`,`payment_date`,`payment_fee`,`payment_method`,`notes`,`staff_id`)
		values(cus,rcid,rcnum,cdate,tt_fee,'TM','Đăng ký mới',sid);
		set tid=Last_Insert_Id();
		insert into `parking_vehicleregistration`(`card_id`,`customer_id`,`level_fee_id`,`registration_date`,`first_renewal_effective_date`,
			`start_date`, `expired_date`, `last_renewal_date`, `last_renewal_effective_date`,`vehicle_driver_name`,
			`vehicle_driver_id`,`vehicle_driver_phone`,`vehicle_type_id`, `vehicle_number`,`vehicle_brand`,`vehicle_paint`,`status`,`staff_id`)
		values (cid,cus,flevel,cdate,f_date, f_date,t_date,f_date,f_date,d_name,0,d_phone,vtype,v_num,v_paint,nt,1,sid);
		set rid=Last_Insert_Id();
		insert into parking_ticketpaymentdetail(ticket_payment_id,vehicle_registration_id,vehicle_number,
			level_fee,effective_date,duration,day_duration,old_expired_date,expired_date,payment_detail_fee, used)
		values(tid,rid,v_num,fpm,f_date,drt,0,t_date,t_date,tt_fee,1);
        select 1;
    commit;
	set SQL_SAFE_UPDATES = 1;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `vehicletypedele` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `vehicletypedele`(vcode bigint)
begin
	delete from parking_vehicletype where id=vcode;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `vehicletypereport` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `vehicletypereport`()
begin
	select pt.`name` from parking_vehicletype pt;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `vehicletypesave` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8 */ ;
/*!50003 SET character_set_results = utf8 */ ;
/*!50003 SET collation_connection  = utf8_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `vehicletypesave`(vcode bigint, vname varchar(128), k varchar(20))
begin
	if(k='add') then
		insert into `parking_vehicletype`(`id`,`category`,`name`) values(vcode,vcode,vname);
	else
		update `parking_vehicletype`
        set `name`= vname
        where `id`=vcode;
    end if;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2019-10-16 13:54:39
