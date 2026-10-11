-- Run as RESTO in FREEPDB1, after 01_schema.sql
-- Sample menu and tables. Users are NOT seeded here: the API creates the
-- first admin account at startup so the password is hashed properly.

WHENEVER SQLERROR EXIT FAILURE ROLLBACK

-- ---------------------------------------------------------------
-- Categories
-- ---------------------------------------------------------------
INSERT INTO categories (name, sort_order) VALUES ('Rice', 1);
INSERT INTO categories (name, sort_order) VALUES ('Noodles', 2);
INSERT INTO categories (name, sort_order) VALUES ('Snacks', 3);
INSERT INTO categories (name, sort_order) VALUES ('Drinks', 4);
INSERT INTO categories (name, sort_order) VALUES ('Desserts', 5);

-- ---------------------------------------------------------------
-- Menu items (category looked up by name, not hard-coded IDs)
-- ---------------------------------------------------------------
INSERT INTO menu_items (category_id, name, description, price)
VALUES ((SELECT category_id FROM categories WHERE name = 'Rice'),
        'Nasi Lemak Ayam', 'Coconut rice with fried chicken, sambal, egg, peanuts and anchovies', 12.90);
INSERT INTO menu_items (category_id, name, description, price)
VALUES ((SELECT category_id FROM categories WHERE name = 'Rice'),
        'Nasi Goreng Kampung', 'Village-style fried rice with anchovies and water spinach', 10.50);
INSERT INTO menu_items (category_id, name, description, price)
VALUES ((SELECT category_id FROM categories WHERE name = 'Rice'),
        'Nasi Ayam Penyet', 'Smashed fried chicken with rice and spicy sambal', 13.90);

INSERT INTO menu_items (category_id, name, description, price)
VALUES ((SELECT category_id FROM categories WHERE name = 'Noodles'),
        'Mee Goreng Mamak', 'Spicy stir-fried yellow noodles with tofu and potato', 9.90);
INSERT INTO menu_items (category_id, name, description, price)
VALUES ((SELECT category_id FROM categories WHERE name = 'Noodles'),
        'Char Kuey Teow', 'Wok-fried flat rice noodles with prawns and bean sprouts', 11.50);
-- Laksa is unavailable, for testing
-- (SQL*Plus rule: never put a comment after the ; on the same line)
INSERT INTO menu_items (category_id, name, description, price, is_available)
VALUES ((SELECT category_id FROM categories WHERE name = 'Noodles'),
        'Laksa', 'Spicy coconut curry noodle soup', 12.00, 0);

INSERT INTO menu_items (category_id, name, description, price)
VALUES ((SELECT category_id FROM categories WHERE name = 'Snacks'),
        'Roti Canai', 'Flaky flatbread served with dhal curry', 2.50);
INSERT INTO menu_items (category_id, name, description, price)
VALUES ((SELECT category_id FROM categories WHERE name = 'Snacks'),
        'Satay Ayam (10 sticks)', 'Grilled chicken skewers with peanut sauce', 15.00);
INSERT INTO menu_items (category_id, name, description, price)
VALUES ((SELECT category_id FROM categories WHERE name = 'Snacks'),
        'Pisang Goreng', 'Crispy fried banana fritters', 5.00);

INSERT INTO menu_items (category_id, name, description, price)
VALUES ((SELECT category_id FROM categories WHERE name = 'Drinks'),
        'Teh Tarik', 'Pulled milk tea', 3.00);
INSERT INTO menu_items (category_id, name, description, price)
VALUES ((SELECT category_id FROM categories WHERE name = 'Drinks'),
        'Kopi O', 'Black coffee with sugar', 2.50);
INSERT INTO menu_items (category_id, name, description, price)
VALUES ((SELECT category_id FROM categories WHERE name = 'Drinks'),
        'Milo Ais', 'Iced chocolate malt drink', 4.00);
INSERT INTO menu_items (category_id, name, description, price)
VALUES ((SELECT category_id FROM categories WHERE name = 'Drinks'),
        'Sirap Bandung', 'Rose syrup with milk', 3.50);

INSERT INTO menu_items (category_id, name, description, price)
VALUES ((SELECT category_id FROM categories WHERE name = 'Desserts'),
        'Cendol', 'Shaved ice with green jelly, coconut milk and palm sugar', 6.50);
INSERT INTO menu_items (category_id, name, description, price)
VALUES ((SELECT category_id FROM categories WHERE name = 'Desserts'),
        'Ais Kacang (ABC)', 'Shaved ice with red beans, corn and syrup', 7.00);

-- ---------------------------------------------------------------
-- Restaurant tables
-- ---------------------------------------------------------------
INSERT INTO restaurant_tables (table_no, seats) VALUES ('T01', 2);
INSERT INTO restaurant_tables (table_no, seats) VALUES ('T02', 2);
INSERT INTO restaurant_tables (table_no, seats) VALUES ('T03', 4);
INSERT INTO restaurant_tables (table_no, seats) VALUES ('T04', 4);
INSERT INTO restaurant_tables (table_no, seats) VALUES ('T05', 4);
INSERT INTO restaurant_tables (table_no, seats) VALUES ('T06', 6);
INSERT INTO restaurant_tables (table_no, seats) VALUES ('T07', 6);
INSERT INTO restaurant_tables (table_no, seats) VALUES ('T08', 8);

COMMIT;

PROMPT Seed data inserted.
EXIT
