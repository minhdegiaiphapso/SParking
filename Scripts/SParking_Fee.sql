CREATE DATABASE  IF NOT EXISTS `hd_fee` /*!40100 DEFAULT CHARACTER SET utf8mb3 */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `hd_fee`;
-- MySQL dump 10.13  Distrib 8.0.40, for Win64 (x86_64)
--
-- Host: localhost    Database: hd_fee
-- ------------------------------------------------------
-- Server version	8.0.40

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `billformula`
--

DROP TABLE IF EXISTS `billformula`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `billformula` (
  `id` int NOT NULL AUTO_INCREMENT,
  `callname` varchar(128) DEFAULT NULL,
  `detail` varchar(2000) DEFAULT NULL,
  `usercreate` int DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `billformula`
--

LOCK TABLES `billformula` WRITE;
/*!40000 ALTER TABLE `billformula` DISABLE KEYS */;
/*!40000 ALTER TABLE `billformula` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `config`
--

DROP TABLE IF EXISTS `config`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `config` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `callname` varchar(128) DEFAULT NULL,
  `val` varchar(128) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `config`
--

LOCK TABLES `config` WRITE;
/*!40000 ALTER TABLE `config` DISABLE KEYS */;
INSERT INTO `config` VALUES (1,'Cách tính phí','new');
/*!40000 ALTER TABLE `config` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `cycleredemption`
--

DROP TABLE IF EXISTS `cycleredemption`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cycleredemption` (
  `id` int NOT NULL AUTO_INCREMENT,
  `fromtime` time DEFAULT NULL,
  `totime` time DEFAULT NULL,
  `formulabill` int DEFAULT NULL,
  `datetypeid` int DEFAULT NULL,
  `cycletype` int DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `cycleredemption`
--

LOCK TABLES `cycleredemption` WRITE;
/*!40000 ALTER TABLE `cycleredemption` DISABLE KEYS */;
/*!40000 ALTER TABLE `cycleredemption` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `cycletime`
--

DROP TABLE IF EXISTS `cycletime`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cycletime` (
  `id` int NOT NULL AUTO_INCREMENT,
  `fromtime` time DEFAULT NULL,
  `totime` time DEFAULT NULL,
  `formulauseinfirst` int DEFAULT NULL,
  `formula` int DEFAULT NULL,
  `cycletype` int DEFAULT NULL,
  `dateetypeid` int DEFAULT NULL,
  `fullfee` int DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=338 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `cycletime`
--

LOCK TABLES `cycletime` WRITE;
/*!40000 ALTER TABLE `cycletime` DISABLE KEYS */;
INSERT INTO `cycletime` VALUES (330,'00:00:00','23:59:59',NULL,40,1,80,NULL),(331,'04:00:00','23:59:59',NULL,42,1,81,NULL),(332,'00:00:00','03:59:59',NULL,44,1,81,NULL),(336,'00:00:00','06:00:00',NULL,55,1,83,NULL),(337,'06:00:01','23:59:59',NULL,54,1,83,NULL);
/*!40000 ALTER TABLE `cycletime` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `datetyperedemption`
--

DROP TABLE IF EXISTS `datetyperedemption`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `datetyperedemption` (
  `id` int NOT NULL AUTO_INCREMENT,
  `callname` varchar(128) DEFAULT NULL,
  `weekmap` varchar(128) DEFAULT NULL,
  `redemptfeeid` int DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=25 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `datetyperedemption`
--

LOCK TABLES `datetyperedemption` WRITE;
/*!40000 ALTER TABLE `datetyperedemption` DISABLE KEYS */;
INSERT INTO `datetyperedemption` VALUES (15,'Ngày thường','2,3,4,5,6,7,1',53),(16,'Ngày thường','2,3,4,5,6,7,1',54),(20,'Ngày thường','2,3,4,5,6,7,1',64),(21,'Ngày thường','2,3,4,5,6,7,1',65),(22,'Ngày thường','2,3,4,5,6,7,1',70),(23,'Ngày thường','2,3,4,5,6,7,1',71),(24,'Ngày thường','2,3,4,5,6,7,1',73);
/*!40000 ALTER TABLE `datetyperedemption` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `fee24detail`
--

DROP TABLE IF EXISTS `fee24detail`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `fee24detail` (
  `id` int NOT NULL AUTO_INCREMENT,
  `samplefeeid` int DEFAULT NULL,
  `blockhours` varchar(256) DEFAULT NULL,
  `blockfee` varchar(256) DEFAULT NULL,
  `affterfee` int DEFAULT NULL,
  `canrepeat` int DEFAULT NULL,
  `exceptfee` int DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `fee24detail`
--

LOCK TABLES `fee24detail` WRITE;
/*!40000 ALTER TABLE `fee24detail` DISABLE KEYS */;
INSERT INTO `fee24detail` VALUES (1,104,'[{\"blockhour\": \"12\", \"blockfee\": \"5000\"}]','[{\"blockhour\": \"12\", \"blockfee\": \"5000\"}]',0,0,0);
/*!40000 ALTER TABLE `fee24detail` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `feeformula`
--

DROP TABLE IF EXISTS `feeformula`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `feeformula` (
  `id` int NOT NULL AUTO_INCREMENT,
  `feetype` int DEFAULT NULL,
  `callname` varchar(128) DEFAULT NULL,
  `detail` varchar(2000) DEFAULT NULL,
  `fullfee` int DEFAULT NULL,
  `usercreate` int DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=56 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `feeformula`
--

LOCK TABLES `feeformula` WRITE;
/*!40000 ALTER TABLE `feeformula` DISABLE KEYS */;
INSERT INTO `feeformula` VALUES (40,2,'2021-03-15 Car','[{\"hours\": \"2\", \"money\": \"35000\", \"des\": \"trên block\", \"isonly\": false}, {\"hours\": 0, \"money\": \"20000\", \"des\": \"trên giờ\", \"isonly\": true}]',0,1),(42,1,'2021-03-15 Bike Day','',0,1),(44,1,'2021-03-15 Bike Night','',0,1),(45,1,'20210615_BikeDay','',0,1),(46,1,'20210615_BikeNight','',0,1),(47,1,'20210615_CarNight','',0,1),(51,1,'20210615_CarDay','',0,1),(52,1,'20210615_BikecicleDay','',0,1),(53,1,'20210615_BikecicleNight','',0,1),(54,2,'GardenMaill_Oto','[{\"hours\": \"2\", \"money\": \"30000\", \"des\": \"trên block\", \"isonly\": true}, {\"hours\": 0, \"money\": \"15000\", \"des\": \"trên giờ\", \"isonly\": true}]',0,1),(55,1,'GardenMaill_Oto_dem','',0,1);
/*!40000 ALTER TABLE `feeformula` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `groupuserpermission`
--

DROP TABLE IF EXISTS `groupuserpermission`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `groupuserpermission` (
  `id` int NOT NULL AUTO_INCREMENT,
  `menuid` int DEFAULT NULL,
  `groupid` int DEFAULT NULL,
  `isadd` int DEFAULT NULL,
  `isedit` int DEFAULT NULL,
  `isdel` int DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=67 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `groupuserpermission`
--

LOCK TABLES `groupuserpermission` WRITE;
/*!40000 ALTER TABLE `groupuserpermission` DISABLE KEYS */;
INSERT INTO `groupuserpermission` VALUES (21,9,1,1,2,2),(31,1,1,1,2,2),(32,1,1,1,2,2),(35,5,1,1,2,2),(36,5,1,1,2,2),(37,2,1,1,2,2),(42,7,1,1,2,2),(43,6,1,1,2,2),(44,3,1,1,2,2),(45,4,1,1,2,2),(46,10,1,1,2,2),(47,11,1,1,2,2),(48,12,1,1,2,2),(49,11,1,1,2,2),(50,13,1,1,2,2),(51,8,1,1,2,2),(52,5,8,1,2,2),(53,5,8,0,0,0),(54,1,8,1,2,2),(55,2,8,1,2,2),(56,7,8,1,2,2),(57,6,8,1,2,2),(58,3,8,1,2,2),(59,7,8,1,2,2),(60,4,8,1,2,2),(61,10,8,1,2,2),(62,11,8,1,2,2),(63,12,8,1,2,2),(64,13,8,1,2,2),(65,8,8,1,2,2),(66,9,8,1,2,2);
/*!40000 ALTER TABLE `groupuserpermission` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `historyaccess`
--

DROP TABLE IF EXISTS `historyaccess`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `historyaccess` (
  `id` int NOT NULL AUTO_INCREMENT,
  `target` varchar(256) DEFAULT NULL,
  `useraction` varchar(128) DEFAULT NULL,
  `content` varchar(4000) DEFAULT NULL,
  `actiondate` datetime DEFAULT NULL,
  `userid` int DEFAULT NULL,
  `username` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=840 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `historyaccess`
--

LOCK TABLES `historyaccess` WRITE;
/*!40000 ALTER TABLE `historyaccess` DISABLE KEYS */;
INSERT INTO `historyaccess` VALUES (81,'Nhóm đối tác','Thêm','Amin','2018-07-19 16:12:52',1,'Support'),(82,'Bảng dữ liệu: tenantgroup','Xóa','ID: 4','2018-07-19 16:12:56',1,'Support'),(83,'Nhóm đối tác','Thêm','All Group','2018-07-19 16:13:09',1,'Support'),(84,'Bảng dữ liệu: feeformula','Xóa','ID: 2','2018-07-24 06:38:25',1,'Support'),(85,'Công thức phí - dạng phức hợp','Thêm','2018-07-26 Bike Guest','2018-07-24 06:40:59',1,'Support'),(86,'Công thức phí - dạng phức hợp','Thêm','2018-07-26 Car Guest','2018-07-24 06:41:16',1,'Support'),(87,'Công thức phí - dạng phức hợp','Thêm','2018-07-26 Car Delivery','2018-07-24 06:41:40',1,'Support'),(88,'Công thức phí - dạng phức hợp','Thêm','2018-07-26 Bike Delivery','2018-07-24 06:41:59',1,'Support'),(89,'Công thức phí - dạng phức hợp','Cập nhật','2018-07-26 Bike Delivery','2018-07-24 06:45:30',1,'Support'),(90,'Công thức phí - dạng phức hợp','Cập nhật','2018-07-26 Car Delivery','2018-07-24 06:46:03',1,'Support'),(91,'Công thức phí - dạng phức hợp','Cập nhật','2018-07-26 Car Guest','2018-07-24 06:46:48',1,'Support'),(92,'Công thức phí - dạng phức hợp','Cập nhật','2018-07-26 Car Delivery','2018-07-24 06:47:05',1,'Support'),(93,'Công thức phí - dạng phức hợp','Cập nhật','2018-07-26 Bike Guest','2018-07-24 06:47:50',1,'Support'),(94,'Biểu phí','Thêm','loại thẻ: Thẻ vãng lai, loại xe: Xe máy, ngày hiệu lực: 2018-07-26','2018-07-24 06:49:26',1,'Support'),(95,'Biểu phí','Thêm','loại thẻ: Element, loại xe: Xe máy, ngày hiệu lực: 2018-07-26','2018-07-24 06:49:26',1,'Support'),(96,'Biểu phí','Thêm','loại thẻ: Thẻ vãng lai, loại xe: Xe Van, ngày hiệu lực: 2018-07-26','2018-07-24 06:49:40',1,'Support'),(97,'Biểu phí','Thêm','loại thẻ: Thẻ vãng lai, loại xe: Ô tô, ngày hiệu lực: 2018-07-26','2018-07-24 06:49:40',1,'Support'),(98,'Biểu phí','Thêm','loại thẻ: Element, loại xe: Xe Van, ngày hiệu lực: 2018-07-26','2018-07-24 06:49:40',1,'Support'),(99,'Biểu phí','Thêm','loại thẻ: Element, loại xe: Ô tô, ngày hiệu lực: 2018-07-26','2018-07-24 06:49:40',1,'Support'),(100,'Biểu phí','Thêm','loại thẻ: Thẻ vãng lai, loại xe: XM-giao hàng, ngày hiệu lực: 2018-07-26','2018-07-24 06:49:53',1,'Support'),(101,'Biểu phí','Thêm','loại thẻ: Element, loại xe: XM-giao hàng, ngày hiệu lực: 2018-07-26','2018-07-24 06:49:53',1,'Support'),(102,'Biểu phí','Thêm','loại thẻ: Thẻ vãng lai, loại xe: Tải giao hàng, ngày hiệu lực: 2018-07-26','2018-07-24 06:50:01',1,'Support'),(103,'Biểu phí','Thêm','loại thẻ: Element, loại xe: Tải giao hàng, ngày hiệu lực: 2018-07-26','2018-07-24 06:50:01',1,'Support'),(104,'Công thức phí - dạng ngày đêm','Thêm','Unexpired Monthly','2018-07-24 06:53:44',1,'Support'),(105,'Biểu phí','Thêm','loại thẻ: Thẻ tháng, loại xe: Xe máy, ngày hiệu lực: 2018-07-26','2018-07-24 06:54:02',1,'Support'),(106,'Biểu phí','Thêm','loại thẻ: FOC, loại xe: Xe máy, ngày hiệu lực: 2018-07-26','2018-07-24 06:54:02',1,'Support'),(107,'Bảng dữ liệu: sampleactive','Xóa','ID: 58','2018-07-24 06:54:34',1,'Support'),(108,'Bảng dữ liệu: sampleactive','Xóa','ID: 59','2018-07-24 06:54:38',1,'Support'),(109,'Biểu phí','Thêm','loại thẻ: Thẻ tháng, loại xe: Xe máy, ngày hiệu lực: 2018-07-26','2018-07-24 06:55:08',1,'Support'),(110,'Biểu phí','Thêm','loại thẻ: FOC, loại xe: Xe máy, ngày hiệu lực: 2018-07-26','2018-07-24 06:55:08',1,'Support'),(111,'Biểu phí','Thêm','loại thẻ: Thẻ tháng, loại xe: Xe Van, ngày hiệu lực: 2018-07-26','2018-07-24 06:55:26',1,'Support'),(112,'Biểu phí','Thêm','loại thẻ: Thẻ tháng, loại xe: Ô tô, ngày hiệu lực: 2018-07-26','2018-07-24 06:55:26',1,'Support'),(113,'Biểu phí','Thêm','loại thẻ: FOC, loại xe: Xe Van, ngày hiệu lực: 2018-07-26','2018-07-24 06:55:26',1,'Support'),(114,'Biểu phí','Thêm','loại thẻ: FOC, loại xe: Ô tô, ngày hiệu lực: 2018-07-26','2018-07-24 06:55:26',1,'Support'),(115,'Biểu phí','Thêm','loại thẻ: Thẻ tháng, loại xe: XM-giao hàng, ngày hiệu lực: 2018-07-26','2018-07-24 06:55:40',1,'Support'),(116,'Biểu phí','Thêm','loại thẻ: FOC, loại xe: XM-giao hàng, ngày hiệu lực: 2018-07-26','2018-07-24 06:55:40',1,'Support'),(117,'Biểu phí','Thêm','loại thẻ: Thẻ tháng, loại xe: Tải giao hàng, ngày hiệu lực: 2018-07-26','2018-07-24 06:55:50',1,'Support'),(118,'Biểu phí','Thêm','loại thẻ: FOC, loại xe: Tải giao hàng, ngày hiệu lực: 2018-07-26','2018-07-24 06:55:50',1,'Support'),(119,'Biểu phí','Thêm','loại thẻ: Thẻ tháng, loại xe: Bất kỳ, ngày hiệu lực: 2018-07-26','2018-07-24 06:56:15',1,'Support'),(120,'Biểu phí','Thêm','loại thẻ: FOC, loại xe: Bất kỳ, ngày hiệu lực: 2018-07-26','2018-07-24 06:56:15',1,'Support'),(121,'Phân quyền - nhân viên','Xóa','Chức năng: 5, Nhóm: -1','2018-07-26 09:44:52',1,'Support'),(122,'Phân quyền - nhân viên','Xóa','Chức năng: 5, Nhân viên: 44','2018-07-26 09:44:52',1,'Support'),(123,'Phân quyền - nhân viên','Xóa','Chức năng: 5, Nhân viên: 44','2018-07-26 09:44:52',1,'Support'),(124,'Phân quyền - nhóm','Cập nhật','Chức năng: 5, Nhóm: 1','2018-07-26 09:44:52',1,'Support'),(125,'Phân quyền - nhân viên','Cập nhật','Chức năng: 5, Nhân viên: 38','2018-07-26 09:44:52',1,'Support'),(126,'Phân quyền - nhân viên','Cập nhật','Chức năng: 5, Nhân viên: 44','2018-07-26 09:44:52',1,'Support'),(127,'Phân quyền - nhân viên','Xóa','Chức năng: 5, Nhân viên: 44','2018-07-26 09:44:52',1,'Support'),(128,'Phân quyền - nhân viên','Xóa','Chức năng: 5, Nhân viên: 44','2018-07-26 09:44:52',1,'Support'),(129,'Phân quyền - nhân viên','Xóa','Chức năng: 5, Nhân viên: 44','2018-07-26 09:44:52',1,'Support'),(130,'Phân quyền - nhân viên','Xóa','Chức năng: 5, Nhân viên: 44','2018-07-26 09:44:52',1,'Support'),(131,'Phân quyền - nhân viên','Xóa','Chức năng: 1, Nhóm: -1','2018-07-26 09:45:04',1,'Support'),(132,'Phân quyền - nhân viên','Xóa','Chức năng: 1, Nhân viên: 42','2018-07-26 09:45:04',1,'Support'),(133,'Phân quyền - nhân viên','Xóa','Chức năng: 1, Nhân viên: 42','2018-07-26 09:45:04',1,'Support'),(134,'Phân quyền - nhóm','Cập nhật','Chức năng: 1, Nhóm: 1','2018-07-26 09:45:04',1,'Support'),(135,'Phân quyền - nhân viên','Cập nhật','Chức năng: 1, Nhân viên: 35','2018-07-26 09:45:04',1,'Support'),(136,'Phân quyền - nhân viên','Cập nhật','Chức năng: 1, Nhân viên: 42','2018-07-26 09:45:04',1,'Support'),(137,'Phân quyền - nhân viên','Xóa','Chức năng: 1, Nhân viên: 42','2018-07-26 09:45:04',1,'Support'),(138,'Phân quyền - nhân viên','Xóa','Chức năng: 1, Nhân viên: 42','2018-07-26 09:45:04',1,'Support'),(139,'Phân quyền - nhân viên','Xóa','Chức năng: 1, Nhân viên: 42','2018-07-26 09:45:04',1,'Support'),(140,'Phân quyền - nhân viên','Xóa','Chức năng: 1, Nhân viên: 42','2018-07-26 09:45:04',1,'Support'),(141,'Phân quyền - nhân viên','Xóa','Chức năng: 2, Nhóm: -1','2018-07-26 09:45:15',1,'Support'),(142,'Phân quyền - nhân viên','Xóa','Chức năng: 2, Nhân viên: 43','2018-07-26 09:45:15',1,'Support'),(143,'Phân quyền - nhân viên','Xóa','Chức năng: 2, Nhân viên: 43','2018-07-26 09:45:15',1,'Support'),(144,'Phân quyền - nhóm','Cập nhật','Chức năng: 2, Nhóm: 1','2018-07-26 09:45:15',1,'Support'),(145,'Phân quyền - nhân viên','Cập nhật','Chức năng: 2, Nhân viên: 43','2018-07-26 09:45:15',1,'Support'),(146,'Phân quyền - nhân viên','Xóa','Chức năng: 2, Nhân viên: 43','2018-07-26 09:45:15',1,'Support'),(147,'Phân quyền - nhân viên','Xóa','Chức năng: 2, Nhân viên: 43','2018-07-26 09:45:15',1,'Support'),(148,'Phân quyền - nhân viên','Xóa','Chức năng: 2, Nhân viên: 43','2018-07-26 09:45:15',1,'Support'),(149,'Phân quyền - nhân viên','Xóa','Chức năng: 2, Nhân viên: 43','2018-07-26 09:45:15',1,'Support'),(150,'Phân quyền - nhóm','Cập nhật','Chức năng: 5, Nhóm: 1','2018-07-26 09:45:57',1,'Support'),(151,'Phân quyền - nhân viên','Xóa','Chức năng: 5, Nhân viên: 38','2018-07-26 09:45:57',1,'Support'),(152,'Phân quyền - nhân viên','Thêm','Chức năng: 5, Nhân viên: 107','2018-07-26 09:45:57',1,'Support'),(153,'Phân quyền - nhân viên','Thêm','Chức năng: 5, Nhân viên: 1','2018-07-26 09:45:57',1,'Support'),(154,'Phân quyền - nhóm','Cập nhật','Chức năng: 1, Nhóm: 1','2018-07-26 09:46:07',1,'Support'),(155,'Phân quyền - nhân viên','Xóa','Chức năng: 1, Nhân viên: 35','2018-07-26 09:46:07',1,'Support'),(156,'Phân quyền - nhân viên','Thêm','Chức năng: 1, Nhân viên: 107','2018-07-26 09:46:07',1,'Support'),(157,'Phân quyền - nhân viên','Thêm','Chức năng: 1, Nhân viên: 1','2018-07-26 09:46:07',1,'Support'),(158,'Phân quyền - nhân viên','Xóa','Chức năng: 5, Nhân viên: 46','2018-07-26 09:46:20',1,'Support'),(159,'Phân quyền - nhân viên','Xóa','Chức năng: 5, Nhân viên: 46','2018-07-26 09:46:20',1,'Support'),(160,'Phân quyền - nhóm','Cập nhật','Chức năng: 5, Nhóm: 1','2018-07-26 09:46:20',1,'Support'),(161,'Phân quyền - nhân viên','Xóa','Chức năng: 5, Nhân viên: 39','2018-07-26 09:46:20',1,'Support'),(162,'Phân quyền - nhân viên','Cập nhật','Chức năng: 5, Nhân viên: 45','2018-07-26 09:46:20',1,'Support'),(163,'Phân quyền - nhân viên','Cập nhật','Chức năng: 5, Nhân viên: 46','2018-07-26 09:46:20',1,'Support'),(164,'Phân quyền - nhân viên','Xóa','Chức năng: 5, Nhân viên: 46','2018-07-26 09:46:20',1,'Support'),(165,'Phân quyền - nhân viên','Xóa','Chức năng: 5, Nhân viên: 46','2018-07-26 09:46:20',1,'Support'),(166,'Phân quyền - nhân viên','Xóa','Chức năng: 5, Nhân viên: 46','2018-07-26 09:46:20',1,'Support'),(167,'Phân quyền - nhân viên','Xóa','Chức năng: 5, Nhân viên: 46','2018-07-26 09:46:20',1,'Support'),(168,'Phân quyền - nhóm','Cập nhật','Chức năng: 5, Nhóm: 1','2018-07-26 09:46:31',1,'Support'),(169,'Phân quyền - nhân viên','Cập nhật','Chức năng: 5, Nhân viên: 45','2018-07-26 09:46:31',1,'Support'),(170,'Phân quyền - nhân viên','Thêm','Chức năng: 5, Nhân viên: 1','2018-07-26 09:46:31',1,'Support'),(171,'Phân quyền - nhóm','Cập nhật','Chức năng: 5, Nhóm: 1','2018-07-26 09:46:33',1,'Support'),(172,'Phân quyền - nhân viên','Cập nhật','Chức năng: 5, Nhân viên: 45','2018-07-26 09:46:33',1,'Support'),(173,'Phân quyền - nhân viên','Thêm','Chức năng: 5, Nhân viên: 1','2018-07-26 09:46:33',1,'Support'),(174,'Phân quyền - nhân viên','Xóa','Chức năng: 1, Nhân viên: 48','2018-07-26 09:46:49',1,'Support'),(175,'Phân quyền - nhân viên','Xóa','Chức năng: 1, Nhân viên: 48','2018-07-26 09:46:49',1,'Support'),(176,'Phân quyền - nhóm','Cập nhật','Chức năng: 1, Nhóm: 1','2018-07-26 09:46:49',1,'Support'),(177,'Phân quyền - nhân viên','Xóa','Chức năng: 1, Nhân viên: 36','2018-07-26 09:46:49',1,'Support'),(178,'Phân quyền - nhân viên','Cập nhật','Chức năng: 1, Nhân viên: 47','2018-07-26 09:46:49',1,'Support'),(179,'Phân quyền - nhân viên','Cập nhật','Chức năng: 1, Nhân viên: 48','2018-07-26 09:46:49',1,'Support'),(180,'Phân quyền - nhân viên','Xóa','Chức năng: 1, Nhân viên: 48','2018-07-26 09:46:49',1,'Support'),(181,'Phân quyền - nhân viên','Xóa','Chức năng: 1, Nhân viên: 48','2018-07-26 09:46:49',1,'Support'),(182,'Phân quyền - nhân viên','Xóa','Chức năng: 1, Nhân viên: 48','2018-07-26 09:46:49',1,'Support'),(183,'Phân quyền - nhân viên','Xóa','Chức năng: 1, Nhân viên: 48','2018-07-26 09:46:49',1,'Support'),(184,'Phân quyền - nhóm','Cập nhật','Chức năng: 1, Nhóm: 1','2018-07-26 09:46:57',1,'Support'),(185,'Phân quyền - nhân viên','Cập nhật','Chức năng: 1, Nhân viên: 47','2018-07-26 09:46:57',1,'Support'),(186,'Phân quyền - nhân viên','Thêm','Chức năng: 1, Nhân viên: 1','2018-07-26 09:46:57',1,'Support'),(187,'Phân quyền - nhóm','Cập nhật','Chức năng: 2, Nhóm: 1','2018-07-26 09:47:22',1,'Support'),(188,'Phân quyền - nhân viên','Thêm','Chức năng: 2, Nhân viên: 107','2018-07-26 09:47:22',1,'Support'),(189,'Phân quyền - nhân viên','Thêm','Chức năng: 2, Nhân viên: 1','2018-07-26 09:47:22',1,'Support'),(190,'Phân quyền - nhân viên','Xóa','Chức năng: 7, Nhóm: -1','2018-07-26 09:47:56',1,'Support'),(191,'Phân quyền - nhân viên','Xóa','Chức năng: 7, Nhân viên: 27','2018-07-26 09:47:56',1,'Support'),(192,'Phân quyền - nhân viên','Xóa','Chức năng: 7, Nhân viên: 27','2018-07-26 09:47:56',1,'Support'),(193,'Phân quyền - nhóm','Thêm','Chức năng: 7, Nhóm: 1','2018-07-26 09:47:56',1,'Support'),(194,'Phân quyền - nhân viên','Thêm','Chức năng: 7, Nhân viên: 107','2018-07-26 09:47:56',1,'Support'),(195,'Phân quyền - nhân viên','Cập nhật','Chức năng: 7, Nhân viên: 27','2018-07-26 09:47:56',1,'Support'),(196,'Phân quyền - nhân viên','Xóa','Chức năng: 7, Nhân viên: 27','2018-07-26 09:47:56',1,'Support'),(197,'Phân quyền - nhân viên','Xóa','Chức năng: 7, Nhân viên: 27','2018-07-26 09:47:56',1,'Support'),(198,'Phân quyền - nhân viên','Xóa','Chức năng: 7, Nhân viên: 27','2018-07-26 09:47:56',1,'Support'),(199,'Phân quyền - nhân viên','Xóa','Chức năng: 7, Nhân viên: 27','2018-07-26 09:47:56',1,'Support'),(200,'Phân quyền - nhóm','Cập nhật','Chức năng: 7, Nhóm: 1','2018-07-26 09:48:05',1,'Support'),(201,'Phân quyền - nhân viên','Cập nhật','Chức năng: 7, Nhân viên: 54','2018-07-26 09:48:05',1,'Support'),(202,'Phân quyền - nhân viên','Thêm','Chức năng: 7, Nhân viên: 1','2018-07-26 09:48:05',1,'Support'),(203,'Phân quyền - nhân viên','Xóa','Chức năng: 6, Nhóm: -1','2018-07-26 09:48:24',1,'Support'),(204,'Phân quyền - nhân viên','Xóa','Chức năng: 6, Nhân viên: 25','2018-07-26 09:48:24',1,'Support'),(205,'Phân quyền - nhân viên','Xóa','Chức năng: 6, Nhân viên: 25','2018-07-26 09:48:24',1,'Support'),(206,'Phân quyền - nhóm','Thêm','Chức năng: 6, Nhóm: 1','2018-07-26 09:48:24',1,'Support'),(207,'Phân quyền - nhân viên','Thêm','Chức năng: 6, Nhân viên: 107','2018-07-26 09:48:24',1,'Support'),(208,'Phân quyền - nhân viên','Cập nhật','Chức năng: 6, Nhân viên: 25','2018-07-26 09:48:24',1,'Support'),(209,'Phân quyền - nhân viên','Xóa','Chức năng: 6, Nhân viên: 25','2018-07-26 09:48:24',1,'Support'),(210,'Phân quyền - nhân viên','Xóa','Chức năng: 6, Nhân viên: 25','2018-07-26 09:48:24',1,'Support'),(211,'Phân quyền - nhân viên','Xóa','Chức năng: 6, Nhân viên: 25','2018-07-26 09:48:24',1,'Support'),(212,'Phân quyền - nhân viên','Xóa','Chức năng: 6, Nhân viên: 25','2018-07-26 09:48:24',1,'Support'),(213,'Phân quyền - nhóm','Cập nhật','Chức năng: 6, Nhóm: 1','2018-07-26 09:48:33',1,'Support'),(214,'Phân quyền - nhân viên','Cập nhật','Chức năng: 6, Nhân viên: 56','2018-07-26 09:48:33',1,'Support'),(215,'Phân quyền - nhân viên','Thêm','Chức năng: 6, Nhân viên: 1','2018-07-26 09:48:33',1,'Support'),(216,'Phân quyền - nhân viên','Xóa','Chức năng: 3, Nhóm: -1','2018-07-26 09:48:59',1,'Support'),(217,'Phân quyền - nhân viên','Xóa','Chức năng: 3, Nhân viên: 6','2018-07-26 09:48:59',1,'Support'),(218,'Phân quyền - nhân viên','Xóa','Chức năng: 3, Nhân viên: 6','2018-07-26 09:48:59',1,'Support'),(219,'Phân quyền - nhóm','Thêm','Chức năng: 3, Nhóm: 1','2018-07-26 09:48:59',1,'Support'),(220,'Phân quyền - nhân viên','Thêm','Chức năng: 3, Nhân viên: 107','2018-07-26 09:48:59',1,'Support'),(221,'Phân quyền - nhân viên','Cập nhật','Chức năng: 3, Nhân viên: 6','2018-07-26 09:48:59',1,'Support'),(222,'Phân quyền - nhân viên','Xóa','Chức năng: 3, Nhân viên: 6','2018-07-26 09:48:59',1,'Support'),(223,'Phân quyền - nhân viên','Xóa','Chức năng: 3, Nhân viên: 6','2018-07-26 09:48:59',1,'Support'),(224,'Phân quyền - nhân viên','Xóa','Chức năng: 3, Nhân viên: 6','2018-07-26 09:48:59',1,'Support'),(225,'Phân quyền - nhân viên','Xóa','Chức năng: 3, Nhân viên: 6','2018-07-26 09:48:59',1,'Support'),(226,'Phân quyền - nhóm','Cập nhật','Chức năng: 3, Nhóm: 1','2018-07-26 09:49:10',1,'Support'),(227,'Phân quyền - nhân viên','Cập nhật','Chức năng: 3, Nhân viên: 58','2018-07-26 09:49:10',1,'Support'),(228,'Phân quyền - nhân viên','Thêm','Chức năng: 3, Nhân viên: 1','2018-07-26 09:49:10',1,'Support'),(229,'Phân quyền - nhân viên','Xóa','Chức năng: 4, Nhóm: -1','2018-07-26 09:49:33',1,'Support'),(230,'Phân quyền - nhân viên','Xóa','Chức năng: 4, Nhân viên: 8','2018-07-26 09:49:33',1,'Support'),(231,'Phân quyền - nhân viên','Xóa','Chức năng: 4, Nhân viên: 8','2018-07-26 09:49:33',1,'Support'),(232,'Phân quyền - nhóm','Thêm','Chức năng: 4, Nhóm: 1','2018-07-26 09:49:33',1,'Support'),(233,'Phân quyền - nhân viên','Thêm','Chức năng: 4, Nhân viên: 107','2018-07-26 09:49:33',1,'Support'),(234,'Phân quyền - nhân viên','Cập nhật','Chức năng: 4, Nhân viên: 8','2018-07-26 09:49:33',1,'Support'),(235,'Phân quyền - nhân viên','Xóa','Chức năng: 4, Nhân viên: 8','2018-07-26 09:49:33',1,'Support'),(236,'Phân quyền - nhân viên','Xóa','Chức năng: 4, Nhân viên: 8','2018-07-26 09:49:33',1,'Support'),(237,'Phân quyền - nhân viên','Xóa','Chức năng: 4, Nhân viên: 8','2018-07-26 09:49:33',1,'Support'),(238,'Phân quyền - nhân viên','Xóa','Chức năng: 4, Nhân viên: 8','2018-07-26 09:49:33',1,'Support'),(239,'Phân quyền - nhóm','Cập nhật','Chức năng: 4, Nhóm: 1','2018-07-26 09:49:41',1,'Support'),(240,'Phân quyền - nhân viên','Cập nhật','Chức năng: 4, Nhân viên: 60','2018-07-26 09:49:41',1,'Support'),(241,'Phân quyền - nhân viên','Thêm','Chức năng: 4, Nhân viên: 1','2018-07-26 09:49:41',1,'Support'),(242,'Phân quyền - nhóm','Thêm','Chức năng: 10, Nhóm: 1','2018-07-26 09:50:03',1,'Support'),(243,'Phân quyền - nhân viên','Thêm','Chức năng: 10, Nhân viên: 107','2018-07-26 09:50:03',1,'Support'),(244,'Phân quyền - nhân viên','Thêm','Chức năng: 10, Nhân viên: 1','2018-07-26 09:50:03',1,'Support'),(245,'Phân quyền - nhóm','Thêm','Chức năng: 11, Nhóm: 1','2018-07-26 09:50:24',1,'Support'),(246,'Phân quyền - nhân viên','Thêm','Chức năng: 11, Nhân viên: 107','2018-07-26 09:50:24',1,'Support'),(247,'Phân quyền - nhân viên','Thêm','Chức năng: 11, Nhân viên: 1','2018-07-26 09:50:24',1,'Support'),(248,'Phân quyền - nhóm','Thêm','Chức năng: 12, Nhóm: 1','2018-07-26 09:50:50',1,'Support'),(249,'Phân quyền - nhân viên','Thêm','Chức năng: 12, Nhân viên: 107','2018-07-26 09:50:50',1,'Support'),(250,'Phân quyền - nhân viên','Thêm','Chức năng: 12, Nhân viên: 1','2018-07-26 09:50:50',1,'Support'),(251,'Phân quyền - nhóm','Thêm','Chức năng: 11, Nhóm: 1','2018-07-26 09:50:52',1,'Support'),(252,'Phân quyền - nhân viên','Thêm','Chức năng: 11, Nhân viên: 107','2018-07-26 09:50:52',1,'Support'),(253,'Phân quyền - nhân viên','Thêm','Chức năng: 11, Nhân viên: 1','2018-07-26 09:50:52',1,'Support'),(254,'Phân quyền - nhóm','Thêm','Chức năng: 13, Nhóm: 1','2018-07-26 09:51:15',1,'Support'),(255,'Phân quyền - nhân viên','Thêm','Chức năng: 13, Nhân viên: 107','2018-07-26 09:51:15',1,'Support'),(256,'Phân quyền - nhân viên','Thêm','Chức năng: 13, Nhân viên: 1','2018-07-26 09:51:15',1,'Support'),(257,'Phân quyền - nhân viên','Xóa','Chức năng: 8, Nhóm: -1','2018-07-26 09:51:46',1,'Support'),(258,'Phân quyền - nhân viên','Xóa','Chức năng: 8, Nhân viên: 17','2018-07-26 09:51:46',1,'Support'),(259,'Phân quyền - nhân viên','Xóa','Chức năng: 8, Nhân viên: 17','2018-07-26 09:51:46',1,'Support'),(260,'Phân quyền - nhóm','Thêm','Chức năng: 8, Nhóm: 1','2018-07-26 09:51:46',1,'Support'),(261,'Phân quyền - nhân viên','Thêm','Chức năng: 8, Nhân viên: 107','2018-07-26 09:51:46',1,'Support'),(262,'Phân quyền - nhân viên','Cập nhật','Chức năng: 8, Nhân viên: 17','2018-07-26 09:51:46',1,'Support'),(263,'Phân quyền - nhân viên','Xóa','Chức năng: 8, Nhân viên: 17','2018-07-26 09:51:46',1,'Support'),(264,'Phân quyền - nhân viên','Xóa','Chức năng: 8, Nhân viên: 17','2018-07-26 09:51:46',1,'Support'),(265,'Phân quyền - nhân viên','Xóa','Chức năng: 8, Nhân viên: 17','2018-07-26 09:51:46',1,'Support'),(266,'Phân quyền - nhân viên','Xóa','Chức năng: 8, Nhân viên: 17','2018-07-26 09:51:46',1,'Support'),(267,'Phân quyền - nhân viên','Xóa','Chức năng: 9, Nhóm: -1','2018-07-26 09:52:00',1,'Support'),(268,'Phân quyền - nhân viên','Xóa','Chức năng: 9, Nhân viên: 41','2018-07-26 09:52:00',1,'Support'),(269,'Phân quyền - nhân viên','Xóa','Chức năng: 9, Nhân viên: 28','2018-07-26 09:52:00',1,'Support'),(270,'Phân quyền - nhân viên','Xóa','Chức năng: 9, Nhân viên: 41','2018-07-26 09:52:00',1,'Support'),(271,'Phân quyền - nhóm','Cập nhật','Chức năng: 9, Nhóm: 1','2018-07-26 09:52:00',1,'Support'),(272,'Phân quyền - nhân viên','Thêm','Chức năng: 9, Nhân viên: 107','2018-07-26 09:52:00',1,'Support'),(273,'Phân quyền - nhân viên','Cập nhật','Chức năng: 9, Nhân viên: 41','2018-07-26 09:52:00',1,'Support'),(274,'Phân quyền - nhân viên','Xóa','Chức năng: 9, Nhân viên: 41','2018-07-26 09:52:00',1,'Support'),(275,'Phân quyền - nhân viên','Xóa','Chức năng: 9, Nhân viên: 41','2018-07-26 09:52:00',1,'Support'),(276,'Phân quyền - nhân viên','Xóa','Chức năng: 9, Nhân viên: 41','2018-07-26 09:52:00',1,'Support'),(277,'Phân quyền - nhân viên','Xóa','Chức năng: 9, Nhân viên: 41','2018-07-26 09:52:00',1,'Support'),(278,'Phân quyền - nhóm','Cập nhật','Chức năng: 8, Nhóm: 1','2018-07-26 09:54:24',1,'Support'),(279,'Phân quyền - nhân viên','Cập nhật','Chức năng: 8, Nhân viên: 72','2018-07-26 09:54:24',1,'Support'),(280,'Phân quyền - nhân viên','Thêm','Chức năng: 8, Nhân viên: 1','2018-07-26 09:54:24',1,'Support'),(281,'Công thức phí - dạng phức hợp','Thêm','2018-08-01 Bike Guest','2018-07-28 18:11:29',1,'Support'),(282,'Công thức phí - dạng phức hợp','Thêm','2018-08-01 Car Guest','2018-07-28 18:11:44',1,'Support'),(283,'Công thức phí - dạng phức hợp','Thêm','2018-08-01 Car Delivery','2018-07-28 18:12:06',1,'Support'),(284,'Công thức phí - dạng phức hợp','Thêm','2018-08-01 Bike Delivery','2018-07-28 18:12:18',1,'Support'),(285,'Công thức phí - dạng phức hợp','Cập nhật','2018-08-01 Bike Guest','2018-07-28 18:13:16',1,'Support'),(286,'Công thức phí - dạng phức hợp','Cập nhật','2018-08-01 Car Guest','2018-07-28 18:13:46',1,'Support'),(287,'Công thức phí - dạng phức hợp','Cập nhật','2018-08-01 Car Delivery','2018-07-28 18:14:23',1,'Support'),(288,'Công thức phí - dạng phức hợp','Cập nhật','2018-08-01 Car Guest','2018-07-28 18:14:25',1,'Support'),(289,'Công thức phí - dạng phức hợp','Cập nhật','2018-08-01 Bike Delivery','2018-07-28 18:15:46',1,'Support'),(290,'Biểu phí','Thêm','loại thẻ: Thẻ vãng lai, loại xe: Xe máy, ngày hiệu lực: 2018-08-01','2018-07-28 18:16:48',1,'Support'),(291,'Biểu phí','Thêm','loại thẻ: Element, loại xe: Xe máy, ngày hiệu lực: 2018-08-01','2018-07-28 18:16:48',1,'Support'),(292,'Biểu phí','Thêm','loại thẻ: Thẻ vãng lai, loại xe: Xe Van, ngày hiệu lực: 2018-08-01','2018-07-28 18:17:13',1,'Support'),(293,'Biểu phí','Thêm','loại thẻ: Thẻ vãng lai, loại xe: Ô tô, ngày hiệu lực: 2018-08-01','2018-07-28 18:17:13',1,'Support'),(294,'Biểu phí','Thêm','loại thẻ: Element, loại xe: Xe Van, ngày hiệu lực: 2018-08-01','2018-07-28 18:17:13',1,'Support'),(295,'Biểu phí','Thêm','loại thẻ: Element, loại xe: Ô tô, ngày hiệu lực: 2018-08-01','2018-07-28 18:17:13',1,'Support'),(296,'Biểu phí','Thêm','loại thẻ: Thẻ vãng lai, loại xe: XM-giao hàng, ngày hiệu lực: 2018-08-01','2018-07-28 18:17:24',1,'Support'),(297,'Biểu phí','Thêm','loại thẻ: Element, loại xe: XM-giao hàng, ngày hiệu lực: 2018-08-01','2018-07-28 18:17:24',1,'Support'),(298,'Biểu phí','Thêm','loại thẻ: Thẻ vãng lai, loại xe: Tải giao hàng, ngày hiệu lực: 2018-08-01','2018-07-28 18:17:43',1,'Support'),(299,'Biểu phí','Thêm','loại thẻ: Element, loại xe: Tải giao hàng, ngày hiệu lực: 2018-08-01','2018-07-28 18:17:43',1,'Support'),(300,'Biểu phí','Thêm','loại thẻ: Thẻ tháng, loại xe: Xe máy, ngày hiệu lực: 2018-08-01','2018-07-28 18:34:27',1,'Support'),(301,'Biểu phí','Thêm','loại thẻ: FOC, loại xe: Xe máy, ngày hiệu lực: 2018-08-01','2018-07-28 18:34:27',1,'Support'),(302,'Biểu phí','Thêm','loại thẻ: Thẻ tháng, loại xe: Xe Van, ngày hiệu lực: 2018-08-01','2018-07-28 18:48:15',1,'Support'),(303,'Biểu phí','Thêm','loại thẻ: Thẻ tháng, loại xe: Ô tô, ngày hiệu lực: 2018-08-01','2018-07-28 18:48:15',1,'Support'),(304,'Biểu phí','Thêm','loại thẻ: FOC, loại xe: Xe Van, ngày hiệu lực: 2018-08-01','2018-07-28 18:48:15',1,'Support'),(305,'Biểu phí','Thêm','loại thẻ: FOC, loại xe: Ô tô, ngày hiệu lực: 2018-08-01','2018-07-28 18:48:15',1,'Support'),(306,'Khai báo phí','Thêm','2018-08-01 Bike Guest','2018-07-31 15:56:07',3,'Le Ngoc Dieu Thao'),(307,'Khai báo phí','Thêm','2018-08-01 Car Guest','2018-07-31 15:56:52',3,'Le Ngoc Dieu Thao'),(308,'Bảng dữ liệu: feeformula','Xóa','ID: 11','2018-07-31 15:57:16',3,'Le Ngoc Dieu Thao'),(309,'Bảng dữ liệu: feeformula','Xóa','ID: 10','2018-07-31 15:57:20',3,'Le Ngoc Dieu Thao'),(310,'Khai báo phí','Thêm','2018-08-01 Day Car Guest','2018-07-31 15:59:47',3,'Le Ngoc Dieu Thao'),(311,'Khai báo phí','Thêm','2018-08-01 Day Bike Guest','2018-07-31 16:00:04',3,'Le Ngoc Dieu Thao'),(312,'Khai báo phí','Thêm','2018-08-01 Night Bike Guest','2018-07-31 16:00:24',3,'Le Ngoc Dieu Thao'),(313,'Khai báo phí','Thêm','2018-08-01 Night Car Guest','2018-07-31 16:00:33',3,'Le Ngoc Dieu Thao'),(314,'Khai báo phí','Thêm','2018-08-01 Gold Car Guest','2018-07-31 16:00:44',3,'Le Ngoc Dieu Thao'),(315,'Khai báo phí','Thêm','2018-08-01 Gold Bike Guest','2018-07-31 16:00:51',3,'Le Ngoc Dieu Thao'),(316,'Bảng dữ liệu: feeformula','Xóa','ID: 17','2018-07-31 16:01:20',3,'Le Ngoc Dieu Thao'),(317,'Khai báo phí','Thêm','2018-08-01 Gold Bike Guest','2018-07-31 16:01:29',3,'Le Ngoc Dieu Thao'),(318,'Định mức khấu trừ','Thêm','2018-08-01 Day Car','2018-07-31 18:12:32',1,'Support'),(319,'Định mức khấu trừ','Thêm','2018-08-01 Day Bike','2018-07-31 18:13:12',1,'Support'),(320,'Bảng dữ liệu: billformula','Xóa','ID: 8','2018-07-31 18:13:39',1,'Support'),(321,'Định mức khấu trừ','Thêm','2018-08-01 Day Bike','2018-07-31 18:14:02',1,'Support'),(322,'Định mức khấu trừ','Thêm','2018-08-01 Gold Car','2018-07-31 18:14:24',1,'Support'),(323,'Định mức khấu trừ','Thêm','2018-08-01 Gold Bike','2018-07-31 18:14:34',1,'Support'),(324,'Công thức Redemption','Thêm','2018-08-01 Bike Redemption','2018-07-31 18:15:06',1,'Support'),(325,'Công thức Redemption','Thêm','2018-08-01 Car Redemption','2018-07-31 18:15:15',1,'Support'),(326,'Công thức redemption','Cập nhật','2018-08-01 Bike Redemption','2018-07-31 18:15:52',1,'Support'),(327,'Công thức redemption','Cập nhật','2018-08-01 Car Redemption','2018-07-31 18:16:13',1,'Support'),(328,'Biểu khấu trừ phí','Thêm','Nhóm cửa hàng: 5, loại xe: Xe máy, ngày hiệu lực: 2018-08-01','2018-07-31 18:16:37',1,'Support'),(329,'Biểu khấu trừ phí','Thêm','Nhóm cửa hàng: 5, loại xe: XM-giao hàng, ngày hiệu lực: 2018-08-01','2018-07-31 18:16:47',1,'Support'),(330,'Biểu khấu trừ phí','Thêm','Nhóm cửa hàng: 5, loại xe: Xe Van, ngày hiệu lực: 2018-08-01','2018-07-31 18:16:58',1,'Support'),(331,'Biểu khấu trừ phí','Thêm','Nhóm cửa hàng: 5, loại xe: Ô tô, ngày hiệu lực: 2018-08-01','2018-07-31 18:16:58',1,'Support'),(332,'Biểu khấu trừ phí','Thêm','Nhóm cửa hàng: 5, loại xe: Tải giao hàng, ngày hiệu lực: 2018-08-01','2018-07-31 18:16:58',1,'Support'),(333,'Phân quyền - nhóm','Cập nhật','Chức năng: 5, Nhóm: 1','2018-08-14 15:37:45',107,'Le Thi Thuan'),(334,'Phân quyền - nhân viên','Xóa','Chức năng: 5, Nhân viên: 76','2018-08-14 15:37:45',107,'Le Thi Thuan'),(335,'Phân quyền - nhân viên','Cập nhật','Chức năng: 5, Nhân viên: 45','2018-08-14 15:37:45',107,'Le Thi Thuan'),(336,'Phân quyền - nhóm','Cập nhật','Chức năng: 1, Nhóm: 1','2018-08-14 15:38:27',107,'Le Thi Thuan'),(337,'Phân quyền - nhân viên','Xóa','Chức năng: 1, Nhân viên: 77','2018-08-14 15:38:27',107,'Le Thi Thuan'),(338,'Phân quyền - nhân viên','Cập nhật','Chức năng: 1, Nhân viên: 47','2018-08-14 15:38:27',107,'Le Thi Thuan'),(339,'Phân quyền - nhóm','Cập nhật','Chức năng: 2, Nhóm: 1','2018-08-14 15:38:57',107,'Le Thi Thuan'),(340,'Phân quyền - nhân viên','Xóa','Chức năng: 2, Nhân viên: 78','2018-08-14 15:38:57',107,'Le Thi Thuan'),(341,'Phân quyền - nhân viên','Cập nhật','Chức năng: 2, Nhân viên: 52','2018-08-14 15:38:57',107,'Le Thi Thuan'),(342,'Phân quyền - nhóm','Cập nhật','Chức năng: 7, Nhóm: 1','2018-08-14 15:39:20',107,'Le Thi Thuan'),(343,'Phân quyền - nhân viên','Xóa','Chức năng: 7, Nhân viên: 79','2018-08-14 15:39:20',107,'Le Thi Thuan'),(344,'Phân quyền - nhân viên','Cập nhật','Chức năng: 7, Nhân viên: 54','2018-08-14 15:39:20',107,'Le Thi Thuan'),(345,'Phân quyền - nhóm','Cập nhật','Chức năng: 6, Nhóm: 1','2018-08-14 15:39:38',107,'Le Thi Thuan'),(346,'Phân quyền - nhân viên','Xóa','Chức năng: 6, Nhân viên: 80','2018-08-14 15:39:38',107,'Le Thi Thuan'),(347,'Phân quyền - nhân viên','Cập nhật','Chức năng: 6, Nhân viên: 56','2018-08-14 15:39:38',107,'Le Thi Thuan'),(348,'Phân quyền - nhóm','Cập nhật','Chức năng: 3, Nhóm: 1','2018-08-14 15:39:54',107,'Le Thi Thuan'),(349,'Phân quyền - nhân viên','Xóa','Chức năng: 3, Nhân viên: 81','2018-08-14 15:39:54',107,'Le Thi Thuan'),(350,'Phân quyền - nhân viên','Cập nhật','Chức năng: 3, Nhân viên: 58','2018-08-14 15:39:54',107,'Le Thi Thuan'),(351,'Phân quyền - nhóm','Cập nhật','Chức năng: 3, Nhóm: 1','2018-08-14 15:40:03',107,'Le Thi Thuan'),(352,'Phân quyền - nhân viên','Xóa','Chức năng: 3, Nhân viên: 81','2018-08-14 15:40:03',107,'Le Thi Thuan'),(353,'Phân quyền - nhân viên','Cập nhật','Chức năng: 3, Nhân viên: 58','2018-08-14 15:40:03',107,'Le Thi Thuan'),(354,'Phân quyền - nhóm','Cập nhật','Chức năng: 4, Nhóm: 1','2018-08-14 15:40:34',107,'Le Thi Thuan'),(355,'Phân quyền - nhân viên','Xóa','Chức năng: 4, Nhân viên: 82','2018-08-14 15:40:34',107,'Le Thi Thuan'),(356,'Phân quyền - nhân viên','Cập nhật','Chức năng: 4, Nhân viên: 60','2018-08-14 15:40:34',107,'Le Thi Thuan'),(357,'Phân quyền - nhóm','Cập nhật','Chức năng: 10, Nhóm: 1','2018-08-14 15:40:49',107,'Le Thi Thuan'),(358,'Phân quyền - nhân viên','Xóa','Chức năng: 10, Nhân viên: 83','2018-08-14 15:40:49',107,'Le Thi Thuan'),(359,'Phân quyền - nhân viên','Cập nhật','Chức năng: 10, Nhân viên: 62','2018-08-14 15:40:49',107,'Le Thi Thuan'),(360,'Phân quyền - nhóm','Cập nhật','Chức năng: 11, Nhóm: 1','2018-08-14 15:41:03',107,'Le Thi Thuan'),(361,'Phân quyền - nhân viên','Xóa','Chức năng: 11, Nhân viên: 84','2018-08-14 15:41:03',107,'Le Thi Thuan'),(362,'Phân quyền - nhân viên','Cập nhật','Chức năng: 11, Nhân viên: 64','2018-08-14 15:41:03',107,'Le Thi Thuan'),(363,'Phân quyền - nhóm','Cập nhật','Chức năng: 11, Nhóm: 1','2018-08-14 15:41:19',107,'Le Thi Thuan'),(364,'Phân quyền - nhân viên','Xóa','Chức năng: 11, Nhân viên: 86','2018-08-14 15:41:19',107,'Le Thi Thuan'),(365,'Phân quyền - nhân viên','Cập nhật','Chức năng: 11, Nhân viên: 64','2018-08-14 15:41:19',107,'Le Thi Thuan'),(366,'Phân quyền - nhóm','Cập nhật','Chức năng: 10, Nhóm: 1','2018-08-14 15:41:31',107,'Le Thi Thuan'),(367,'Phân quyền - nhân viên','Cập nhật','Chức năng: 10, Nhân viên: 62','2018-08-14 15:41:31',107,'Le Thi Thuan'),(368,'Phân quyền - nhóm','Cập nhật','Chức năng: 12, Nhóm: 1','2018-08-14 15:41:42',107,'Le Thi Thuan'),(369,'Phân quyền - nhân viên','Xóa','Chức năng: 12, Nhân viên: 85','2018-08-14 15:41:42',107,'Le Thi Thuan'),(370,'Phân quyền - nhân viên','Cập nhật','Chức năng: 12, Nhân viên: 66','2018-08-14 15:41:42',107,'Le Thi Thuan'),(371,'Phân quyền - nhóm','Cập nhật','Chức năng: 13, Nhóm: 1','2018-08-14 15:41:54',107,'Le Thi Thuan'),(372,'Phân quyền - nhân viên','Xóa','Chức năng: 13, Nhân viên: 88','2018-08-14 15:41:54',107,'Le Thi Thuan'),(373,'Phân quyền - nhân viên','Cập nhật','Chức năng: 13, Nhân viên: 70','2018-08-14 15:41:54',107,'Le Thi Thuan'),(374,'Phân quyền - nhóm','Cập nhật','Chức năng: 8, Nhóm: 1','2018-08-14 15:42:12',107,'Le Thi Thuan'),(375,'Phân quyền - nhân viên','Xóa','Chức năng: 8, Nhân viên: 87','2018-08-14 15:42:12',107,'Le Thi Thuan'),(376,'Phân quyền - nhân viên','Cập nhật','Chức năng: 8, Nhân viên: 72','2018-08-14 15:42:12',107,'Le Thi Thuan'),(377,'Phân quyền - nhóm','Cập nhật','Chức năng: 9, Nhóm: 1','2018-08-14 15:42:24',107,'Le Thi Thuan'),(378,'Phân quyền - nhân viên','Cập nhật','Chức năng: 9, Nhân viên: 73','2018-08-14 15:42:24',107,'Le Thi Thuan'),(379,'Khai báo phí','Thêm','Bike Test','2018-12-12 18:46:27',107,'Le Thi Thuan'),(380,'Khai báo phí','Thêm','2019-test','2018-12-13 00:20:35',1,'Support'),(381,'Định mức phí','Xóa','Bike Test','2019-03-09 10:32:15',107,'Le Thi Thuan'),(382,'Định mức phí','Xóa','2019-test','2019-03-09 10:32:17',107,'Le Thi Thuan'),(383,'Khai báo phí','Thêm','2019-04-01 Day Bike Guest','2019-03-09 10:33:25',107,'Le Thi Thuan'),(384,'Khai báo phí','Thêm','2019-04-01 Night Bike Guest','2019-03-09 10:33:57',107,'Le Thi Thuan'),(385,'Khai báo phí','Thêm','2019-04-01 Gold Bike Guest','2019-03-09 10:34:08',107,'Le Thi Thuan'),(386,'Khai báo phí','Thêm','2019-04-01 Day Car Guest','2019-03-09 10:34:55',107,'Le Thi Thuan'),(387,'Khai báo phí','Thêm','2019-04-01 Night Car Guest','2019-03-09 10:35:07',107,'Le Thi Thuan'),(388,'Khai báo phí','Thêm','2019-04-01 Gold Car Guest','2019-03-09 10:35:28',107,'Le Thi Thuan'),(389,'Công thức phí - dạng phức hợp','Thêm','2019-04-01 Bike Guest','2019-03-09 10:37:52',107,'Le Thi Thuan'),(390,'Công thức phí - dạng phức hợp','Thêm','2019-04-01 Car Guest','2019-03-09 10:38:00',107,'Le Thi Thuan'),(391,'Công thức phí - dạng phức hợp','Thêm','2019-04-01 Bike Delivery','2019-03-09 10:38:53',107,'Le Thi Thuan'),(392,'Công thức phí - dạng phức hợp','Thêm','2019-04-01 Car Delivery','2019-03-09 10:39:12',107,'Le Thi Thuan'),(393,'Công thức phí - dạng phức hợp','Cập nhật','2019-04-01 Bike Guest','2019-03-09 10:41:23',107,'Le Thi Thuan'),(394,'Công thức phí - dạng phức hợp','Cập nhật','2019-04-01 Car Guest','2019-03-09 10:42:40',107,'Le Thi Thuan'),(395,'Công thức phí - dạng phức hợp','Cập nhật','2019-04-01 Bike Delivery','2019-03-09 10:44:21',107,'Le Thi Thuan'),(396,'Công thức phí - dạng phức hợp','Cập nhật','2019-04-01 Car Delivery','2019-03-09 10:44:53',107,'Le Thi Thuan'),(397,'Biểu phí','Thêm','Loại thẻ: Thẻ vãng lai, Loại xe: Xe máy, Ngày hiệu lực: 2019-04-01','2019-03-09 10:50:07',107,'Le Thi Thuan'),(398,'Định mức khấu trừ','Thêm','2019-04-01 Day Bike','2019-03-09 10:53:11',107,'Le Thi Thuan'),(399,'Định mức khấu trừ','Thêm','2019-04-01 Night Bike','2019-03-09 10:53:50',107,'Le Thi Thuan'),(400,'Định mức khấu trừ','Thêm','2019-04-01 Day Car','2019-03-09 10:58:58',107,'Le Thi Thuan'),(401,'Định mức khấu trừ','Xóa','2019-04-01 Night Bike','2019-03-09 10:59:15',107,'Le Thi Thuan'),(402,'Định mức khấu trừ','Thêm','2019-04-01 Gold Car','2019-03-09 10:59:41',107,'Le Thi Thuan'),(403,'Định mức khấu trừ','Thêm','2019-04-01 Gold Bike','2019-03-09 11:00:09',107,'Le Thi Thuan'),(404,'Công thức khấu trừ','Thêm','2019-04-01','2019-03-09 11:02:30',107,'Le Thi Thuan'),(405,'Công thức khấu trừ','Xóa','2019-04-01','2019-03-09 11:02:35',107,'Le Thi Thuan'),(406,'Công thức khấu trừ','Thêm','2019-04-01 Bike Redemption','2019-03-09 11:02:51',107,'Le Thi Thuan'),(407,'Công thức khấu trừ','Thêm','2019-04-01 Car Redemption','2019-03-09 11:02:56',107,'Le Thi Thuan'),(408,'Công thức khấu trừ','Cập nhật','2019-04-01 Bike Redemption','2019-03-09 11:04:35',107,'Le Thi Thuan'),(409,'Công thức khấu trừ','Cập nhật','2019-04-01 Car Redemption','2019-03-09 11:06:37',107,'Le Thi Thuan'),(410,'Biểu phí','Thêm','Loại thẻ: Thẻ vãng lai, Loại xe: XM-giao hàng, Ngày hiệu lực: 2019-04-01','2019-03-09 11:09:39',107,'Le Thi Thuan'),(411,'Biểu phí','Thêm','Loại thẻ: Thẻ vãng lai, Loại xe: Xe Van, Ngày hiệu lực: 2019-04-01','2019-03-09 11:10:00',107,'Le Thi Thuan'),(412,'Biểu phí','Thêm','Loại thẻ: Thẻ vãng lai, Loại xe: Ô tô, Ngày hiệu lực: 2019-04-01','2019-03-09 11:10:00',107,'Le Thi Thuan'),(413,'Biểu phí','Thêm','Loại thẻ: Thẻ vãng lai, Loại xe: Tải giao hàng, Ngày hiệu lực: 2019-04-01','2019-03-09 11:10:10',107,'Le Thi Thuan'),(414,'Biểu phí','Thêm','Loại thẻ: Thẻ tháng, Loại xe: Xe máy, Ngày hiệu lực: 2019-04-01','2019-03-09 11:10:37',107,'Le Thi Thuan'),(415,'Biểu phí','Thêm','Loại thẻ: Thẻ tháng, Loại xe: Xe Van, Ngày hiệu lực: 2019-04-01','2019-03-09 11:10:48',107,'Le Thi Thuan'),(416,'Biểu phí','Thêm','Loại thẻ: Thẻ tháng, Loại xe: Ô tô, Ngày hiệu lực: 2019-04-01','2019-03-09 11:10:48',107,'Le Thi Thuan'),(417,'Biểu phí','Thêm','Loại thẻ: FOC, Loại xe: Xe máy, Ngày hiệu lực: 2019-04-01','2019-03-09 11:11:06',107,'Le Thi Thuan'),(418,'Biểu phí','Thêm','Loại thẻ: FOC, Loại xe: Xe Van, Ngày hiệu lực: 2019-04-01','2019-03-09 11:11:21',107,'Le Thi Thuan'),(419,'Biểu phí','Thêm','Loại thẻ: FOC, Loại xe: Ô tô, Ngày hiệu lực: 2019-04-01','2019-03-09 11:11:21',107,'Le Thi Thuan'),(420,'Biểu phí','Thêm','Loại thẻ: FOC, Loại xe: Tải giao hàng, Ngày hiệu lực: 2019-04-01','2019-03-09 11:11:21',107,'Le Thi Thuan'),(421,'Biểu phí','Thêm','Loại thẻ: Thẻ tháng, Loại xe: Tải giao hàng, Ngày hiệu lực: 2019-04-01','2019-03-09 11:11:50',107,'Le Thi Thuan'),(422,'Công thức khấu trừ','Xóa','2019-04-01 Bike Redemption','2019-03-09 11:14:13',107,'Le Thi Thuan'),(423,'Công thức khấu trừ','Xóa','2019-04-01 Car Redemption','2019-03-09 11:14:15',107,'Le Thi Thuan'),(424,'Định mức khấu trừ','Xóa','2019-04-01 Day Bike','2019-03-09 11:14:23',107,'Le Thi Thuan'),(425,'Định mức khấu trừ','Xóa','2019-04-01 Day Car','2019-03-09 11:14:24',107,'Le Thi Thuan'),(426,'Định mức khấu trừ','Thêm','2019-04-01 Day Bike','2019-03-09 11:15:12',107,'Le Thi Thuan'),(427,'Định mức khấu trừ','Thêm','2019-04-01 Day Car','2019-03-09 11:16:30',107,'Le Thi Thuan'),(428,'Công thức khấu trừ','Thêm','2019-04-01 Bike Redemption','2019-03-09 11:18:55',107,'Le Thi Thuan'),(429,'Công thức khấu trừ','Thêm','2019-04-01 Car Redemption','2019-03-09 11:19:04',107,'Le Thi Thuan'),(430,'Định mức khấu trừ','Xóa','2019-04-01 Gold Car','2019-03-09 11:22:13',107,'Le Thi Thuan'),(431,'Định mức khấu trừ','Xóa','2019-04-01 Gold Bike','2019-03-09 11:22:18',107,'Le Thi Thuan'),(432,'Định mức khấu trừ','Thêm','2019-04-01 Gold Bike','2019-03-09 11:23:00',107,'Le Thi Thuan'),(433,'Định mức khấu trừ','Thêm','2019-04-01 Gold Car','2019-03-09 11:23:21',107,'Le Thi Thuan'),(434,'Công thức khấu trừ','Cập nhật','2019-04-01 Bike Redemption','2019-03-09 11:24:22',107,'Le Thi Thuan'),(435,'Công thức khấu trừ','Cập nhật','2019-04-01 Car Redemption','2019-03-09 11:25:08',107,'Le Thi Thuan'),(436,'Biểu khấu trừ phí','Thêm','Nhóm cửa hàng: All, Loại xe: Xe máy, Ngày hiệu lực: 2019-04-01','2019-03-09 11:25:27',107,'Le Thi Thuan'),(437,'Biểu khấu trừ phí','Thêm','Nhóm cửa hàng: All, Loại xe: XM-giao hàng, Ngày hiệu lực: 2019-04-01','2019-03-09 11:25:27',107,'Le Thi Thuan'),(438,'Biểu khấu trừ phí','Thêm','Nhóm cửa hàng: All, Loại xe: Xe Van, Ngày hiệu lực: 2019-04-01','2019-03-09 11:25:35',107,'Le Thi Thuan'),(439,'Biểu khấu trừ phí','Thêm','Nhóm cửa hàng: All, Loại xe: Ô tô, Ngày hiệu lực: 2019-04-01','2019-03-09 11:25:35',107,'Le Thi Thuan'),(440,'Biểu khấu trừ phí','Thêm','Nhóm cửa hàng: All, Loại xe: Tải giao hàng, Ngày hiệu lực: 2019-04-01','2019-03-09 11:25:35',107,'Le Thi Thuan'),(441,'Biểu phí','Thêm','Loại thẻ: Element, Loại xe: Xe máy, Ngày hiệu lực: 2019-04-01','2019-04-19 16:46:09',107,'Le Thi Thuan'),(442,'Biểu phí','Thêm','Loại thẻ: Element, Loại xe: Xe Van, Ngày hiệu lực: 2019-04-01','2019-04-19 16:46:43',107,'Le Thi Thuan'),(443,'Biểu phí','Thêm','Loại thẻ: Element, Loại xe: Ô tô, Ngày hiệu lực: 2019-04-01','2019-04-19 16:46:43',107,'Le Thi Thuan'),(444,'Biểu phí','Thêm','Loại thẻ: Element, Loại xe: XM-giao hàng, Ngày hiệu lực: 2019-04-01','2019-04-19 16:47:06',107,'Le Thi Thuan'),(445,'Biểu phí','Thêm','Loại thẻ: Element, Loại xe: Tải giao hàng, Ngày hiệu lực: 2019-04-01','2019-04-19 16:48:05',107,'Le Thi Thuan'),(446,'Phân quyền - nhóm','Cập nhật','Chức năng: Khai báo phí, Nhóm: Others','2019-05-02 11:38:46',107,'Le Thi Thuan'),(447,'Phân quyền - nhân viên','Cập nhật','Chức năng: Khai báo phí, Nhân viên: Le Thi Thuan','2019-05-02 11:38:46',107,'Le Thi Thuan'),(448,'Phân quyền - nhân viên','Thêm','Chức năng: Khai báo phí, Nhân viên: Nguyen Ba Hoc','2019-05-02 11:38:46',107,'Le Thi Thuan'),(449,'Phân quyền - nhóm','Thêm','Chức năng: Khai báo phí, Nhóm: Parking Staff','2019-05-02 11:38:46',107,'Le Thi Thuan'),(450,'Phân quyền - nhân viên','Thêm','Chức năng: Khai báo phí, Nhân viên: Le Ngoc Dieu Thao','2019-05-02 11:38:46',107,'Le Thi Thuan'),(451,'Phân quyền - nhóm','Cập nhật','Chức năng: Khai báo phí, Nhóm: Others','2019-05-02 11:38:51',107,'Le Thi Thuan'),(452,'Phân quyền - nhân viên','Cập nhật','Chức năng: Khai báo phí, Nhân viên: Le Thi Thuan','2019-05-02 11:38:51',107,'Le Thi Thuan'),(453,'Phân quyền - nhân viên','Thêm','Chức năng: Khai báo phí, Nhân viên: Nguyen Ba Hoc','2019-05-02 11:38:51',107,'Le Thi Thuan'),(454,'Phân quyền - nhóm','Thêm','Chức năng: Khai báo phí, Nhóm: Parking Staff','2019-05-02 11:38:51',107,'Le Thi Thuan'),(455,'Phân quyền - nhân viên','Thêm','Chức năng: Khai báo phí, Nhân viên: Le Ngoc Dieu Thao','2019-05-02 11:38:51',107,'Le Thi Thuan'),(456,'Phân quyền - nhóm','Cập nhật','Chức năng: Công thức phí, Nhóm: Others','2019-05-02 11:38:57',107,'Le Thi Thuan'),(457,'Phân quyền - nhân viên','Cập nhật','Chức năng: Công thức phí, Nhân viên: Le Thi Thuan','2019-05-02 11:38:57',107,'Le Thi Thuan'),(458,'Phân quyền - nhân viên','Thêm','Chức năng: Công thức phí, Nhân viên: Nguyen Ba Hoc','2019-05-02 11:38:57',107,'Le Thi Thuan'),(459,'Phân quyền - nhóm','Thêm','Chức năng: Công thức phí, Nhóm: Parking Staff','2019-05-02 11:38:57',107,'Le Thi Thuan'),(460,'Phân quyền - nhân viên','Thêm','Chức năng: Công thức phí, Nhân viên: Le Ngoc Dieu Thao','2019-05-02 11:38:57',107,'Le Thi Thuan'),(461,'Phân quyền - nhóm','Cập nhật','Chức năng: Biểu phí, Nhóm: Others','2019-05-02 11:39:33',107,'Le Thi Thuan'),(462,'Phân quyền - nhân viên','Cập nhật','Chức năng: Biểu phí, Nhân viên: Le Thi Thuan','2019-05-02 11:39:33',107,'Le Thi Thuan'),(463,'Phân quyền - nhân viên','Thêm','Chức năng: Biểu phí, Nhân viên: Nguyen Ba Hoc','2019-05-02 11:39:33',107,'Le Thi Thuan'),(464,'Phân quyền - nhóm','Thêm','Chức năng: Biểu phí, Nhóm: Parking Staff','2019-05-02 11:39:33',107,'Le Thi Thuan'),(465,'Phân quyền - nhân viên','Thêm','Chức năng: Biểu phí, Nhân viên: Le Ngoc Dieu Thao','2019-05-02 11:39:33',107,'Le Thi Thuan'),(466,'Phân quyền - nhóm','Cập nhật','Chức năng: Nhóm đối tác, cửa hàng, Nhóm: Others','2019-05-02 11:40:25',107,'Le Thi Thuan'),(467,'Phân quyền - nhân viên','Cập nhật','Chức năng: Nhóm đối tác, cửa hàng, Nhân viên: Le Thi Thuan','2019-05-02 11:40:25',107,'Le Thi Thuan'),(468,'Phân quyền - nhân viên','Thêm','Chức năng: Nhóm đối tác, cửa hàng, Nhân viên: Nguyen Ba Hoc','2019-05-02 11:40:25',107,'Le Thi Thuan'),(469,'Phân quyền - nhóm','Thêm','Chức năng: Nhóm đối tác, cửa hàng, Nhóm: Parking Staff','2019-05-02 11:40:25',107,'Le Thi Thuan'),(470,'Phân quyền - nhân viên','Thêm','Chức năng: Nhóm đối tác, cửa hàng, Nhân viên: Le Ngoc Dieu Thao','2019-05-02 11:40:25',107,'Le Thi Thuan'),(471,'Phân quyền - nhóm','Cập nhật','Chức năng: Định mức khấu trừ, Nhóm: Others','2019-05-02 11:41:04',107,'Le Thi Thuan'),(472,'Phân quyền - nhân viên','Cập nhật','Chức năng: Định mức khấu trừ, Nhân viên: Le Thi Thuan','2019-05-02 11:41:04',107,'Le Thi Thuan'),(473,'Phân quyền - nhân viên','Thêm','Chức năng: Định mức khấu trừ, Nhân viên: Nguyen Ba Hoc','2019-05-02 11:41:04',107,'Le Thi Thuan'),(474,'Phân quyền - nhóm','Thêm','Chức năng: Định mức khấu trừ, Nhóm: Parking Staff','2019-05-02 11:41:04',107,'Le Thi Thuan'),(475,'Phân quyền - nhân viên','Thêm','Chức năng: Định mức khấu trừ, Nhân viên: Le Ngoc Dieu Thao','2019-05-02 11:41:04',107,'Le Thi Thuan'),(476,'Phân quyền - nhóm','Cập nhật','Chức năng: Biểu khấu trừ phí, Nhóm: Others','2019-05-02 11:42:47',107,'Le Thi Thuan'),(477,'Phân quyền - nhân viên','Cập nhật','Chức năng: Biểu khấu trừ phí, Nhân viên: Le Thi Thuan','2019-05-02 11:42:47',107,'Le Thi Thuan'),(478,'Phân quyền - nhân viên','Thêm','Chức năng: Biểu khấu trừ phí, Nhân viên: Nguyen Ba Hoc','2019-05-02 11:42:47',107,'Le Thi Thuan'),(479,'Phân quyền - nhóm','Thêm','Chức năng: Biểu khấu trừ phí, Nhóm: Parking Staff','2019-05-02 11:42:47',107,'Le Thi Thuan'),(480,'Phân quyền - nhân viên','Thêm','Chức năng: Biểu khấu trừ phí, Nhân viên: Le Ngoc Dieu Thao','2019-05-02 11:42:47',107,'Le Thi Thuan'),(481,'Phân quyền - nhóm','Cập nhật','Chức năng: Nhóm đối tác, cửa hàng, Nhóm: Others','2019-05-02 11:42:57',107,'Le Thi Thuan'),(482,'Phân quyền - nhân viên','Cập nhật','Chức năng: Nhóm đối tác, cửa hàng, Nhân viên: Le Thi Thuan','2019-05-02 11:42:57',107,'Le Thi Thuan'),(483,'Phân quyền - nhân viên','Thêm','Chức năng: Nhóm đối tác, cửa hàng, Nhân viên: Nguyen Ba Hoc','2019-05-02 11:42:57',107,'Le Thi Thuan'),(484,'Phân quyền - nhóm','Thêm','Chức năng: Nhóm đối tác, cửa hàng, Nhóm: Parking Staff','2019-05-02 11:42:57',107,'Le Thi Thuan'),(485,'Phân quyền - nhân viên','Thêm','Chức năng: Nhóm đối tác, cửa hàng, Nhân viên: Le Ngoc Dieu Thao','2019-05-02 11:42:57',107,'Le Thi Thuan'),(486,'Phân quyền - nhóm','Cập nhật','Chức năng: Kiểm tra công thức, Nhóm: Others','2019-05-02 11:54:01',107,'Le Thi Thuan'),(487,'Phân quyền - nhân viên','Cập nhật','Chức năng: Kiểm tra công thức, Nhân viên: Le Thi Thuan','2019-05-02 11:54:01',107,'Le Thi Thuan'),(488,'Phân quyền - nhân viên','Thêm','Chức năng: Kiểm tra công thức, Nhân viên: Nguyen Ba Hoc','2019-05-02 11:54:01',107,'Le Thi Thuan'),(489,'Phân quyền - nhóm','Thêm','Chức năng: Kiểm tra công thức, Nhóm: Parking Staff','2019-05-02 11:54:01',107,'Le Thi Thuan'),(490,'Phân quyền - nhân viên','Thêm','Chức năng: Kiểm tra công thức, Nhân viên: Le Ngoc Dieu Thao','2019-05-02 11:54:01',107,'Le Thi Thuan'),(491,'Phân quyền - nhóm','Cập nhật','Chức năng: Báo cáo phí vãng lai, Nhóm: Others','2019-05-02 11:54:56',107,'Le Thi Thuan'),(492,'Phân quyền - nhân viên','Cập nhật','Chức năng: Báo cáo phí vãng lai, Nhân viên: Le Thi Thuan','2019-05-02 11:54:56',107,'Le Thi Thuan'),(493,'Phân quyền - nhân viên','Thêm','Chức năng: Báo cáo phí vãng lai, Nhân viên: Nguyen Ba Hoc','2019-05-02 11:54:56',107,'Le Thi Thuan'),(494,'Phân quyền - nhóm','Thêm','Chức năng: Báo cáo phí vãng lai, Nhóm: Parking Staff','2019-05-02 11:54:56',107,'Le Thi Thuan'),(495,'Phân quyền - nhân viên','Thêm','Chức năng: Báo cáo phí vãng lai, Nhân viên: Le Ngoc Dieu Thao','2019-05-02 11:54:56',107,'Le Thi Thuan'),(496,'Phân quyền - nhóm','Cập nhật','Chức năng: Báo cáo redemption, Nhóm: Others','2019-05-02 11:55:29',107,'Le Thi Thuan'),(497,'Phân quyền - nhân viên','Cập nhật','Chức năng: Báo cáo redemption, Nhân viên: Le Thi Thuan','2019-05-02 11:55:29',107,'Le Thi Thuan'),(498,'Phân quyền - nhân viên','Thêm','Chức năng: Báo cáo redemption, Nhân viên: Nguyen Ba Hoc','2019-05-02 11:55:29',107,'Le Thi Thuan'),(499,'Phân quyền - nhóm','Thêm','Chức năng: Báo cáo redemption, Nhóm: Parking Staff','2019-05-02 11:55:29',107,'Le Thi Thuan'),(500,'Phân quyền - nhân viên','Thêm','Chức năng: Báo cáo redemption, Nhân viên: Le Ngoc Dieu Thao','2019-05-02 11:55:29',107,'Le Thi Thuan'),(501,'Phân quyền - nhóm','Cập nhật','Chức năng: Báo cáo lịch sử tác động, Nhóm: Others','2019-05-02 11:56:06',107,'Le Thi Thuan'),(502,'Phân quyền - nhân viên','Cập nhật','Chức năng: Báo cáo lịch sử tác động, Nhân viên: Le Thi Thuan','2019-05-02 11:56:06',107,'Le Thi Thuan'),(503,'Phân quyền - nhân viên','Thêm','Chức năng: Báo cáo lịch sử tác động, Nhân viên: Nguyen Ba Hoc','2019-05-02 11:56:06',107,'Le Thi Thuan'),(504,'Phân quyền - nhóm','Thêm','Chức năng: Báo cáo lịch sử tác động, Nhóm: Parking Staff','2019-05-02 11:56:06',107,'Le Thi Thuan'),(505,'Phân quyền - nhân viên','Thêm','Chức năng: Báo cáo lịch sử tác động, Nhân viên: Le Ngoc Dieu Thao','2019-05-02 11:56:06',107,'Le Thi Thuan'),(506,'Phân quyền - nhóm','Cập nhật','Chức năng: Báo cáo quân quyền, Nhóm: Others','2019-05-02 11:56:52',107,'Le Thi Thuan'),(507,'Phân quyền - nhân viên','Cập nhật','Chức năng: Báo cáo quân quyền, Nhân viên: Le Thi Thuan','2019-05-02 11:56:52',107,'Le Thi Thuan'),(508,'Phân quyền - nhân viên','Thêm','Chức năng: Báo cáo quân quyền, Nhân viên: Nguyen Ba Hoc','2019-05-02 11:56:52',107,'Le Thi Thuan'),(509,'Phân quyền - nhóm','Thêm','Chức năng: Báo cáo quân quyền, Nhóm: Parking Staff','2019-05-02 11:56:52',107,'Le Thi Thuan'),(510,'Phân quyền - nhóm','Cập nhật','Chức năng: Ngày đặc biệt, Nhóm: Others','2019-05-02 11:57:18',107,'Le Thi Thuan'),(511,'Phân quyền - nhân viên','Cập nhật','Chức năng: Ngày đặc biệt, Nhân viên: Le Thi Thuan','2019-05-02 11:57:18',107,'Le Thi Thuan'),(512,'Phân quyền - nhân viên','Thêm','Chức năng: Ngày đặc biệt, Nhân viên: Nguyen Ba Hoc','2019-05-02 11:57:18',107,'Le Thi Thuan'),(513,'Phân quyền - nhóm','Thêm','Chức năng: Ngày đặc biệt, Nhóm: Parking Staff','2019-05-02 11:57:18',107,'Le Thi Thuan'),(514,'Phân quyền - nhân viên','Thêm','Chức năng: Ngày đặc biệt, Nhân viên: Le Ngoc Dieu Thao','2019-05-02 11:57:18',107,'Le Thi Thuan'),(515,'Phân quyền - nhóm','Cập nhật','Chức năng: Phân quyền, Nhóm: Others','2019-05-06 17:13:13',107,'Le Thi Thuan'),(516,'Phân quyền - nhân viên','Thêm','Chức năng: Phân quyền, Nhân viên: Le Thi Thuan','2019-05-06 17:13:13',107,'Le Thi Thuan'),(517,'Phân quyền - nhóm','Thêm','Chức năng: Phân quyền, Nhóm: Parking Staff','2019-05-06 17:13:13',107,'Le Thi Thuan'),(518,'Phân quyền - nhân viên','Thêm','Chức năng: Phân quyền, Nhân viên: Le Ngoc Dieu Thao','2019-05-06 17:13:13',107,'Le Thi Thuan'),(519,'Phân quyền - nhóm','Cập nhật','Chức năng: Phân quyền, Nhóm: Others','2019-05-23 11:48:29',3,'Le Ngoc Dieu Thao'),(520,'Phân quyền - nhân viên','Thêm','Chức năng: Phân quyền, Nhân viên: Nguyen Ba Hoc','2019-05-23 11:48:29',3,'Le Ngoc Dieu Thao'),(521,'Phân quyền - nhóm','Cập nhật','Chức năng: Phân quyền, Nhóm: Parking Staff','2019-05-23 11:48:29',3,'Le Ngoc Dieu Thao'),(522,'Phân quyền - nhân viên','Thêm','Chức năng: Phân quyền, Nhân viên: Le Ngoc Dieu Thao','2019-05-23 11:48:29',3,'Le Ngoc Dieu Thao'),(523,'Khai báo phí','Thêm','2019-06-01 Day Bike Guest','2019-05-24 15:00:39',3,'Le Ngoc Dieu Thao'),(524,'Khai báo phí','Thêm','2019-06-01 Night Bike Guest','2019-05-24 15:01:28',3,'Le Ngoc Dieu Thao'),(525,'Khai báo phí','Thêm','2019-06-01 Gold Bike Guest','2019-05-24 15:01:45',3,'Le Ngoc Dieu Thao'),(526,'Khai báo phí','Thêm','2019-06-01 Day Car Guest','2019-05-24 15:02:02',3,'Le Ngoc Dieu Thao'),(527,'Khai báo phí','Thêm','2019-06-01 Night Car Guest','2019-05-24 15:02:19',3,'Le Ngoc Dieu Thao'),(528,'Khai báo phí','Thêm','2019-06-01 Gold Car Guest','2019-05-24 15:02:38',3,'Le Ngoc Dieu Thao'),(529,'Công thức phí - dạng phức hợp','Thêm','2019-06-01 Bike Guest','2019-05-24 15:04:12',3,'Le Ngoc Dieu Thao'),(530,'Công thức phí - dạng phức hợp','Cập nhật','2019-06-01 Bike Guest','2019-05-24 15:05:39',3,'Le Ngoc Dieu Thao'),(531,'Công thức phí - dạng phức hợp','Thêm','2019-06-01 Car Guest','2019-05-24 15:06:09',3,'Le Ngoc Dieu Thao'),(532,'Công thức phí - dạng phức hợp','Cập nhật','2019-06-01 Car Guest','2019-05-24 15:07:10',3,'Le Ngoc Dieu Thao'),(533,'Công thức phí - dạng phức hợp','Thêm','2019-06-01 Car Delivery','2019-05-24 15:07:47',3,'Le Ngoc Dieu Thao'),(534,'Công thức phí - dạng phức hợp','Cập nhật','2019-06-01 Car Delivery','2019-05-24 15:08:47',3,'Le Ngoc Dieu Thao'),(535,'Công thức phí - dạng phức hợp','Thêm','2019-06-01 Bike Delivery','2019-05-24 15:09:10',3,'Le Ngoc Dieu Thao'),(536,'Công thức phí - dạng phức hợp','Cập nhật','2019-06-01 Bike Delivery','2019-05-24 15:10:03',3,'Le Ngoc Dieu Thao'),(537,'Biểu phí','Thêm','Loại thẻ: Thẻ vãng lai, Loại xe: Xe máy, Ngày hiệu lực: 2019-06-01','2019-05-24 15:10:46',3,'Le Ngoc Dieu Thao'),(538,'Biểu phí','Thêm','Loại thẻ: Thẻ vãng lai, Loại xe: XM-giao hàng, Ngày hiệu lực: 2019-06-01','2019-05-24 15:10:57',3,'Le Ngoc Dieu Thao'),(539,'Biểu phí','Thêm','Loại thẻ: Thẻ vãng lai, Loại xe: Xe Van, Ngày hiệu lực: 2019-06-01','2019-05-24 15:11:08',3,'Le Ngoc Dieu Thao'),(540,'Biểu phí','Thêm','Loại thẻ: Thẻ vãng lai, Loại xe: Ô tô, Ngày hiệu lực: 2019-06-01','2019-05-24 15:11:08',3,'Le Ngoc Dieu Thao'),(541,'Biểu phí','Thêm','Loại thẻ: Thẻ vãng lai, Loại xe: Tải giao hàng, Ngày hiệu lực: 2019-06-01','2019-05-24 15:11:18',3,'Le Ngoc Dieu Thao'),(542,'Biểu phí','Thêm','Loại thẻ: Thẻ tháng, Loại xe: Xe máy, Ngày hiệu lực: 2019-06-01','2019-05-24 15:11:47',3,'Le Ngoc Dieu Thao'),(543,'Biểu phí','Thêm','Loại thẻ: Thẻ tháng, Loại xe: XM-giao hàng, Ngày hiệu lực: 2019-06-01','2019-05-24 15:11:47',3,'Le Ngoc Dieu Thao'),(544,'Biểu phí','Thêm','Loại thẻ: Thẻ tháng, Loại xe: Xe Van, Ngày hiệu lực: 2019-06-01','2019-05-24 15:12:12',3,'Le Ngoc Dieu Thao'),(545,'Biểu phí','Thêm','Loại thẻ: Thẻ tháng, Loại xe: Ô tô, Ngày hiệu lực: 2019-06-01','2019-05-24 15:12:12',3,'Le Ngoc Dieu Thao'),(546,'Biểu phí','Thêm','Loại thẻ: Thẻ tháng, Loại xe: Tải giao hàng, Ngày hiệu lực: 2019-06-01','2019-05-24 15:12:12',3,'Le Ngoc Dieu Thao'),(547,'Biểu phí','Thêm','Loại thẻ: FOC, Loại xe: Xe máy, Ngày hiệu lực: 2019-06-01','2019-05-24 15:12:30',3,'Le Ngoc Dieu Thao'),(548,'Biểu phí','Thêm','Loại thẻ: FOC, Loại xe: XM-giao hàng, Ngày hiệu lực: 2019-06-01','2019-05-24 15:12:30',3,'Le Ngoc Dieu Thao'),(549,'Biểu phí','Thêm','Loại thẻ: FOC, Loại xe: Xe Van, Ngày hiệu lực: 2019-06-01','2019-05-24 15:12:44',3,'Le Ngoc Dieu Thao'),(550,'Biểu phí','Thêm','Loại thẻ: FOC, Loại xe: Ô tô, Ngày hiệu lực: 2019-06-01','2019-05-24 15:12:44',3,'Le Ngoc Dieu Thao'),(551,'Biểu phí','Thêm','Loại thẻ: FOC, Loại xe: Tải giao hàng, Ngày hiệu lực: 2019-06-01','2019-05-24 15:12:44',3,'Le Ngoc Dieu Thao'),(552,'Biểu phí','Thêm','Loại thẻ: Element, Loại xe: Xe máy, Ngày hiệu lực: 2019-06-01','2019-05-24 15:13:45',3,'Le Ngoc Dieu Thao'),(553,'Biểu phí','Thêm','Loại thẻ: Element, Loại xe: Xe Van, Ngày hiệu lực: 2019-06-01','2019-05-24 15:14:02',3,'Le Ngoc Dieu Thao'),(554,'Biểu phí','Thêm','Loại thẻ: Element, Loại xe: Ô tô, Ngày hiệu lực: 2019-06-01','2019-05-24 15:14:02',3,'Le Ngoc Dieu Thao'),(555,'Biểu phí','Thêm','Loại thẻ: Element, Loại xe: XM-giao hàng, Ngày hiệu lực: 2019-06-01','2019-05-24 15:14:12',3,'Le Ngoc Dieu Thao'),(556,'Biểu phí','Thêm','Loại thẻ: Element, Loại xe: Tải giao hàng, Ngày hiệu lực: 2019-06-01','2019-05-24 15:14:21',3,'Le Ngoc Dieu Thao'),(557,'Định mức khấu trừ','Thêm','2019-06-01 Day Bike','2019-05-24 15:16:51',3,'Le Ngoc Dieu Thao'),(558,'Định mức khấu trừ','Thêm','2019-06-01 Gold Bike','2019-05-24 15:17:26',3,'Le Ngoc Dieu Thao'),(559,'Định mức khấu trừ','Thêm','2019-06-01 Gold Car','2019-05-24 15:17:45',3,'Le Ngoc Dieu Thao'),(560,'Định mức khấu trừ','Thêm','2019-06-01 Day Car','2019-05-24 15:18:18',3,'Le Ngoc Dieu Thao'),(561,'Công thức khấu trừ','Thêm','2019-06-01 Bike Redemption','2019-05-24 15:19:22',3,'Le Ngoc Dieu Thao'),(562,'Công thức khấu trừ','Cập nhật','2019-06-01 Bike Redemption','2019-05-24 15:20:29',3,'Le Ngoc Dieu Thao'),(563,'Công thức khấu trừ','Thêm','2019-06-01 Car Redemption','2019-05-24 15:20:49',3,'Le Ngoc Dieu Thao'),(564,'Công thức khấu trừ','Cập nhật','2019-06-01 Car Redemption','2019-05-24 15:21:26',3,'Le Ngoc Dieu Thao'),(565,'Biểu khấu trừ phí','Thêm','Nhóm cửa hàng: All, Loại xe: Xe máy, Ngày hiệu lực: 2019-06-01','2019-05-24 15:21:49',3,'Le Ngoc Dieu Thao'),(566,'Biểu khấu trừ phí','Thêm','Nhóm cửa hàng: All, Loại xe: XM-giao hàng, Ngày hiệu lực: 2019-06-01','2019-05-24 15:21:49',3,'Le Ngoc Dieu Thao'),(567,'Biểu khấu trừ phí','Thêm','Nhóm cửa hàng: All, Loại xe: Ô tô, Ngày hiệu lực: 2019-06-01','2019-05-24 15:22:06',3,'Le Ngoc Dieu Thao'),(568,'Biểu khấu trừ phí','Thêm','Nhóm cửa hàng: All, Loại xe: Tải giao hàng, Ngày hiệu lực: 2019-06-01','2019-05-24 15:22:06',3,'Le Ngoc Dieu Thao'),(569,'Biểu khấu trừ phí','Thêm','Nhóm cửa hàng: All, Loại xe: Xe Van, Ngày hiệu lực: 2019-06-01','2019-05-24 15:22:47',3,'Le Ngoc Dieu Thao'),(570,'Công thức phí - dạng phức hợp','Thêm','2018-06-01  Bike Guest','2019-06-01 07:45:24',3,'Le Ngoc Dieu Thao'),(571,'Công thức phí - dạng phức hợp','Cập nhật','2018-06-01  Bike Guest','2019-06-01 07:47:20',3,'Le Ngoc Dieu Thao'),(572,'Công thức phí - dạng phức hợp','Cập nhật','2018-06-01  Bike Guest','2019-06-01 07:54:31',3,'Le Ngoc Dieu Thao'),(573,'Công thức phí - dạng phức hợp','Cập nhật','2018-06-01  Bike Guest','2019-06-01 07:54:40',3,'Le Ngoc Dieu Thao'),(574,'Biểu phí','Thêm','Loại thẻ: Thẻ tháng, Loại xe: Xe máy, Ngày hiệu lực: 2019-07-02','2019-07-02 13:58:05',3,'Le Ngoc Dieu Thao'),(575,'Biểu phí','Thêm','Loại thẻ: FOC, Loại xe: Xe máy, Ngày hiệu lực: 2019-07-02','2019-07-02 13:58:05',3,'Le Ngoc Dieu Thao'),(576,'Phân quyền - nhân viên','Xóa','Chức năng: Khai báo phí, Nhân viên: Support','2019-12-20 11:30:45',111,'Nguyen Ba Hoc'),(577,'Phân quyền - nhóm','Cập nhật','Chức năng: Khai báo phí, Nhóm: Others','2019-12-20 11:30:45',111,'Nguyen Ba Hoc'),(578,'Phân quyền - nhân viên','Cập nhật','Chức năng: Khai báo phí, Nhân viên: Nguyen Ba Hoc','2019-12-20 11:30:45',111,'Nguyen Ba Hoc'),(579,'Phân quyền - nhóm','Cập nhật','Chức năng: Khai báo phí, Nhóm: Parking Staff','2019-12-20 11:30:45',111,'Nguyen Ba Hoc'),(580,'Phân quyền - nhân viên','Thêm','Chức năng: Khai báo phí, Nhân viên: Tran Thi Bich Tram','2019-12-20 11:30:45',111,'Nguyen Ba Hoc'),(581,'Phân quyền - nhân viên','Xóa','Chức năng: Công thức phí, Nhân viên: Support','2019-12-20 11:30:47',111,'Nguyen Ba Hoc'),(582,'Phân quyền - nhóm','Cập nhật','Chức năng: Công thức phí, Nhóm: Others','2019-12-20 11:30:47',111,'Nguyen Ba Hoc'),(583,'Phân quyền - nhân viên','Cập nhật','Chức năng: Công thức phí, Nhân viên: Nguyen Ba Hoc','2019-12-20 11:30:47',111,'Nguyen Ba Hoc'),(584,'Phân quyền - nhóm','Cập nhật','Chức năng: Công thức phí, Nhóm: Parking Staff','2019-12-20 11:30:47',111,'Nguyen Ba Hoc'),(585,'Phân quyền - nhân viên','Thêm','Chức năng: Công thức phí, Nhân viên: Tran Thi Bich Tram','2019-12-20 11:30:47',111,'Nguyen Ba Hoc'),(586,'Phân quyền - nhân viên','Xóa','Chức năng: Biểu phí, Nhân viên: Support','2019-12-20 11:30:49',111,'Nguyen Ba Hoc'),(587,'Phân quyền - nhóm','Cập nhật','Chức năng: Biểu phí, Nhóm: Others','2019-12-20 11:30:49',111,'Nguyen Ba Hoc'),(588,'Phân quyền - nhân viên','Cập nhật','Chức năng: Biểu phí, Nhân viên: Nguyen Ba Hoc','2019-12-20 11:30:49',111,'Nguyen Ba Hoc'),(589,'Phân quyền - nhóm','Cập nhật','Chức năng: Biểu phí, Nhóm: Parking Staff','2019-12-20 11:30:49',111,'Nguyen Ba Hoc'),(590,'Phân quyền - nhân viên','Thêm','Chức năng: Biểu phí, Nhân viên: Tran Thi Bich Tram','2019-12-20 11:30:49',111,'Nguyen Ba Hoc'),(591,'Phân quyền - nhân viên','Xóa','Chức năng: Biểu phí, Nhân viên: Support','2019-12-20 11:30:54',111,'Nguyen Ba Hoc'),(592,'Phân quyền - nhóm','Cập nhật','Chức năng: Biểu phí, Nhóm: Others','2019-12-20 11:30:54',111,'Nguyen Ba Hoc'),(593,'Phân quyền - nhân viên','Cập nhật','Chức năng: Biểu phí, Nhân viên: Nguyen Ba Hoc','2019-12-20 11:30:54',111,'Nguyen Ba Hoc'),(594,'Phân quyền - nhóm','Cập nhật','Chức năng: Biểu phí, Nhóm: Parking Staff','2019-12-20 11:30:54',111,'Nguyen Ba Hoc'),(595,'Phân quyền - nhân viên','Thêm','Chức năng: Biểu phí, Nhân viên: Tran Thi Bich Tram','2019-12-20 11:30:54',111,'Nguyen Ba Hoc'),(596,'Phân quyền - nhân viên','Xóa','Chức năng: Nhóm đối tác, cửa hàng, Nhân viên: Support','2019-12-20 11:30:56',111,'Nguyen Ba Hoc'),(597,'Phân quyền - nhóm','Cập nhật','Chức năng: Nhóm đối tác, cửa hàng, Nhóm: Others','2019-12-20 11:30:56',111,'Nguyen Ba Hoc'),(598,'Phân quyền - nhân viên','Cập nhật','Chức năng: Nhóm đối tác, cửa hàng, Nhân viên: Nguyen Ba Hoc','2019-12-20 11:30:56',111,'Nguyen Ba Hoc'),(599,'Phân quyền - nhóm','Cập nhật','Chức năng: Nhóm đối tác, cửa hàng, Nhóm: Parking Staff','2019-12-20 11:30:56',111,'Nguyen Ba Hoc'),(600,'Phân quyền - nhân viên','Thêm','Chức năng: Nhóm đối tác, cửa hàng, Nhân viên: Tran Thi Bich Tram','2019-12-20 11:30:56',111,'Nguyen Ba Hoc'),(601,'Phân quyền - nhân viên','Xóa','Chức năng: Định mức khấu trừ, Nhân viên: Support','2019-12-20 11:31:05',111,'Nguyen Ba Hoc'),(602,'Phân quyền - nhóm','Cập nhật','Chức năng: Định mức khấu trừ, Nhóm: Others','2019-12-20 11:31:05',111,'Nguyen Ba Hoc'),(603,'Phân quyền - nhân viên','Cập nhật','Chức năng: Định mức khấu trừ, Nhân viên: Nguyen Ba Hoc','2019-12-20 11:31:05',111,'Nguyen Ba Hoc'),(604,'Phân quyền - nhóm','Cập nhật','Chức năng: Định mức khấu trừ, Nhóm: Parking Staff','2019-12-20 11:31:05',111,'Nguyen Ba Hoc'),(605,'Phân quyền - nhân viên','Thêm','Chức năng: Định mức khấu trừ, Nhân viên: Tran Thi Bich Tram','2019-12-20 11:31:05',111,'Nguyen Ba Hoc'),(606,'Phân quyền - nhân viên','Xóa','Chức năng: Biểu khấu trừ phí, Nhân viên: Support','2019-12-20 11:31:13',111,'Nguyen Ba Hoc'),(607,'Phân quyền - nhóm','Cập nhật','Chức năng: Biểu khấu trừ phí, Nhóm: Others','2019-12-20 11:31:13',111,'Nguyen Ba Hoc'),(608,'Phân quyền - nhân viên','Cập nhật','Chức năng: Biểu khấu trừ phí, Nhân viên: Nguyen Ba Hoc','2019-12-20 11:31:13',111,'Nguyen Ba Hoc'),(609,'Phân quyền - nhóm','Cập nhật','Chức năng: Biểu khấu trừ phí, Nhóm: Parking Staff','2019-12-20 11:31:13',111,'Nguyen Ba Hoc'),(610,'Phân quyền - nhân viên','Thêm','Chức năng: Biểu khấu trừ phí, Nhân viên: Tran Thi Bich Tram','2019-12-20 11:31:13',111,'Nguyen Ba Hoc'),(611,'Phân quyền - nhân viên','Xóa','Chức năng: Kiểm tra công thức, Nhân viên: Support','2019-12-20 11:31:20',111,'Nguyen Ba Hoc'),(612,'Phân quyền - nhóm','Cập nhật','Chức năng: Kiểm tra công thức, Nhóm: Others','2019-12-20 11:31:20',111,'Nguyen Ba Hoc'),(613,'Phân quyền - nhân viên','Cập nhật','Chức năng: Kiểm tra công thức, Nhân viên: Nguyen Ba Hoc','2019-12-20 11:31:20',111,'Nguyen Ba Hoc'),(614,'Phân quyền - nhóm','Cập nhật','Chức năng: Kiểm tra công thức, Nhóm: Parking Staff','2019-12-20 11:31:20',111,'Nguyen Ba Hoc'),(615,'Phân quyền - nhân viên','Thêm','Chức năng: Kiểm tra công thức, Nhân viên: Tran Thi Bich Tram','2019-12-20 11:31:20',111,'Nguyen Ba Hoc'),(616,'Phân quyền - nhân viên','Xóa','Chức năng: Báo cáo phí vãng lai, Nhân viên: Support','2019-12-20 11:31:26',111,'Nguyen Ba Hoc'),(617,'Phân quyền - nhóm','Cập nhật','Chức năng: Báo cáo phí vãng lai, Nhóm: Others','2019-12-20 11:31:26',111,'Nguyen Ba Hoc'),(618,'Phân quyền - nhân viên','Cập nhật','Chức năng: Báo cáo phí vãng lai, Nhân viên: Nguyen Ba Hoc','2019-12-20 11:31:26',111,'Nguyen Ba Hoc'),(619,'Phân quyền - nhóm','Cập nhật','Chức năng: Báo cáo phí vãng lai, Nhóm: Parking Staff','2019-12-20 11:31:26',111,'Nguyen Ba Hoc'),(620,'Phân quyền - nhân viên','Thêm','Chức năng: Báo cáo phí vãng lai, Nhân viên: Tran Thi Bich Tram','2019-12-20 11:31:26',111,'Nguyen Ba Hoc'),(621,'Phân quyền - nhân viên','Xóa','Chức năng: Báo cáo redemption, Nhân viên: Support','2019-12-20 11:31:35',111,'Nguyen Ba Hoc'),(622,'Phân quyền - nhóm','Cập nhật','Chức năng: Báo cáo redemption, Nhóm: Others','2019-12-20 11:31:35',111,'Nguyen Ba Hoc'),(623,'Phân quyền - nhân viên','Cập nhật','Chức năng: Báo cáo redemption, Nhân viên: Nguyen Ba Hoc','2019-12-20 11:31:35',111,'Nguyen Ba Hoc'),(624,'Phân quyền - nhóm','Cập nhật','Chức năng: Báo cáo redemption, Nhóm: Parking Staff','2019-12-20 11:31:35',111,'Nguyen Ba Hoc'),(625,'Phân quyền - nhân viên','Thêm','Chức năng: Báo cáo redemption, Nhân viên: Tran Thi Bich Tram','2019-12-20 11:31:35',111,'Nguyen Ba Hoc'),(626,'Phân quyền - nhân viên','Xóa','Chức năng: Báo cáo lịch sử tác động, Nhân viên: Support','2019-12-20 11:31:41',111,'Nguyen Ba Hoc'),(627,'Phân quyền - nhóm','Cập nhật','Chức năng: Báo cáo lịch sử tác động, Nhóm: Others','2019-12-20 11:31:41',111,'Nguyen Ba Hoc'),(628,'Phân quyền - nhân viên','Cập nhật','Chức năng: Báo cáo lịch sử tác động, Nhân viên: Nguyen Ba Hoc','2019-12-20 11:31:41',111,'Nguyen Ba Hoc'),(629,'Phân quyền - nhóm','Cập nhật','Chức năng: Báo cáo lịch sử tác động, Nhóm: Parking Staff','2019-12-20 11:31:41',111,'Nguyen Ba Hoc'),(630,'Phân quyền - nhân viên','Thêm','Chức năng: Báo cáo lịch sử tác động, Nhân viên: Tran Thi Bich Tram','2019-12-20 11:31:41',111,'Nguyen Ba Hoc'),(631,'Phân quyền - nhân viên','Xóa','Chức năng: Báo cáo quân quyền, Nhân viên: Support','2019-12-20 11:31:48',111,'Nguyen Ba Hoc'),(632,'Phân quyền - nhóm','Cập nhật','Chức năng: Báo cáo quân quyền, Nhóm: Others','2019-12-20 11:31:48',111,'Nguyen Ba Hoc'),(633,'Phân quyền - nhân viên','Cập nhật','Chức năng: Báo cáo quân quyền, Nhân viên: Nguyen Ba Hoc','2019-12-20 11:31:48',111,'Nguyen Ba Hoc'),(634,'Phân quyền - nhóm','Cập nhật','Chức năng: Báo cáo quân quyền, Nhóm: Parking Staff','2019-12-20 11:31:48',111,'Nguyen Ba Hoc'),(635,'Phân quyền - nhân viên','Thêm','Chức năng: Báo cáo quân quyền, Nhân viên: Tran Thi Bich Tram','2019-12-20 11:31:48',111,'Nguyen Ba Hoc'),(636,'Phân quyền - nhân viên','Xóa','Chức năng: Ngày đặc biệt, Nhân viên: Support','2019-12-20 11:31:56',111,'Nguyen Ba Hoc'),(637,'Phân quyền - nhóm','Cập nhật','Chức năng: Ngày đặc biệt, Nhóm: Others','2019-12-20 11:31:56',111,'Nguyen Ba Hoc'),(638,'Phân quyền - nhân viên','Cập nhật','Chức năng: Ngày đặc biệt, Nhân viên: Nguyen Ba Hoc','2019-12-20 11:31:56',111,'Nguyen Ba Hoc'),(639,'Phân quyền - nhóm','Cập nhật','Chức năng: Ngày đặc biệt, Nhóm: Parking Staff','2019-12-20 11:31:56',111,'Nguyen Ba Hoc'),(640,'Phân quyền - nhân viên','Thêm','Chức năng: Ngày đặc biệt, Nhân viên: Tran Thi Bich Tram','2019-12-20 11:31:56',111,'Nguyen Ba Hoc'),(641,'Định mức khấu trừ','Thêm','Rewards Member','2019-12-20 11:35:50',36,'Tran Thi Bich Tram'),(642,'Công thức khấu trừ','Thêm','Rewards Member','2019-12-20 11:36:16',36,'Tran Thi Bich Tram'),(643,'Công thức khấu trừ','Cập nhật','Rewards Member','2019-12-20 11:36:51',36,'Tran Thi Bich Tram'),(644,'Biểu khấu trừ phí','Thêm','Nhóm cửa hàng: Rewards Member, Loại xe: Xe máy, Ngày hiệu lực: 2019-12-20','2019-12-20 11:37:07',36,'Tran Thi Bich Tram'),(645,'Công thức phí - dạng phức hợp','Thêm','2020-02-24 Bike Guest','2020-02-21 14:42:58',36,'Tran Thi Bich Tram'),(646,'Công thức phí - dạng phức hợp','Thêm','2020-02-24 Bike Guest','2020-02-21 14:42:58',36,'Tran Thi Bich Tram'),(647,'Công thức phí - dạng phức hợp','Cập nhật','2020-02-24 Bike Guest','2020-02-21 14:44:52',36,'Tran Thi Bich Tram'),(648,'Biểu phí','Thêm','Loại thẻ: Thẻ vãng lai, Loại xe: Xe máy, Ngày hiệu lực: 2020-02-24','2020-02-21 14:45:23',36,'Tran Thi Bich Tram'),(649,'Biểu phí','Xóa','Loại thẻ: Thẻ vãng lai,Loại xe: Xe máy, Ngày hiệu lực: 2020-02-24','2020-02-21 14:47:06',36,'Tran Thi Bich Tram'),(650,'Biểu phí','Thêm','Loại thẻ: Thẻ vãng lai, Loại xe: Xe máy, Ngày hiệu lực: 2020-02-24','2020-02-21 14:47:12',36,'Tran Thi Bich Tram'),(651,'Công thức phí - dạng phức hợp','Cập nhật','2020-02-24 Bike Guest','2020-02-21 14:48:54',36,'Tran Thi Bich Tram'),(652,'Công thức phí - dạng phức hợp','Cập nhật','2020-02-24 Bike Guest','2020-02-21 15:06:45',36,'Tran Thi Bich Tram'),(653,'Khai báo phí','Thêm','2020-02-24 Gold Bike Guest','2020-02-21 15:12:37',36,'Tran Thi Bich Tram'),(654,'Biểu phí','Xóa','Loại thẻ: Thẻ vãng lai,Loại xe: Xe máy, Ngày hiệu lực: 2020-02-24','2020-02-21 15:12:57',36,'Tran Thi Bich Tram'),(655,'Công thức phí','Xóa','2020-02-24 Bike Guest','2020-02-21 15:13:01',36,'Tran Thi Bich Tram'),(656,'Công thức phí - dạng phức hợp','Thêm','2020-02-24 Bike Guest','2020-02-21 15:13:12',36,'Tran Thi Bich Tram'),(657,'Công thức phí - dạng phức hợp','Thêm','2020-02-24 Bike Guest','2020-02-21 15:13:12',36,'Tran Thi Bich Tram'),(658,'Công thức phí - dạng phức hợp','Cập nhật','2020-02-24 Bike Guest','2020-02-21 15:15:13',36,'Tran Thi Bich Tram'),(659,'Biểu phí','Thêm','Loại thẻ: Thẻ vãng lai, Loại xe: Xe máy, Ngày hiệu lực: 2020-02-24','2020-02-21 15:15:37',36,'Tran Thi Bich Tram'),(660,'Biểu phí','Xóa','Loại thẻ: Thẻ vãng lai,Loại xe: Xe máy, Ngày hiệu lực: 2020-02-24','2020-02-21 15:19:05',36,'Tran Thi Bich Tram'),(661,'Công thức phí','Xóa','2020-02-24 Bike Guest','2020-02-21 15:19:08',36,'Tran Thi Bich Tram'),(662,'Công thức phí - dạng phức hợp','Thêm','2020-02-24 Bike Guest','2020-02-21 15:19:16',36,'Tran Thi Bich Tram'),(663,'Công thức phí - dạng phức hợp','Thêm','2020-02-24 Bike Guest','2020-02-21 15:19:16',36,'Tran Thi Bich Tram'),(664,'Công thức phí - dạng phức hợp','Cập nhật','2020-02-24 Bike Guest','2020-02-21 15:21:38',36,'Tran Thi Bich Tram'),(665,'Công thức phí - dạng phức hợp','Thêm','2020-02-24 Car Guest','2020-02-21 15:21:50',36,'Tran Thi Bich Tram'),(666,'Công thức phí - dạng phức hợp','Thêm','2020-02-24 Car Guest','2020-02-21 15:21:50',36,'Tran Thi Bich Tram'),(667,'Công thức phí - dạng phức hợp','Cập nhật','2020-02-24 Car Guest','2020-02-21 15:24:02',36,'Tran Thi Bich Tram'),(668,'Biểu phí','Thêm','Loại thẻ: Thẻ vãng lai, Loại xe: Ô tô, Ngày hiệu lực: 2020-02-24','2020-02-21 15:24:54',36,'Tran Thi Bich Tram'),(669,'Biểu phí','Xóa','Loại thẻ: Thẻ vãng lai,Loại xe: Ô tô, Ngày hiệu lực: 2020-02-24','2020-02-21 15:27:01',36,'Tran Thi Bich Tram'),(670,'Biểu phí','Thêm','Loại thẻ: Thẻ vãng lai, Loại xe: Xe máy, Ngày hiệu lực: 2020-02-25','2020-02-21 15:27:21',36,'Tran Thi Bich Tram'),(671,'Biểu phí','Thêm','Loại thẻ: Thẻ vãng lai, Loại xe: Ô tô, Ngày hiệu lực: 2020-02-25','2020-02-21 15:27:41',36,'Tran Thi Bich Tram'),(672,'Biểu phí','Xóa','Loại thẻ: Thẻ vãng lai,Loại xe: Xe máy, Ngày hiệu lực: 2020-02-25','2020-02-24 16:05:14',36,'Tran Thi Bich Tram'),(673,'Biểu phí','Xóa','Loại thẻ: Thẻ vãng lai,Loại xe: Ô tô, Ngày hiệu lực: 2020-02-25','2020-02-24 16:05:38',36,'Tran Thi Bich Tram'),(674,'Công thức phí','Xóa','2020-02-','2020-02-25 09:08:45',36,'Tran Thi Bich Tram'),(675,'Công thức phí - dạng phức hợp','Thêm','2020-02-25 Test','2020-02-25 09:09:44',36,'Tran Thi Bich Tram'),(676,'Công thức phí - dạng phức hợp','Cập nhật','2020-02-25 Test','2020-02-25 09:12:26',36,'Tran Thi Bich Tram'),(677,'Biểu phí','Thêm','Loại thẻ: Thẻ vãng lai, Loại xe: Xe máy, Ngày hiệu lực: 2020-02-29','2020-02-25 09:12:45',36,'Tran Thi Bich Tram'),(678,'Công thức phí - dạng phức hợp','Sao chép','2020-02-25 Test_2020-02-25 09:14:47','2020-02-25 09:14:47',36,'Tran Thi Bich Tram'),(679,'Công thức phí - dạng phức hợp','Cập nhật','2020-02-25 Test_2020-02-25 09:14:47','2020-02-25 09:16:09',36,'Tran Thi Bich Tram'),(680,'Biểu phí','Xóa','Loại thẻ: Thẻ vãng lai,Loại xe: Xe máy, Ngày hiệu lực: 2020-02-29','2020-02-25 09:16:19',36,'Tran Thi Bich Tram'),(681,'Biểu phí','Thêm','Loại thẻ: Thẻ vãng lai, Loại xe: Xe máy, Ngày hiệu lực: 2020-02-29','2020-02-25 09:16:30',36,'Tran Thi Bich Tram'),(682,'Biểu phí','Xóa','Loại thẻ: Thẻ vãng lai,Loại xe: Xe máy, Ngày hiệu lực: 2020-02-29','2020-02-25 09:20:28',36,'Tran Thi Bich Tram'),(683,'Biểu phí','Thêm','Loại thẻ: Thẻ vãng lai, Loại xe: Xe máy, Ngày hiệu lực: 2020-02-29','2020-02-25 09:20:51',36,'Tran Thi Bich Tram'),(684,'Biểu phí','Xóa','Loại thẻ: Thẻ vãng lai,Loại xe: Xe máy, Ngày hiệu lực: 2020-02-29','2020-02-25 09:21:15',36,'Tran Thi Bich Tram'),(685,'Công thức phí','Xóa','2020-02-25 Test_2020-02-25 09:14:47','2020-02-25 09:21:19',36,'Tran Thi Bich Tram'),(686,'Công thức phí','Xóa','2020-02-25 Test','2020-02-25 09:21:22',36,'Tran Thi Bich Tram'),(687,'Khai báo phí','Thêm','2020-02-25 Bike Day Weekend','2020-02-25 09:23:16',36,'Tran Thi Bich Tram'),(688,'Công thức phí - dạng phức hợp','Thêm','2020-02-25 test','2020-02-25 09:29:34',36,'Tran Thi Bich Tram'),(689,'Công thức phí - dạng phức hợp','Thêm','2020-02-25 test','2020-02-25 09:29:34',36,'Tran Thi Bich Tram'),(690,'Công thức phí - dạng phức hợp','Cập nhật','2020-02-25 test','2020-02-25 09:32:26',36,'Tran Thi Bich Tram'),(691,'Biểu phí','Thêm','Loại thẻ: Thẻ vãng lai, Loại xe: Xe máy, Ngày hiệu lực: 2020-02-29','2020-02-25 09:32:41',36,'Tran Thi Bich Tram'),(692,'Công thức phí - dạng phức hợp','Sao chép','2020-02-25 test_2020-02-25 09:51:47','2020-02-25 09:51:47',36,'Tran Thi Bich Tram'),(693,'Công thức phí','Xóa','2020-02-25 test_2020-02-25 09:51:47','2020-02-25 09:52:00',36,'Tran Thi Bich Tram'),(694,'Công thức phí - dạng phức hợp','Thêm','2020-02-25 test2','2020-02-25 09:52:33',36,'Tran Thi Bich Tram'),(695,'Công thức phí - dạng phức hợp','Thêm','2020-02-25 test2','2020-02-25 09:52:33',36,'Tran Thi Bich Tram'),(696,'Công thức phí','Xóa','2020-02-25 test2','2020-02-25 09:53:45',36,'Tran Thi Bich Tram'),(697,'Công thức phí - dạng phức hợp','Thêm','2020-02-25 test2','2020-02-25 09:53:52',36,'Tran Thi Bich Tram'),(698,'Công thức phí - dạng phức hợp','Thêm','2020-02-25 test2','2020-02-25 09:53:52',36,'Tran Thi Bich Tram'),(699,'Công thức phí - dạng phức hợp','Cập nhật','2020-02-25 test2','2020-02-25 09:56:27',36,'Tran Thi Bich Tram'),(700,'Biểu phí','Xóa','Loại thẻ: Thẻ vãng lai,Loại xe: Xe máy, Ngày hiệu lực: 2020-02-29','2020-02-25 09:56:31',36,'Tran Thi Bich Tram'),(701,'Biểu phí','Thêm','Loại thẻ: Thẻ vãng lai, Loại xe: Xe máy, Ngày hiệu lực: 2020-02-29','2020-02-25 09:56:47',36,'Tran Thi Bich Tram'),(702,'Biểu phí','Xóa','Loại thẻ: Thẻ vãng lai,Loại xe: Xe máy, Ngày hiệu lực: 2020-02-29','2020-02-25 09:59:58',36,'Tran Thi Bich Tram'),(703,'Khai báo phí','Thêm','2020-02-25 Bike Day Weekend 2','2020-02-25 10:01:28',36,'Tran Thi Bich Tram'),(704,'Công thức phí - dạng phức hợp','Sao chép','2020-02-25 test2_2020-02-25 10:01:42','2020-02-25 10:01:42',36,'Tran Thi Bich Tram'),(705,'Công thức phí - dạng phức hợp','Cập nhật','2020-02-25 test2_2020-02-25 10:01:42','2020-02-25 10:01:50',36,'Tran Thi Bich Tram'),(706,'Biểu phí','Thêm','Loại thẻ: Thẻ vãng lai, Loại xe: Xe máy, Ngày hiệu lực: 2020-02-29','2020-02-25 10:01:57',36,'Tran Thi Bich Tram'),(707,'Biểu phí','Xóa','Loại thẻ: Thẻ vãng lai,Loại xe: Xe máy, Ngày hiệu lực: 2020-02-29','2020-02-25 10:03:28',36,'Tran Thi Bich Tram'),(708,'Biểu phí','Thêm','Loại thẻ: Thẻ vãng lai, Loại xe: Xe máy, Ngày hiệu lực: 2020-02-29','2020-02-25 10:03:37',36,'Tran Thi Bich Tram'),(709,'Biểu phí','Xóa','Loại thẻ: Thẻ vãng lai,Loại xe: Xe máy, Ngày hiệu lực: 2020-02-29','2020-02-25 10:11:51',36,'Tran Thi Bich Tram'),(710,'Công thức phí','Xóa','2020-02-25 test','2020-02-25 10:12:49',36,'Tran Thi Bich Tram'),(711,'Công thức phí','Xóa','2020-02-25 test2','2020-02-25 10:13:12',36,'Tran Thi Bich Tram'),(712,'Công thức phí - dạng phức hợp','Sao chép','2020-02-25 Bike Free 4h Weekend','2020-02-25 10:13:43',36,'Tran Thi Bich Tram'),(713,'Công thức phí','Xóa','2020-02-25 test2_2020-02-25 10:01:42','2020-02-25 10:13:45',36,'Tran Thi Bich Tram'),(714,'Công thức phí - dạng phức hợp','Thêm','2020-02-25 Bike Weekend with Gold time','2020-02-25 10:14:56',36,'Tran Thi Bich Tram'),(715,'Công thức phí - dạng phức hợp','Thêm','2020-02-25 Bike Weekend with Gold time','2020-02-25 10:14:56',36,'Tran Thi Bich Tram'),(716,'Công thức phí - dạng phức hợp','Cập nhật','2020-02-25 Bike Weekend with Gold time','2020-02-25 10:17:43',36,'Tran Thi Bich Tram'),(717,'Biểu phí','Thêm','Loại thẻ: Thẻ vãng lai, Loại xe: Xe máy, Ngày hiệu lực: 2020-02-29','2020-02-25 10:18:04',36,'Tran Thi Bich Tram'),(718,'Khai báo phí','Thêm','2020-02-25 Car Weekend','2020-02-25 10:52:35',36,'Tran Thi Bich Tram'),(719,'Khai báo phí','Thêm','2020-02-25 Car Day Weekend','2020-02-25 10:52:50',36,'Tran Thi Bich Tram'),(720,'Định mức phí','Xóa','2020-02-25 Car Weekend','2020-02-25 10:52:52',36,'Tran Thi Bich Tram'),(721,'Công thức phí - dạng phức hợp','Thêm','2020-02-25 Car Weekend with Gold time','2020-02-25 10:53:17',36,'Tran Thi Bich Tram'),(722,'Công thức phí - dạng phức hợp','Thêm','2020-02-25 Car Weekend with Gold time','2020-02-25 10:53:17',36,'Tran Thi Bich Tram'),(723,'Công thức phí - dạng phức hợp','Cập nhật','2020-02-25 Car Weekend with Gold time','2020-02-25 10:55:39',36,'Tran Thi Bich Tram'),(724,'Biểu phí','Thêm','Loại thẻ: Thẻ vãng lai, Loại xe: Ô tô, Ngày hiệu lực: 2020-02-29','2020-02-25 10:56:12',36,'Tran Thi Bich Tram'),(725,'Công thức phí - dạng phức hợp','Cập nhật','2020-02-25 Car Weekend with Gold time','2020-02-25 11:05:58',36,'Tran Thi Bich Tram'),(726,'Công thức phí - dạng phức hợp','Cập nhật','2020-02-25 Car Weekend with Gold time','2020-02-25 11:05:58',36,'Tran Thi Bich Tram'),(727,'Công thức phí - dạng phức hợp','Cập nhật','2020-02-25 Car Weekend with Gold time','2020-02-25 11:08:25',36,'Tran Thi Bich Tram'),(728,'Công thức phí - dạng phức hợp','Cập nhật','2020-02-25 Car Weekend with Gold time','2020-02-25 11:18:06',36,'Tran Thi Bich Tram'),(729,'Công thức phí - dạng phức hợp','Thêm','2020-02-25 Car Weekend with Gold time 2','2020-02-25 11:19:13',36,'Tran Thi Bich Tram'),(730,'Công thức phí - dạng phức hợp','Thêm','2020-02-25 Car Weekend with Gold time 2','2020-02-25 11:19:13',36,'Tran Thi Bich Tram'),(731,'Công thức phí - dạng phức hợp','Cập nhật','2020-02-25 Car Weekend with Gold time 2','2020-02-25 11:21:06',36,'Tran Thi Bich Tram'),(732,'Biểu phí','Xóa','Loại thẻ: Thẻ vãng lai,Loại xe: Ô tô, Ngày hiệu lực: 2020-02-29','2020-02-25 11:21:11',36,'Tran Thi Bich Tram'),(733,'Biểu phí','Thêm','Loại thẻ: Thẻ vãng lai, Loại xe: Ô tô, Ngày hiệu lực: 2020-02-29','2020-02-25 11:21:26',36,'Tran Thi Bich Tram'),(734,'Công thức phí - dạng phức hợp','Cập nhật','2020-02-25 Car Weekend with Gold time 2','2020-02-25 11:27:09',36,'Tran Thi Bich Tram'),(735,'Công thức phí - dạng phức hợp','Cập nhật','2020-02-25 Car Weekend with Gold time 2','2020-02-25 11:31:29',36,'Tran Thi Bich Tram'),(736,'Công thức phí - dạng phức hợp','Cập nhật','2020-02-25 Car Weekend with Gold time 2','2020-02-25 11:36:53',36,'Tran Thi Bich Tram'),(737,'Công thức phí - dạng phức hợp','Cập nhật','2020-02-25 Car Weekend with Gold time 2','2020-02-25 11:37:32',36,'Tran Thi Bich Tram'),(738,'Công thức phí - dạng phức hợp','Thêm','2020-02-25 Bike Weekend with Gold time 2','2020-02-25 11:50:25',36,'Tran Thi Bich Tram'),(739,'Công thức phí - dạng phức hợp','Thêm','2020-02-25 Bike Weekend with Gold time 2','2020-02-25 11:50:25',36,'Tran Thi Bich Tram'),(740,'Công thức phí - dạng phức hợp','Cập nhật','2020-02-25 Bike Weekend with Gold time 2','2020-02-25 11:53:10',36,'Tran Thi Bich Tram'),(741,'Biểu phí','Xóa','Loại thẻ: Thẻ vãng lai,Loại xe: Xe máy, Ngày hiệu lực: 2020-02-29','2020-02-25 11:53:23',36,'Tran Thi Bich Tram'),(742,'Biểu phí','Thêm','Loại thẻ: Thẻ vãng lai, Loại xe: Xe máy, Ngày hiệu lực: 2020-02-29','2020-02-25 11:53:32',36,'Tran Thi Bich Tram'),(743,'Biểu phí','Xóa','Loại thẻ: Thẻ vãng lai,Loại xe: Xe máy, Ngày hiệu lực: 2020-02-29','2020-02-25 11:53:56',36,'Tran Thi Bich Tram'),(744,'Biểu phí','Thêm','Loại thẻ: Thẻ vãng lai, Loại xe: Xe máy, Ngày hiệu lực: 2020-02-29','2020-02-25 11:54:11',36,'Tran Thi Bich Tram'),(745,'Công thức phí - dạng phức hợp','Cập nhật','2020-02-25 Bike Weekend with Gold time 2','2020-02-25 13:47:48',36,'Tran Thi Bich Tram'),(746,'Công thức phí - dạng phức hợp','Cập nhật','2020-02-25 Bike Free 4h Weekend','2020-02-25 13:51:35',36,'Tran Thi Bich Tram'),(747,'Biểu phí','Xóa','Loại thẻ: Thẻ vãng lai,Loại xe: Xe máy, Ngày hiệu lực: 2020-02-29','2020-02-25 13:51:41',36,'Tran Thi Bich Tram'),(748,'Biểu phí','Thêm','Loại thẻ: Thẻ vãng lai, Loại xe: Xe máy, Ngày hiệu lực: 2020-02-29','2020-02-25 13:52:04',36,'Tran Thi Bich Tram'),(749,'Công thức phí - dạng phức hợp','Cập nhật','2020-02-25 Bike Free 4h Weekend','2020-02-25 14:00:11',36,'Tran Thi Bich Tram'),(750,'Biểu phí','Xóa','Loại thẻ: Thẻ vãng lai,Loại xe: Xe máy, Ngày hiệu lực: 2020-02-29','2020-02-25 14:15:33',36,'Tran Thi Bich Tram'),(751,'Định mức phí','Xóa','2020-02-24 Gold Bike Guest','2020-02-25 14:25:08',36,'Tran Thi Bich Tram'),(752,'Biểu phí','Xóa','Loại thẻ: Thẻ vãng lai,Loại xe: Ô tô, Ngày hiệu lực: 2020-02-29','2020-02-25 14:41:47',36,'Tran Thi Bich Tram'),(753,'Biểu phí','Thêm','Loại thẻ: Thẻ vãng lai, Loại xe: Xe máy, Ngày hiệu lực: 2020-02-29','2020-02-25 16:16:36',36,'Tran Thi Bich Tram'),(754,'Công thức phí - dạng phức hợp','Cập nhật','2020-02-25 Bike Free 4h Weekend','2020-02-25 16:22:20',36,'Tran Thi Bich Tram'),(755,'Biểu phí','Xóa','Loại thẻ: Thẻ vãng lai,Loại xe: Xe máy, Ngày hiệu lực: 2020-02-29','2020-02-25 16:23:50',36,'Tran Thi Bich Tram'),(756,'Biểu phí','Thêm','Loại thẻ: Thẻ vãng lai, Loại xe: Xe máy, Ngày hiệu lực: 2020-02-29','2020-02-25 16:24:03',36,'Tran Thi Bich Tram'),(757,'Biểu phí','Xóa','Loại thẻ: Thẻ vãng lai,Loại xe: Xe máy, Ngày hiệu lực: 2020-02-29','2020-02-25 17:59:01',36,'Tran Thi Bich Tram'),(758,'Khai báo phí','Thêm','2020-02-26 Car Day Weekend 2','2020-02-26 11:23:44',36,'Tran Thi Bich Tram'),(759,'Công thức phí - dạng phức hợp','Cập nhật','2020-02-25 Bike Weekend with Gold time 2','2020-02-26 11:24:26',36,'Tran Thi Bich Tram'),(760,'Công thức phí - dạng phức hợp','Cập nhật','2020-02-25 Bike Weekend with Gold time 2','2020-02-26 11:24:47',36,'Tran Thi Bich Tram'),(761,'Công thức phí - dạng phức hợp','Thêm','2020-02-26 Bike Guest Test','2020-02-26 11:26:07',36,'Tran Thi Bich Tram'),(762,'Công thức phí - dạng phức hợp','Thêm','2020-02-26 Bike Guest Test','2020-02-26 11:26:07',36,'Tran Thi Bich Tram'),(763,'Công thức phí','Xóa','2020-02-26 Bike Guest Test','2020-02-26 11:29:06',36,'Tran Thi Bich Tram'),(764,'Công thức phí - dạng phức hợp','Thêm','2020-02-26 Bike Guest Test','2020-02-26 11:29:10',36,'Tran Thi Bich Tram'),(765,'Công thức phí - dạng phức hợp','Thêm','2020-02-26 Bike Guest Test','2020-02-26 11:29:10',36,'Tran Thi Bich Tram'),(766,'Công thức phí - dạng phức hợp','Cập nhật','2020-02-26 Bike Guest Test','2020-02-26 11:33:02',36,'Tran Thi Bich Tram'),(767,'Biểu phí','Thêm','Loại thẻ: Thẻ vãng lai, Loại xe: Xe máy, Ngày hiệu lực: 2020-02-29','2020-02-26 11:33:25',36,'Tran Thi Bich Tram'),(768,'Biểu phí','Xóa','Loại thẻ: Thẻ vãng lai,Loại xe: Xe máy, Ngày hiệu lực: 2020-02-29','2020-02-26 11:38:13',36,'Tran Thi Bich Tram'),(769,'Khai báo phí','Thêm','2020-02-26 Bike Day Weekend 3','2020-02-26 11:43:29',36,'Tran Thi Bich Tram'),(770,'Công thức phí','Xóa','2020-02-26 Bike Guest Test','2020-02-26 11:44:06',36,'Tran Thi Bich Tram'),(771,'Công thức phí - dạng phức hợp','Thêm','2020-02-26 Bike Guest Test','2020-02-26 11:44:10',36,'Tran Thi Bich Tram'),(772,'Công thức phí - dạng phức hợp','Thêm','2020-02-26 Bike Guest Test','2020-02-26 11:44:10',36,'Tran Thi Bich Tram'),(773,'Công thức phí - dạng phức hợp','Cập nhật','2020-02-26 Bike Guest Test','2020-02-26 11:48:29',36,'Tran Thi Bich Tram'),(774,'Biểu phí','Thêm','Loại thẻ: Thẻ vãng lai, Loại xe: Xe máy, Ngày hiệu lực: 2020-02-29','2020-02-26 11:48:41',36,'Tran Thi Bich Tram'),(775,'Công thức phí - dạng phức hợp','Cập nhật','2020-02-26 Bike Guest Test','2020-02-26 11:52:44',36,'Tran Thi Bich Tram'),(776,'Công thức phí - dạng phức hợp','Cập nhật','2020-02-26 Bike Guest Test','2020-02-26 11:52:56',36,'Tran Thi Bich Tram'),(777,'Biểu phí','Xóa','Loại thẻ: Thẻ vãng lai,Loại xe: Xe máy, Ngày hiệu lực: 2020-02-29','2020-02-26 13:56:01',36,'Tran Thi Bich Tram'),(778,'Công thức phí','Xóa','2020-02-26 Bike Guest Test','2020-02-26 13:56:13',36,'Tran Thi Bich Tram'),(779,'Định mức phí','Xóa','2020-02-26 Bike Day Weekend 3','2020-02-26 13:56:25',36,'Tran Thi Bich Tram'),(780,'Công thức phí - dạng phức hợp','Thêm','2020-02-26 Bike Guest test','2020-02-26 13:58:01',36,'Tran Thi Bich Tram'),(781,'Công thức phí - dạng phức hợp','Thêm','2020-02-26 Bike Guest test','2020-02-26 13:58:01',36,'Tran Thi Bich Tram'),(782,'Công thức phí - dạng phức hợp','Cập nhật','2020-02-26 Bike Guest test','2020-02-26 14:00:46',36,'Tran Thi Bich Tram'),(783,'Công thức phí - dạng phức hợp','Thêm','2020-02-26 Car Guest test','2020-02-26 14:00:54',36,'Tran Thi Bich Tram'),(784,'Công thức phí - dạng phức hợp','Thêm','2020-02-26 Car Guest test','2020-02-26 14:00:54',36,'Tran Thi Bich Tram'),(785,'Công thức phí - dạng phức hợp','Cập nhật','2020-02-26 Car Guest test','2020-02-26 14:03:22',36,'Tran Thi Bich Tram'),(786,'Biểu phí','Thêm','Loại thẻ: Thẻ vãng lai, Loại xe: Xe máy, Ngày hiệu lực: 2020-02-29','2020-02-26 14:03:40',36,'Tran Thi Bich Tram'),(787,'Biểu phí','Thêm','Loại thẻ: Thẻ vãng lai, Loại xe: Ô tô, Ngày hiệu lực: 2020-02-29','2020-02-26 14:05:22',36,'Tran Thi Bich Tram'),(788,'Biểu phí','Xóa','Loại thẻ: Thẻ vãng lai,Loại xe: Xe máy, Ngày hiệu lực: 2020-02-29','2020-02-26 15:40:58',36,'Tran Thi Bich Tram'),(789,'Biểu phí','Xóa','Loại thẻ: Thẻ vãng lai,Loại xe: Ô tô, Ngày hiệu lực: 2020-02-29','2020-02-26 15:41:05',36,'Tran Thi Bich Tram'),(790,'Khai báo phí','Thêm','2021-03-15 Car','2021-03-15 17:13:48',1,'Support'),(791,'Khai báo phí','Thêm','2021-03-15 Bike','2021-03-15 17:14:03',1,'Support'),(792,'Định mức phí','Xóa','2021-03-15 Bike','2021-03-15 17:14:14',1,'Support'),(793,'Khai báo phí','Thêm','2021-03-15 Bike Day','2021-03-15 17:14:24',1,'Support'),(794,'Khai báo phí','Thêm','2021-03-15 Bike Nighgt','2021-03-15 17:14:32',1,'Support'),(795,'Định mức phí','Xóa','2021-03-15 Bike Nighgt','2021-03-15 17:14:35',1,'Support'),(796,'Khai báo phí','Thêm','2021-03-15 Bike Night','2021-03-15 17:14:40',1,'Support'),(797,'Công thức phí - dạng phức hợp','Thêm','2021-03-15 Car','2021-03-15 17:14:54',1,'Support'),(798,'Công thức phí - dạng phức hợp','Cập nhật','2021-03-15 Car','2021-03-15 17:24:21',1,'Support'),(799,'Công thức phí - dạng phức hợp','Thêm','2021-03-15 Bike','2021-03-15 17:32:49',1,'Support'),(800,'Công thức phí - dạng phức hợp','Cập nhật','2021-03-15 Bike','2021-03-15 17:33:42',1,'Support'),(801,'Công thức phí - dạng phức hợp','Cập nhật','2021-03-15 Bike','2021-03-15 17:41:37',1,'Support'),(802,'Biểu phí','Thêm','Loại thẻ: Thẻ vãng lai, Loại xe: Xe máy, Ngày hiệu lực: 2021-03-15','2021-03-15 17:42:28',1,'Support'),(803,'Biểu phí','Thêm','Loại thẻ: Thẻ vãng lai, Loại xe: Ô tô, Ngày hiệu lực: 2021-03-15','2021-03-15 17:42:44',1,'Support'),(804,'Khai báo phí','Thêm','20210615_BikeDay','2021-06-14 17:34:06',1,'Support'),(805,'Khai báo phí','Thêm','20210615_BikeNight','2021-06-14 17:34:32',1,'Support'),(806,'Khai báo phí','Thêm','20210615_CarNight','2021-06-14 17:35:07',1,'Support'),(807,'Khai báo phí','Thêm','20210615_CarDay','2021-06-14 17:35:48',1,'Support'),(808,'Khai báo phí','Thêm','20210615_BikecicleDay','2021-06-14 17:37:14',1,'Support'),(809,'Khai báo phí','Thêm','20210615_BikecicleNight','2021-06-14 17:37:29',1,'Support'),(810,'Định mức phí','Xóa','20210615_CarDay','2021-06-14 17:38:09',1,'Support'),(811,'Khai báo phí','Thêm','20210615_CarDay','2021-06-14 17:38:43',1,'Support'),(812,'Định mức phí','Xóa','20210615_BikecicleNight','2021-06-14 17:39:09',1,'Support'),(813,'Định mức phí','Xóa','20210615_BikecicleDay','2021-06-14 17:39:13',1,'Support'),(814,'Khai báo phí','Thêm','20210615_BikecicleDay','2021-06-14 17:39:50',1,'Support'),(815,'Khai báo phí','Thêm','20210615_BikecicleNight','2021-06-14 17:40:03',1,'Support'),(816,'Công thức phí - dạng ngày đêm','Thêm','20210615_MotoBike','2021-06-14 17:44:07',1,'Support'),(817,'Công thức phí - dạng ngày đêm','Thêm','20210615_Car','2021-06-14 17:44:54',1,'Support'),(818,'Công thức phí - dạng ngày đêm','Thêm','20210615_Bike','2021-06-14 17:46:05',1,'Support'),(819,'Biểu phí','Thêm','Loại thẻ: Thẻ vãng lai, Loại xe: Xe máy, Ngày hiệu lực: 2021-06-15','2021-06-14 17:46:58',1,'Support'),(820,'Biểu phí','Thêm','Loại thẻ: Thẻ vãng lai, Loại xe: Xe đạp điện, Ngày hiệu lực: 2021-06-15','2021-06-14 17:46:58',1,'Support'),(821,'Biểu phí','Thêm','Loại thẻ: Thẻ vãng lai, Loại xe: Ô tô, Ngày hiệu lực: 2021-06-15','2021-06-14 17:47:19',1,'Support'),(822,'Biểu phí','Thêm','Loại thẻ: Thẻ vãng lai, Loại xe: Xe đạp, Ngày hiệu lực: 2021-06-15','2021-06-14 17:47:52',1,'Support'),(823,'Biểu phí','Thêm','Loại thẻ: FOC, Loại xe: Xe máy, Ngày hiệu lực: 2021-06-16','2021-06-14 18:33:24',1,'Support'),(824,'Biểu phí','Thêm','Loại thẻ: FOC, Loại xe: Xe đạp điện, Ngày hiệu lực: 2021-06-16','2021-06-14 18:33:24',1,'Support'),(825,'Biểu phí','Thêm','Loại thẻ: FOC, Loại xe: Ô tô, Ngày hiệu lực: 2021-06-16','2021-06-14 18:33:37',1,'Support'),(826,'Biểu phí','Thêm','Loại thẻ: FOC, Loại xe: Xe đạp, Ngày hiệu lực: 2021-06-16','2021-06-14 18:33:53',1,'Support'),(827,'Công thức phí','Xóa','CityGateFee','2023-03-20 10:49:19',1,'Support'),(828,'Công thức phí - dạng phức hợp','Thêm','CityGateFee','2023-03-20 10:50:29',1,'Support'),(829,'Công thức phí','Xóa','CityGateFee','2023-03-20 10:51:15',1,'Support'),(830,'Công thức phí - dạng 24h','Thêm','CityGateFee','2023-03-20 10:52:11',1,'Support'),(831,'Biểu phí','Thêm','Loại thẻ: Thẻ vãng lai, Loại xe: Xe máy, Ngày hiệu lực: 2023-03-20','2023-03-20 10:52:38',1,'Support'),(832,'Công thức phí - dạng phức hợp','Thêm','GardenMall','2023-08-07 04:24:11',1,'Support'),(833,'Khai báo phí','Thêm','GardenMaill_Oto','2023-08-07 04:27:39',1,'Support'),(834,'Khai báo phí','Thêm','GardenMaill_Oto_dem','2023-08-07 04:30:02',1,'Support'),(835,'Công thức phí','Xóa','GardenMall','2023-08-07 04:30:40',1,'Support'),(836,'Công thức phí - dạng phức hợp','Thêm','GardenMall','2023-08-07 04:31:17',1,'Support'),(837,'Công thức phí - dạng phức hợp','Cập nhật','GardenMall','2023-08-07 04:33:07',1,'Support'),(838,'Biểu phí','Thêm','Loại thẻ: Thẻ vãng lai, Loại xe: Ô tô, Ngày hiệu lực: 2023-08-07','2023-08-07 04:34:46',1,'Support'),(839,'Biểu phí','Thêm','Loại thẻ: Thẻ vãng lai, Loại xe: Bất kỳ, Ngày hiệu lực: 2023-08-07','2023-08-07 04:34:46',1,'Support');
/*!40000 ALTER TABLE `historyaccess` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `natconfig`
--

DROP TABLE IF EXISTS `natconfig`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `natconfig` (
  `id` int NOT NULL AUTO_INCREMENT,
  `natfordershared` varchar(256) DEFAULT NULL,
  `username` varchar(128) DEFAULT NULL,
  `password` varchar(128) DEFAULT NULL,
  `repeatseconds` int DEFAULT NULL,
  `scheduletime` varchar(128) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `natconfig`
--

LOCK TABLES `natconfig` WRITE;
/*!40000 ALTER TABLE `natconfig` DISABLE KEYS */;
INSERT INTO `natconfig` VALUES (1,'\\\\\\\\172.16.0.13\\\\phase2-report',NULL,NULL,60,'18:02:00');
/*!40000 ALTER TABLE `natconfig` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `redemptionactive`
--

DROP TABLE IF EXISTS `redemptionactive`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `redemptionactive` (
  `id` int NOT NULL AUTO_INCREMENT,
  `activedate` date DEFAULT NULL,
  `expireddate` date DEFAULT NULL,
  `grouptenant` int DEFAULT NULL,
  `samplefeeid` int DEFAULT NULL,
  `usercreate` int DEFAULT NULL,
  `sampleid1` int DEFAULT NULL,
  `vehicletypeid` int DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `redemptionactive`
--

LOCK TABLES `redemptionactive` WRITE;
/*!40000 ALTER TABLE `redemptionactive` DISABLE KEYS */;
/*!40000 ALTER TABLE `redemptionactive` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `redemtionactive`
--

DROP TABLE IF EXISTS `redemtionactive`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `redemtionactive` (
  `id` int NOT NULL,
  `tenantgroupid` int DEFAULT NULL,
  `fromdate` date DEFAULT NULL,
  `todate` date DEFAULT NULL,
  `sampleid` int DEFAULT NULL,
  `sampleid1` int DEFAULT NULL,
  `vehicletypeid` int DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `redemtionactive`
--

LOCK TABLES `redemtionactive` WRITE;
/*!40000 ALTER TABLE `redemtionactive` DISABLE KEYS */;
/*!40000 ALTER TABLE `redemtionactive` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `sampleactive`
--

DROP TABLE IF EXISTS `sampleactive`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sampleactive` (
  `id` int NOT NULL AUTO_INCREMENT,
  `activedate` date DEFAULT NULL,
  `expireddate` date DEFAULT NULL,
  `vehicletype` int DEFAULT NULL,
  `cardtype` int DEFAULT NULL,
  `samplefeeid` int DEFAULT NULL,
  `samplefeeid1` int DEFAULT NULL,
  `usercreate` int DEFAULT NULL,
  `samplefeeid2` int DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=137 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `sampleactive`
--

LOCK TABLES `sampleactive` WRITE;
/*!40000 ALTER TABLE `sampleactive` DISABLE KEYS */;
INSERT INTO `sampleactive` VALUES (128,'2021-03-15','2021-06-14',1000001,0,97,97,1,97),(129,'2021-03-15','2021-06-14',2000101,0,96,96,1,96),(130,'2021-06-15','2023-03-19',1000001,0,99,99,1,99),(131,'2021-06-15',NULL,5000401,0,99,99,1,99),(132,'2021-06-15','2023-08-06',2000101,0,100,100,1,100),(133,'2021-06-15',NULL,4000301,0,101,101,1,101),(134,'2023-03-20',NULL,1000001,0,104,104,1,104),(135,'2023-08-07',NULL,2000101,0,106,106,1,106),(136,'2023-08-07',NULL,100000000,0,106,106,1,106);
/*!40000 ALTER TABLE `sampleactive` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `samplefee`
--

DROP TABLE IF EXISTS `samplefee`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `samplefee` (
  `id` int NOT NULL AUTO_INCREMENT,
  `callname` varchar(128) DEFAULT NULL,
  `freetime` int DEFAULT NULL,
  `tolerancetime` int DEFAULT NULL,
  `feetype` int DEFAULT NULL,
  `inused` int DEFAULT NULL,
  `createbyid` int DEFAULT NULL,
  `totalfees` int DEFAULT NULL,
  `startdate` time DEFAULT NULL,
  `subtractfree` int DEFAULT NULL,
  `subtracttolerance` int DEFAULT NULL,
  `feeday` int DEFAULT NULL,
  `startnight` time DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=107 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `samplefee`
--

LOCK TABLES `samplefee` WRITE;
/*!40000 ALTER TABLE `samplefee` DISABLE KEYS */;
INSERT INTO `samplefee` VALUES (96,'2021-03-15 Car',15,0,3,1,1,NULL,'00:00:00',1,1,NULL,NULL),(97,'2021-03-15 Bike',15,0,3,1,1,NULL,'00:00:00',1,1,NULL,NULL),(99,'20210615_MotoBike',15,NULL,1,1,1,0,'05:00:00',NULL,NULL,0,'22:00:00'),(100,'20210615_Car',15,NULL,1,1,1,0,'05:00:00',NULL,NULL,0,'22:00:00'),(101,'20210615_Bike',15,NULL,1,1,1,0,'05:00:00',NULL,NULL,0,'22:00:00'),(104,'CityGateFee',0,NULL,2,1,1,NULL,NULL,NULL,NULL,NULL,NULL),(106,'GardenMall',0,15,3,1,1,NULL,'06:00:00',1,1,NULL,NULL);
/*!40000 ALTER TABLE `samplefee` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `sampleredemptionactive`
--

DROP TABLE IF EXISTS `sampleredemptionactive`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sampleredemptionactive` (
  `id` int NOT NULL AUTO_INCREMENT,
  `activedate` date DEFAULT NULL,
  `expireddate` date DEFAULT NULL,
  `samplefeeid` int DEFAULT NULL,
  `usercreate` int DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `sampleredemptionactive`
--

LOCK TABLES `sampleredemptionactive` WRITE;
/*!40000 ALTER TABLE `sampleredemptionactive` DISABLE KEYS */;
/*!40000 ALTER TABLE `sampleredemptionactive` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `specialdate`
--

DROP TABLE IF EXISTS `specialdate`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `specialdate` (
  `id` int NOT NULL AUTO_INCREMENT,
  `dateactive` date DEFAULT NULL,
  `callname` varchar(128) DEFAULT NULL,
  `percentupordown` int DEFAULT NULL,
  `usercreate` int DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `specialdate`
--

LOCK TABLES `specialdate` WRITE;
/*!40000 ALTER TABLE `specialdate` DISABLE KEYS */;
/*!40000 ALTER TABLE `specialdate` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tenantgroup`
--

DROP TABLE IF EXISTS `tenantgroup`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `tenantgroup` (
  `id` int NOT NULL AUTO_INCREMENT,
  `groupname` varchar(128) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tenantgroup`
--

LOCK TABLES `tenantgroup` WRITE;
/*!40000 ALTER TABLE `tenantgroup` DISABLE KEYS */;
INSERT INTO `tenantgroup` VALUES (5,'All Group');
/*!40000 ALTER TABLE `tenantgroup` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tenants`
--

DROP TABLE IF EXISTS `tenants`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `tenants` (
  `id` int NOT NULL AUTO_INCREMENT,
  `refid` int NOT NULL,
  `tenantgroupid` int DEFAULT NULL,
  `tennantsname` varchar(128) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tenants`
--

LOCK TABLES `tenants` WRITE;
/*!40000 ALTER TABLE `tenants` DISABLE KEYS */;
/*!40000 ALTER TABLE `tenants` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `toolfeemenu`
--

DROP TABLE IF EXISTS `toolfeemenu`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `toolfeemenu` (
  `id` int NOT NULL AUTO_INCREMENT,
  `menuname` varchar(128) DEFAULT NULL,
  `orderindex` int DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=14 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `toolfeemenu`
--

LOCK TABLES `toolfeemenu` WRITE;
/*!40000 ALTER TABLE `toolfeemenu` DISABLE KEYS */;
INSERT INTO `toolfeemenu` VALUES (1,'Công thức phí',2),(2,'Biểu phí',3),(3,'Biểu khấu trừ phí',6),(4,'Kiểm tra công thức',7),(5,'Khai báo phí',1),(6,'Định mức khấu trừ',5),(7,'Nhóm đối tác, cửa hàng',4),(8,'Ngày đặc biệt',12),(9,'Phân quyền',13),(10,'Báo cáo phí vãng lai',8),(11,'Báo cáo redemption',9),(12,'Báo cáo lịch sử tác động',10),(13,'Báo cáo quân quyền',11);
/*!40000 ALTER TABLE `toolfeemenu` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `typeofdate`
--

DROP TABLE IF EXISTS `typeofdate`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `typeofdate` (
  `id` int NOT NULL AUTO_INCREMENT,
  `callname` varchar(128) DEFAULT NULL,
  `weekmap` varchar(128) DEFAULT NULL,
  `samplefeeid` int DEFAULT NULL,
  `feefullday` int DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=84 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `typeofdate`
--

LOCK TABLES `typeofdate` WRITE;
/*!40000 ALTER TABLE `typeofdate` DISABLE KEYS */;
INSERT INTO `typeofdate` VALUES (80,'ngày thường','2,3,4,5,6,7,1',96,475000),(81,'ngày thường','2,3,4,5,6,7,1',97,10000),(83,'ngày thường','2,3,4,5,6,7,1',106,420000);
/*!40000 ALTER TABLE `typeofdate` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `userpermission`
--

DROP TABLE IF EXISTS `userpermission`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `userpermission` (
  `id` int NOT NULL AUTO_INCREMENT,
  `menuid` int DEFAULT NULL,
  `userid` int DEFAULT NULL,
  `isadd` int DEFAULT NULL,
  `isedit` int DEFAULT NULL,
  `isdel` int DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=120 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `userpermission`
--

LOCK TABLES `userpermission` WRITE;
/*!40000 ALTER TABLE `userpermission` DISABLE KEYS */;
INSERT INTO `userpermission` VALUES (45,5,1,1,2,2),(47,1,1,1,2,2),(50,5,1,1,2,2),(52,2,1,1,2,2),(54,7,1,1,2,2),(56,6,1,1,2,2),(58,3,1,1,2,2),(60,4,1,1,2,2),(62,10,1,1,2,2),(64,11,1,1,2,2),(66,12,1,1,2,2),(68,11,1,1,2,2),(69,11,1,1,2,2),(70,13,1,1,2,2),(72,8,1,1,2,2),(73,9,1,1,2,2),(74,9,1,1,2,2),(76,5,1,1,2,2),(77,5,1,0,0,0),(78,5,1,1,2,2),(79,5,1,0,0,0),(80,1,1,1,2,2),(81,1,1,1,2,2),(82,2,1,1,2,2),(83,2,1,1,2,2),(84,7,1,1,2,2),(85,7,1,1,2,2),(86,6,1,1,2,2),(87,6,1,1,2,2),(88,3,1,1,2,2),(89,3,1,1,2,2),(90,7,1,1,2,2),(91,7,1,1,2,2),(92,4,1,1,2,2),(93,4,1,1,2,2),(94,10,1,1,2,2),(95,10,1,1,2,2),(96,11,1,1,2,2),(97,11,1,1,2,2),(98,12,1,1,2,2),(99,12,1,1,2,2),(100,13,1,1,2,2),(101,8,1,1,2,2),(102,8,1,1,2,2),(103,9,1,1,2,2),(104,9,1,1,2,2),(105,9,1,1,2,2),(106,9,1,1,2,2),(107,5,1,1,2,2),(108,1,1,1,2,2),(109,2,1,1,2,2),(110,2,1,1,2,2),(111,7,1,1,2,2),(112,6,1,1,2,2),(113,3,1,1,2,2),(114,4,1,1,2,2),(115,10,1,1,2,2),(116,11,1,1,2,2),(117,12,1,1,2,2),(118,13,1,1,2,2),(119,8,1,1,2,2);
/*!40000 ALTER TABLE `userpermission` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `userroot`
--

DROP TABLE IF EXISTS `userroot`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `userroot` (
  `id` int NOT NULL AUTO_INCREMENT,
  `userid` int DEFAULT NULL,
  `levelroot` int DEFAULT NULL,
  `usercreate` int DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `userroot`
--

LOCK TABLES `userroot` WRITE;
/*!40000 ALTER TABLE `userroot` DISABLE KEYS */;
INSERT INTO `userroot` VALUES (1,107,1,1),(2,3,1,1);
/*!40000 ALTER TABLE `userroot` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping routines for database 'hd_fee'
--
/*!50003 DROP FUNCTION IF EXISTS `getlockstatesample` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   FUNCTION `getlockstatesample`(sid int) RETURNS int
    DETERMINISTIC
BEGIN
	Return	case when 
			exists (select 1 from sampleactive r where r.samplefeeid=sid or r.samplefeeid1=sid) 
			and not exists (select 1 from sampleactive r where (r.samplefeeid=sid or r.samplefeeid1=sid)  and( r.expireddate is null or r.expireddate>=Curdate())) then 1 else 0 end; 
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP FUNCTION IF EXISTS `getlockstatesampleredempt` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   FUNCTION `getlockstatesampleredempt`(sid int) RETURNS int
    DETERMINISTIC
BEGIN
	Return	case when 
		exists (select 1 from redemptionactive r where r.samplefeeid=sid or r.sampleid1=sid) 
		and not exists (select 1 from redemptionactive r where (r.samplefeeid=sid or r.sampleid1=sid) and (r.expireddate is null or r.expireddate>=Curdate())) then 1 else 0 end; 
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
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
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
/*!50003 DROP PROCEDURE IF EXISTS `checkexistbill` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `checkexistbill`(n varchar(512))
begin
	if exists (select 1 from billformula where callname=n) then
		select 1;
	else 
		select 0;
	end if;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `checkexistfee` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `checkexistfee`(n varchar(512))
begin
	if exists (select 1 from feeformula where callname=n) then
		select 1;
	else 
		select 0;
	end if;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `checkexistsample` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `checkexistsample`(n varchar(512))
begin
	if exists (select 1 from samplefee where callname=n) then
		select 1;
	else 
		select 0;
	end if;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `findsampleredemtion` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `findsampleredemtion`(activeid int, t int)
begin
	select s.* from samplefee s
    where (t=0 and exists(select 1 from redemptionactive r where r.id=activeid and r.samplefeeid=s.id))
		or (t=1 and exists(select 1 from redemptionactive r where r.id=activeid and r.sampleid1=s.id));
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `geredemtiontactiveid` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `geredemtiontactiveid`(vid int,gt int,rdtime date)
BEGIN
	select min(t.id) from 
	(
	select max(id) as id from redemptionactive
    where grouptenant=gt and vehicletypeid=vid
		and((rdtime>=activedate and expireddate is null) or (rdtime between activedate and expireddate))
	union
    select min(id) as id from redemptionactive
    where  grouptenant=gt and vehicletypeid=vid
		and rdtime < activedate
	) t
    where t.id is not null;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `geredemtiontactivelist` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `geredemtiontactivelist`(vid int)
BEGIN
	select r.id,s.id as `sampleid`,s.callname,g.groupname  
    from redemptionactive r
    left join samplefee s on s.id = r.samplefeeid
    left join tenantgroup g on g.id=r.grouptenant
    where r.vehicletypeid=vid
    order by r.id desc
    limit 1;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getactiveid` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getactiveid`(vid int,cid int,cotime date)
BEGIN
	select min(t.id) from 
	(
	select max(id) as id from sampleactive
    where vehicletype=vid and cardtype=cid
		and((cotime>=activedate and expireddate is null) or (cotime between activedate and expireddate))
	union
    select min(id) as id from sampleactive
    where  vehicletype=vid and cardtype=cid
		and cotime < activedate
	) t
    where t.id is not null;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getactivepermission` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getactivepermission`(uid int,meid int)
begin
	select * 
    from userpermission
    where menuid=meid and userid=uid;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getbillformula` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getbillformula`()
begin
	select b.*, 
	case when exists (select 1 from cycleredemption c where c.formulabill=b.id) then 0 else 1 end as `ischange`  
    from `billformula` b
    order by id desc;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getcyclecomplex` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getcyclecomplex`( dateid int)
begin
	select * 
    from `cycletime` 
    where `dateetypeid`=dateid
    order by `cycletype`;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getcycleredemption` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getcycleredemption`( dtypeid int)
begin
	select * 
    from `cycleredemption` 
    where `datetypeid` =dtypeid;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getdatetypecomplex` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getdatetypecomplex`( sampleid int)
begin
	select * 
    from `typeofdate` 
    where `samplefeeid`=sampleid;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getdatetyperedemption` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getdatetyperedemption`( sampleid int)
begin
	select * 
    from `datetyperedemption` 
    where `redemptfeeid`=sampleid;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getdatetyperedemtion` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getdatetyperedemtion`( sampleid int)
begin
	select * 
    from `datetyperedemption` 
    where `redemptfeeid`=sampleid;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getdetailfee24h` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getdetailfee24h`(sampleid int)
begin
	select * 
    from fee24detail 
    where samplefeeid=  sampleid;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getfeeformular` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getfeeformular`()
begin
	select f.*,
		case when exists (select 1 from cycletime c where c.formula=f.id) then 0 else 1 end as `ischange`
    from `feeformula` f
    order  by id desc, f.feetype desc;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getfeeformularbyid` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getfeeformularbyid`(fid int)
begin
	select * 
    from `feeformula`
    where `id`=fid; 
    
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getfixfee` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'NO_AUTO_VALUE_ON_ZERO' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getfixfee`(fid int)
begin
	select td.feefullday,minfixfee(td.id),maxfixfee(td.id)
    from typeofdate td  
    where td.samplefeeid=fid and weekmap='2,3,4,5,6,7,1';
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getgrouppermission` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getgrouppermission`(meid int)
begin
	select * 
    from groupuserpermission
    where menuid=meid;
   
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
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `gethistoryaccess`(fromtime datetime, totime datetime)
begin
	select DATE_FORMAT( h.actiondate, '%d/%m/%Y %H:%i:%s') as actiontime,h.username,h.target,h.useraction,h.content
    from historyaccess h
    where h.actiondate between fromtime and totime
    order by h.actiondate desc,h.target;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getinforemove` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getinforemove`(tblname varchar(128),tblid int)
begin
	if tblname='samplefee' then
		select case when feetype=4 then 'Công thức khấu trừ' else 'Công thức phí' end as `tg`, 
			callname as `ct`  from samplefee where id=tblid;
	elseif tblname='feeformula' then
		select 'Định mức phí' as `tg`, callname as `ct` from feeformula where id=tblid;
	elseif tblname='billformula' then
		select 'Định mức khấu trừ' as `tg`, callname as `ct` from billformula where id=tblid;
	elseif tblname='specialdate' then
		select 'Ngày đặc biệt' as `tg`, callname as `ct` from specialdate where id=tblid;
	elseif tblname='sampleactive' then
		select cardtype,vehicletype,activedate from sampleactive where id=tblid;
    elseif tblname='redemptionactive' then
		select grouptenant, vehicletypeid, activedate from redemptionactive where id=tblid;
	else
		select tblname as `tg`,tblid as `ct`;
	end if;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getMenuName` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getMenuName`(mid int)
begin
	select menuname from toolfeemenu where id=mid;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getmenupermission` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getmenupermission`()
begin
select t.* 
from
(
	select id,menuname,'Can See' as `Action`
	from toolfeemenu 
	union 
	select id,menuname,'Can Add' as `Action`
	from toolfeemenu 
	union 
	select id,menuname,'Can Edit' as `Action`
	from toolfeemenu 
	union 
	select id,menuname,'Can Edit All' as `Action`
	from toolfeemenu 
	union 
	select id,menuname,'Can Delete' as `Action`
	from toolfeemenu 
	union 
	select id,menuname,'Can Delete All' as `Action`
	from toolfeemenu 
) t
order by t.id,t.`Action`;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getmenupermissionss` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getmenupermissionss`()
begin
	select t.* 
	from
	(
		
		select id,menuname,'Thêm mới' as `Action`, orderindex, 1 as 'orderin'
		from toolfeemenu 
		union 
		select id,menuname,'Cập nhật' as `Action`, orderindex, 2 as 'orderin'
		from toolfeemenu 
		union 
		select id,menuname,'Cập nhật tất cả' as `Action`, orderindex, 3 as 'orderin'
		from toolfeemenu 
		union 
		select id,menuname,'Xóa' as `Action`, orderindex, 4 as 'orderin'
		from toolfeemenu 
		union 
		select id,menuname,'Xóa tất cả' as `Action`, orderindex, 5 as 'orderin'
		from toolfeemenu 
	) t
	order by t.`orderindex`,t.`orderin`;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getmenus` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getmenus`()
begin
	select * 
	from toolfeemenu 
    order by orderindex;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getnatconfig` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getnatconfig`()
begin
	select *
    from hd_fee.natconfig
    limit 1;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getpaymentid` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
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
/*!50003 DROP PROCEDURE IF EXISTS `getredemtionactive` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getredemtionactive`(gt int, vid int)
begin
	select * 
    from redemptionactive
    where grouptenant=gt and vehicletypeid=vid;
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
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
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
/*!50003 DROP PROCEDURE IF EXISTS `getrootbyuser` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getrootbyuser`(uid int)
begin
	select * 
    from userroot
    where userid=uid and levelroot>0;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getrootlevel` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getrootlevel`()
begin
	select * from userroot;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getsampleactive` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getsampleactive`()
begin
	select * 
    from `sampleactive`
    order by vehicletype,cardtype, activedate desc;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getsampleactivebyid` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getsampleactivebyid`(aid int)
begin
	select * 
    from `sampleactive`
    where id=aid
    order by vehicletype,cardtype, activedate desc;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getsampleactivedetail` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getsampleactivedetail`(vtype int,ctype int)
begin
	select sa.id,sa.activedate,sa.expireddate,sa.vehicletype,sa.cardtype,sa.samplefeeid,s.callname as `samplename`,sa.samplefeeid1,s1.callname as `samplename1`,sa.usercreate,
		case when (sa.expireddate is null and sa.activedate >CURDATE()) then 1 else 0 end as `ischange`,sa.samplefeeid2,s2.callname as `samplename2`
    from sampleactive sa
    left join samplefee s on s.id=sa.samplefeeid
    left join samplefee s1 on s1.id=sa.samplefeeid1
    left join samplefee s2 on s2.id=sa.samplefeeid2
    where vehicletype=vtype and cardtype=ctype
    order by sa.activedate desc;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getsampleactivetoupdate` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getsampleactivetoupdate`(vtype int,ctype int)
begin
	select *
    from sampleactive
    where vehicletype=vtype and cardtype=ctype and ifnull(expireddate,'')='';
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
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getsamplefee`(sid varchar(128), ftype int)
begin
	Select s.id,s.callname,s.freetime,s.tolerancetime,s.feetype,
		case when s.inused=3 then 3 
			else
			case when s.feetype=4 then 
						case
							when exists(select 1 from redemptionactive r where r.samplefeeid=s.id or r.sampleid1=s.id) then 2 
							else s.inused 
						end 
				 else   case 
							when exists(select 1 from sampleactive sa where sa.samplefeeid=s.id or sa.samplefeeid1=s.id) then 2 
							else s.inused 
						end 
			end 
		end as `inused`,
		s.createbyid,s.totalfees,s.startdate,s.subtractfree,s.subtracttolerance,s.feeday,s.startnight,
		case when s.feetype=4 then 
				case 
					when exists(select 1 from redemptionactive r where r.samplefeeid=s.id or r.sampleid1=s.id) then 0 else 1 
				end 
			 else  
				case when exists(select 1 from sampleactive sa where sa.samplefeeid=s.id or sa.samplefeeid1=s.id) then 0 else 1 
				end 
		end as `ischange`,
        case when s.inused=3 then 2
			 else 
				case when s.feetype=4 then getlockstatesampleredempt(s.id) else getlockstatesample(s.id) 
                end 
		end as `canlock`
    from samplefee s
    where (ifnull(sid,0)=0 or s.id=sid)
    and (ifnull(ftype,0)=0 or s.feetype=ftype)
    order by id desc, feetype;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getsamplefeecomplexbyid` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getsamplefeecomplexbyid`(feeid int)
BEGIN
	select s.id,s.feetype,s.callname,s.freetime,s.subtractfree,s.tolerancetime,s.subtracttolerance,
    s.startdate,s.optioncase
    from samplefee s 
    where s.id =feeid and feetype=3;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getsamplefeedetailnn` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getsamplefeedetailnn`(sid int)
begin
	select s.id,s.callname,s.startdate,s.startnight,s.freetime,s.full24hfee,s.maxfee,s.optioncase,
		s.formuladay,fd.callname as `dayname`,fd.detail as `daydetail`,
        s.formulanight, fn.callname as `nightname`,fn.detail as `nightdetail`
    from samplefee s
    left join feeformula fd on fd.id=s.formuladay
    left join feeformula fn on fn.id=s.formulanight
    where s.id=sid and s.feetype=5;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getsampleredemptionactive` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getsampleredemptionactive`()
begin
	select * 
    from `sampleredemptionactive`    
    order by activedate desc;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getsemplefeebyactive` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getsemplefeebyactive`(aid int)
begin
	select s.* 
    from samplefee s
	where id in (select samplefeeid from sampleactive where id=aid)
    union
    select s.* 
    from samplefee s
	where id in (select samplefeeid1 from sampleactive where id=aid);
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getspecialdate` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getspecialdate`()
begin
	select s.*,
	case when s.dateactive>now() then 1 else 0 end as `ischange`
    from `specialdate` s;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getspecialdatefee` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getspecialdatefee`(dt date)
begin
	select * 
    from specialdate
    where dateactive=dt;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `gettenantgroup` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `gettenantgroup`()
begin
	select g.*,
		case when exists(select 1 from redemptionactive r where r.grouptenant=g.id) then 0 else 1 end as `ischange`
    from tenantgroup g
    order by g.groupname;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `gettennants` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `gettennants`(gt int)
begin
	select * 
	from tenants 
	where ifnull(gt,'-1')='-1' or tenantgroupid=gt
    order by tenantgroupid,tennantsname;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getuserpermission` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getuserpermission`(meid int)
begin
	select * 
    from userpermission
    where menuid=meid;
   
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `getuserpermissionss` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `getuserpermissionss`()
begin
	select * from userpermission order by menuid,userid;
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
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
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
/*!50003 DROP PROCEDURE IF EXISTS `gtredemptionactive` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `gtredemptionactive`(gt int, vtid int)
begin
	select r.*,
		case when (r.expireddate is null and r.activedate >CURDATE()) then 1 else 0 end as `ischange`
    from redemptionactive r
    where r.grouptenant=gt and r.vehicletypeid=vtid
    order by r.activedate desc;
end ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `isactivetoolfee` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb3 */ ;
/*!50003 SET character_set_results = utf8mb3 */ ;
/*!50003 SET collation_connection  = utf8mb3_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = '' */ ;
DELIMITER ;;
CREATE   PROCEDURE `isactivetoolfee`()
begin
	select val from Config limit 1;
End ;;
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

-- Dump completed on 2025-10-03  8:45:55
