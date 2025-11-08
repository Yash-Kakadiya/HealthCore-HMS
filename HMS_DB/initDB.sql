-- =======================
-- Insert into [User] - Marvel & DC Characters
-- =======================
INSERT INTO [User] (UserName, Password, Email, MobileNo, IsActive, Created, Modified, ProfileImage)
VALUES
(N'Tony Stark', 'IAmIronMan@3000', 'tony.stark@starkindustries.com', '9999999001', 1, GETDATE(), GETDATE(), 'Profile/ironman.jpg'),
(N'Bruce Wayne', 'DarkKnight@Gotham02', 'bruce.wayne@wayneenterprises.com', '9999999002', 1, GETDATE(), GETDATE(), 'Profile/batman.jpg'),
(N'Steve Rogers', 'Captain@America03', 'steve.rogers@shield.gov', '9999999003', 1, GETDATE(), GETDATE(), 'Profile/captainamerica.jpg'),
(N'Natasha Romanoff', 'BlackWidow@Spy04', 'natasha.romanoff@shield.gov', '9999999004', 1, GETDATE(), GETDATE(), 'Profile/blackwidow.jpg'),
(N'Victor Von Doom', 'DoomRules@Latveria05', 'doom@latveria.gov', '9999999005', 1, GETDATE(), GETDATE(), null),
(N'Charles Xavier', 'ProfessorX@Mind06', 'charles.xavier@xmen.edu', '9999999006', 1, GETDATE(), GETDATE(), 'Profile/professorx.jpeg'),
(N'¥@$# 𝕂𝕒𝕜𝕒𝕕𝕚𝕪𝕒', 'Yash@8055', 'kakadiyayash77@gmail.com', '9999999007', 1, GETDATE(), GETDATE(), 'Profile/yashXhealthcore.gif'),
(N'Thor Odinson', 'GodOfThunder@Asgard08', 'thor@asgard.realm', '9999999008', 1, GETDATE(), GETDATE(), 'Profile/thor.jpg'),
(N'Wanda Maximoff', 'ScarletWitch@Chaos09', 'wanda.maximoff@avengers.org', '9999999009', 1, GETDATE(), GETDATE(), 'Profile/scarletwitch.jpg'),
(N'Loki Laufeyson', 'GodOfMischief@Tricks10', 'loki@asgard.realm', '9999999010', 1, GETDATE(), GETDATE(), 'Profile/loki.jpg'),
(N'Nick Fury', 'DirectorFury@SHIELD11', 'nick.fury@shield.gov', '9999999011', 1, GETDATE(), GETDATE(), 'Profile/nickfury.jpeg');
-- =======================
-- Insert into Department (Keeping as original)
-- =======================
INSERT INTO Department (DepartmentName, Description, IsActive, Created, Modified, UserID)
VALUES
(N'Cardiology', 'Heart and blood vessel related treatments', 1, GETDATE(), GETDATE(), 1),
(N'Neurology', 'Brain and nervous system treatments', 1, GETDATE(), GETDATE(), 2),
(N'Orthopedics', 'Bone and joint care', 1, GETDATE(), GETDATE(), 3),
(N'Pediatrics', 'Child healthcare', 1, GETDATE(), GETDATE(), 4),
(N'Gynecology', 'Women health and pregnancy care', 1, GETDATE(), GETDATE(), 5),
(N'Dermatology', 'Skin and hair treatments', 1, GETDATE(), GETDATE(), 6),
(N'Ophthalmology', 'Eye care and surgeries', 1, GETDATE(), GETDATE(), 7),
(N'ENT', 'Ear, Nose, Throat care', 1, GETDATE(), GETDATE(), 8);

