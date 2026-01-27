
DELETE FROM SeatReservations;
DELETE FROM Seats;
DELETE FROM Sessions;
DELETE FROM Movies;
DELETE FROM Halls;
DELETE FROM SeatTypes;

-- Insert Movies
INSERT INTO Movies (Name, DurationMinutes, AgeLimit, Genre, Description, ReleaseDate, ImdbRating, PosterUrl, Country)
VALUES 
('Dune: Part Two', 150, 12, 1, 'Epic continuation of the Dune saga', '2026-01-15', 8.5, 'https://donaldthompson.com/wp-content/uploads/2024/10/placeholder-image-vertical.png', 'USA'),
('Avatar 3', 220, 12, 2, 'Return to Pandora for another adventure', '2026-01-10', 8.9, 'https://donaldthompson.com/wp-content/uploads/2024/10/placeholder-image-vertical.png', 'USA'),
('The Grand Adventure', 142, 12, 0, 'An epic adventure across distant lands', '2026-01-20', 7.8, 'https://donaldthompson.com/wp-content/uploads/2024/10/placeholder-image-vertical.png', 'UK');

-- Insert Halls (using raw SQL to bypass constructor)
SET IDENTITY_INSERT Halls ON;
INSERT INTO Halls (Id, Name, Rows, Columns)
VALUES 
(1, 'Hall 1', 10, 12),
(2, 'Hall 2', 10, 12),
(3, 'Hall 3', 8, 10);
SET IDENTITY_INSERT Halls OFF;

-- Insert SeatTypes
SET IDENTITY_INSERT SeatTypes ON;
INSERT INTO SeatTypes (Id, Name, AddedPrice, IsActive)
VALUES 
(1, 'Standard', 0, 1),
(2, 'VIP', 50, 1),
(3, 'Premium', 30, 1);
SET IDENTITY_INSERT SeatTypes OFF;

-- Insert Sessions
INSERT INTO Sessions (MovieId, HallId, StartTime, BasePrice, MovieFormat)
VALUES
-- Today (Jan 27, 2026)
((SELECT Id FROM Movies WHERE Name = 'Dune: Part Two'), 1, '2026-01-27 14:00:00', 150, 0),
((SELECT Id FROM Movies WHERE Name = 'Avatar 3'), 2, '2026-01-27 18:00:00', 180, 1),

-- Hall 1 - Feb 4
((SELECT Id FROM Movies WHERE Name = 'Dune: Part Two'), 1, '2026-02-04 10:30:00', 150, 0), -- Dune: Part Two, 2D
((SELECT Id FROM Movies WHERE Name = 'The Grand Adventure'), 1, '2026-02-04 14:00:00', 120, 0),
((SELECT Id FROM Movies WHERE Name = 'Avatar 3'), 1, '2026-02-04 18:30:00', 180, 1), -- Avatar 3, 3D

-- Hall 2 - Feb 4  
((SELECT Id FROM Movies WHERE Name = 'Avatar 3'), 2, '2026-02-04 08:00:00', 220, 1), -- Avatar 3, 3D (full - 120/120)
((SELECT Id FROM Movies WHERE Name = 'Avatar 3'), 2, '2026-02-04 11:45:00', 220, 1), -- Avatar 3, 3D (low - 5/120)
((SELECT Id FROM Movies WHERE Name = 'Dune: Part Two'), 2, '2026-02-04 16:00:00', 150, 0),

-- Hall 3 - Feb 4
((SELECT Id FROM Movies WHERE Name = 'The Grand Adventure'), 3, '2026-02-04 09:00:00', 120, 0),
((SELECT Id FROM Movies WHERE Name = 'Dune: Part Two'), 3, '2026-02-04 13:00:00', 150, 0),
((SELECT Id FROM Movies WHERE Name = 'Avatar 3'), 3, '2026-02-04 17:30:00', 200, 2), -- IMAX

-- Additional days for testing
((SELECT Id FROM Movies WHERE Name = 'Dune: Part Two'), 1, '2026-02-05 10:00:00', 150, 0),
((SELECT Id FROM Movies WHERE Name = 'Avatar 3'), 1, '2026-02-05 15:00:00', 180, 1),
((SELECT Id FROM Movies WHERE Name = 'The Grand Adventure'), 2, '2026-02-05 12:00:00', 120, 0),
((SELECT Id FROM Movies WHERE Name = 'Dune: Part Two'), 3, '2026-02-05 19:00:00', 150, 0);

-- Insert Seats for all halls
DECLARE @HallId INT;
DECLARE @Row INT;
DECLARE @SeatNum INT;
DECLARE @SeatTypeId INT;

-- Hall 1: 10 rows x 12 seats = 120 seats
SET @HallId = 1;
SET @Row = 1;
WHILE @Row <= 10
BEGIN
    SET @SeatNum = 1;
    WHILE @SeatNum <= 12
    BEGIN
        -- VIP: last 2 rows, Premium: middle rows, Standard: front rows
        IF @Row >= 9
            SET @SeatTypeId = 2; -- VIP
        ELSE IF @Row >= 5
            SET @SeatTypeId = 3; -- Premium
        ELSE
            SET @SeatTypeId = 1; -- Standard
            
        INSERT INTO Seats (HallId, RowNum, SeatNum, SeatTypeId)
        VALUES (@HallId, @Row, @SeatNum, @SeatTypeId);
        
        SET @SeatNum = @SeatNum + 1;
    END
    SET @Row = @Row + 1;
END

