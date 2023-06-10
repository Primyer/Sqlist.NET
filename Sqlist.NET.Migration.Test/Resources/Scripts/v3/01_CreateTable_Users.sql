create table if not exists "Users"
(
	"Id" serial not null primary key,
	"Name" name,
	"Email" varchar (125),
	"PhoneNumber" varchar (30),
	"EmailIsConfirmed" boolean
);