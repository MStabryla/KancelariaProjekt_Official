export class DateAdapter {

  constructor() {
  }

  public toUTCDate(date: Date) {
    return date.getUTCFullYear() + '-' +
      ('0' + (date.getUTCMonth() + 1)).slice(-2) + '-' +
      ('0' + date.getUTCDate()).slice(-2);
  }
}

