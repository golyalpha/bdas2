-- IO1/SP1: Kapacita místnosti > 0
ALTER TABLE rooms 
ADD CONSTRAINT chk_room_capacity_positive 
CHECK (capacity > 0);

-- IO1/SP1: Minimální kapacita požadavku > 0
ALTER TABLE room_requests 
ADD CONSTRAINT chk_request_min_capacity_positive 
CHECK (min_capacity > 0);

-- IO3/SP3: Velikost pódia > 0
ALTER TABLE presentation_rooms 
ADD CONSTRAINT chk_podium_size_positive 
CHECK (podium_size > 0);

-- IO3/SP3: Velikost pódia v requestu > 0
ALTER TABLE presentation_rrequests 
ADD CONSTRAINT chk_request_podium_size_positive 
CHECK (podium_size > 0);

-- IO4/SP4: Počátek dostupnosti < konec
ALTER TABLE locations
ADD CONSTRAINT chk_location_availability_valid
CHECK (availability_start < availability_end);

-- IO5/SP5: Začátek rezervace < konec
ALTER TABLE reservations
ADD CONSTRAINT chk_reservation_dates_valid
CHECK ("start" < "end");