-- =======================
-- Insert into Doctor - Marvel & Anime Characters
-- =======================
INSERT INTO Doctor (DoctorName, Phone, Email, Qualification, Specialization, IsActive, Created, Modified, UserID)
VALUES
(N'Dr. Stephen Strange', '9876543001', 'stephen.strange@sanctum.com', 'MD, Neurosurgery', 'Neurosurgeon & Mystic Arts', 1, GETDATE(), GETDATE(), 1),
(N'Dr. Bruce Banner', '9876543002', 'bruce.banner@avengers.org', 'PhD Nuclear Physics, MD', 'Radiation Specialist', 1, GETDATE(), GETDATE(), 2),
(N'Dr. Helen Cho', '9876543003', 'helen.cho@uchotech.com', 'MD, Bioengineering', 'Regeneration Specialist', 1, GETDATE(), GETDATE(), 3),
(N'Dr. Shuri', '9876543004', 'shuri@wakanda.gov', 'Wakandan Tech Genius', 'Technology & Medicine', 1, GETDATE(), GETDATE(), 4),
(N'Dr. Otto Octavius', '9876543005', 'otto.octavius@oscorp.com', 'PhD Nuclear Physics', 'Mechanical Enhancement', 1, GETDATE(), GETDATE(), 5),
(N'Dr. Shinobu Kocho', '9876543006', 'shinobu@demonslayer.jp', 'Insect Breathing Master', 'Poison & Medicine Expert', 1, GETDATE(), GETDATE(), 6),
(N'Dr. Tamayo', '9876543007', 'tamayo@demonslayer.jp', 'Demon Doctor', 'Blood Medicine Specialist', 1, GETDATE(), GETDATE(), 7),
(N'Dr. Yushiro', '9876543008', 'yushiro@demonslayer.jp', 'Medical Assistant', 'General Medicine', 1, GETDATE(), GETDATE(), 8);

-- =======================
-- Insert into DoctorDepartment
-- =======================
INSERT INTO DoctorDepartment (DoctorID, DepartmentID, Created, Modified, UserID)
VALUES
(1, 2, GETDATE(), GETDATE(), 1),  -- Dr. Strange - Neurology
(2, 1, GETDATE(), GETDATE(), 2),  -- Dr. Banner - Cardiology
(3, 5, GETDATE(), GETDATE(), 3),  -- Dr. Helen Cho - Gynecology
(4, 4, GETDATE(), GETDATE(), 4),  -- Shuri - Pediatrics
(5, 3, GETDATE(), GETDATE(), 5),  -- Dr. Octavius - Orthopedics
(6, 6, GETDATE(), GETDATE(), 6),  -- Shinobu - Dermatology
(7, 7, GETDATE(), GETDATE(), 7),  -- Tamayo - Ophthalmology
(8, 8, GETDATE(), GETDATE(), 8);  -- Yushiro - ENT

-- =======================
-- Insert into Patient - Superheroes & Anime Characters
-- =======================
INSERT INTO Patient (PatientName, DateOfBirth, Gender, Email, Phone, Address, City, State, IsActive, Created, Modified, UserID)
VALUES
(N'Peter Parker', '2001-08-10', 'Male', 'spiderman@dailybugle.com', '9111111001', '20 Ingram Street, Forest Hills', 'Queens', 'New York', 1, GETDATE(), GETDATE(), 1),
(N'Eren Yeager', '1935-03-30', 'Male', 'eren.yeager@survey.corps', '9111111002', 'Shiganshina District', 'Paradis', 'Island', 1, GETDATE(), GETDATE(), 2),
(N'Wade Wilson', '1973-11-22', 'Male', 'deadpool@mercenary.com', '9111111003', '1234 Weapon X Facility', 'Vancouver', 'Canada', 1, GETDATE(), GETDATE(), 3),
(N'Levi Ackerman', '1938-12-25', 'Male', 'levi@survey.corps', '9111111004', 'Underground City', 'Paradis', 'Island', 1, GETDATE(), GETDATE(), 4),
(N'James Howlett', '1932-01-01', 'Male', 'wolverine@xmen.edu', '9111111005', 'Xavier Institute', 'Westchester', 'New York', 1, GETDATE(), GETDATE(), 5),
(N'Tanjiro Kamado', '1900-07-14', 'Male', 'tanjiro@demonslayer.jp', '9111111006', 'Mount Kumotori', 'Tokyo', 'Japan', 1, GETDATE(), GETDATE(), 6),
(N'Thanos', '2000-01-01', 'Male', 'thanos@titan.galaxy', '9111111007', 'Sanctuary II', 'Titan', 'Space', 1, GETDATE(), GETDATE(), 7),
(N'Arthur Fleck', '1981-11-19', 'Male', 'joker@gotham.city', '9111111008', 'Gotham Apartments', 'Gotham', 'New Jersey', 1, GETDATE(), GETDATE(), 8);

