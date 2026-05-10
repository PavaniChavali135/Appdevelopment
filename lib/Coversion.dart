import 'package:flutter/material.dart';

void main() {
  runApp(const MeasuresConverter());
}

class MeasuresConverter extends StatelessWidget {
  const MeasuresConverter({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Measures Converter',
      theme: ThemeData(primarySwatch: Colors.blue),
      home: const ConverterHome(),
    );
  }
}

class ConverterHome extends StatefulWidget {
  const ConverterHome({super.key});

  @override
  State<ConverterHome> createState() => _ConverterHomeState();
}

class _ConverterHomeState extends State<ConverterHome> {
  final TextEditingController _numberController = TextEditingController();

  String _startMeasure = 'meters';
  String _convertedMeasure = 'feet';
  String _resultMessage = '';

  // Conversion factors relative to 1 meter
  final Map<String, double> _measures = {
    'meters': 1,
    'kilometers': 1000,
    'inches': 0.0254,
    'feet': 0.3048,
    'miles': 1609.34,
  };

  void _convert() {
    double? input = double.tryParse(_numberController.text);
    if (input == null || input == 0) return;

    // Logic: Convert input to meters, then convert meters to target unit
    double meters = input * _measures[_startMeasure]!;
    double result = meters / _measures[_convertedMeasure]!;

    setState(() {
      _resultMessage =
          "${input.toString()} $_startMeasure are ${result.toStringAsFixed(3)} $_convertedMeasure";
    });
  }

  @override
  Widget build(BuildContext context) {
    // Style for the labels (Value, From, To)
    const labelStyle = TextStyle(
      fontSize: 20,
      color: Colors.grey,
      fontWeight: FontWeight.w500,
    );

    return Scaffold(
      appBar: AppBar(
        title: const Text('Measures Converter'),
        centerTitle: true,
        elevation: 0,
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.symmetric(horizontal: 40, vertical: 20),
        child: Column(
          children: [
            const Text('Value', style: labelStyle),
            TextField(
              controller: _numberController,
              keyboardType: TextInputType.number,
              textAlign: TextAlign.center,
              decoration: const InputDecoration(hintText: "Enter value"),
            ),
            const SizedBox(height: 25),

            const Text('From', style: labelStyle),
            DropdownButton<String>(
              isExpanded: true,
              value: _startMeasure,
              items: _measures.keys.map((String value) {
                return DropdownMenuItem<String>(
                  value: value,
                  child: Text(
                    value,
                    style: const TextStyle(color: Colors.blueAccent),
                  ),
                );
              }).toList(),
              onChanged: (value) => setState(() => _startMeasure = value!),
            ),
            const SizedBox(height: 25),

            const Text('To', style: labelStyle),
            DropdownButton<String>(
              isExpanded: true,
              value: _convertedMeasure,
              items: _measures.keys.map((String value) {
                return DropdownMenuItem<String>(
                  value: value,
                  child: Text(
                    value,
                    style: const TextStyle(color: Colors.blueAccent),
                  ),
                );
              }).toList(),
              onChanged: (value) => setState(() => _convertedMeasure = value!),
            ),
            const SizedBox(height: 40),

            ElevatedButton(
              onPressed: _convert,
              style: ElevatedButton.styleFrom(
                backgroundColor: Colors.grey[300],
                foregroundColor: Colors.blue[900],
                padding: const EdgeInsets.symmetric(
                  horizontal: 30,
                  vertical: 12,
                ),
              ),
              child: const Text('Convert', style: TextStyle(fontSize: 18)),
            ),
            const SizedBox(height: 40),

            Text(
              _resultMessage,
              textAlign: TextAlign.center,
              style: const TextStyle(fontSize: 20, color: Colors.grey),
            ),
          ],
        ),
      ),
    );
  }
}
