CREATE DATABASE wood_db
GO
USE wood_db
GO
CREATE TABLE deal(
	deal_number nvarchar(50) PRIMARY KEY,
	seller_name nvarchar(400),
	seller_inn nvarchar(100),
	buyer_name nvarchar(400),
	buyer_inn nvarchar(100),
	deal_date date,
	wood_volume_buyer float,
	wood_volume_seller float,
)