-- =======================
-- Insert into Appointment - October 2025
-- =======================

-- Week 1 of October 2025 (Oct 1-7)
INSERT INTO Appointment (DoctorID, PatientID, AppointmentDate, AppointmentStatus, Description, SpecialRemarks, Created, Modified, UserID, TotalConsultedAmount)
VALUES
(1, 2, '2025-10-01 09:00:00', 'Confirmed', 'Titan transformation side effects', 'Brain scan for memory issues', GETDATE(), GETDATE(), 1, 2500.00),
(5, 4, '2025-10-01 14:30:00', 'Confirmed', '3DMG injury assessment', 'Bring previous injury reports', GETDATE(), GETDATE(), 5, 1800.00),
(3, 6, '2025-10-02 10:00:00', 'Confirmed', 'Demon slayer mark checkup', 'Monitor physical enhancements', GETDATE(), GETDATE(), 3, 2200.00),
(1, 1, '2025-10-02 15:00:00', 'Cancelled', 'Spider-sense headache', 'Patient busy saving city', GETDATE(), GETDATE(), 1, 1500.00),
(4, 5, '2025-10-03 11:00:00', 'Confirmed', 'Adamantium skeleton scan', 'X-ray immune system check', GETDATE(), GETDATE(), 4, 3500.00),
(6, 3, '2025-10-03 16:30:00', 'Confirmed', 'Healing factor analysis', 'Accelerated cell regeneration study', GETDATE(), GETDATE(), 6, 2800.00),
(7, 7, '2025-10-04 09:30:00', 'Confirmed', 'Infinity Stone exposure treatment', 'Radiation damage assessment', GETDATE(), GETDATE(), 7, 5000.00),
(8, 8, '2025-10-04 13:00:00', 'Pending', 'Laughing gas chemical exposure', 'Toxicology screening needed', GETDATE(), GETDATE(), 8, 1900.00),

-- Week 2 of October 2025 (Oct 8-14)
(2, 5, '2025-10-08 10:30:00', 'Confirmed', 'Gamma radiation resistance test', 'Compare healing factors', GETDATE(), GETDATE(), 2, 4500.00),
(1, 6, '2025-10-08 14:00:00', 'Confirmed', 'Water breathing technique impact', 'Lung capacity evaluation', GETDATE(), GETDATE(), 1, 2100.00),
(5, 7, '2025-10-09 09:00:00', 'Confirmed', 'Titan bone structure analysis', 'Super strength bone density test', GETDATE(), GETDATE(), 5, 3800.00),
(4, 2, '2025-10-09 11:30:00', 'Confirmed', 'Growth development monitoring', 'Titan shifter metabolism study', GETDATE(), GETDATE(), 4, 2400.00),
(3, 4, '2025-10-10 15:30:00', 'Confirmed', 'Combat injury rehabilitation', 'Survey Corps physical therapy', GETDATE(), GETDATE(), 3, 2600.00),
(6, 8, '2025-10-10 10:00:00', 'Pending', 'Pale skin condition evaluation', 'Chemical burn assessment', GETDATE(), GETDATE(), 6, 1700.00),
(7, 1, '2025-10-11 12:00:00', 'Confirmed', 'Enhanced vision screening', 'Spider-sense eye coordination', GETDATE(), GETDATE(), 7, 1800.00),
(8, 3, '2025-10-11 16:00:00', 'Confirmed', 'Vocal cord damage repair', 'Excessive talking side effects', GETDATE(), GETDATE(), 8, 2000.00),

