export class BasicModelClass {
  constructor(rawData: unknown) {
    Object.keys(rawData).forEach(x => {
      this[x] = rawData[x];
    })
  }
}