-- Hall 2: 10 rows x 12 seats = 120 seats
SET @HallId = 2;
SET @Row = 1;
WHILE @Row <= 10
BEGIN
    SET @SeatNum = 1;
    WHILE @SeatNum <= 12
    BEGIN
        IF @Row >= 9
            SET @SeatTypeId = 2;
        ELSE IF @Row >= 5
            SET @SeatTypeId = 3;
        ELSE
            SET @SeatTypeId = 1;
            
        INSERT INTO Seats (HallId, RowNum, SeatNum, SeatTypeId)
        VALUES (@HallId, @Row, @SeatNum, @SeatTypeId);
        
        SET @SeatNum = @SeatNum + 1;
    END
    SET @Row = @Row + 1;
END

-- Hall 3: 8 rows x 10 seats = 80 seats
SET @HallId = 3;
SET @Row = 1;
WHILE @Row <= 8
BEGIN
    SET @SeatNum = 1;
    WHILE @SeatNum <= 10
    BEGIN
        IF @Row >= 7
            SET @SeatTypeId = 2;
        ELSE IF @Row >= 4
            SET @SeatTypeId = 3;
        ELSE
            SET @SeatTypeId = 1;
            
        INSERT INTO Seats (HallId, RowNum, SeatNum, SeatTypeId)
        VALUES (@HallId, @Row, @SeatNum, @SeatTypeId);
        
        SET @SeatNum = @SeatNum + 1;
    END
    SET @Row = @Row + 1;
END

-- Add some seat reservations to simulate occupancy
-- For the first session in Hall 2 (Avatar 3 at 08:00) - make it almost full
DECLARE @SessionId INT = (SELECT TOP 1 Id FROM Sessions WHERE HallId = 2 AND StartTime = '2026-02-04 08:00:00');
DECLARE @BasePrice DECIMAL(18,2) = (SELECT BasePrice FROM Sessions WHERE Id = @SessionId);
DECLARE @SeatId INT;
DECLARE @SeatTypeAddedPrice DECIMAL(18,2);
DECLARE @Counter INT = 0;

DECLARE seat_cursor CURSOR FOR 
SELECT s.Id, st.AddedPrice FROM Seats s
JOIN SeatTypes st ON s.SeatTypeId = st.Id
WHERE s.HallId = 2;

OPEN seat_cursor;
FETCH NEXT FROM seat_cursor INTO @SeatId, @SeatTypeAddedPrice;

WHILE @@FETCH_STATUS = 0 AND @Counter < 120
BEGIN
    INSERT INTO SeatReservations (SessionId, SeatId, Status, Price)
    VALUES (@SessionId, @SeatId, 1, @BasePrice + @SeatTypeAddedPrice); -- 1 = Confirmed
    
    SET @Counter = @Counter + 1;
    FETCH NEXT FROM seat_cursor INTO @SeatId, @SeatTypeAddedPrice;
END

CLOSE seat_cursor;
DEALLOCATE seat_cursor;

-- For second session in Hall 2 (Avatar 3 at 11:45) - make it partially filled (5 seats available)
SET @SessionId = (SELECT TOP 1 Id FROM Sessions WHERE HallId = 2 AND StartTime = '2026-02-04 11:45:00');
SET @BasePrice = (SELECT BasePrice FROM Sessions WHERE Id = @SessionId);
SET @Counter = 0;

DECLARE seat_cursor2 CURSOR FOR 
SELECT s.Id, st.AddedPrice FROM Seats s
JOIN SeatTypes st ON s.SeatTypeId = st.Id
WHERE s.HallId = 2 ORDER BY s.Id;

OPEN seat_cursor2;
FETCH NEXT FROM seat_cursor2 INTO @SeatId, @SeatTypeAddedPrice;

WHILE @@FETCH_STATUS = 0 AND @Counter < 115
BEGIN
    INSERT INTO SeatReservations (SessionId, SeatId, Status, Price)
    VALUES (@SessionId, @SeatId, 1, @BasePrice + @SeatTypeAddedPrice);
    
    SET @Counter = @Counter + 1;
    FETCH NEXT FROM seat_cursor2 INTO @SeatId, @SeatTypeAddedPrice;
END

CLOSE seat_cursor2;
DEALLOCATE seat_cursor2;

-- For session in Hall 1 (Dune at 10:30) - partially filled (45 out of 120)
SET @SessionId = (SELECT TOP 1 Id FROM Sessions WHERE HallId = 1 AND StartTime = '2026-02-04 10:30:00');
SET @BasePrice = (SELECT BasePrice FROM Sessions WHERE Id = @SessionId);
SET @Counter = 0;

DECLARE seat_cursor3 CURSOR FOR 
SELECT s.Id, st.AddedPrice FROM Seats s
JOIN SeatTypes st ON s.SeatTypeId = st.Id
WHERE s.HallId = 1 ORDER BY s.Id;

OPEN seat_cursor3;
FETCH NEXT FROM seat_cursor3 INTO @SeatId, @SeatTypeAddedPrice;

WHILE @@FETCH_STATUS = 0 AND @Counter < 45
BEGIN
    INSERT INTO SeatReservations (SessionId, SeatId, Status, Price)
    VALUES (@SessionId, @SeatId, 1, @BasePrice + @SeatTypeAddedPrice);
    
    SET @Counter = @Counter + 1;
    FETCH NEXT FROM seat_cursor3 INTO @SeatId, @SeatTypeAddedPrice;
END

CLOSE seat_cursor3;
DEALLOCATE seat_cursor3;

PRINT 'Test data inserted successfully!';