-- Week 3 of October 2025 (Oct 15-21)
(2, 3, '2025-10-15 09:30:00', 'Confirmed', 'Heart rate during regeneration', 'Cardiac stress during healing', GETDATE(), GETDATE(), 2, 3200.00),
(1, 8, '2025-10-15 13:30:00', 'Confirmed', 'Psychological evaluation', 'Mental health assessment', GETDATE(), GETDATE(), 1, 2500.00),
(5, 1, '2025-10-16 10:00:00', 'Confirmed', 'Web-slinging shoulder injury', 'Rotator cuff strain treatment', GETDATE(), GETDATE(), 5, 1900.00),
(4, 7, '2025-10-16 14:30:00', 'Cancelled', 'Titan physiology consultation', 'Patient busy with universe', GETDATE(), GETDATE(), 4, 3000.00),
(3, 2, '2025-10-17 11:00:00', 'Confirmed', 'Post-transformation recovery', 'Cellular regeneration therapy', GETDATE(), GETDATE(), 3, 2800.00),
(6, 4, '2025-10-17 15:00:00', 'Confirmed', 'Scar tissue removal', 'Underground fighting injuries', GETDATE(), GETDATE(), 6, 2200.00),
(7, 5, '2025-10-18 09:00:00', 'Pending', 'Retinal adamantium compatibility', 'Metal poisoning eye check', GETDATE(), GETDATE(), 7, 2600.00),
(8, 6, '2025-10-18 12:30:00', 'Confirmed', 'Breathing technique consultation', 'Oxygen intake optimization', GETDATE(), GETDATE(), 8, 1800.00),

-- Week 4 of October 2025 (Oct 22-28)
(2, 7, '2025-10-22 10:00:00', 'Confirmed', 'Infinity Gauntlet aftermath', 'Cosmic energy cardiac damage', GETDATE(), GETDATE(), 2, 6000.00),
(1, 4, '2025-10-22 14:00:00', 'Confirmed', 'PTSD neurological treatment', 'War trauma counseling', GETDATE(), GETDATE(), 1, 2400.00),
(5, 3, '2025-10-23 09:30:00', 'Confirmed', 'Bone fracture healing speed test', 'Enhanced recovery analysis', GETDATE(), GETDATE(), 5, 2700.00),
(4, 6, '2025-10-23 11:00:00', 'Confirmed', 'Demon slayer mark progression', 'Life expectancy monitoring', GETDATE(), GETDATE(), 4, 2900.00),
(3, 8, '2025-10-24 15:00:00', 'Confirmed', 'Chemical burn reconstruction', 'Skin grafting consultation', GETDATE(), GETDATE(), 3, 3500.00),
(6, 1, '2025-10-24 10:30:00', 'Pending', 'Radioactive spider bite effects', 'Long-term DNA mutation study', GETDATE(), GETDATE(), 6, 2800.00),
(7, 2, '2025-10-25 13:00:00', 'Confirmed', 'Titan eye strain evaluation', 'Vision during transformation', GETDATE(), GETDATE(), 7, 2300.00),
(8, 5, '2025-10-25 16:30:00', 'Confirmed', 'Enhanced hearing assessment', 'Animal-like sensory testing', GETDATE(), GETDATE(), 8, 2100.00),

-- Current Week (Oct 28-31, 2025)
(2, 1, '2025-10-28 09:00:00', 'Confirmed', 'Web fluid heart toxicity check', 'Chemical exposure screening', GETDATE(), GETDATE(), 2, 2200.00),
(1, 3, '2025-10-28 11:30:00', 'Confirmed', 'Brain during regeneration', 'Neural activity during healing', GETDATE(), GETDATE(), 1, 3000.00),
(5, 6, '2025-10-29 10:00:00', 'Confirmed', 'Sword combat injuries', 'Multiple fracture treatment', GETDATE(), GETDATE(), 5, 2400.00),
(4, 8, '2025-10-29 14:00:00', 'Confirmed', 'Childhood trauma evaluation', 'Developmental psychology', GETDATE(), GETDATE(), 4, 2000.00),
(3, 2, '2025-10-30 09:30:00', 'Confirmed', 'Titan serum genetic analysis', 'DNA modification study', GETDATE(), GETDATE(), 3, 3800.00),
(6, 4, '2025-10-30 13:00:00', 'Pending', 'Combat scars treatment', 'Aesthetic dermatology', GETDATE(), GETDATE(), 6, 1800.00),
(7, 7, '2025-10-31 10:30:00', 'Confirmed', 'Purple skin examination', 'Alien physiology study', GETDATE(), GETDATE(), 7, 4500.00),
(8, 5, '2025-10-31 15:00:00', 'Pending', 'Berserker rage vocal damage', 'Throat injury assessment', GETDATE(), GETDATE(), 8, 1900.00);

