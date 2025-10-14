-- MySQL dump 10.13  Distrib 8.0.43, for Win64 (x86_64)
--
-- Host: localhost    Database: hd
-- ------------------------------------------------------
-- Server version	8.0.43

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
-- Table structure for table `parking_apitoken`
--

DROP TABLE IF EXISTS `parking_apitoken`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `parking_apitoken` (
  `id` int NOT NULL AUTO_INCREMENT,
  `user_id` int NOT NULL,
  `key` varchar(64) NOT NULL,
  `created` datetime NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `user_id` (`user_id`),
  UNIQUE KEY `key` (`key`),
  KEY `parking_apitoken_key` (`key`),
  CONSTRAINT `parking_apitoken_user_id_fk` FOREIGN KEY (`user_id`) REFERENCES `auth_user` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_consolidatedinvoice`
--

DROP TABLE IF EXISTS `parking_consolidatedinvoice`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `parking_consolidatedinvoice` (
  `id` int NOT NULL AUTO_INCREMENT,
  `parkingids` longtext NOT NULL,
  `refid` varchar(50) NOT NULL,
  `transactionid` varchar(50) DEFAULT NULL,
  `amountrequested` tinyint NOT NULL DEFAULT '0',
  `requestedtime` datetime NOT NULL,
  `parkingfees` int NOT NULL,
  `iscompleted` tinyint(1) NOT NULL,
  `invoicedate` date NOT NULL,
  `contentrequest` longtext,
  `contentresponse` longtext,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=25 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_invoiceapiinitation`
--

DROP TABLE IF EXISTS `parking_invoiceapiinitation`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `parking_invoiceapiinitation` (
  `id` int NOT NULL AUTO_INCREMENT,
  `c_url` varchar(256) NOT NULL,
  `c_partner` int NOT NULL,
  `c_headers` longtext,
  `c_body` longtext,
  `c_target` int NOT NULL,
  `c_method` int NOT NULL,
  PRIMARY KEY (`id`),
  KEY `parking_parking_partnerinvoice_c7dd990` (`c_partner`),
  CONSTRAINT `c_partner_refs_id_0d958dda` FOREIGN KEY (`c_partner`) REFERENCES `parking_partnerinvoice` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_invoicebuyer`
--

DROP TABLE IF EXISTS `parking_invoicebuyer`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `parking_invoicebuyer` (
  `id` int NOT NULL AUTO_INCREMENT,
  `mode` int NOT NULL,
  `code` varchar(50) DEFAULT NULL,
  `buyername` varchar(50) DEFAULT NULL,
  `legalname` varchar(256) DEFAULT NULL,
  `phone` varchar(50) DEFAULT NULL,
  `email` varchar(50) DEFAULT NULL,
  `taxcode` varchar(50) DEFAULT NULL,
  `receivername` varchar(50) DEFAULT NULL,
  `receiveremails` varchar(256) DEFAULT NULL,
  `address` varchar(256) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_invoiceconnector`
--

DROP TABLE IF EXISTS `parking_invoiceconnector`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `parking_invoiceconnector` (
  `id` int NOT NULL AUTO_INCREMENT,
  `c_partner` int NOT NULL,
  `username` varchar(50) NOT NULL,
  `password` varchar(256) NOT NULL,
  `taxcode` varchar(50) DEFAULT NULL,
  `lastupdate` datetime DEFAULT NULL,
  `token` text,
  `invoiceserie` varchar(50) DEFAULT NULL,
  `invoicetmp` varchar(50) DEFAULT NULL,
  `maxamount` tinyint NOT NULL DEFAULT '3',
  `appid` varchar(128) DEFAULT NULL,
  `isvaliddate` tinyint(1) NOT NULL DEFAULT '0',
  `scheduletime` time DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `parking_parking_partnerinvoice_c6ee9ea` (`c_partner`),
  CONSTRAINT `c_partner_refs_id_0d375dda` FOREIGN KEY (`c_partner`) REFERENCES `parking_partnerinvoice` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_invoicetaxrule`
--

DROP TABLE IF EXISTS `parking_invoicetaxrule`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `parking_invoicetaxrule` (
  `id` int NOT NULL AUTO_INCREMENT,
  `mode` int NOT NULL,
  `taxpercent` tinyint NOT NULL,
  `feeincludesvat` tinyint(1) NOT NULL DEFAULT '0',
  `activated` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_partnerinvoice`
--

DROP TABLE IF EXISTS `parking_partnerinvoice`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `parking_partnerinvoice` (
  `id` int NOT NULL AUTO_INCREMENT,
  `code` varchar(128) NOT NULL,
  `name` varchar(128) NOT NULL,
  `activated` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parking_retailinvoice`
--

DROP TABLE IF EXISTS `parking_retailinvoice`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `parking_retailinvoice` (
  `id` int NOT NULL AUTO_INCREMENT,
  `parkingrefid` varchar(11) NOT NULL,
  `refid` varchar(50) NOT NULL,
  `transactionid` varchar(50) DEFAULT NULL,
  `buyer` int DEFAULT NULL,
  `parkingfee` int NOT NULL,
  `amountrequested` tinyint NOT NULL DEFAULT '0',
  `completed` tinyint(1) NOT NULL DEFAULT '1',
  `requestedtime` datetime NOT NULL,
  `invoicedate` date NOT NULL,
  `parkingcompleted` tinyint(1) NOT NULL DEFAULT '1',
  `contentrequest` longtext,
  `iscompleted` tinyint(1) NOT NULL,
  `contentresponse` longtext,
  PRIMARY KEY (`id`),
  KEY `parking_parking_invoicebuyer_c3dd920` (`buyer`),
  CONSTRAINT `buyer_refs_id_0d99ddda` FOREIGN KEY (`buyer`) REFERENCES `parking_invoicebuyer` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=17549 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-10-07 15:27:10
