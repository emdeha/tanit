int longtitude[7] = {1, 2,3, '.', 5,6, 7};  //Storing longitude
int latitude[8];  //Storing latitude
int height[6];  //Storing height
int next = 0;
char mode = 0;
int  time = 1; 
int  chipselect[] = {2, 3, 4, 5, 6, 7,9,8,11,10,13,12};
char chipselectcount = 12;
int  datapins1[] = {22, 24, 26, 28, 30, 32, 34, 36, 38, 40};
int  datapins2[] = {23, 25, 27, 29, 31, 33, 35, 37, 39, 41};
char datapinscount = 10;
int  data = 0x02;
char ledBarCount = 10;
int  zoom, zoomOld, velocity;
char sign; //0  no sign, 1 - minus
/*
void clearDataPins() {
  for(char pin = 0; pin < datapinscount; pin++)
    digitalWrite(datapins[pin], HIGH);
}
*/
void clearCsPins() {
  for(char pin = 0; pin < chipselectcount; pin++)
    digitalWrite(chipselect[pin], LOW);
}

void writeData1Pins(int data) {
    for(char i = 0; i < 10; i++) {
    if( !(data & 1) )
      digitalWrite(datapins1[i], HIGH);
    else
      digitalWrite(datapins1[i], LOW);
    data >>=1;
  }
}
void writeData2Pins(int data) {
    for(char i = 0; i < 10; i++) {
    if( !(data & 1) )
      digitalWrite(datapins2[i], HIGH);
    else
      digitalWrite(datapins2[i], LOW);
    data >>=1;
  }
}

void writeLedBar(int data) {
  data = pow(2, data) - 1;
  if(data >= 2)  //Some problem while float to int conversion is applied
    data++;
  clearCsPins();
  writeData1Pins(data);
  digitalWrite(chipselect[0],  HIGH);
  delay(time);
}

void writeLeds(int data) {
  data = pow(2, data) - 1;
  if(data >= 2)  //Some problem while float to int conversion is applied
    data++;
  clearCsPins();
  writeData1Pins(data);
  digitalWrite(chipselect[1],  HIGH);
  delay(time);
}

void readZoom() {
  //Read analog value ;
  zoom = analogRead(A0);
  //Send to RS232 
  //Update only new values to PC
  if (zoom != zoomOld) {
    Serial.println(zoom);
  }
  zoomOld = zoom;
  //Show analog value to 10 segment bar
  zoom = (zoom * ledBarCount) / 669;
  writeLedBar(zoom);  
}

//, - 64, 
void writeLetters(char d1, char d2) {
  clearCsPins();
  writeData1Pins(d1);
  digitalWrite(chipselect[2],  HIGH);
  delay(time);
  
  clearCsPins();
  writeData1Pins(d2);
  digitalWrite(chipselect[3],  HIGH);
  delay(time);
  
  clearCsPins();
  writeData1Pins(64);
  digitalWrite(chipselect[4],  HIGH);
  delay(time);
}

int toBCD(int data)
{
  switch (data) {
    case 0: return 0x3F;
    case 1: return 0x06;
    case 2: return 0x5b;
    case 3: return 0x4f;
    case 4: return 0x66;
    case 5: return 0x6d;
    case 6: return 0x7d;
    case 7: return 0x07;
    case 8: return 0x7f;
    case 9: return 0x6f;
    case 'a': return 119;
    case 'c': return 57;
    case 'd': return 94;
    case 'e': return 121;
    case 'h': return 118;
    case 'i': return 6;
    case 'l': return 56;
    case 'o': return 63;
    case '-': return 64;
    case '.': return 128;
    case '_': return 0;
  }
}

void writeLetterID() {
  writeLetters(toBCD('i'),toBCD('d')); //Id
}
void writeLetterLO() {
  writeLetters(toBCD('l'),toBCD('o')); //Longitude
}
void writeLetterLA() {
  writeLetters(toBCD('l'),toBCD('a')); //Latitude
}
void writeLetterHE() {
  writeLetters(toBCD('h'),toBCD('e')); //Height
}

void writeSign(char sign) {
  clearCsPins();
  if (sign)
    writeData2Pins(1);
  else
    writeData2Pins(2);
  digitalWrite(chipselect[5],  HIGH);
  delay(time);  
}
void writeDigits(int *data) {
  
  for(char i = 0; i < 6; i++) {
    clearCsPins();
    writeData2Pins(toBCD(data[i]));
    digitalWrite(chipselect[6+i],  HIGH);
    delay(time);
  }
}

void writeSegment() {

  switch (mode) {
    case 0:
        writeLetterLO();
        writeSign(longtitude[6]);
        writeDigits(longtitude+1);
      break;
    case 1:
        writeLetterLA();
        writeSign(latitude[7]);
        writeDigits(latitude+1);
      break;
    case 2:
        writeLetterHE();
        writeSign(0);
       writeDigits(height);
      break;
  }
  if(next > 300)
  {
    next = 0;
    mode++;
    if(mode == 3)
      mode = 0;
  }
  next++;
}

void setup() {
  Serial.begin(9600);
  //Chip Select pins
  for(char pin = 0; pin < chipselectcount; pin++)
    pinMode(chipselect[pin], OUTPUT);
  //Data pins
  for(char pin = 0; pin < datapinscount; pin++) {
    pinMode(datapins1[pin], OUTPUT);
    pinMode(datapins2[pin], OUTPUT);
  }
}  


void loop() {
  readZoom();
  writeLeds(velocity); 
  writeSegment();
}

void serialEvent() {
  //Values are received as follows:  Longitude, Latitude, Height, Velocity,
  // values are separated using comma ,
  //float poin is dot .
  //Velocity 0 - 10
  //Longitude +- 90.414
  //Latitude  +- 180.414
  char value = 0;
  char i = 0;
  char tmp;
  
 
  while (Serial.available()) {
    tmp = (char)Serial.read();
    
    switch (tmp) {
      case ',': i = 0; value++; continue;
      case ';': i = 0; value = 0; continue;
    }
    
    switch (value) {
      case 0: longtitude[i] += tmp; break;
      case 1: latitude[i] += tmp; break;
      case 2: height[i] += tmp; break;
      case 3: velocity = tmp; break;
    }
  i++;    
  }  
} 