-- =======================
-- November 2025 Appointments (Upcoming)
-- =======================

-- Week 1 of November 2025 (Nov 1-7)
INSERT INTO Appointment (DoctorID, PatientID, AppointmentDate, AppointmentStatus, Description, SpecialRemarks, Created, Modified, UserID, TotalConsultedAmount)
VALUES
(2, 6, '2025-11-01 10:00:00', 'Pending', 'Demon mark cardiac screening', 'Heart strain from enhancement', GETDATE(), GETDATE(), 2, 2800.00),
(5, 3, '2025-11-01 14:30:00', 'Pending', 'Healing factor joint analysis', 'Regeneration speed test', GETDATE(), GETDATE(), 5, 2600.00),
(1, 7, '2025-11-02 09:30:00', 'Pending', 'Mad Titan brain scan', 'Cognitive function after snap', GETDATE(), GETDATE(), 1, 5500.00),
(4, 1, '2025-11-02 11:00:00', 'Pending', 'Teenage superhero health check', 'Growth and power balance', GETDATE(), GETDATE(), 4, 1800.00),
(3, 8, '2025-11-03 15:00:00', 'Pending', 'Psychological therapy session', 'Mental health support', GETDATE(), GETDATE(), 3, 2200.00),
(6, 2, '2025-11-03 10:30:00', 'Pending', 'Titan transformation skin damage', 'Steam burn treatment', GETDATE(), GETDATE(), 6, 2400.00),
(7, 4, '2025-11-04 13:00:00', 'Pending', 'Enhanced vision maintenance', 'Ackerman bloodline eye check', GETDATE(), GETDATE(), 7, 2100.00),
(8, 5, '2025-11-04 16:00:00', 'Pending', 'Adamantium implant checkup', 'Metal integration assessment', GETDATE(), GETDATE(), 8, 3200.00),

-- Week 2 of November 2025 (Nov 8-14)
(2, 8, '2025-11-05 09:00:00', 'Pending', 'Chemical exposure follow-up', 'Toxin levels monitoring', GETDATE(), GETDATE(), 2, 2000.00),
(1, 2, '2025-11-05 14:00:00', 'Pending', 'Founding Titan memory issues', 'Past memories neurological exam', GETDATE(), GETDATE(), 1, 3500.00),
(5, 4, '2025-11-06 10:00:00', 'Pending', 'ODM gear spinal damage', 'Back injury surgical consult', GETDATE(), GETDATE(), 5, 3000.00),
(4, 6, '2025-11-06 11:30:00', 'Pending', 'Young demon slayer checkup', 'Physical development monitoring', GETDATE(), GETDATE(), 4, 1700.00),
(3, 3, '2025-11-07 15:30:00', 'Pending', 'Regeneration cycle optimization', 'Healing speed enhancement', GETDATE(), GETDATE(), 3, 3200.00),
(6, 7, '2025-11-07 10:00:00', 'Pending', 'Purple skin condition study', 'Titan genetics dermatology', GETDATE(), GETDATE(), 6, 4200.00),
(7, 1, '2025-11-08 12:00:00', 'Pending', 'Spider-vision enhancement', 'Night vision improvement', GETDATE(), GETDATE(), 7, 2000.00),
(8, 5, '2025-11-08 16:30:00', 'Pending', 'Enhanced hearing protection', 'Acoustic damage prevention', GETDATE(), GETDATE(), 8, 1800.00),

