namespace FHSMS.TelegramBot.Localization;

/// <summary>
/// Bilingual bot copy, mirroring the same English/Amharic split used in the
/// web frontend (see frontend/src/i18n). Kept as a small static dictionary
/// here rather than sharing code with the frontend, since this is a separate
/// .NET process with its own deployment lifecycle - but the language keys and
/// tone match, so switching between the PWA and the bot feels consistent.
/// </summary>
public static class BotTranslations
{
    public static string Get(BotLanguage lang, string key) =>
        lang == BotLanguage.Amharic && Amharic.TryGetValue(key, out var am)
            ? am
            : English[key];

    private static readonly Dictionary<string, string> English = new()
    {
        ["welcome"] =
            "Welcome to AgriLink - Farm-Hotel Supply Management.\n\n" +
            "Are you a hotel/restaurant wanting to order, or an AgriLink field agent (hotel agent, farmer agent, or driver)?\n\n" +
            "\u2022 Hotel/restaurant: send /register to set up your account, then /order.\n" +
            "\u2022 Field agent or driver: send /login with the email and password your admin gave you.\n\n" +
            "Send /help any time to see what you can do.",
        ["help"] =
            "Commands:\n" +
            "/register - set up your hotel/restaurant account (no ID needed)\n" +
            "/order - start a new order\n" +
            "/myorders - view your recent orders\n" +
            "/orderstatus <orderId> - check one order's status\n" +
            "/login - sign in as an AgriLink field agent instead\n" +
            "/language en|am - switch language\n" +
            "/cancel - cancel whatever you're in the middle of",
        ["helpHotelAgent"] =
            "Signed in as {0} (hotel agent). Commands:\n" +
            "/newhotel - register a new hotel/restaurant\n" +
            "/neworder - log an order you took for a hotel\n" +
            "/myinvoices - see your hotels' invoices\n" +
            "/pay - pay an invoice (Chapa, Telebirr, CBE Birr, or any bank)\n" +
            "/mycommission - see your commission earned so far\n" +
            "/whoami - show who you're signed in as\n" +
            "/logout - sign out\n" +
            "/language en|am - switch language\n" +
            "/cancel - cancel whatever you're in the middle of",
        ["helpFarmerAgent"] =
            "Signed in as {0} (farmer agent). Commands:\n" +
            "/newfarmer - register a new farmer or cooperative\n" +
            "/stockin - log stock you received from a farmer\n" +
            "/myfarmerinvoices - see what the company owes for stock you've logged\n" +
            "/mycommission - see your commission earned so far\n" +
            "/whoami - show who you're signed in as\n" +
            "/logout - sign out\n" +
            "/language en|am - switch language\n" +
            "/cancel - cancel whatever you're in the middle of",
        ["helpDriver"] =
            "Signed in as {0} (driver). Commands:\n" +
            "/registerdriver - set up or update your vehicle profile (one per account)\n" +
            "/trips - see and accept available trips\n" +
            "/mytrips - see trips you've accepted\n" +
            "/deliver - confirm handoff on a trip you've accepted\n" +
            "/mypayments - see what the company owes you for completed trips\n" +
            "/whoami - show who you're signed in as\n" +
            "/logout - sign out\n" +
            "/language en|am - switch language\n" +
            "/cancel - cancel whatever you're in the middle of",
        ["languageSet"] = "Language set to English.",
        ["languageUsage"] = "Usage: /language en  or  /language am",

        // --- Agent login ---
        ["loginAskEmail"] = "Enter your email address:",
        ["loginAskPassword"] = "Enter your password:",
        ["loginFailed"] = "That email or password isn't right. Try /login again, or ask your admin to reset your password.",
        ["loginRequiresTwoFactor"] = "Your account has two-factor authentication enabled - please sign in on the AgriLink web app instead, the bot doesn't support the verification code step.",
        ["loginWrongRole"] = "This bot is for hotel and farmer agents. Your account's role ({0}) doesn't use these commands - try the AgriLink web app instead.",
        ["loginSuccessHotelAgent"] = "Signed in as {0} (hotel agent). Send /help to see what you can do.",
        ["loginSuccessFarmerAgent"] = "Signed in as {0} (farmer agent). Send /help to see what you can do.",
        ["loggedOut"] = "Signed out.",
        ["notLoggedIn"] = "You're not signed in. Send /login first.",
        ["wrongAgentRole"] = "This command is for {0} accounts only.",
        ["whoAmIAgent"] = "Signed in as {0} ({1}).",
        ["whoAmICustomer"] = "Registered as customer {0}.",
        ["whoAmINone"] = "You're not registered or signed in yet. Send /register (hotel/restaurant) or /login (field agent).",
        ["actionFailed"] = "Sorry, that didn't work: {0}",

        // --- Hotel customer self-registration (replaces the old ID-based /register) ---
        ["registerAskName"] = "Let's set up your hotel/restaurant account. What's the name of your hotel or restaurant?",
        ["askPhone"] = "What's the best phone number to reach you on?",
        ["askAddress"] = "What's the address or area/zone you're in?",
        ["askLocation"] = "Which area or location are they based in?",
        ["registered"] = "You're all set, {0}! Send /order any time you want to place an order.",
        ["notRegistered"] = "You haven't set up your account yet. Send /register to get started.",

        // --- Ordering (shared by hotel customers and hotel agents ordering for a hotel) ---
        ["noProducts"] = "No products are available to order right now. Please try again later.",
        ["chooseProduct"] = "Choose a product by number, or /cancel:\n\n{0}",
        ["invalidProductNumber"] = "Please reply with a valid product number from the list, or /cancel.",
        ["askQuantity"] = "How many {0} would you like? (enter a number)",
        ["invalidQuantity"] = "Please enter a positive number, or /cancel.",
        ["itemAdded"] = "Added {0} x {1} to your order.\n\nReply with another product number to add more, /done when finished, or /cancel.",
        ["itemAddedThenDone"] = "Added {0} x {1} to the order.\n\nReply with another product number to add more, /done when finished, or /cancel.",
        ["cartEmpty"] = "Your cart is empty. Choose a product number to add one, or /cancel.",
        ["orderSummary"] = "Order summary:\n{0}\nTotal (before tax): {1} ETB\n\nSend /confirm to place this order, or /cancel to discard it.",
        ["orderCancelled"] = "Cancelled.",
        ["orderConfirmedFallback"] = "Please send /confirm to place the order, or /cancel to discard it.",
        ["orderPlaced"] = "Order placed! Order reference: {0}\n\nAn AgriLink staff member will generate the invoice shortly.",
        ["orderFailed"] = "Sorry, something went wrong placing that order: {0}",
        ["unknownCommand"] = "Sorry, I didn't understand that. Send /help to see what I can do.",
        ["myOrdersEmpty"] = "You have no orders yet. Send /order to place one.",
        ["myOrdersHeader"] = "Your recent orders:\n\n{0}",
        ["orderStatusUsage"] = "Usage: /orderstatus <orderId>",
        ["orderStatusNotFound"] = "No order found with that ID, or it doesn't belong to you.",
        ["orderStatusResult"] = "Order {0}\nStatus: {1}\nDate: {2}\nSubtotal: {3} ETB\n\n{4}",
        ["orderStatusNoDelivery"] = "\U0001F69A Delivery: not posted for pickup yet.",
        ["orderStatusDelivery"] = "\U0001F69A Delivery: {0}\nDriver: {1}\nVehicle: {2}",
        ["orderStatusNoDriverYet"] = "not yet assigned",

        // --- Hotel agent: register a hotel ---
        ["newHotelAskName"] = "What's the name of the hotel/restaurant?",
        ["newHotelCreated"] = "Hotel \"{0}\" registered. You can log an order for them now with /neworder.\n(id: {1})",

        // --- Hotel agent: log an order for a hotel ---
        ["noHotelsYet"] = "No hotels are registered yet. Send /newhotel to register one first.",
        ["chooseHotel"] = "Which hotel is this order for? Reply with a number, or /cancel:\n\n{0}",
        ["invalidHotelNumber"] = "Please reply with a valid hotel number from the list, or /cancel.",

        // --- Farmer agent: register a farmer ---
        ["newFarmerAskName"] = "What's the farmer's or cooperative's name?",
        ["newFarmerCreated"] = "Farmer \"{0}\" registered. You can log stock from them now with /stockin.\n(id: {1})",

        // --- Farmer agent: log stock received ---
        ["noFarmersYet"] = "No farmers are registered yet. Send /newfarmer to register one first.",
        ["chooseFarmer"] = "Which farmer is this stock from? Reply with a number, or /cancel:\n\n{0}",
        ["invalidFarmerNumber"] = "Please reply with a valid farmer number from the list, or /cancel.",
        ["askStockQuantity"] = "How much {0} did you receive? (enter a number, in {1})",
        ["stockItemAdded"] = "Logged {0} {1} of {2}.\n\nReply with another product number to log more from the same farmer, /stockdone when finished, or /cancel.",
        ["stockCartEmpty"] = "Nothing logged yet. Choose a product number first, or /cancel.",
        ["stockSummary"] = "Stock received from {0}:\n{1}\nOwed to farmer: {2:0.##} ETB\nSend /stockconfirm to save this, or /cancel to discard it.",
        ["stockConfirmedFallback"] = "Please send /stockconfirm to save this, or /cancel to discard it.",
        ["stockRecorded"] = "Stock logged. Your commission for this has been recorded automatically.\n(reference: {0})",

        // --- Commission (/mycommission) ---
        ["noCommissionYet"] = "No commission recorded yet. It appears here once an order you placed is invoiced (hotel agents) or stock you logged is recorded (farmer agents).",
        ["myCommission"] = "Your commission:\nToday: {0} ETB\nThis month: {1} ETB\nAll time: {2} ETB ({3} entries)",

        // --- Invoices & payment (/myinvoices, /pay) ---
        ["noInvoicesYet"] = "You have no invoices yet.",
        ["myInvoicesHeader"] = "Your invoices:\n{0}\nUse /pay to pay one that's still due.",
        ["noUnpaidInvoices"] = "Nothing due right now - every invoice you can see is fully paid.",
        ["choosePayInvoice"] = "Which invoice would you like to pay?\n{0}\nReply with the number.",
        ["invalidInvoiceNumber"] = "That's not one of the numbers shown. Try again, or /cancel.",
        ["choosePayMethod"] = "How would you like to pay?\n{0}\nReply with the number.",
        ["anyBankOption"] = "Any bank in Ethiopia (transfer, then tell us the reference)",
        ["invalidChoice"] = "That's not one of the options shown. Try again, or /cancel.",
        ["payOnlineLink"] = "Open this link to pay:\n{0}\nCome back and check /myinvoices once you're done.",
        ["askCbeBirrReference"] = "What's the CBE Birr transaction reference?",
        ["askCbeBirrPhone"] = "What's the payer's CBE Birr phone number?",
        ["paymentVerified"] = "Payment confirmed - invoice {0} is now marked paid.",
        ["paymentVerificationFailed"] = "Couldn't confirm that payment: {0}\nYou can try again, or use /pay and choose \"Any bank\" to record it manually for an admin to check.",
        ["noBankAccounts"] = "No bank accounts are set up yet - ask an admin to add one under Settings.",
        ["chooseBankAccount"] = "Transfer to which account?\n{0}\nReply with the number.",
        ["askBankReference"] = "Transfer to: {0}\nOnce you've sent it, what's the transaction reference?",
        ["paymentRecordedPendingReconciliation"] = "Got it - recorded against invoice {0}. An admin will confirm it against the bank statement.",

        // --- Farmer invoices (/myfarmerinvoices) ---
        ["noFarmerInvoicesYet"] = "No farmer invoices yet - these appear automatically once you log stock with /stockin.",
        ["myFarmerInvoicesHeader"] = "Your farmer invoices:\n{0}",

        // --- Driver payments (/mypayments) ---
        ["noDriverPaymentsYet"] = "No trip payments yet - these appear automatically once you complete a delivery with /deliver.",
        ["myDriverPaymentsHeader"] = "Your trip payments:\n{0}",

        // --- Driver: register/update profile (/registerdriver) ---
        ["driverAskName"] = "Let's set up your driver profile. What's your full name?",
        ["driverAskPlate"] = "What's your vehicle's license plate number?",
        ["driverAskTruckType"] = "What type of truck do you drive? Reply with a number:\n1. Pickup\n2. Small truck\n3. Medium truck\n4. Isuzu\n5. Heavy truck\n6. Trailer\n7. Other",
        ["invalidTruckTypeNumber"] = "Please reply with a number from 1 to 7.",
        ["driverRegistered"] = "Profile saved: {0}, plate {1}, {2}. You can update this any time with /registerdriver. Send /trips to see available trips.",
        ["driverProfileRequired"] = "Please set up your driver profile first with /registerdriver.",
        ["noTripsAvailable"] = "No trips available right now - a trip appears here once an admin posts a delivery and its order is fully paid. Check back soon.",
        ["noTripsYet"] = "You haven't accepted any trips yet. Use /trips to see what's available.",

        // --- Driver: trip board (/trips, /mytrips) ---
        ["chooseTrip"] = "Available trips - reply with a number to accept, or /cancel:\n\n{0}",
        ["invalidTripNumber"] = "Please reply with a valid trip number from the list, or /cancel.",
        ["tripAccepted"] = "Trip accepted: order {0} to {1}. Use /mytrips to see it, and /deliver once you've dropped it off.",
        ["myTripsHeader"] = "Your trips:\n\n{0}",
        ["noTripsToDeliver"] = "You have no trips awaiting delivery. Use /trips to accept one first.",
        ["chooseTripToDeliver"] = "Which trip did you just deliver? Reply with a number, or /cancel:\n\n{0}",
        ["askReceivedByName"] = "Who received the delivery? (name of the person at the hotel)",
        ["tripDelivered"] = "Marked delivered for order {0}. Thank you!"
    };

