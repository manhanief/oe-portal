-- Run as RESTO in FREEPDB1 - ONE TIME ONLY, on the existing database.
-- Order numbers (A001, A002, ...) restart every day, so the old rule
-- "order_no is unique forever" must become "unique within the same day".
-- (A fresh rebuild with the updated 01_schema.sql already includes this.)

WHENEVER SQLERROR EXIT FAILURE

ALTER TABLE orders DROP CONSTRAINT uq_orders_no;

CREATE UNIQUE INDEX uq_orders_no_day ON orders (order_no, TRUNC(created_at));

PROMPT Order number is now unique per day.
EXIT
