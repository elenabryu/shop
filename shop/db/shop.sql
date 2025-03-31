-- MySQL dump 10.13  Distrib 8.0.41, for Win64 (x86_64)
--
-- Host: localhost    Database: shop
-- ------------------------------------------------------
-- Server version	8.0.41

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
-- Table structure for table `brand`
--
create database shop;
use shop;
DROP TABLE IF EXISTS `brand`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `brand` (
  `BrandID` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(50) NOT NULL,
  `Description` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`BrandID`),
  UNIQUE KEY `Name` (`Name`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `brand`
--

LOCK TABLES `brand` WRITE;
/*!40000 ALTER TABLE `brand` DISABLE KEYS */;
INSERT INTO `brand` VALUES (1,'Chanel','Французский дом моды, известный своей парфюмерией и косметикой'),(2,'Dior','Французский дом моды, выпускающий парфюмерию, косметику и одежду'),(3,'Lancome','Французская марка косметики и парфюмерии'),(4,'Estee Lauder','Американская марка косметики и парфюмерии'),(5,'MAC Cosmetics','Канадская марка декоративной косметики'),(6,'L\'Oreal Paris','Французская марка косметики и парфюмерии');
/*!40000 ALTER TABLE `brand` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `category`
--

DROP TABLE IF EXISTS `category`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `category` (
  `CategoryID` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(50) NOT NULL,
  `Description` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`CategoryID`),
  UNIQUE KEY `Name` (`Name`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `category`
--

LOCK TABLES `category` WRITE;
/*!40000 ALTER TABLE `category` DISABLE KEYS */;
INSERT INTO `category` VALUES (1,'Парфюмерия','Ароматы для мужчин и женщин'),(2,'Декоративная косметика','Средства для макияжа'),(3,'Уход за кожей','Средства для очищения, увлажнения и питания кожи'),(4,'Уход за волосами','Шампуни, бальзамы, маски и средства для укладки'),(5,'Аксессуары','Кисти, спонжи, косметички и другие аксессуары');
/*!40000 ALTER TABLE `category` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `client`
--

DROP TABLE IF EXISTS `client`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `client` (
  `ClientID` int NOT NULL AUTO_INCREMENT,
  `ClientSurname` varchar(50) NOT NULL,
  `ClientName` varchar(50) NOT NULL,
  `ClientPatronymic` varchar(50) NOT NULL,
  `Email` varchar(100) DEFAULT NULL,
  `PhoneNumber` varchar(11) NOT NULL,
  PRIMARY KEY (`ClientID`)
) ENGINE=InnoDB AUTO_INCREMENT=51 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `client`
--

LOCK TABLES `client` WRITE;
/*!40000 ALTER TABLE `client` DISABLE KEYS */;
INSERT INTO `client` VALUES (1,'Смирнов','Алексей','Иванович','smirnov@example.com','89161112233'),(2,'Иванова','Мария','Петровна','ivanova@example.com','89264445566'),(3,'Петров','Дмитрий','Сергеевич','petrov@example.com','89037778899'),(4,'Сидорова','Екатерина','Алексеевна','sidorova@example.com','89670001122'),(5,'Кузнецов','Андрей','Михайлович','kuznetsov@example.com','89853334455'),(6,'Попова','Ольга','Ивановна','popova@example.com','89169990011'),(7,'Васильев','Сергей','Дмитриевич','vasiliev@example.com','89266667788'),(8,'Соколова','Наталья','Андреевна','sokolova@example.com','89032223344'),(9,'Михайлов','Игорь','Олегович','mihailov@example.com','89675556677'),(10,'Федорова','Татьяна','Юрьевна','fedorova@example.com','89858889900'),(11,'Морозов','Артем','Павлович','morozov@example.com','89164447788'),(12,'Николаева','Светлана','Викторовна','nikolaeva@example.com','89261114455'),(13,'Лебедев','Роман','Андреевич','lebedev@example.com','89033336677'),(14,'Волкова','Анастасия','Дмитриевна','volkova@example.com','89677779900'),(15,'Егоров','Владимир','Сергеевич','egorov@example.com','89850002233'),(16,'Степанова','Елена','Александровна','stepanova@example.com','89166668899'),(17,'Орлов','Максим','Игоревич','orlov@example.com','89269991122'),(18,'Белова','Юлия','Олеговна','belova@example.com','89035557788'),(19,'Жуков','Антон','Андреевич','zhukov@example.com','89672224455'),(20,'Козлова','Дарья','Викторовна','kozlova@example.com','89855550011'),(21,'Новиков','Павел','Алексеевич','novikov@example.com','89168883344'),(22,'Соловьева','Евгения','Павловна','soloveva@example.com','89263335566'),(23,'Богданов','Денис','Игоревич','bogdanov@example.com','89036669900'),(24,'Яковлева','Маргарита','Андреевна','yakovleva@example.com','89679992233'),(25,'Григорьев','Илья','Сергеевич','grigorev@example.com','89852225566'),(26,'Андреева','Кристина','Дмитриевна','andreeva@example.com','89165558899'),(27,'Филиппов','Георгий','Алексеевич','filippov@example.com','89268881122'),(28,'Сергеева','Вероника','Игоревна','sergeeva@example.com','89031114455'),(29,'Воробьев','Никита','Олегович','vorobev@example.com','89674447788'),(30,'Александрова','Алина','Павловна','alexandrova@example.com','89857770011'),(31,'Ефимов','Степан','Андреевич','efimov@example.com','89160003344'),(32,'Родионова','Полина','Викторовна','rodionova@example.com','89262226677'),(33,'Данилов','Артур','Алексеевич','danilov@example.com','89034449900'),(34,'Зайцева','Анна','Игоревна','zaitseva@example.com','89676662233'),(35,'Кузьмин','Вадим','Олегович','kuzmin@example.com','89859995566'),(36,'Симонова','Александра','Павловна','simonova@example.com','89163338899'),(37,'Беляев','Егор','Андреевич','belyaev@example.com','89265551122'),(38,'Денисова','Валерия','Викторовна','denisova@example.com','89038884455'),(39,'Осипов','Кирилл','Алексеевич','osipov@example.com','89671117788'),(40,'Русакова','Варвара','Игоревна','rusakova@example.com','89854440011'),(41,'Герасимов','Иван','Олегович','gerasimov@example.com','89167773344'),(42,'Виноградова','София','Павловна','vinogradova@example.com','89260005566'),(43,'Ершов','Глеб','Андреевич','ershov@example.com','89032229900'),(44,'Панова','Виктория','Викторовна','panova@example.com','89673332233'),(45,'Тимофеев','Арсений','Алексеевич','timofeev@example.com','89856665566'),(46,'Журавлева','Ульяна','Игоревна','zhuravleva@example.com','89169998899'),(47,'Макаров','Лев','Олегович','makarov@example.com','89261111122'),(48,'Захарова','Полина','Павловна','zaharova@example.com','89035554455'),(49,'Ширяев','Михаил','Андреевич','shiryaev@example.com','89678887788'),(50,'Звездина','Виктория','Алексеевна','zvezdina@mail.ru','89765478394');
/*!40000 ALTER TABLE `client` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `employee`
--

DROP TABLE IF EXISTS `employee`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `employee` (
  `EmployeeID` int NOT NULL AUTO_INCREMENT,
  `EmployeeSurname` varchar(50) NOT NULL,
  `EmployeeName` varchar(50) NOT NULL,
  `EmployeePatronymic` varchar(50) NOT NULL,
  `Email` varchar(100) NOT NULL,
  `PhoneNumber` varchar(11) NOT NULL,
  `Address` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`EmployeeID`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `employee`
--

LOCK TABLES `employee` WRITE;
/*!40000 ALTER TABLE `employee` DISABLE KEYS */;
INSERT INTO `employee` VALUES (1,'Иванов','Иван','Иванович','ivanov@example.com','89161234567','Москва, ул. Тверская, 1'),(2,'Петров','Петр','Петрович','petrov@example.com','89267890123','Санкт-Петербург, Невский пр., 2'),(3,'Сидоров','Сидор','Сидорович','sidorov@example.com','89034567890','Казань, ул. Баумана, 3'),(4,'Смирнова','Елена','Сергеевна','smirnova@example.com','89671112233','Екатеринбург, пр. Ленина, 4'),(5,'Кузнецова','Ольга','Алексеевна','kuznetsova@example.com','89854445566','Новосибирск, Красный пр., 5');
/*!40000 ALTER TABLE `employee` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `product`
--

DROP TABLE IF EXISTS `product`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `product` (
  `ProductID` int NOT NULL AUTO_INCREMENT,
  `ProductName` varchar(50) NOT NULL,
  `ProductDescription` varchar(255) DEFAULT NULL,
  `Price` decimal(10,2) NOT NULL,
  `StockQuantity` int NOT NULL,
  `CategoryID` int DEFAULT NULL,
  `BrandID` int DEFAULT NULL,
  `ProductPhoto` varchar(255) DEFAULT NULL,
  `SupplierID` int DEFAULT NULL,
  PRIMARY KEY (`ProductID`),
  KEY `CategoryID` (`CategoryID`),
  KEY `BrandID` (`BrandID`),
  KEY `SupplierID` (`SupplierID`),
  CONSTRAINT `product_ibfk_1` FOREIGN KEY (`CategoryID`) REFERENCES `category` (`CategoryID`),
  CONSTRAINT `product_ibfk_2` FOREIGN KEY (`BrandID`) REFERENCES `brand` (`BrandID`),
  CONSTRAINT `product_ibfk_3` FOREIGN KEY (`SupplierID`) REFERENCES `supplier` (`SupplierID`)
) ENGINE=InnoDB AUTO_INCREMENT=50 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `product`
--

LOCK TABLES `product` WRITE;
/*!40000 ALTER TABLE `product` DISABLE KEYS */;
INSERT INTO `product` VALUES (1,'Chanel No. 5','Классический аромат для женщин',12000.00,10,1,1,'chanel_no5.jpg',1),(2,'Dior Sauvage','Мужской аромат',9500.00,15,1,2,'dior_sauvage.jpg',2),(3,'Lancome Hypnose','Тушь для ресниц',2500.00,20,2,3,'lancome_hypnose.jpg',3),(4,'Estee Lauder Advanced Night Repair','Сыворотка для лица',7000.00,12,3,4,'estee_lauder_anr.jpg',4),(5,'MAC Lipstick Ruby Woo','Красная помада',1800.00,25,2,5,'mac_ruby_woo.jpg',1),(6,'L\'Oreal Elvive Shampoo','Шампунь для поврежденных волос',400.00,50,4,6,'loreal_elvive_shampoo.jpg',2),(7,'Chanel Coco Mademoiselle','Аромат для женщин',11000.00,8,1,1,'chanel_coco.jpg',1),(8,'Dior J\'adore','Аромат для женщин',10000.00,13,1,2,'dior_jadore.jpg',2),(9,'Lancome La Vie Est Belle','Аромат для женщин',9000.00,17,1,3,'lancome_lavie.jpg',3),(10,'Estee Lauder Double Wear Foundation','Тональный крем',4500.00,10,2,4,'estee_lauder_dw.jpg',4),(11,'MAC Eyeshadow Palette','Палетка теней для век',3500.00,15,2,5,'mac_eyeshadow.jpg',1),(12,'L\'Oreal Hair Mask','Маска для волос',600.00,40,4,6,'loreal_hair_mask.jpg',2),(13,'Chanel Bleu de Chanel','Мужской аромат',10500.00,9,1,1,'chanel_bleu.jpg',1),(14,'Dior Miss Dior','Аромат для женщин',9800.00,14,1,2,'dior_missdior.jpg',2),(15,'Lancome Teint Miracle Foundation','Тональный крем',4200.00,11,2,3,'lancome_teint.jpg',3),(16,'Estee Lauder DayWear Cream','Дневной крем для лица',5500.00,16,3,4,'estee_lauder_dwcream.jpg',4),(17,'MAC Blush','Румяна',2200.00,20,2,5,'mac_blush.jpg',1),(18,'L\'Oreal Volume Filler Conditioner','Кондиционер для объема волос',450.00,45,4,6,'loreal_vf_conditioner.jpg',2),(19,'Chanel Rouge Allure Velvet Lipstick','Матовая помада',3200.00,18,2,1,'chanel_rouge_allure.jpg',1),(20,'Dior Addict Lip Glow','Бальзам для губ',2800.00,22,2,2,'dior_addict_lip.jpg',2),(21,'Lancome Bi-Facil Eye Makeup Remover','Средство для снятия макияжа с глаз',1500.00,30,3,3,'lancome_bifacil.jpg',3),(22,'Estee Lauder Micro Essence','Микро-эссенция для лица',6000.00,13,3,4,'estee_lauder_micro.jpg',4),(23,'MAC Prep + Prime Fix+','Спрей для фиксации макияжа',2000.00,25,2,5,'mac_fixplus.jpg',1),(24,'L\'Oreal Extraordinary Oil','Масло для волос',700.00,35,4,6,'loreal_exo.jpg',2),(25,'Chanel Chance Eau Tendre','Аромат для женщин',11500.00,10,1,1,'chanel_chance.jpg',1),(26,'Dior Homme','Мужской аромат',9200.00,15,1,2,'dior_homme.jpg',2),(27,'Lancome Grandiose Mascara','Тушь для ресниц',2600.00,20,2,3,'lancome_grand.jpg',3),(28,'Estee Lauder Revitalizing Supreme+ Cream','Крем для лица',7500.00,12,3,4,'estee_lauder_revitalizing.jpg',4),(29,'MAC Studio Fix Fluid Foundation','Тональный крем',3800.00,25,2,5,'mac_studiofix.jpg',1),(30,'L\'Oreal Color Riche Lipstick','Помада для губ',550.00,50,2,6,'loreal_colorriche.jpg',2),(31,'Chanel Le Vernis Nail Colour','Лак для ногтей',2000.00,18,2,1,'chanel_le_vernis.jpg',1),(32,'Dior Forever Skin Glow Foundation','Тональный крем',4800.00,22,2,2,'dior_forever.jpg',2),(33,'Lancome Tonique Confort','Успокаивающий тоник для лица',1800.00,30,3,3,'lancome_tonique.jpg',3),(34,'Estee Lauder Perfectionist Pro Serum','Сыворотка для лица',6500.00,13,3,4,'estee_lauder_perfectionist.jpg',4),(35,'MAC Mineralize Skinfinish','Пудра для лица',3000.00,25,2,5,'mac_mineralize.jpg',1),(36,'L\'Oreal Elseve Dream Lengths Leave-In','Несмываемый уход для волос',500.00,35,4,6,'loreal_dreamlengths.jpg',2),(37,'Chanel Hydra Beauty Micro Serum','Увлажняющая сыворотка для лица',8000.00,10,3,1,'chanel_hydra_beauty.jpg',1),(38,'Dior Backstage Eyeshadow Palette','Палетка теней для век',5000.00,15,2,2,'dior_backstage.jpg',2),(39,'Lancome Advanced Genifique Serum','Сыворотка для лица',7200.00,20,3,3,'lancome_genifique.jpg',3),(40,'Estee Lauder Sumptuous Extreme Mascara','Тушь для ресниц',2700.00,12,2,4,'estee_lauder_sumptuous.jpg',4),(41,'MAC Strobe Cream','Крем-хайлайтер',2500.00,25,2,5,'mac_strobe.jpg',1),(42,'L\'Oreal Studio Line Hair Gel','Гель для волос',350.00,50,4,6,'loreal_studioline.jpg',2),(43,'Chanel Sublimage La Creeeme','Крем для лица',25000.00,5,3,1,'chanel_sublimage.jpg',1),(44,'Dior Rouge Dior Lipstick','Помада для губ',3500.00,20,2,2,'dior_rougedior.jpg',2),(45,'Lancome Renergie Multi-Lift Cream','Крем для лица',8500.00,10,3,3,'lancome_renergie.jpg',3),(46,'Estee Lauder Bronze Goddess Bronzer','Бронзер',3200.00,15,2,4,'estee_lauder_bronze.jpg',4),(47,'MAC Pro Longwear Paint Pot','Кремовые тени',2100.00,22,2,5,'mac_paintpot.jpg',1),(48,'L\'Oreal Age Perfect Cleansing Milk','Молочко для снятия макияжа',400.00,30,3,6,'loreal_ageperfect.jpg',4),(49,'Chanel Boy de Chanel Foundation','Тональный крем для мужчин',6000.00,8,2,1,'chanel_boy.jpg',1);
/*!40000 ALTER TABLE `product` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `role`
--

DROP TABLE IF EXISTS `role`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `role` (
  `RoleID` int NOT NULL AUTO_INCREMENT,
  `RoleName` varchar(50) NOT NULL,
  PRIMARY KEY (`RoleID`),
  UNIQUE KEY `RoleName` (`RoleName`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `role`
--

LOCK TABLES `role` WRITE;
/*!40000 ALTER TABLE `role` DISABLE KEYS */;
INSERT INTO `role` VALUES (1,'Администратор'),(2,'Продавец');
/*!40000 ALTER TABLE `role` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `sale`
--

DROP TABLE IF EXISTS `sale`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sale` (
  `SaleID` int NOT NULL AUTO_INCREMENT,
  `ClientID` int DEFAULT NULL,
  `EmployeeID` int DEFAULT NULL,
  `SaleDate` datetime DEFAULT CURRENT_TIMESTAMP,
  `TotalAmount` decimal(10,2) NOT NULL,
  `Discount` decimal(10,2) NOT NULL,
  `SaleStatus` varchar(20) NOT NULL,
  PRIMARY KEY (`SaleID`),
  KEY `ClientID` (`ClientID`),
  KEY `EmployeeID` (`EmployeeID`),
  CONSTRAINT `sale_ibfk_1` FOREIGN KEY (`ClientID`) REFERENCES `client` (`ClientID`),
  CONSTRAINT `sale_ibfk_2` FOREIGN KEY (`EmployeeID`) REFERENCES `employee` (`EmployeeID`)
) ENGINE=InnoDB AUTO_INCREMENT=51 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `sale`
--

LOCK TABLES `sale` WRITE;
/*!40000 ALTER TABLE `sale` DISABLE KEYS */;
INSERT INTO `sale` VALUES (1,1,2,'2023-10-26 10:00:00',13800.00,1380.00,'Завершен'),(2,2,3,'2025-01-05 11:00:00',2000.00,0.00,'Завершен'),(3,3,2,'2025-02-26 12:00:00',9500.00,950.00,'Завершен'),(4,4,3,'2025-10-26 13:00:00',9000.00,900.00,'Отменен'),(5,5,2,'2025-02-26 14:00:00',4200.00,0.00,'Завершен'),(6,6,3,'2025-10-26 15:00:00',7500.00,750.00,'Завершен'),(7,7,2,'2025-10-26 16:00:00',7000.00,700.00,'Завершен'),(8,8,3,'2025-01-26 17:00:00',1200.00,0.00,'Завершен'),(9,9,2,'2025-10-26 18:00:00',11000.00,1100.00,'Завершен'),(10,10,3,'2025-01-26 19:00:00',10000.00,1000.00,'Завершен'),(11,11,2,'2025-01-27 10:00:00',9000.00,900.00,'Завершен'),(12,12,3,'2025-01-27 11:00:00',7000.00,700.00,'Завершен'),(13,13,2,'2025-01-27 12:00:00',10500.00,1050.00,'Отменен'),(14,14,3,'2025-01-27 13:00:00',9800.00,980.00,'Завершен'),(15,15,2,'2025-02-27 14:00:00',11000.00,1100.00,'Завершен'),(16,16,3,'2025-02-27 15:00:00',2200.00,0.00,'Завершен'),(17,17,2,'2025-02-27 16:00:00',1350.00,0.00,'Завершен'),(18,18,3,'2025-02-27 17:00:00',6400.00,640.00,'Завершен'),(19,19,2,'2025-02-27 18:00:00',2800.00,0.00,'Завершен'),(20,20,3,'2025-02-27 19:00:00',6000.00,600.00,'Завершен'),(21,21,2,'2025-02-28 10:00:00',6000.00,600.00,'Отменен'),(22,22,3,'2025-02-28 11:00:00',4000.00,0.00,'Завершен'),(23,23,2,'2025-01-28 12:00:00',700.00,0.00,'Завершен'),(24,24,3,'2025-02-28 13:00:00',11500.00,1150.00,'Завершен'),(25,25,2,'2025-02-28 14:00:00',9200.00,920.00,'Завершен'),(26,26,3,'2025-02-28 15:00:00',5200.00,520.00,'Завершен'),(27,27,2,'2025-02-28 16:00:00',7500.00,750.00,'Завершен'),(28,28,3,'2025-02-28 17:00:00',7600.00,760.00,'Завершен'),(29,29,2,'2025-02-28 18:00:00',1650.00,0.00,'Завершен'),(30,30,3,'2025-02-28 19:00:00',2000.00,0.00,'Завершен'),(31,31,2,'2025-02-24 10:00:00',9600.00,960.00,'Завершен'),(32,32,3,'2025-01-29 11:00:00',7200.00,720.00,'Завершен'),(33,33,2,'2025-02-23 12:00:00',6500.00,650.00,'Завершен'),(34,34,3,'2025-02-22 13:00:00',6000.00,600.00,'Завершен'),(35,35,2,'2025-02-17 14:00:00',2500.00,0.00,'Завершен'),(36,36,3,'2025-02-13 15:00:00',8000.00,800.00,'Завершен'),(37,37,2,'2025-02-27 16:00:00',5000.00,500.00,'Завершен'),(38,38,3,'2025-02-25 17:00:00',7200.00,720.00,'Завершен'),(39,39,2,'2025-02-16 18:00:00',5400.00,540.00,'Отменен'),(40,40,3,'2025-02-06 19:00:00',7500.00,750.00,'Завершен'),(41,41,2,'2025-03-06 10:00:00',1400.00,0.00,'Завершен'),(42,42,3,'2025-02-23 11:00:00',25000.00,2500.00,'Завершен'),(43,43,2,'2025-03-04 12:00:00',7000.00,700.00,'Завершен'),(44,44,3,'2025-01-30 13:00:00',8500.00,850.00,'Завершен'),(45,45,2,'2025-03-05 14:00:00',6400.00,640.00,'Завершен'),(46,46,3,'2025-03-05 15:00:00',6300.00,630.00,'Завершен'),(47,47,2,'2025-03-02 16:00:00',400.00,0.00,'Завершен'),(48,48,3,'2025-02-12 17:00:00',6000.00,600.00,'Завершен'),(49,49,2,'2025-01-30 18:00:00',12000.00,1200.00,'Отменен'),(50,50,3,'2025-02-23 19:00:00',3600.00,0.00,'Завершен');
/*!40000 ALTER TABLE `sale` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `saledetail`
--

DROP TABLE IF EXISTS `saledetail`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `saledetail` (
  `SaleID` int NOT NULL,
  `ProductID` int NOT NULL,
  `Quantity` int NOT NULL,
  `Price` decimal(10,2) NOT NULL,
  PRIMARY KEY (`SaleID`,`ProductID`),
  KEY `ProductID` (`ProductID`),
  CONSTRAINT `saledetail_ibfk_1` FOREIGN KEY (`SaleID`) REFERENCES `sale` (`SaleID`),
  CONSTRAINT `saledetail_ibfk_2` FOREIGN KEY (`ProductID`) REFERENCES `product` (`ProductID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `saledetail`
--

LOCK TABLES `saledetail` WRITE;
/*!40000 ALTER TABLE `saledetail` DISABLE KEYS */;
INSERT INTO `saledetail` VALUES (1,1,1,12000.00),(1,5,2,1800.00),(2,6,5,400.00),(3,2,1,9500.00),(4,10,2,4500.00),(5,15,1,4200.00),(6,3,3,2500.00),(7,4,1,7000.00),(8,12,2,600.00),(9,7,1,11000.00),(10,8,1,10000.00),(11,9,1,9000.00),(12,11,2,3500.00),(13,13,1,10500.00),(14,14,1,9800.00),(15,16,2,5500.00),(16,17,1,2200.00),(17,18,3,450.00),(18,19,2,3200.00),(19,20,1,2800.00),(20,21,4,1500.00),(21,22,1,6000.00),(22,23,2,2000.00),(23,24,1,700.00),(24,25,1,11500.00),(25,26,1,9200.00),(26,27,2,2600.00),(27,28,1,7500.00),(28,29,2,3800.00),(29,30,3,550.00),(30,31,1,2000.00),(31,32,2,4800.00),(32,33,4,1800.00),(33,34,1,6500.00),(34,35,2,3000.00),(35,36,5,500.00),(36,37,1,8000.00),(37,38,1,5000.00),(38,39,1,7200.00),(39,40,2,2700.00),(40,41,3,2500.00),(41,42,4,350.00),(42,43,1,25000.00),(43,44,2,3500.00),(44,45,1,8500.00),(45,46,2,3200.00),(46,47,3,2100.00),(47,48,1,400.00),(48,49,1,6000.00),(49,1,1,12000.00),(50,5,2,1800.00);
/*!40000 ALTER TABLE `saledetail` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `supplier`
--

DROP TABLE IF EXISTS `supplier`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `supplier` (
  `SupplierID` int NOT NULL AUTO_INCREMENT,
  `SupplierName` varchar(50) NOT NULL,
  PRIMARY KEY (`SupplierID`),
  UNIQUE KEY `SupplierName` (`SupplierName`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `supplier`
--

LOCK TABLES `supplier` WRITE;
/*!40000 ALTER TABLE `supplier` DISABLE KEYS */;
INSERT INTO `supplier` VALUES (3,'ЗАО \"Бьюти-Импорт\"'),(2,'ИП \"Парфюм-Сервис\"'),(4,'ООО \"Восток-Запад\"'),(1,'ООО \"Косметик-Трейд\"');
/*!40000 ALTER TABLE `supplier` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `user`
--

DROP TABLE IF EXISTS `user`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `user` (
  `UserID` int NOT NULL AUTO_INCREMENT,
  `UserEmployeeID` int NOT NULL,
  `UserLogin` varchar(50) NOT NULL,
  `UserPassword` varchar(255) NOT NULL,
  `UserRole` int NOT NULL,
  PRIMARY KEY (`UserID`),
  UNIQUE KEY `UserEmployeeID` (`UserEmployeeID`),
  UNIQUE KEY `UserLogin` (`UserLogin`),
  KEY `UserRole` (`UserRole`),
  CONSTRAINT `user_ibfk_1` FOREIGN KEY (`UserRole`) REFERENCES `role` (`RoleID`),
  CONSTRAINT `user_ibfk_2` FOREIGN KEY (`UserEmployeeID`) REFERENCES `employee` (`EmployeeID`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `user`
--

LOCK TABLES `user` WRITE;
/*!40000 ALTER TABLE `user` DISABLE KEYS */;
INSERT INTO `user` VALUES (1,1,'admin','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918',1),(2,2,'petrov_seller','0ecb2a914d6e4f47b2c64da04eee284635b760bae5e471e5b2c8890d8d911731',2),(3,3,'sidorov_seller','16290066e3df58ede0452a593839f209ae045e63639f1d8cae425100a15288c8',2),(4,4,'smirnova_seller','9523641f425d823c48f1d86dd6c7d1736cc5846a3eae9ede8dceb647f9f10456',2),(5,5,'seller','a4279eae47aaa7417da62434795a011ccb0ec870f7f56646d181b5500a892a9a',2);
/*!40000 ALTER TABLE `user` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-03-06 18:29:38