-- Week 3 of November 2025 (Nov 15-21)
(2, 3, '2025-11-11 09:30:00', 'Pending', 'Heart during rapid healing', 'Cardiovascular stress test', GETDATE(), GETDATE(), 2, 2900.00),
(1, 4, '2025-11-11 13:30:00', 'Pending', 'Combat trauma therapy', 'War veteran counseling', GETDATE(), GETDATE(), 1, 2300.00),
(5, 8, '2025-11-12 10:00:00', 'Pending', 'Joint flexibility assessment', 'Chaos movement analysis', GETDATE(), GETDATE(), 5, 2100.00),
(4, 2, '2025-11-12 14:30:00', 'Pending', 'Titan shifter development', 'Adolescent health screening', GETDATE(), GETDATE(), 4, 2500.00),
(3, 5, '2025-11-13 11:00:00', 'Pending', 'Mutant gene therapy', 'X-gene enhancement consultation', GETDATE(), GETDATE(), 3, 3800.00),
(6, 1, '2025-11-13 15:00:00', 'Pending', 'Spider bite mutation tracking', 'Genetic stability monitoring', GETDATE(), GETDATE(), 6, 2400.00),
(7, 6, '2025-11-14 09:00:00', 'Pending', 'Sun breathing eye protection', 'UV damage prevention', GETDATE(), GETDATE(), 7, 2200.00),
(8, 7, '2025-11-14 12:30:00', 'Pending', 'Titan vocal cord damage', 'Roar impact assessment', GETDATE(), GETDATE(), 8, 3500.00),

-- Week 4 of November 2025 (Nov 22-28)
(2, 2, '2025-11-18 10:00:00', 'Pending', 'Attack Titan heart monitoring', 'Future memory cardiac stress', GETDATE(), GETDATE(), 2, 3200.00),
(1, 6, '2025-11-18 14:00:00', 'Pending', 'Demon slayer mark brain scan', 'Enhanced cognition study', GETDATE(), GETDATE(), 1, 2800.00),
(5, 1, '2025-11-19 09:30:00', 'Pending', 'Web-swinging injury prevention', 'Preventive orthopedic care', GETDATE(), GETDATE(), 5, 2000.00),
(4, 7, '2025-11-19 11:00:00', 'Pending', 'Alien physiology study', 'Titan biology comparison', GETDATE(), GETDATE(), 4, 4500.00),
(3, 4, '2025-11-20 15:00:00', 'Pending', 'Survey Corps medical evaluation', 'Elite soldier health check', GETDATE(), GETDATE(), 3, 2400.00),
(6, 8, '2025-11-20 10:30:00', 'Pending', 'Facial reconstruction consult', 'Cosmetic surgery planning', GETDATE(), GETDATE(), 6, 3800.00),
(7, 2, '2025-11-21 13:00:00', 'Pending', 'Titan eye transformation study', 'Vision during shifting', GETDATE(), GETDATE(), 7, 2700.00),
(8, 3, '2025-11-21 16:30:00', 'Pending', 'Regeneration vocal impact', 'Voice change during healing', GETDATE(), GETDATE(), 8, 2100.00),

-- Last Week of November 2025 (Nov 25-30)
(2, 7, '2025-11-25 09:00:00', 'Pending', 'Infinity Stone heart damage', 'Cosmic radiation treatment', GETDATE(), GETDATE(), 2, 7000.00),
(1, 1, '2025-11-25 11:30:00', 'Pending', 'Spider-sense neurological map', 'Precognition brain activity', GETDATE(), GETDATE(), 1, 2600.00),
(5, 5, '2025-11-26 10:00:00', 'Pending', 'Adamantium bone density', 'Metal skeleton maintenance', GETDATE(), GETDATE(), 5, 3400.00),
(4, 3, '2025-11-26 14:00:00', 'Pending', 'Mercenary health insurance', 'Comprehensive physical exam', GETDATE(), GETDATE(), 4, 2800.00),
(3, 6, '2025-11-27 09:30:00', 'Pending', 'Demon mark life extension', 'Survival rate consultation', GETDATE(), GETDATE(), 3, 4200.00),
(6, 5, '2025-11-27 13:00:00', 'Pending', 'Healing factor skin analysis', 'Scar tissue study', GETDATE(), GETDATE(), 6, 2500.00),
(7, 8, '2025-11-28 10:30:00', 'Pending', 'Chemical vision damage repair', 'Corneal reconstruction', GETDATE(), GETDATE(), 7, 3200.00),
(8, 4, '2025-11-28 15:00:00', 'Pending', 'Elite soldier hearing test', 'Enhanced auditory assessment', GETDATE(), GETDATE(), 8, 1900.00);