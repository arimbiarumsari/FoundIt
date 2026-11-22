-- Skema database FoundIt (PostgreSQL).
-- Mengikuti ERD Modul 4: User, Admin, Report, Listing, Category.
-- Admin disimpan di tabel users dengan kolom role = 'Admin', sama seperti class Admin : User di kode.
-- Nama tabel pakai "users" karena "user" adalah kata khusus di PostgreSQL.
-- Script ini aman dijalankan berulang kali (IF NOT EXISTS).

CREATE TABLE IF NOT EXISTS categories (
    id          uuid         PRIMARY KEY,
    name        varchar(100) NOT NULL UNIQUE,
    description text         NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS users (
    id            uuid         PRIMARY KEY,
    name          varchar(100) NOT NULL,
    email         varchar(100) NOT NULL UNIQUE,
    password_hash varchar(255) NOT NULL,
    status        varchar(20)  NOT NULL DEFAULT 'Verified'
                  CHECK (status IN ('Pending', 'Verified', 'Suspended')),
    role          varchar(10)  NOT NULL DEFAULT 'User'
                  CHECK (role IN ('User', 'Admin'))
);

CREATE TABLE IF NOT EXISTS reports (
    id               uuid         PRIMARY KEY,
    user_id          uuid         NOT NULL REFERENCES users (id),
    category_id      uuid         NOT NULL REFERENCES categories (id),
    type             varchar(10)  NOT NULL CHECK (type IN ('Lost', 'Found')),
    title            varchar(150) NOT NULL,
    description      text         NOT NULL,
    location         varchar(200) NOT NULL,
    incident_date    date         NOT NULL,
    image_url        varchar(500),
    image_data       bytea,
    status           varchar(20)  NOT NULL DEFAULT 'Pending'
                     CHECK (status IN ('Pending', 'Approved', 'Rejected', 'Cancelled')),
    rejection_reason text,
    reviewed_by      uuid         REFERENCES users (id),
    reviewed_at      timestamptz,
    created_at       timestamptz  NOT NULL,
    updated_at       timestamptz  NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_reports_user_id ON reports (user_id);
CREATE INDEX IF NOT EXISTS ix_reports_status  ON reports (status);

-- Satu report hanya boleh punya satu listing (report_id UNIQUE).
CREATE TABLE IF NOT EXISTS listings (
    id           uuid        PRIMARY KEY,
    report_id    uuid        NOT NULL UNIQUE REFERENCES reports (id),
    category_id  uuid        NOT NULL REFERENCES categories (id),
    status       varchar(20) NOT NULL DEFAULT 'Active'
                 CHECK (status IN ('Active', 'Resolved', 'Archived', 'Deleted')),
    published_at timestamptz NOT NULL,
    updated_at   timestamptz NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_listings_status ON listings (status);

ALTER TABLE reports ADD COLUMN IF NOT EXISTS image_data bytea;