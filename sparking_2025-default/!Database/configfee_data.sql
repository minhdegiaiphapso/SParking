-- MySQL dump 10.13  Distrib 5.7.12, for Win64 (x86_64)
--
-- Host: 172.16.0.10    Database: configfeedb
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
-- Dumping data for table `Config`
--

LOCK TABLES `Config` WRITE;
/*!40000 ALTER TABLE `Config` DISABLE KEYS */;
INSERT INTO `Config` VALUES (1,'Cách tính phí','new');
/*!40000 ALTER TABLE `Config` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping data for table `groupuserpermission`
--

LOCK TABLES `groupuserpermission` WRITE;
/*!40000 ALTER TABLE `groupuserpermission` DISABLE KEYS */;
INSERT INTO `groupuserpermission` VALUES (3,3,-1,1,2,2),(8,8,-1,1,2,2),(21,9,1,1,2,2),(31,1,1,1,2,2),(32,1,1,1,2,2),(37,2,1,1,2,2),(38,9,-1,1,2,2),(39,1,-1,1,2,2),(43,7,1,1,2,2),(44,6,1,1,2,2),(45,3,1,1,2,2),(46,4,1,1,2,2),(47,4,1,1,2,2),(49,10,1,1,2,2),(50,11,-1,0,0,0),(51,11,1,1,2,2),(52,12,-1,1,2,2),(53,12,1,1,2,2),(54,13,-1,1,0,2),(55,13,1,1,2,2),(56,13,-1,1,2,2),(57,13,1,1,2,2),(58,8,1,1,2,2),(59,8,1,1,2,2),(60,13,-1,1,2,2),(61,13,1,1,2,2),(62,12,-1,1,2,2),(63,12,1,1,2,2),(64,11,-1,1,2,2),(65,11,1,1,2,2),(66,4,1,1,2,2),(67,3,1,1,2,2),(68,6,1,1,2,2),(69,7,1,1,2,2),(70,5,-1,1,2,2),(71,5,1,1,2,2),(73,6,2,1,2,1),(74,6,4,0,0,0),(76,5,4,1,1,0),(77,5,2,1,2,0),(78,1,5,1,0,0),(79,7,5,1,2,1),(82,4,5,1,0,0),(83,4,5,1,0,0),(84,2,-1,1,2,2);
/*!40000 ALTER TABLE `groupuserpermission` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping data for table `natconfig`
--

LOCK TABLES `natconfig` WRITE;
/*!40000 ALTER TABLE `natconfig` DISABLE KEYS */;
INSERT INTO `natconfig` VALUES (1,'\\\\\\\\172.16.0.1\\\\SharingNATTest','nhide','123',60,'09:00:00');
/*!40000 ALTER TABLE `natconfig` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping data for table `userpermission`
--

LOCK TABLES `userpermission` WRITE;
/*!40000 ALTER TABLE `userpermission` DISABLE KEYS */;
INSERT INTO `userpermission` VALUES (6,3,1,1,2,2),(8,4,1,1,2,2),(17,8,1,1,2,2),(23,8,6,1,1,1),(25,6,1,1,2,2),(27,7,1,1,2,2),(28,9,104,1,1,1),(35,1,3,1,2,2),(36,1,3,1,2,2),(38,5,3,1,0,1),(39,5,3,1,2,0),(41,9,1,1,2,2),(42,1,1,1,2,2),(43,2,1,1,2,2),(46,5,3,1,2,2),(47,5,1,1,2,2),(48,5,3,1,2,2),(49,1,1,1,2,2),(50,1,3,1,2,2),(51,7,1,1,2,2),(52,7,3,1,2,2),(53,2,1,1,2,2),(54,2,3,1,2,2),(55,6,1,1,2,2),(56,6,3,1,2,2),(57,3,1,1,2,2),(58,3,3,1,2,2),(59,4,1,1,2,2),(60,4,3,1,2,2),(61,4,1,1,2,2),(62,4,3,1,2,2),(63,10,1,0,0,0),(64,10,3,1,2,2),(65,11,3,1,2,2),(66,12,1,1,2,2),(67,12,3,1,2,2),(68,13,1,0,0,0),(69,13,3,1,2,2),(70,13,1,1,2,2),(71,13,3,1,2,2),(72,8,1,1,2,2),(73,8,3,1,2,2),(74,9,1,1,2,2),(75,9,3,1,2,2),(76,8,1,1,2,2),(77,8,3,1,2,2),(78,13,1,1,2,2),(79,13,3,1,2,2),(80,12,1,1,2,2),(81,12,3,1,2,2),(82,11,1,1,2,2),(83,11,3,1,2,2),(84,4,1,1,2,2),(85,4,3,1,2,2),(86,3,1,1,2,2),(87,3,3,1,2,2),(88,6,1,1,2,2),(89,6,3,1,2,2),(90,7,1,1,2,2),(91,7,3,1,2,2),(92,2,1,1,2,2),(93,2,3,1,2,2),(94,5,1,0,2,2),(95,5,3,1,2,2),(96,1,1,1,2,2),(97,1,3,1,2,2),(98,2,1,1,2,2),(99,2,3,1,2,2),(100,5,3,0,2,0),(101,5,3,1,2,2),(102,5,3,1,0,2),(103,5,3,1,2,0),(104,5,1,1,2,2),(105,5,1,1,2,2),(106,1,1,1,2,2),(107,5,1,1,2,2),(108,5,1,1,2,2),(109,5,1,1,2,2),(110,2,1,1,2,2),(111,1,1,1,2,2),(113,6,1,1,2,2),(114,6,100,1,2,0),(115,6,18,1,0,1),(116,10,1,1,2,2),(117,10,2,1,2,2),(120,1,53,1,2,2),(122,5,1,1,2,2),(123,5,8,1,1,0),(124,5,85,1,1,0),(125,5,102,0,1,0),(126,5,96,1,0,0),(127,5,1,1,2,2),(128,7,95,1,2,1),(129,4,1,1,2,2),(130,12,1,0,2,0),(132,4,19,1,0,0),(133,4,53,1,2,2),(134,4,1,1,2,2),(135,4,19,1,0,0),(136,12,1,1,2,2),(137,13,1,1,2,2),(138,13,53,1,0,2),(139,2,1,1,2,2);
/*!40000 ALTER TABLE `userpermission` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping data for table `userroot`
--

LOCK TABLES `userroot` WRITE;
/*!40000 ALTER TABLE `userroot` DISABLE KEYS */;
INSERT INTO `userroot` VALUES (4,1,1,1);
/*!40000 ALTER TABLE `userroot` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping events for database 'configfeedb'
--
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2019-10-16 14:15:42
