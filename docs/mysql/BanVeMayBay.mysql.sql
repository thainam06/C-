-- Converted from the supplied SQL Server schema. MySQL 8.4+, UTF-8/InnoDB.
-- Run in a NEW EMPTY database selected in MySQL Workbench. No USE, DROP or data inserts.
-- Existing objects cause an error: do not use --force or disable foreign_key_checks.
-- MySQL DDL auto-commits; if import fails, inspect the partial NEW database before retrying.
SET NAMES utf8mb4;

CREATE TABLE `ChiTietHoaDon` (
  `MaHD` varchar(10) NOT NULL,
  `MaVe` varchar(10) NOT NULL,
  `DonGiaVe` decimal(18, 0) NOT NULL,
  `SL` int NOT NULL DEFAULT 1,
  `ThanhTien` decimal(29,0) GENERATED ALWAYS AS (SL * DonGiaVe) STORED,
  PRIMARY KEY (`MaHD`, `MaVe`),
  CONSTRAINT `CK_ChiTietHoaDon_1` CHECK ((`SL`>(0))),
  CONSTRAINT `CK_ChiTietHoaDon_DonGiaVe` CHECK ((`DonGiaVe`>=(0)))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `ChuyenBay` (
  `MaChuyenBay` varchar(10) NOT NULL,
  `MaHHK` varchar(10) NOT NULL,
  `MaSanBayDi` varchar(10) NOT NULL,
  `MaSanBayDen` varchar(10) NOT NULL,
  `MaMayBay` varchar(10) NOT NULL,
  `NgayKhoiHanh` datetime(3) NOT NULL,
  `NgayDen` datetime(3) NOT NULL,
  `ThoiGianBay` int NULL,
  `ThoiGianDen` time(6) NULL,
  `TongSoGhe` int NOT NULL,
  `SoVeDaBan` int NOT NULL DEFAULT 0,
  `GiaVe` decimal(18, 0) NOT NULL,
  `TrangThai` varchar(30) NOT NULL DEFAULT 'Chưa khởi hành',
  PRIMARY KEY (`MaChuyenBay`),
  CONSTRAINT `CK_ChuyenBay_1` CHECK ((`GiaVe`>=(0))),
  CONSTRAINT `CK_ChuyenBay_2` CHECK ((`SoVeDaBan`>=(0))),
  CONSTRAINT `CK_ChuyenBay_3` CHECK ((`TongSoGhe`>=(0))),
  CONSTRAINT `CK_CB_Ngay` CHECK ((`NgayDen`>=`NgayKhoiHanh`)),
  CONSTRAINT `CK_CB_SanBay` CHECK ((`MaSanBayDi`<>`MaSanBayDen`)),
  CONSTRAINT `CK_CB_SoVe` CHECK ((`SoVeDaBan`<=`TongSoGhe`))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `CTKM` (
  `MaCTKM` varchar(10) NOT NULL,
  `TenCTKM` varchar(100) NOT NULL,
  `NgayBatDau` date NOT NULL,
  `NgayKetThuc` date NOT NULL,
  PRIMARY KEY (`MaCTKM`),
  CONSTRAINT `CK_CTKM_Ngay` CHECK ((`NgayKetThuc`>=`NgayBatDau`))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `DatVe` (
  `MaDV` varchar(10) NOT NULL,
  `MaKH` varchar(10) NOT NULL,
  `MaNV` varchar(10) NULL,
  `MaChuyenBay` varchar(10) NOT NULL,
  `NgayDat` datetime(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
  `TongTien` decimal(18, 0) NOT NULL DEFAULT 0,
  `GiamGia` decimal(18, 2) NOT NULL DEFAULT 0,
  `TrangThai` varchar(30) NOT NULL DEFAULT 'Chờ thanh toán',
  `MaVoucher` varchar(20) NULL,
  PRIMARY KEY (`MaDV`),
  CONSTRAINT `UQ_DatVe_1` UNIQUE (`MaDV`, `MaChuyenBay`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `Ghe` (
  `MaGhe` varchar(10) NOT NULL,
  `MaMayBay` varchar(10) NOT NULL,
  `MaHV` varchar(10) NOT NULL,
  PRIMARY KEY (`MaGhe`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `HangHangKhong` (
  `MaHHK` varchar(10) NOT NULL,
  `TenHang` varchar(100) NOT NULL,
  PRIMARY KEY (`MaHHK`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `HangVe` (
  `MaHV` varchar(10) NOT NULL,
  `TenHV` varchar(50) NOT NULL,
  `HanhLyKg` int NOT NULL,
  `DonGiaVe` decimal(18, 0) NOT NULL,
  PRIMARY KEY (`MaHV`),
  CONSTRAINT `CK_HangVe_1` CHECK ((`DonGiaVe`>=(0))),
  CONSTRAINT `CK_HangVe_2` CHECK ((`HanhLyKg`>=(0)))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `HanhKhach` (
  `MaHK` varchar(10) NOT NULL,
  `HoTen` varchar(100) NOT NULL,
  `NgaySinh` date NULL,
  `CCCD` varchar(12) NULL,
  `GioiTinh` varchar(10) NULL,
  `MaKH` varchar(10) NOT NULL,
  `LoaiHK` varchar(30) NULL,
  PRIMARY KEY (`MaHK`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `HoaDon` (
  `MaHD` varchar(10) NOT NULL,
  `MaKH` varchar(10) NOT NULL,
  `MaNV` varchar(10) NULL,
  `MaDV` varchar(10) NOT NULL,
  `NgayLap` datetime(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
  `TongTien` decimal(18, 0) NOT NULL DEFAULT 0,
  `PhuongThucTT` varchar(30) NULL,
  `TrangThai` varchar(30) NOT NULL DEFAULT 'Chưa thanh toán',
  PRIMARY KEY (`MaHD`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `KhachHang` (
  `MaKH` varchar(10) NOT NULL,
  `HoTen` varchar(100) NOT NULL,
  `CCCD` varchar(12) NOT NULL,
  `SDT` varchar(15) NULL,
  `Email` varchar(100) NULL,
  `TaiKhoan` varchar(50) NOT NULL,
  `MatKhau` varchar(255) NOT NULL,
  `DiaChi` varchar(200) NULL,
  PRIMARY KEY (`MaKH`),
  CONSTRAINT `UQ_KhachHang_1` UNIQUE (`CCCD`),
  CONSTRAINT `UQ_KhachHang_2` UNIQUE (`TaiKhoan`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `MayBay` (
  `MaMayBay` varchar(10) NOT NULL,
  `MaHHK` varchar(10) NOT NULL,
  PRIMARY KEY (`MaMayBay`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `NhanVien` (
  `MaNV` varchar(10) NOT NULL,
  `Ho` varchar(50) NOT NULL,
  `Ten` varchar(50) NOT NULL,
  `ChucVu` varchar(50) NULL,
  PRIMARY KEY (`MaNV`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `SanBay` (
  `MaSanBay` varchar(10) NOT NULL,
  `Ten` varchar(100) NOT NULL,
  `LoaiSanBay` varchar(50) NULL,
  PRIMARY KEY (`MaSanBay`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `Ve` (
  `MaVe` varchar(10) NOT NULL,
  `MaChuyenBay` varchar(10) NOT NULL,
  `MaHV` varchar(10) NOT NULL,
  `MaGhe` varchar(10) NOT NULL,
  `MaHK` varchar(10) NOT NULL,
  `DonGiaVe` decimal(18, 0) NOT NULL,
  `Ngay` datetime(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
  `MaDV` varchar(10) NULL,
  PRIMARY KEY (`MaVe`),
  CONSTRAINT `UQ_Ve_1` UNIQUE (`MaChuyenBay`, `MaGhe`),
  CONSTRAINT `CK__Ve__DonGiaVe__00200768` CHECK ((`DonGiaVe`>=(0)))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `Voucher` (
  `MaVoucher` varchar(20) NOT NULL,
  `MaCTKM` varchar(10) NOT NULL,
  `GiamGia` decimal(18, 2) NOT NULL,
  PRIMARY KEY (`MaVoucher`),
  CONSTRAINT `CK_Voucher_1` CHECK ((`GiamGia`>=(0)))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

ALTER TABLE `ChiTietHoaDon` ADD CONSTRAINT `FK_ChiTietHoaDon_HoaDon` FOREIGN KEY (`MaHD`) REFERENCES `HoaDon` (`MaHD`);
ALTER TABLE `ChiTietHoaDon` ADD CONSTRAINT `FK_ChiTietHoaDon_Ve` FOREIGN KEY (`MaVe`) REFERENCES `Ve` (`MaVe`);
ALTER TABLE `ChuyenBay` ADD CONSTRAINT `FK_CB_HHK` FOREIGN KEY (`MaHHK`) REFERENCES `HangHangKhong` (`MaHHK`);
ALTER TABLE `ChuyenBay` ADD CONSTRAINT `FK_CB_MayBay` FOREIGN KEY (`MaMayBay`) REFERENCES `MayBay` (`MaMayBay`);
ALTER TABLE `ChuyenBay` ADD CONSTRAINT `FK_CB_SBDen` FOREIGN KEY (`MaSanBayDen`) REFERENCES `SanBay` (`MaSanBay`);
ALTER TABLE `ChuyenBay` ADD CONSTRAINT `FK_CB_SBDi` FOREIGN KEY (`MaSanBayDi`) REFERENCES `SanBay` (`MaSanBay`);
ALTER TABLE `DatVe` ADD CONSTRAINT `FK_DV_CB` FOREIGN KEY (`MaChuyenBay`) REFERENCES `ChuyenBay` (`MaChuyenBay`);
ALTER TABLE `DatVe` ADD CONSTRAINT `FK_DV_KH` FOREIGN KEY (`MaKH`) REFERENCES `KhachHang` (`MaKH`);
ALTER TABLE `DatVe` ADD CONSTRAINT `FK_DV_NV` FOREIGN KEY (`MaNV`) REFERENCES `NhanVien` (`MaNV`);
ALTER TABLE `DatVe` ADD CONSTRAINT `FK_DV_Voucher` FOREIGN KEY (`MaVoucher`) REFERENCES `Voucher` (`MaVoucher`);
ALTER TABLE `Ghe` ADD CONSTRAINT `FK_Ghe_HangVe` FOREIGN KEY (`MaHV`) REFERENCES `HangVe` (`MaHV`);
ALTER TABLE `Ghe` ADD CONSTRAINT `FK_Ghe_MayBay` FOREIGN KEY (`MaMayBay`) REFERENCES `MayBay` (`MaMayBay`);
ALTER TABLE `HanhKhach` ADD CONSTRAINT `FK_HK_KH` FOREIGN KEY (`MaKH`) REFERENCES `KhachHang` (`MaKH`);
ALTER TABLE `HoaDon` ADD CONSTRAINT `FK_HD_DV` FOREIGN KEY (`MaDV`) REFERENCES `DatVe` (`MaDV`);
ALTER TABLE `HoaDon` ADD CONSTRAINT `FK_HD_KH` FOREIGN KEY (`MaKH`) REFERENCES `KhachHang` (`MaKH`);
ALTER TABLE `HoaDon` ADD CONSTRAINT `FK_HD_NV` FOREIGN KEY (`MaNV`) REFERENCES `NhanVien` (`MaNV`);
ALTER TABLE `MayBay` ADD CONSTRAINT `FK_MayBay_HHK` FOREIGN KEY (`MaHHK`) REFERENCES `HangHangKhong` (`MaHHK`);
ALTER TABLE `Ve` ADD CONSTRAINT `FK_Ve_CB` FOREIGN KEY (`MaChuyenBay`) REFERENCES `ChuyenBay` (`MaChuyenBay`);
ALTER TABLE `Ve` ADD CONSTRAINT `FK_Ve_DatVe` FOREIGN KEY (`MaDV`, `MaChuyenBay`) REFERENCES `DatVe` (`MaDV`, `MaChuyenBay`);
ALTER TABLE `Ve` ADD CONSTRAINT `FK_Ve_Ghe` FOREIGN KEY (`MaGhe`) REFERENCES `Ghe` (`MaGhe`);
ALTER TABLE `Ve` ADD CONSTRAINT `FK_Ve_HK` FOREIGN KEY (`MaHK`) REFERENCES `HanhKhach` (`MaHK`);
ALTER TABLE `Ve` ADD CONSTRAINT `FK_Ve_HV` FOREIGN KEY (`MaHV`) REFERENCES `HangVe` (`MaHV`);
ALTER TABLE `Voucher` ADD CONSTRAINT `FK_Voucher_CTKM` FOREIGN KEY (`MaCTKM`) REFERENCES `CTKM` (`MaCTKM`);
CREATE INDEX `IX_CTHD_MaVe` ON `ChiTietHoaDon` (`MaVe`);
CREATE INDEX `IX_Ve_MaDV` ON `Ve` (`MaDV`);