    private static readonly Dictionary<string, string> Amharic = new()
    {
        ["welcome"] =
            "እንኳን ወደ AgriLink - የእርሻ-ሆቴል አቅርቦት አስተዳደር በደህና መጡ።\n\n" +
            "እርስዎ ትዕዛዝ መስጠት የሚፈልጉ ሆቴል/ሬስቶራንት ነዎት፣ ወይስ የAgriLink የመስክ ወኪል (የሆቴል ወኪል፣ የገበሬ ወኪል ወይም ሹፌር) ነዎት?\n\n" +
            "\u2022 ሆቴል/ሬስቶራንት: መለያዎን ለማዘጋጀት /register ይላኩ፣ ከዚያ /order።\n" +
            "\u2022 የመስክ ወኪል ወይም ሹፌር: አስተዳዳሪዎ በሰጠዎት ኢሜይል እና የይለፍ ቃል /login ይላኩ።\n\n" +
            "የሚችሉትን ለማየት በማንኛውም ጊዜ /help ይላኩ።",
        ["help"] =
            "ትዕዛዞች:\n" +
            "/register - የሆቴል/ሬስቶራንት መለያዎን ያዘጋጁ (መለያ ቁጥር አያስፈልግም)\n" +
            "/order - አዲስ ትዕዛዝ ይጀምሩ\n" +
            "/myorders - የቅርብ ጊዜ ትዕዛዞችዎን ይመልከቱ\n" +
            "/orderstatus <orderId> - የአንድ ትዕዛዝ ሁኔታ ይመልከቱ\n" +
            "/login - በምትኩ እንደ AgriLink የመስክ ወኪል ይግቡ\n" +
            "/language en|am - ቋንቋ ይቀይሩ\n" +
            "/cancel - በመሃል ላይ ያለውን ይሰርዙ",
        ["helpHotelAgent"] =
            "እንደ {0} (የሆቴል ወኪል) ገብተዋል። ትዕዛዞች:\n" +
            "/newhotel - አዲስ ሆቴል/ሬስቶራንት ይመዝግቡ\n" +
            "/neworder - ለሆቴል የወሰዱትን ትዕዛዝ ይመዝግቡ\n" +
            "/myinvoices - የሆቴሎችዎን ደረሰኞች ይመልከቱ\n" +
            "/pay - ደረሰኝ ይክፈሉ (ቻፓ፣ ቴሌብር፣ CBE Birr፣ ወይም ማንኛውም ባንክ)\n" +
            "/mycommission - እስካሁን ያገኙትን ኮሚሽን ይመልከቱ\n" +
            "/whoami - እንደ ማን እንደገቡ ያሳዩ\n" +
            "/logout - ይውጡ\n" +
            "/language en|am - ቋንቋ ይቀይሩ\n" +
            "/cancel - በመሃል ላይ ያለውን ይሰርዙ",
        ["helpFarmerAgent"] =
            "እንደ {0} (የገበሬ ወኪል) ገብተዋል። ትዕዛዞች:\n" +
            "/newfarmer - አዲስ አርሶ አደር ወይም ህብረት ስራ ማህበር ይመዝግቡ\n" +
            "/stockin - ከአርሶ አደር የተቀበሉትን ምርት ይመዝግቡ\n" +
            "/myfarmerinvoices - ስለ መዘገቡት ምርት ኩባንያው የሚከፍለውን ይመልከቱ\n" +
            "/mycommission - እስካሁን ያገኙትን ኮሚሽን ይመልከቱ\n" +
            "/whoami - እንደ ማን እንደገቡ ያሳዩ\n" +
            "/logout - ይውጡ\n" +
            "/language en|am - ቋንቋ ይቀይሩ\n" +
            "/cancel - በመሃል ላይ ያለውን ይሰርዙ",
        ["helpDriver"] =
            "እንደ {0} (ሹፌር) ገብተዋል። ትዕዛዞች:\n" +
            "/registerdriver - የተሽከርካሪ መገለጫዎን ያዘጋጁ ወይም ያዘምኑ (በመለያ አንድ ብቻ)\n" +
            "/trips - ያሉ ጉዞዎችን ይመልከቱ እና ይቀበሉ\n" +
            "/mytrips - የተቀበሏቸውን ጉዞዎች ይመልከቱ\n" +
            "/deliver - የተቀበሉትን ጉዞ ርክክብ ያረጋግጡ\n" +
            "/mypayments - ስላጠናቀቋቸው ጉዞዎች ኩባንያው የሚከፍልዎትን ይመልከቱ\n" +
            "/whoami - እንደ ማን እንደገቡ ያሳዩ\n" +
            "/logout - ይውጡ\n" +
            "/language en|am - ቋንቋ ይቀይሩ\n" +
            "/cancel - በመሃል ላይ ያለውን ይሰርዙ",
        ["languageSet"] = "ቋንቋ ወደ አማርኛ ተቀይሯል።",
        ["languageUsage"] = "አጠቃቀም: /language en  ወይም  /language am",

        ["loginAskEmail"] = "የኢሜይል አድራሻዎን ያስገቡ:",
        ["loginAskPassword"] = "የይለፍ ቃልዎን ያስገቡ:",
        ["loginFailed"] = "ኢሜይሉ ወይም የይለፍ ቃሉ ትክክል አይደለም። እንደገና /login ይሞክሩ፣ ወይም አስተዳዳሪዎ የይለፍ ቃልዎን እንዲያድስ ይጠይቁ።",
        ["loginRequiresTwoFactor"] = "የእርስዎ መለያ ባለሁለት-ደረጃ ማረጋገጫ አለው - እባክዎ በAgriLink ዌብ አፕሊኬሽን ይግቡ፣ ቦቱ የማረጋገጫ ኮድ ደረጃን አይደግፍም።",
        ["loginWrongRole"] = "ይህ ቦት ለሆቴል እና ለገበሬ ወኪሎች ነው። የእርስዎ መለያ ሚና ({0}) እነዚህን ትዕዛዞች አይጠቀምም - እባክዎ የAgriLink ዌብ አፕሊኬሽን ይሞክሩ።",
        ["loginSuccessHotelAgent"] = "እንደ {0} (የሆቴል ወኪል) ገብተዋል። የሚችሉትን ለማየት /help ይላኩ።",
        ["loginSuccessFarmerAgent"] = "እንደ {0} (የገበሬ ወኪል) ገብተዋል። የሚችሉትን ለማየት /help ይላኩ።",
        ["loggedOut"] = "ወጥተዋል።",
        ["notLoggedIn"] = "አልገቡም። መጀመሪያ /login ይላኩ።",
        ["wrongAgentRole"] = "ይህ ትዕዛዝ ለ{0} መለያዎች ብቻ ነው።",
        ["whoAmIAgent"] = "እንደ {0} ({1}) ገብተዋል።",
        ["whoAmICustomer"] = "እንደ ደንበኛ {0} ተመዝግበዋል።",
        ["whoAmINone"] = "እስካሁን አልተመዘገቡም ወይም አልገቡም። /register (ሆቴል/ሬስቶራንት) ወይም /login (የመስክ ወኪል) ይላኩ።",
        ["actionFailed"] = "ይቅርታ፣ አልተሳካም: {0}",

        ["registerAskName"] = "የሆቴል/ሬስቶራንት መለያዎን እናዘጋጅ። የሆቴልዎ ወይም የሬስቶራንትዎ ስም ማን ይባላል?",
        ["askPhone"] = "ለማግኘት የሚሆን ስልክ ቁጥርዎ ስንት ነው?",
        ["askAddress"] = "አድራሻዎ ወይም ያሉበት ዞን/አካባቢ ምንድን ነው?",
        ["askLocation"] = "የት አካባቢ/ቦታ ላይ ናቸው?",
        ["registered"] = "ተዘጋጅተዋል፣ {0}! ትዕዛዝ ለመስጠት በማንኛውም ጊዜ /order ይላኩ።",
        ["notRegistered"] = "እስካሁን መለያዎን አላዘጋጁም። ለመጀመር /register ይላኩ።",

        ["noProducts"] = "በአሁኑ ጊዜ ምንም የሚታዘዙ ምርቶች የሉም። እባክዎ ቆይተው ይሞክሩ።",
        ["chooseProduct"] = "በቁጥር ምርት ይምረጡ፣ ወይም /cancel:\n\n{0}",
        ["invalidProductNumber"] = "እባክዎ ከዝርዝሩ ትክክለኛ የምርት ቁጥር ይላኩ፣ ወይም /cancel።",
        ["askQuantity"] = "ስንት {0} ይፈልጋሉ? (ቁጥር ያስገቡ)",
        ["invalidQuantity"] = "እባክዎ አዎንታዊ ቁጥር ያስገቡ፣ ወይም /cancel።",
        ["itemAdded"] = "{0} x {1} ወደ ትዕዛዝዎ ታክሏል።\n\nተጨማሪ ለመጨመር ሌላ የምርት ቁጥር ይላኩ፣ ጨርሰው ከሆነ /done፣ ወይም /cancel።",
        ["itemAddedThenDone"] = "{0} x {1} ወደ ትዕዛዙ ታክሏል።\n\nተጨማሪ ለመጨመር ሌላ የምርት ቁጥር ይላኩ፣ ጨርሰው ከሆነ /done፣ ወይም /cancel።",
        ["cartEmpty"] = "ጋሪዎ ባዶ ነው። ለመጨመር የምርት ቁጥር ይምረጡ፣ ወይም /cancel።",
        ["orderSummary"] = "የትዕዛዝ ማጠቃለያ:\n{0}\nጠቅላላ (ከግብር በፊት): {1} ብር\n\nይህን ትዕዛዝ ለማስገባት /confirm ይላኩ፣ ወይም ለመሰረዝ /cancel።",
        ["orderCancelled"] = "ተሰርዟል።",
        ["orderConfirmedFallback"] = "ትዕዛዙን ለማስገባት እባክዎ /confirm ይላኩ፣ ወይም ለመሰረዝ /cancel።",
        ["orderPlaced"] = "ትዕዛዝ ገብቷል! የትዕዛዝ ማጣቀሻ: {0}\n\nየAgriLink ሰራተኛ በቅርቡ ደረሰኙን ያዘጋጃል።",
        ["orderFailed"] = "ይቅርታ፣ ያንን ትዕዛዝ በማስገባት ላይ ችግር ተፈጥሯል: {0}",
        ["unknownCommand"] = "ይቅርታ፣ ያንን አልተረዳሁትም። ምን ማድረግ እንደምችል ለማየት /help ይላኩ።",
        ["myOrdersEmpty"] = "እስካሁን ምንም ትዕዛዝ የለዎትም። አንድ ለማስገባት /order ይላኩ።",
        ["myOrdersHeader"] = "የቅርብ ጊዜ ትዕዛዞችዎ:\n\n{0}",
        ["orderStatusUsage"] = "አጠቃቀም: /orderstatus <orderId>",
        ["orderStatusNotFound"] = "በዚያ መለያ ምንም ትዕዛዝ አልተገኘም፣ ወይም የእርስዎ አይደለም።",
        ["orderStatusResult"] = "ትዕዛዝ {0}\nሁኔታ: {1}\nቀን: {2}\nንዑስ ድምር: {3} ብር\n\n{4}",
        ["orderStatusNoDelivery"] = "\U0001F69A ማድረሻ: እስካሁን ለመውሰድ አልተለጠፈም።",
        ["orderStatusDelivery"] = "\U0001F69A ማድረሻ: {0}\nሹፌር: {1}\nተሽከርካሪ: {2}",
        ["orderStatusNoDriverYet"] = "እስካሁን አልተመደበም",

        ["newHotelAskName"] = "የሆቴል/ሬስቶራንት ስም ማን ይባላል?",
        ["newHotelCreated"] = "ሆቴል \"{0}\" ተመዝግቧል። አሁን /neworder በመጠቀም ለነሱ ትዕዛዝ መመዝገብ ይችላሉ።\n(መለያ: {1})",

        ["noHotelsYet"] = "እስካሁን ምንም ሆቴል አልተመዘገበም። መጀመሪያ /newhotel ይላኩ።",
        ["chooseHotel"] = "ይህ ትዕዛዝ ለየትኛው ሆቴል ነው? በቁጥር ይመልሱ፣ ወይም /cancel:\n\n{0}",
        ["invalidHotelNumber"] = "እባክዎ ከዝርዝሩ ትክክለኛ የሆቴል ቁጥር ይላኩ፣ ወይም /cancel።",

        ["newFarmerAskName"] = "የአርሶ አደሩ ወይም የህብረት ስራ ማህበሩ ስም ማን ይባላል?",
        ["newFarmerCreated"] = "አርሶ አደር \"{0}\" ተመዝግቧል። አሁን /stockin በመጠቀም ከነሱ ምርት መመዝገብ ይችላሉ።\n(መለያ: {1})",

        ["noFarmersYet"] = "እስካሁን ምንም አርሶ አደር አልተመዘገበም። መጀመሪያ /newfarmer ይላኩ።",
        ["chooseFarmer"] = "ይህ ምርት ከየትኛው አርሶ አደር ነው? በቁጥር ይመልሱ፣ ወይም /cancel:\n\n{0}",
        ["invalidFarmerNumber"] = "እባክዎ ከዝርዝሩ ትክክለኛ የአርሶ አደር ቁጥር ይላኩ፣ ወይም /cancel።",
        ["askStockQuantity"] = "ስንት {0} ተቀበሉ? (ቁጥር ያስገቡ፣ በ{1})",
        ["stockItemAdded"] = "{0} {1} {2} ተመዝግቧል።\n\nከዚሁ አርሶ አደር ተጨማሪ ለመመዝገብ ሌላ የምርት ቁጥር ይላኩ፣ ጨርሰው ከሆነ /stockdone፣ ወይም /cancel።",
        ["stockCartEmpty"] = "እስካሁን ምንም አልተመዘገበም። መጀመሪያ የምርት ቁጥር ይምረጡ፣ ወይም /cancel።",
        ["stockSummary"] = "ከ{0} የተቀበለው ምርት:\n{1}\nለአርሶ አደሩ የሚከፈል: {2:0.##} ብር\nይህን ለማስቀመጥ /stockconfirm ይላኩ፣ ወይም ለመሰረዝ /cancel።",
        ["stockConfirmedFallback"] = "ይህን ለማስቀመጥ እባክዎ /stockconfirm ይላኩ፣ ወይም ለመሰረዝ /cancel።",
        ["stockRecorded"] = "ምርት ተመዝግቧል። ለዚህ ኮሚሽንዎ በራስ-ሰር ተመዝግቧል።\n(ማጣቀሻ: {0})",

        // --- ኮሚሽን (/mycommission) ---
        ["noCommissionYet"] = "እስካሁን ምንም ኮሚሽን አልተመዘገበም። የላኩት ትዕዛዝ ደረሰኝ ሲወጣለት (ለሆቴል ወኪሎች) ወይም የመዘገቡት ክምችት ሲመዘገብ (ለገበሬ ወኪሎች) እዚህ ይታያል።",
        ["myCommission"] = "የእርስዎ ኮሚሽን:\nዛሬ: {0} ብር\nበዚህ ወር: {1} ብር\nጠቅላላ: {2} ብር ({3} ግቤቶች)",

        // --- Invoices & payment (/myinvoices, /pay) ---
        ["noInvoicesYet"] = "እስካሁን ምንም ደረሰኝ የለዎትም።",
        ["myInvoicesHeader"] = "የእርስዎ ደረሰኞች:\n{0}\nገና ያልተከፈለውን ለመክፈል /pay ይጠቀሙ።",
        ["noUnpaidInvoices"] = "አሁን ምንም የሚከፈል የለም - የሚያዩት ደረሰኝ ሁሉ ሙሉ በሙሉ ተከፍሏል።",
        ["choosePayInvoice"] = "የትኛውን ደረሰኝ መክፈል ይፈልጋሉ?\n{0}\nቁጥሩን ይላኩ።",
        ["invalidInvoiceNumber"] = "ከታዩት ቁጥሮች ውስጥ አይደለም። እንደገና ይሞክሩ፣ ወይም /cancel።",
        ["choosePayMethod"] = "እንዴት መክፈል ይፈልጋሉ?\n{0}\nቁጥሩን ይላኩ።",
        ["anyBankOption"] = "በኢትዮጵያ ውስጥ ማንኛውም ባንክ (ካስተላለፉ በኋላ ማጣቀሻውን ይንገሩን)",
        ["invalidChoice"] = "ከታዩት አማራጮች ውስጥ አይደለም። እንደገና ይሞክሩ፣ ወይም /cancel።",
        ["payOnlineLink"] = "ለመክፈል ይህን ሊንክ ይክፈቱ:\n{0}\nከጨረሱ በኋላ ተመልሰው /myinvoices ያረጋግጡ።",
        ["askCbeBirrReference"] = "የCBE Birr ግብይት ማጣቀሻ ቁጥር ስንት ነው?",
        ["askCbeBirrPhone"] = "የከፋዩ የCBE Birr ስልክ ቁጥር ስንት ነው?",
        ["paymentVerified"] = "ክፍያው ተረጋግጧል - ደረሰኝ {0} አሁን እንደተከፈለ ተመዝግቧል።",
        ["paymentVerificationFailed"] = "ያንን ክፍያ ማረጋገጥ አልተቻለም: {0}\nእንደገና መሞከር ይችላሉ፣ ወይም /pay ተጠቅመው \"ማንኛውም ባንክ\" ን በመምረጥ አስተዳዳሪ እንዲያረጋግጠው በእጅ መዝግበው ያስቀምጡ።",
        ["noBankAccounts"] = "እስካሁን ምንም የባንክ አካውንት አልተዋቀረም - አስተዳዳሪ በቅንብሮች ውስጥ እንዲጨምር ይጠይቁ።",
        ["chooseBankAccount"] = "ወደየትኛው አካውንት ያስተላልፋሉ?\n{0}\nቁጥሩን ይላኩ።",
        ["askBankReference"] = "ወደዚህ ያስተላልፉ: {0}\nከላኩ በኋላ የግብይት ማጣቀሻ ቁጥሩ ስንት ነው?",
        ["paymentRecordedPendingReconciliation"] = "ተመዝግቧል - በደረሰኝ {0} ላይ ተመዝግቧል። አስተዳዳሪ ከባንክ መግለጫው ጋር በማመሳከር ያረጋግጣል።",

        // --- Farmer invoices (/myfarmerinvoices) ---
        ["noFarmerInvoicesYet"] = "እስካሁን ምንም የአርሶ አደር ደረሰኝ የለም - በ/stockin ክምችት ሲመዘግቡ በራስ-ሰር ይታያሉ።",
        ["myFarmerInvoicesHeader"] = "የእርስዎ የአርሶ አደር ደረሰኞች:\n{0}",

        // --- Driver payments (/mypayments) ---
        ["noDriverPaymentsYet"] = "እስካሁን ምንም የጉዞ ክፍያ የለም - በ/deliver ማድረሻን ሲያጠናቅቁ በራስ-ሰር ይታያሉ።",
        ["myDriverPaymentsHeader"] = "የእርስዎ የጉዞ ክፍያዎች:\n{0}",

        // --- ሹፌር: መገለጫ መመዝገቢያ/ማዘመኛ (/registerdriver) ---
        ["driverAskName"] = "የሹፌር መገለጫዎን እናዘጋጅ። ሙሉ ስምዎ ማን ይባላል?",
        ["driverAskPlate"] = "የተሽከርካሪዎ ታርጋ ቁጥር ስንት ነው?",
        ["driverAskTruckType"] = "የሚያሽከረክሩት የመኪና አይነት ምንድን ነው? በቁጥር ይመልሱ:\n1. ፒክ አፕ\n2. አነስተኛ መኪና\n3. መካከለኛ መኪና\n4. ኢሱዙ\n5. ከባድ መኪና\n6. ትሬይለር\n7. ሌላ",
        ["invalidTruckTypeNumber"] = "እባክዎ ከ1 እስከ 7 ያለ ቁጥር ይላኩ።",
        ["driverRegistered"] = "መገለጫ ተቀምጧል: {0}፣ ታርጋ {1}፣ {2}። ይህን በማንኛውም ጊዜ በ/registerdriver ማዘመን ይችላሉ። ያሉ ጉዞዎችን ለማየት /trips ይላኩ።",
        ["driverProfileRequired"] = "እባክዎ መጀመሪያ በ/registerdriver የሹፌር መገለጫዎን ያዘጋጁ።",
        ["noTripsAvailable"] = "በአሁኑ ጊዜ ምንም ጉዞ የለም - አስተዳዳሪ ማድረሻ ከለጠፈ እና ትዕዛዙ ሙሉ በሙሉ ከተከፈለ በኋላ ጉዞ እዚህ ይታያል። ትንሽ ቆይተው ይመልከቱ።",
        ["noTripsYet"] = "እስካሁን ምንም ጉዞ አልተቀበሉም። ያሉትን ለማየት /trips ይላኩ።",

        // --- ሹፌር: የጉዞ ሰሌዳ (/trips, /mytrips) ---
        ["chooseTrip"] = "ያሉ ጉዞዎች - ለመቀበል በቁጥር ይመልሱ፣ ወይም /cancel:\n\n{0}",
        ["invalidTripNumber"] = "እባክዎ ከዝርዝሩ ትክክለኛ የጉዞ ቁጥር ይላኩ፣ ወይም /cancel።",
        ["tripAccepted"] = "ጉዞ ተቀብለዋል: ትዕዛዝ {0} ወደ {1}። ለማየት /mytrips ይጠቀሙ፣ ካደረሱ በኋላ /deliver ይላኩ።",
        ["myTripsHeader"] = "የእርስዎ ጉዞዎች:\n\n{0}",
        ["noTripsToDeliver"] = "ማድረስ የሚጠብቅ ምንም ጉዞ የለዎትም። መጀመሪያ በ/trips አንድ ይቀበሉ።",
        ["chooseTripToDeliver"] = "የትኛውን ጉዞ ነው ያደረሱት? በቁጥር ይመልሱ፣ ወይም /cancel:\n\n{0}",
        ["askReceivedByName"] = "ማድረሻውን የተቀበለው ማን ነው? (በሆቴሉ ያለው ሰው ስም)",
        ["tripDelivered"] = "ለትዕዛዝ {0} እንደደረሰ ምልክት ተደርጓል። እናመሰግናለን!"
    };
}
