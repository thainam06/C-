-- Optional application-only table, absent from the uploaded schema.
-- Select the target database first. Run once if you want to use Tham so/PDF settings.
CREATE TABLE `ThamSo` (
  `MaThamSo` varchar(50) NOT NULL,
  `GiaTri` varchar(255) NOT NULL,
  `MoTa` varchar(255) NOT NULL,
  PRIMARY KEY (`MaThamSo`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
