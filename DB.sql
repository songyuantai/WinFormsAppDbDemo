create database if not exists costdb;

use costdb;

create table serial_nos
(
	id int NOT NULL AUTO_INCREMENT,
    year_num int,
    month_num int,
    serial_no varchar(20),
    seq_no int,
    primary key(id)
);

drop table serial_nos;

drop index uk_seq_no on serial_nos;

create unique index uk_seq_no on serial_nos(year_num, month_num, seq_no);

select min(a.seq_no) from serial_nos a
left join serial_nos b on a.seq_no + 1 = b.seq_no
where b.id is null